using System;
using System.Collections.Generic;
using UnityEngine;

namespace Obi
{
	public abstract class ObiColliderBase : MonoBehaviour
	{
		public static Dictionary<int, Component> idToCollider = new Dictionary<int, Component>();

		[SerializeField]
		[HideInInspector]
		private ObiCollisionMaterial material;

		public int phase;

		public float thickness;

		[HideInInspector]
		[SerializeField]
		protected Component unityCollider;

		protected IntPtr oniCollider = IntPtr.Zero;

		protected ObiRigidbody obiRigidbody;

		protected int currentLayer = -1;

		protected bool wasUnityColliderEnabled = true;

		protected float oldPhase;

		protected float oldThickness;

		protected HashSet<ObiSolver> solvers = new HashSet<ObiSolver>();

		protected ObiShapeTracker tracker;

		protected Oni.Collider adaptor = default(Oni.Collider);

		public ObiCollisionMaterial CollisionMaterial
		{
			get
			{
				return material;
			}
			set
			{
				material = value;
				if (material != null)
				{
					Oni.SetColliderMaterial(oniCollider, material.OniCollisionMaterial);
				}
				else
				{
					Oni.SetColliderMaterial(oniCollider, IntPtr.Zero);
				}
			}
		}

		public IntPtr OniCollider => oniCollider;

		protected abstract void CreateTracker();

		protected abstract bool IsUnityColliderEnabled();

		protected abstract void UpdateColliderAdaptor();

		protected void CreateRigidbody()
		{
			obiRigidbody = null;
			Rigidbody componentInParent = GetComponentInParent<Rigidbody>();
			if (componentInParent != null)
			{
				obiRigidbody = componentInParent.GetComponent<ObiRigidbody>();
				if (obiRigidbody == null)
				{
					obiRigidbody = componentInParent.gameObject.AddComponent<ObiRigidbody>();
				}
				Oni.SetColliderRigidbody(oniCollider, obiRigidbody.OniRigidbody);
			}
			else
			{
				Oni.SetColliderRigidbody(oniCollider, IntPtr.Zero);
			}
		}

		public void RegisterInSolver(ObiSolver solver, bool addToSolver)
		{
			if (!solvers.Contains(solver) && (int)solver.collisionLayers == ((int)solver.collisionLayers | (1 << base.gameObject.layer)))
			{
				solvers.Add(solver);
				if (addToSolver)
				{
					Oni.AddCollider(solver.OniSolver, oniCollider);
				}
			}
		}

		public void RemoveFromSolver(ObiSolver solver)
		{
			solvers.Remove(solver);
			Oni.RemoveCollider(solver.OniSolver, oniCollider);
		}

		private void FindSolvers(bool addToSolvers)
		{
			if (base.gameObject.layer == currentLayer)
			{
				return;
			}
			currentLayer = base.gameObject.layer;
			foreach (ObiSolver solver2 in solvers)
			{
				Oni.RemoveCollider(solver2.OniSolver, oniCollider);
			}
			solvers.Clear();
			ObiSolver[] array = UnityEngine.Object.FindObjectsOfType<ObiSolver>();
			ObiSolver[] array2 = array;
			foreach (ObiSolver solver in array2)
			{
				RegisterInSolver(solver, addToSolvers);
			}
		}

		private void Update()
		{
			FindSolvers(true);
		}

		protected virtual void Awake()
		{
			wasUnityColliderEnabled = IsUnityColliderEnabled();
			idToCollider.Add(unityCollider.GetInstanceID(), unityCollider);
			CreateTracker();
			oniCollider = Oni.CreateCollider();
			FindSolvers(false);
			if (tracker != null)
			{
				Oni.SetColliderShape(oniCollider, tracker.OniShape);
			}
			ObiArbiter.OnFrameStart += UpdateIfNeeded;
			ObiArbiter.OnFrameEnd += UpdateRigidbody;
			UpdateColliderAdaptor();
			Oni.UpdateCollider(oniCollider, ref adaptor);
			if (material != null)
			{
				Oni.SetColliderMaterial(oniCollider, material.OniCollisionMaterial);
			}
			else
			{
				Oni.SetColliderMaterial(oniCollider, IntPtr.Zero);
			}
			CreateRigidbody();
		}

		private void OnDestroy()
		{
			if (unityCollider != null)
			{
				idToCollider.Remove(unityCollider.GetInstanceID());
			}
			ObiArbiter.OnFrameStart -= UpdateIfNeeded;
			ObiArbiter.OnFrameEnd -= UpdateRigidbody;
			Oni.DestroyCollider(oniCollider);
			oniCollider = IntPtr.Zero;
			if (tracker != null)
			{
				tracker.Destroy();
				tracker = null;
			}
		}

		public void OnEnable()
		{
			foreach (ObiSolver solver in solvers)
			{
				Oni.AddCollider(solver.OniSolver, oniCollider);
			}
		}

		public void OnDisable()
		{
			foreach (ObiSolver solver in solvers)
			{
				Oni.RemoveCollider(solver.OniSolver, oniCollider);
			}
		}

		private void UpdateIfNeeded(object sender, EventArgs e)
		{
			if (unityCollider != null)
			{
				bool flag = IsUnityColliderEnabled();
				if (unityCollider.transform.hasChanged || (float)phase != oldPhase || thickness != oldThickness || flag != wasUnityColliderEnabled)
				{
					unityCollider.transform.hasChanged = false;
					oldPhase = phase;
					oldThickness = thickness;
					wasUnityColliderEnabled = flag;
					foreach (ObiSolver solver in solvers)
					{
						Oni.RemoveCollider(solver.OniSolver, oniCollider);
					}
					UpdateColliderAdaptor();
					Oni.UpdateCollider(oniCollider, ref adaptor);
					if (flag)
					{
						foreach (ObiSolver solver2 in solvers)
						{
							Oni.AddCollider(solver2.OniSolver, oniCollider);
						}
					}
				}
			}
			if (tracker != null)
			{
				tracker.UpdateIfNeeded();
			}
			if (obiRigidbody != null)
			{
				obiRigidbody.UpdateIfNeeded();
			}
		}

		private void UpdateRigidbody(object sender, EventArgs e)
		{
			if (obiRigidbody != null)
			{
				obiRigidbody.UpdateVelocities();
			}
		}
	}
}
