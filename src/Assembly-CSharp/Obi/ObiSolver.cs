using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Obi
{
	[ExecuteInEditMode]
	[AddComponentMenu("Physics/Obi/Obi Solver")]
	[DisallowMultipleComponent]
	public sealed class ObiSolver : MonoBehaviour
	{
		public enum SimulationOrder
		{
			FixedUpdate,
			AfterFixedUpdate,
			LateUpdate
		}

		public class ObiCollisionEventArgs : EventArgs
		{
			public Oni.Contact[] contacts;

			public ObiCollisionEventArgs(Oni.Contact[] contacts)
			{
				this.contacts = contacts;
			}
		}

		public class ObiFluidEventArgs : EventArgs
		{
			public int[] indices;

			public Vector4[] vorticities;

			public float[] densities;

			public ObiFluidEventArgs(int[] indices, Vector4[] vorticities, float[] densities)
			{
				this.indices = indices;
				this.vorticities = vorticities;
				this.densities = densities;
			}
		}

		public class ParticleInActor
		{
			public ObiActor actor;

			public int indexInActor;

			public ParticleInActor(ObiActor actor, int indexInActor)
			{
				this.actor = actor;
				this.indexInActor = indexInActor;
			}
		}

		public const int MAX_NEIGHBOURS = 92;

		public const int CONSTRAINT_GROUPS = 12;

		public int maxParticles = 5000;

		[NonSerialized]
		[HideInInspector]
		public bool simulate = true;

		[Tooltip("If enabled, will force the solver to keep simulating even when not visible from any camera.")]
		public bool simulateWhenInvisible = true;

		[Tooltip("If enabled, the solver object transform will be used as the frame of reference for all actors using this solver, instead of the world's frame.")]
		public bool simulateInLocalSpace;

		[Tooltip("Determines when will the solver update particles.")]
		public SimulationOrder simulationOrder;

		public LayerMask collisionLayers = 1;

		public Oni.SolverParameters parameters = new Oni.SolverParameters(Oni.SolverParameters.Interpolation.None, new Vector4(0f, -9.81f, 0f, 0f));

		[NonSerialized]
		[HideInInspector]
		public List<ObiActor> actors = new List<ObiActor>();

		private int allocatedParticleCount;

		[NonSerialized]
		[HideInInspector]
		public ParticleInActor[] particleToActor;

		[NonSerialized]
		[HideInInspector]
		public int[] materialIndices;

		[NonSerialized]
		[HideInInspector]
		public int[] fluidMaterialIndices;

		private int[] activeParticles;

		private List<ObiEmitterMaterial> emitterMaterials = new List<ObiEmitterMaterial>();

		[NonSerialized]
		[HideInInspector]
		public Vector4[] renderablePositions;

		[HideInInspector]
		public int[] constraintsOrder;

		public Oni.ConstraintParameters distanceConstraintParameters = new Oni.ConstraintParameters(true, Oni.ConstraintParameters.EvaluationOrder.Sequential, 3);

		public Oni.ConstraintParameters bendingConstraintParameters = new Oni.ConstraintParameters(true, Oni.ConstraintParameters.EvaluationOrder.Parallel, 3);

		public Oni.ConstraintParameters particleCollisionConstraintParameters = new Oni.ConstraintParameters(true, Oni.ConstraintParameters.EvaluationOrder.Parallel, 3);

		public Oni.ConstraintParameters collisionConstraintParameters = new Oni.ConstraintParameters(true, Oni.ConstraintParameters.EvaluationOrder.Parallel, 3);

		public Oni.ConstraintParameters skinConstraintParameters = new Oni.ConstraintParameters(true, Oni.ConstraintParameters.EvaluationOrder.Sequential, 3);

		public Oni.ConstraintParameters volumeConstraintParameters = new Oni.ConstraintParameters(true, Oni.ConstraintParameters.EvaluationOrder.Parallel, 3);

		public Oni.ConstraintParameters tetherConstraintParameters = new Oni.ConstraintParameters(true, Oni.ConstraintParameters.EvaluationOrder.Parallel, 3);

		public Oni.ConstraintParameters pinConstraintParameters = new Oni.ConstraintParameters(true, Oni.ConstraintParameters.EvaluationOrder.Parallel, 3);

		public Oni.ConstraintParameters stitchConstraintParameters = new Oni.ConstraintParameters(true, Oni.ConstraintParameters.EvaluationOrder.Parallel, 2);

		public Oni.ConstraintParameters densityConstraintParameters = new Oni.ConstraintParameters(true, Oni.ConstraintParameters.EvaluationOrder.Parallel, 2);

		private IntPtr oniSolver;

		private ObiEmitterMaterial defaultFluidMaterial;

		private Bounds bounds = default(Bounds);

		private Matrix4x4 lastTransform;

		private bool initialized;

		private bool isVisible = true;

		private float smoothDelta = 0.02f;

		private int renderablePositionsClients;

		public IntPtr OniSolver => oniSolver;

		public Bounds Bounds => bounds;

		public Matrix4x4 LastTransform => lastTransform;

		public bool IsVisible => isVisible;

		public int AllocParticleCount => allocatedParticleCount;

		public bool IsUpdating => initialized && simulate && (simulateWhenInvisible || IsVisible);

		public event EventHandler OnFrameBegin;

		public event EventHandler OnStepBegin;

		public event EventHandler OnFixedParticlesUpdated;

		public event EventHandler OnStepEnd;

		public event EventHandler OnBeforePositionInterpolation;

		public event EventHandler OnBeforeActorsFrameEnd;

		public event EventHandler OnFrameEnd;

		public event EventHandler<ObiCollisionEventArgs> OnCollision;

		public event EventHandler<ObiFluidEventArgs> OnFluidUpdated;

		public void RequireRenderablePositions()
		{
			renderablePositionsClients++;
		}

		public void RelinquishRenderablePositions()
		{
			if (renderablePositionsClients > 0)
			{
				renderablePositionsClients--;
			}
		}

		private void Awake()
		{
			lastTransform = base.transform.localToWorldMatrix;
			if (Application.isPlaying)
			{
				Initialize();
			}
		}

		private void Start()
		{
			if (Application.isPlaying)
			{
				ObiColliderBase[] array = UnityEngine.Object.FindObjectsOfType<ObiColliderBase>();
				ObiColliderBase[] array2 = array;
				foreach (ObiColliderBase obiColliderBase in array2)
				{
					obiColliderBase.RegisterInSolver(this, true);
				}
			}
		}

		private void OnDestroy()
		{
			if (Application.isPlaying)
			{
				Teardown();
				ObiColliderBase[] array = UnityEngine.Object.FindObjectsOfType<ObiColliderBase>();
				ObiColliderBase[] array2 = array;
				foreach (ObiColliderBase obiColliderBase in array2)
				{
					obiColliderBase.RemoveFromSolver(this);
				}
			}
		}

		private void OnEnable()
		{
			if (!Application.isPlaying)
			{
				Initialize();
			}
			StartCoroutine("RunLateFixedUpdate");
			ObiArbiter.RegisterSolver(this);
		}

		private void OnDisable()
		{
			if (!Application.isPlaying)
			{
				Teardown();
			}
			StopCoroutine("RunLateFixedUpdate");
			ObiArbiter.UnregisterSolver(this);
		}

		public void Initialize()
		{
			Teardown();
			try
			{
				defaultFluidMaterial = ScriptableObject.CreateInstance<ObiEmitterMaterialFluid>();
				defaultFluidMaterial.hideFlags = HideFlags.HideAndDontSave;
				oniSolver = Oni.CreateSolver(maxParticles, 92);
				actors = new List<ObiActor>();
				activeParticles = new int[maxParticles];
				particleToActor = new ParticleInActor[maxParticles];
				materialIndices = new int[maxParticles];
				fluidMaterialIndices = new int[maxParticles];
				renderablePositions = new Vector4[maxParticles];
				UpdateEmitterMaterials();
				UpdateParameters();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			finally
			{
				initialized = true;
			}
		}

		private void Teardown()
		{
			if (!initialized)
			{
				return;
			}
			try
			{
				while (actors.Count > 0)
				{
					actors[actors.Count - 1].RemoveFromSolver(null);
				}
				Oni.DestroySolver(oniSolver);
				oniSolver = IntPtr.Zero;
				UnityEngine.Object.DestroyImmediate(defaultFluidMaterial);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			finally
			{
				initialized = false;
			}
		}

		public bool AddActor(ObiActor actor, int numParticles)
		{
			if (particleToActor == null || actor == null)
			{
				return false;
			}
			int[] array = new int[numParticles];
			int num = 0;
			for (int i = 0; i < maxParticles; i++)
			{
				if (num >= numParticles)
				{
					break;
				}
				if (particleToActor[i] == null)
				{
					array[num] = i;
					num++;
				}
			}
			if (num < numParticles)
			{
				return false;
			}
			allocatedParticleCount += numParticles;
			for (int j = 0; j < numParticles; j++)
			{
				particleToActor[array[j]] = new ParticleInActor(actor, j);
			}
			actor.particleIndices = array;
			actors.Add(actor);
			UpdateActiveParticles();
			UpdateEmitterMaterials();
			return true;
		}

		public int RemoveActor(ObiActor actor)
		{
			if (particleToActor == null || actor == null)
			{
				return -1;
			}
			int num = actors.IndexOf(actor);
			if (num > -1)
			{
				allocatedParticleCount -= actor.particleIndices.Length;
				for (int i = 0; i < actor.particleIndices.Length; i++)
				{
					particleToActor[actor.particleIndices[i]] = null;
				}
				actors.RemoveAt(num);
				UpdateActiveParticles();
				UpdateEmitterMaterials();
			}
			return num;
		}

		public void UpdateParameters()
		{
			Oni.SetSolverParameters(oniSolver, ref parameters);
			Oni.SetConstraintGroupParameters(oniSolver, 4, ref distanceConstraintParameters);
			Oni.SetConstraintGroupParameters(oniSolver, 3, ref bendingConstraintParameters);
			Oni.SetConstraintGroupParameters(oniSolver, 5, ref particleCollisionConstraintParameters);
			Oni.SetConstraintGroupParameters(oniSolver, 7, ref collisionConstraintParameters);
			Oni.SetConstraintGroupParameters(oniSolver, 6, ref densityConstraintParameters);
			Oni.SetConstraintGroupParameters(oniSolver, 8, ref skinConstraintParameters);
			Oni.SetConstraintGroupParameters(oniSolver, 2, ref volumeConstraintParameters);
			Oni.SetConstraintGroupParameters(oniSolver, 0, ref tetherConstraintParameters);
			Oni.SetConstraintGroupParameters(oniSolver, 1, ref pinConstraintParameters);
			Oni.SetConstraintGroupParameters(oniSolver, 10, ref stitchConstraintParameters);
			if (constraintsOrder == null || constraintsOrder.Length != 12)
			{
				constraintsOrder = Enumerable.Range(0, 12).ToArray();
			}
			Oni.SetConstraintsOrder(oniSolver, constraintsOrder);
		}

		public void UpdateActiveParticles()
		{
			int num = 0;
			for (int i = 0; i < actors.Count; i++)
			{
				ObiActor obiActor = actors[i];
				if (!obiActor.isActiveAndEnabled)
				{
					continue;
				}
				for (int j = 0; j < obiActor.particleIndices.Length; j++)
				{
					if (obiActor.active[j])
					{
						activeParticles[num] = obiActor.particleIndices[j];
						num++;
					}
				}
			}
			Oni.SetActiveParticles(oniSolver, activeParticles, num);
		}

		public void UpdateEmitterMaterials()
		{
			emitterMaterials = new List<ObiEmitterMaterial> { defaultFluidMaterial };
			foreach (ObiActor actor in actors)
			{
				ObiEmitter obiEmitter = actor as ObiEmitter;
				if (obiEmitter == null)
				{
					continue;
				}
				int num = 0;
				if (obiEmitter.EmitterMaterial != null)
				{
					num = emitterMaterials.IndexOf(obiEmitter.EmitterMaterial);
					if (num < 0)
					{
						num = emitterMaterials.Count;
						emitterMaterials.Add(obiEmitter.EmitterMaterial);
						obiEmitter.EmitterMaterial.OnChangesMade += emitterMaterial_OnChangesMade;
					}
				}
				for (int i = 0; i < actor.particleIndices.Length; i++)
				{
					fluidMaterialIndices[actor.particleIndices[i]] = num;
				}
			}
			Oni.SetFluidMaterialIndices(oniSolver, fluidMaterialIndices, fluidMaterialIndices.Length, 0);
			Oni.FluidMaterial[] array = emitterMaterials.ConvertAll((ObiEmitterMaterial a) => a.GetEquivalentOniMaterial(parameters.mode)).ToArray();
			Oni.SetFluidMaterials(oniSolver, array, array.Length, 0);
		}

		private void emitterMaterial_OnChangesMade(object sender, ObiEmitterMaterial.MaterialChangeEventArgs e)
		{
			ObiEmitterMaterial obiEmitterMaterial = sender as ObiEmitterMaterial;
			int num = emitterMaterials.IndexOf(obiEmitterMaterial);
			if (num >= 0)
			{
				Oni.SetFluidMaterials(oniSolver, new Oni.FluidMaterial[1] { obiEmitterMaterial.GetEquivalentOniMaterial(parameters.mode) }, 1, num);
			}
		}

		public void AccumulateSimulationTime(float dt)
		{
			Oni.AddSimulationTime(oniSolver, dt);
		}

		public void ResetSimulationTime()
		{
			Oni.ResetSimulationTime(oniSolver);
		}

		public void SimulateStep(float stepTime)
		{
			Oni.ClearDiffuseParticles(oniSolver);
			if (this.OnStepBegin != null)
			{
				this.OnStepBegin(this, null);
			}
			foreach (ObiActor actor in actors)
			{
				actor.OnSolverStepBegin();
			}
			Oni.UpdateSkeletalAnimation(oniSolver);
			if (this.OnFixedParticlesUpdated != null)
			{
				this.OnFixedParticlesUpdated(this, null);
			}
			ObiArbiter.FrameStart();
			Oni.UpdateSolver(oniSolver, stepTime);
			ObiArbiter.WaitForAllSolvers();
		}

		public void EndFrame(float frameDelta)
		{
			foreach (ObiActor actor in actors)
			{
				actor.OnSolverPreInterpolation();
			}
			if (this.OnBeforePositionInterpolation != null)
			{
				this.OnBeforePositionInterpolation(this, null);
			}
			Oni.ApplyPositionInterpolation(oniSolver, frameDelta);
			if (renderablePositionsClients > 0)
			{
				Oni.GetRenderableParticlePositions(oniSolver, renderablePositions, renderablePositions.Length, 0);
				if (simulateInLocalSpace)
				{
					Matrix4x4 localToWorldMatrix = base.transform.localToWorldMatrix;
					for (int i = 0; i < renderablePositions.Length; i++)
					{
						renderablePositions[i] = localToWorldMatrix.MultiplyPoint3x4(renderablePositions[i]);
					}
				}
			}
			TriggerFluidUpdateEvents();
			if (this.OnBeforeActorsFrameEnd != null)
			{
				this.OnBeforeActorsFrameEnd(this, null);
			}
			CheckVisibility();
			foreach (ObiActor actor2 in actors)
			{
				actor2.OnSolverFrameEnd();
			}
		}

		private void TriggerFluidUpdateEvents()
		{
			int constraintCount = Oni.GetConstraintCount(oniSolver, 6);
			if (constraintCount > 0 && this.OnFluidUpdated != null)
			{
				int[] indices = new int[constraintCount];
				Vector4[] vorticities = new Vector4[maxParticles];
				float[] densities = new float[maxParticles];
				Oni.GetActiveConstraintIndices(oniSolver, indices, constraintCount, 6);
				Oni.GetParticleVorticities(oniSolver, vorticities, maxParticles, 0);
				Oni.GetParticleDensities(oniSolver, densities, maxParticles, 0);
				this.OnFluidUpdated(this, new ObiFluidEventArgs(indices, vorticities, densities));
			}
		}

		private void TriggerCollisionEvents()
		{
			int constraintCount = Oni.GetConstraintCount(oniSolver, 7);
			if (this.OnCollision != null)
			{
				Oni.Contact[] contacts = new Oni.Contact[constraintCount];
				if (constraintCount > 0)
				{
					Oni.GetCollisionContacts(oniSolver, contacts, constraintCount);
				}
				this.OnCollision(this, new ObiCollisionEventArgs(contacts));
			}
		}

		private bool AreBoundsValid(Bounds bounds)
		{
			return !float.IsNaN(bounds.center.x) && !float.IsInfinity(bounds.center.x) && !float.IsNaN(bounds.center.y) && !float.IsInfinity(bounds.center.y) && !float.IsNaN(bounds.center.z) && !float.IsInfinity(bounds.center.z);
		}

		private void CheckVisibility()
		{
			Vector3 min = Vector3.zero;
			Vector3 max = Vector3.zero;
			Oni.GetBounds(oniSolver, ref min, ref max);
			this.bounds.SetMinMax(min, max);
			isVisible = false;
			if (!AreBoundsValid(this.bounds))
			{
				return;
			}
			Bounds bounds = ((!simulateInLocalSpace) ? this.bounds : this.bounds.Transform(base.transform.localToWorldMatrix));
			Camera[] allCameras = Camera.allCameras;
			foreach (Camera camera in allCameras)
			{
				Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
				if (GeometryUtility.TestPlanesAABB(planes, bounds))
				{
					isVisible = true;
					break;
				}
			}
		}

		private void Update()
		{
			if (!Application.isPlaying)
			{
				return;
			}
			if (this.OnFrameBegin != null)
			{
				this.OnFrameBegin(this, null);
			}
			foreach (ObiActor actor in actors)
			{
				actor.OnSolverFrameBegin();
			}
			if (IsUpdating && simulationOrder != SimulationOrder.LateUpdate)
			{
				AccumulateSimulationTime(Time.deltaTime);
			}
		}

		private IEnumerator RunLateFixedUpdate()
		{
			while (true)
			{
				yield return new WaitForFixedUpdate();
				if (Application.isPlaying && IsUpdating && simulationOrder == SimulationOrder.AfterFixedUpdate)
				{
					SimulateStep(Time.fixedDeltaTime);
				}
			}
		}

		private void FixedUpdate()
		{
			if (Application.isPlaying && IsUpdating && simulationOrder == SimulationOrder.FixedUpdate)
			{
				SimulateStep(Time.fixedDeltaTime);
			}
		}

		public void AllSolversStepEnd()
		{
			TriggerCollisionEvents();
			foreach (ObiActor actor in actors)
			{
				actor.OnSolverStepEnd();
			}
			if (this.OnStepEnd != null)
			{
				this.OnStepEnd(this, null);
			}
			lastTransform = base.transform.localToWorldMatrix;
		}

		private void LateUpdate()
		{
			if (Application.isPlaying && IsUpdating && simulationOrder == SimulationOrder.LateUpdate)
			{
				smoothDelta = Mathf.Lerp(Time.deltaTime, smoothDelta, 0.95f);
				AccumulateSimulationTime(smoothDelta);
				SimulateStep(smoothDelta);
			}
			if (Application.isPlaying)
			{
				EndFrame((simulationOrder != SimulationOrder.LateUpdate) ? Time.fixedDeltaTime : smoothDelta);
				if (this.OnFrameEnd != null)
				{
					this.OnFrameEnd(this, null);
				}
			}
		}
	}
}
