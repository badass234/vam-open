using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Obi
{
	[Serializable]
	public class ObiSkinConstraintBatch : ObiConstraintBatch
	{
		[HideInInspector]
		public List<int> skinIndices = new List<int>();

		[HideInInspector]
		public List<Vector4> skinPoints = new List<Vector4>();

		[HideInInspector]
		public List<Vector4> skinNormals = new List<Vector4>();

		[HideInInspector]
		public List<float> skinRadiiBackstop = new List<float>();

		[HideInInspector]
		public List<float> skinStiffnesses = new List<float>();

		private int[] solverIndices = new int[0];

		public ObiSkinConstraintBatch(bool cooked, bool sharesParticles)
			: base(cooked, sharesParticles)
		{
		}

		public ObiSkinConstraintBatch(bool cooked, bool sharesParticles, float minYoungModulus, float maxYoungModulus)
			: base(cooked, sharesParticles, minYoungModulus, maxYoungModulus)
		{
		}

		public override Oni.ConstraintType GetConstraintType()
		{
			return Oni.ConstraintType.Skin;
		}

		public override void Clear()
		{
			activeConstraints.Clear();
			skinIndices.Clear();
			skinPoints.Clear();
			skinNormals.Clear();
			skinRadiiBackstop.Clear();
			skinStiffnesses.Clear();
			constraintCount = 0;
		}

		public void AddConstraint(int index, Vector4 point, Vector4 normal, float radius, float collisionRadius, float backstop, float stiffness)
		{
			activeConstraints.Add(constraintCount);
			skinIndices.Add(index);
			skinPoints.Add(point);
			skinNormals.Add(normal);
			skinRadiiBackstop.Add(radius);
			skinRadiiBackstop.Add(collisionRadius);
			skinRadiiBackstop.Add(backstop);
			skinStiffnesses.Add(stiffness);
			constraintCount++;
		}

		public void RemoveConstraint(int index)
		{
			if (index < 0 || index >= base.ConstraintCount)
			{
				return;
			}
			activeConstraints.Remove(index);
			for (int i = 0; i < activeConstraints.Count; i++)
			{
				if (activeConstraints[i] > index)
				{
					activeConstraints[i]--;
				}
			}
			skinIndices.RemoveAt(index);
			skinPoints.RemoveAt(index);
			skinNormals.RemoveAt(index);
			skinStiffnesses.RemoveAt(index);
			skinRadiiBackstop.RemoveRange(index * 3, 3);
			constraintCount--;
		}

		public override List<int> GetConstraintsInvolvingParticle(int particleIndex)
		{
			List<int> list = new List<int>(1);
			for (int i = 0; i < base.ConstraintCount; i++)
			{
				if (skinIndices[i] == particleIndex)
				{
					list.Add(i);
				}
			}
			return list;
		}

		public override void Cook()
		{
			batch = Oni.CreateBatch(8, true);
			Oni.SetSkinConstraints(batch, skinIndices.ToArray(), skinPoints.ToArray(), skinNormals.ToArray(), skinRadiiBackstop.ToArray(), skinStiffnesses.ToArray(), base.ConstraintCount);
			if (Oni.CookBatch(batch))
			{
				constraintCount = Oni.GetBatchConstraintCount(batch);
				activeConstraints = Enumerable.Range(0, constraintCount).ToList();
				int[] array = new int[constraintCount];
				Vector4[] array2 = new Vector4[constraintCount];
				Vector4[] array3 = new Vector4[constraintCount];
				float[] array4 = new float[constraintCount * 3];
				float[] array5 = new float[constraintCount];
				Oni.GetSkinConstraints(batch, array, array2, array3, array4, array5);
				skinIndices = new List<int>(array);
				skinPoints = new List<Vector4>(array2);
				skinNormals = new List<Vector4>(array3);
				skinRadiiBackstop = new List<float>(array4);
				skinStiffnesses = new List<float>(array5);
				int batchPhaseCount = Oni.GetBatchPhaseCount(batch);
				int[] collection = new int[batchPhaseCount];
				Oni.GetBatchPhaseSizes(batch, collection);
				phaseSizes = new List<int>(collection);
			}
			Oni.DestroyBatch(batch);
			batch = IntPtr.Zero;
		}

		protected override void OnAddToSolver(ObiBatchedConstraints constraints)
		{
			solverIndices = new int[skinIndices.Count];
			for (int i = 0; i < skinIndices.Count; i++)
			{
				solverIndices[i] = constraints.Actor.particleIndices[skinIndices[i]];
				solverIndices[i] = constraints.Actor.particleIndices[skinIndices[i]];
			}
		}

		protected override void OnRemoveFromSolver(ObiBatchedConstraints constraints)
		{
		}

		public override void PushDataToSolver(ObiBatchedConstraints constraints)
		{
			if (!(constraints == null) && !(constraints.Actor == null) && constraints.Actor.InSolver)
			{
				ObiSkinConstraints obiSkinConstraints = (ObiSkinConstraints)constraints;
				float[] array = new float[skinStiffnesses.Count];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = StiffnessToCompliance(skinStiffnesses[i] * obiSkinConstraints.stiffness);
				}
				Oni.SetSkinConstraints(batch, solverIndices, skinPoints.ToArray(), skinNormals.ToArray(), skinRadiiBackstop.ToArray(), array, base.ConstraintCount);
				Oni.SetBatchPhaseSizes(batch, phaseSizes.ToArray(), phaseSizes.Count);
			}
		}

		public override void PullDataFromSolver(ObiBatchedConstraints constraints)
		{
			if (!(constraints == null) && !(constraints.Actor == null) && constraints.Actor.InSolver)
			{
				int[] indices = new int[constraintCount];
				Vector4[] array = new Vector4[constraintCount];
				Vector4[] array2 = new Vector4[constraintCount];
				float[] radiiBackstops = new float[constraintCount * 3];
				float[] stiffnesses = new float[constraintCount];
				Oni.GetSkinConstraints(batch, indices, array, array2, radiiBackstops, stiffnesses);
				skinPoints = new List<Vector4>(array);
				skinNormals = new List<Vector4>(array2);
			}
		}

		public Vector3 GetSkinPosition(int index)
		{
			return skinPoints[index];
		}

		public Vector3 GetSkinNormal(int index)
		{
			return skinNormals[index];
		}
	}
}
