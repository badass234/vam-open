using UnityEngine;

namespace Obi
{
	public class ObiRopeCursor : MonoBehaviour
	{
		public ObiRope rope;

		[Range(0f, 1f)]
		public float normalizedCoord;

		public bool direction = true;

		private int FindHotConstraint(ObiDistanceConstraintBatch distanceBatch, int constraint, int maxAmount)
		{
			if (direction)
			{
				int num = distanceBatch.springIndices[constraint * 2 + 1];
				for (int i = 1; i <= maxAmount; i++)
				{
					if (constraint + i == distanceBatch.ConstraintCount || distanceBatch.springIndices[(constraint + i) * 2] != num)
					{
						return constraint + i - 1;
					}
					num = distanceBatch.springIndices[(constraint + i) * 2 + 1];
				}
				return constraint + maxAmount;
			}
			int num2 = distanceBatch.springIndices[constraint * 2];
			for (int j = 1; j <= maxAmount; j++)
			{
				if (constraint - j < 0 || distanceBatch.springIndices[(constraint - j) * 2 + 1] != num2)
				{
					return constraint - j + 1;
				}
				num2 = distanceBatch.springIndices[(constraint - j) * 2];
			}
			return constraint - maxAmount;
		}

		private int AddParticles(int amount)
		{
			ObiDistanceConstraintBatch obiDistanceConstraintBatch = rope.DistanceConstraints.GetBatches()[0] as ObiDistanceConstraintBatch;
			ObiBendConstraintBatch obiBendConstraintBatch = rope.BendingConstraints.GetBatches()[0] as ObiBendConstraintBatch;
			amount = Mathf.Min(amount, rope.PooledParticles);
			if (amount == 0)
			{
				return 0;
			}
			int constraintIndexAtNormalizedCoordinate = rope.GetConstraintIndexAtNormalizedCoordinate(normalizedCoord);
			rope.DistanceConstraints.RemoveFromSolver(null);
			rope.BendingConstraints.RemoveFromSolver(null);
			int[] array = new int[amount + 2];
			int num = 0;
			int num2 = 0;
			while (num < amount && num2 < rope.TotalParticles)
			{
				if (!rope.active[num2])
				{
					array[num + 1] = num2;
					rope.active[num2] = true;
					rope.invMasses[num2] = 10f;
					num++;
				}
				num2++;
			}
			Vector4[] velocities = new Vector4[1] { Vector4.zero };
			Vector4[] array2 = new Vector4[1];
			Vector4[] array3 = new Vector4[1];
			if (direction)
			{
				array[0] = obiDistanceConstraintBatch.springIndices[constraintIndexAtNormalizedCoordinate * 2];
				array[array.Length - 1] = obiDistanceConstraintBatch.springIndices[constraintIndexAtNormalizedCoordinate * 2 + 1];
				normalizedCoord = (float)constraintIndexAtNormalizedCoordinate / (float)(obiDistanceConstraintBatch.ConstraintCount + amount);
				Oni.GetParticlePositions(rope.Solver.OniSolver, array2, 1, rope.particleIndices[array[0]]);
				Oni.GetParticlePositions(rope.Solver.OniSolver, array3, 1, rope.particleIndices[array[array.Length - 1]]);
				obiDistanceConstraintBatch.SetParticleIndex(constraintIndexAtNormalizedCoordinate, array[array.Length - 2], ObiDistanceConstraintBatch.DistanceIndexType.First, rope.Closed);
				obiBendConstraintBatch.SetParticleIndex(constraintIndexAtNormalizedCoordinate, array[array.Length - 2], ObiBendConstraintBatch.BendIndexType.First, rope.Closed);
				obiBendConstraintBatch.SetParticleIndex(constraintIndexAtNormalizedCoordinate - 1, array[1], ObiBendConstraintBatch.BendIndexType.Second, rope.Closed);
				for (int i = 1; i < array.Length - 1; i++)
				{
					Vector4[] positions = new Vector4[1] { array2[0] + (array3[0] - array2[0]) * i / (array.Length - 1) * 0.5f };
					Oni.SetParticlePositions(rope.Solver.OniSolver, positions, 1, rope.particleIndices[array[i]]);
					Oni.SetParticleVelocities(rope.Solver.OniSolver, velocities, 1, rope.particleIndices[array[i]]);
					int constraintIndex = constraintIndexAtNormalizedCoordinate + i - 1;
					obiDistanceConstraintBatch.InsertConstraint(constraintIndex, array[i - 1], array[i], rope.InterparticleDistance, 0f, 0f);
					obiBendConstraintBatch.InsertConstraint(constraintIndex, array[i - 1], array[i + 1], array[i], 0f, 0f, 0f);
				}
			}
			else
			{
				array[0] = obiDistanceConstraintBatch.springIndices[constraintIndexAtNormalizedCoordinate * 2 + 1];
				array[array.Length - 1] = obiDistanceConstraintBatch.springIndices[constraintIndexAtNormalizedCoordinate * 2];
				normalizedCoord = (float)(constraintIndexAtNormalizedCoordinate + amount) / (float)(obiDistanceConstraintBatch.ConstraintCount + amount);
				Oni.GetParticlePositions(rope.Solver.OniSolver, array2, 1, rope.particleIndices[array[0]]);
				Oni.GetParticlePositions(rope.Solver.OniSolver, array3, 1, rope.particleIndices[array[array.Length - 1]]);
				obiDistanceConstraintBatch.SetParticleIndex(constraintIndexAtNormalizedCoordinate, array[array.Length - 2], ObiDistanceConstraintBatch.DistanceIndexType.Second, rope.Closed);
				obiBendConstraintBatch.SetParticleIndex(constraintIndexAtNormalizedCoordinate, array[1], ObiBendConstraintBatch.BendIndexType.First, rope.Closed);
				obiBendConstraintBatch.SetParticleIndex(constraintIndexAtNormalizedCoordinate - 1, array[array.Length - 2], ObiBendConstraintBatch.BendIndexType.Second, rope.Closed);
				for (int j = 1; j < array.Length - 1; j++)
				{
					Vector4[] positions2 = new Vector4[1] { array2[0] + (array3[0] - array2[0]) * j / (array.Length - 1) * 0.5f };
					Oni.SetParticlePositions(rope.Solver.OniSolver, positions2, 1, rope.particleIndices[array[j]]);
					Oni.SetParticleVelocities(rope.Solver.OniSolver, velocities, 1, rope.particleIndices[array[j]]);
					obiDistanceConstraintBatch.InsertConstraint(constraintIndexAtNormalizedCoordinate + 1, array[j], array[j - 1], rope.InterparticleDistance, 0f, 0f);
					obiBendConstraintBatch.InsertConstraint(constraintIndexAtNormalizedCoordinate, array[j + 1], array[j - 1], array[j], 0f, 0f, 0f);
				}
			}
			rope.DistanceConstraints.AddToSolver(null);
			rope.BendingConstraints.AddToSolver(null);
			rope.PushDataToSolver(ParticleData.ACTIVE_STATUS);
			rope.UsedParticles += amount;
			rope.RegenerateRestPositions();
			return amount;
		}

		private int RemoveParticles(int amount)
		{
			ObiDistanceConstraintBatch obiDistanceConstraintBatch = rope.DistanceConstraints.GetBatches()[0] as ObiDistanceConstraintBatch;
			ObiBendConstraintBatch obiBendConstraintBatch = rope.BendingConstraints.GetBatches()[0] as ObiBendConstraintBatch;
			amount = Mathf.Min(amount, rope.UsedParticles - 2);
			int constraintIndexAtNormalizedCoordinate = rope.GetConstraintIndexAtNormalizedCoordinate(normalizedCoord);
			int num = FindHotConstraint(obiDistanceConstraintBatch, constraintIndexAtNormalizedCoordinate, amount);
			amount = Mathf.Min(amount, Mathf.Abs(num - constraintIndexAtNormalizedCoordinate));
			if (amount == 0)
			{
				return 0;
			}
			rope.DistanceConstraints.RemoveFromSolver(null);
			rope.BendingConstraints.RemoveFromSolver(null);
			if (direction)
			{
				normalizedCoord = (float)constraintIndexAtNormalizedCoordinate / (float)(obiDistanceConstraintBatch.ConstraintCount - amount);
				obiDistanceConstraintBatch.SetParticleIndex(constraintIndexAtNormalizedCoordinate, obiDistanceConstraintBatch.springIndices[num * 2 + 1], ObiDistanceConstraintBatch.DistanceIndexType.Second, rope.Closed);
				obiBendConstraintBatch.SetParticleIndex(constraintIndexAtNormalizedCoordinate - 1, obiDistanceConstraintBatch.springIndices[num * 2 + 1], ObiBendConstraintBatch.BendIndexType.Second, rope.Closed);
				obiBendConstraintBatch.SetParticleIndex(num, obiDistanceConstraintBatch.springIndices[constraintIndexAtNormalizedCoordinate * 2], ObiBendConstraintBatch.BendIndexType.First, rope.Closed);
				for (int num2 = constraintIndexAtNormalizedCoordinate + amount; num2 > constraintIndexAtNormalizedCoordinate; num2--)
				{
					rope.active[obiDistanceConstraintBatch.springIndices[num2 * 2]] = false;
					obiDistanceConstraintBatch.RemoveConstraint(num2);
					obiBendConstraintBatch.RemoveConstraint(num2 - 1);
				}
			}
			else
			{
				normalizedCoord = (float)(constraintIndexAtNormalizedCoordinate - amount) / (float)(obiDistanceConstraintBatch.ConstraintCount - amount);
				obiDistanceConstraintBatch.SetParticleIndex(constraintIndexAtNormalizedCoordinate, obiDistanceConstraintBatch.springIndices[num * 2], ObiDistanceConstraintBatch.DistanceIndexType.First, rope.Closed);
				obiBendConstraintBatch.SetParticleIndex(constraintIndexAtNormalizedCoordinate, obiDistanceConstraintBatch.springIndices[num * 2], ObiBendConstraintBatch.BendIndexType.First, rope.Closed);
				obiBendConstraintBatch.SetParticleIndex(num - 1, obiDistanceConstraintBatch.springIndices[constraintIndexAtNormalizedCoordinate * 2 + 1], ObiBendConstraintBatch.BendIndexType.Second, rope.Closed);
				for (int num3 = constraintIndexAtNormalizedCoordinate - 1; num3 >= constraintIndexAtNormalizedCoordinate - amount; num3--)
				{
					rope.active[obiDistanceConstraintBatch.springIndices[num3 * 2 + 1]] = false;
					obiDistanceConstraintBatch.RemoveConstraint(num3);
					obiBendConstraintBatch.RemoveConstraint(num3);
				}
			}
			rope.DistanceConstraints.AddToSolver(null);
			rope.BendingConstraints.AddToSolver(null);
			rope.PushDataToSolver(ParticleData.ACTIVE_STATUS);
			rope.UsedParticles -= amount;
			rope.RegenerateRestPositions();
			return amount;
		}

		public void ChangeLength(float newLength)
		{
			if (rope == null)
			{
				return;
			}
			newLength = Mathf.Clamp(newLength, 0f, (float)(rope.TotalParticles - 1) * rope.InterparticleDistance);
			int constraintIndexAtNormalizedCoordinate = rope.GetConstraintIndexAtNormalizedCoordinate(normalizedCoord);
			ObiDistanceConstraintBatch obiDistanceConstraintBatch = rope.DistanceConstraints.GetBatches()[0] as ObiDistanceConstraintBatch;
			float num = newLength - rope.RestLength;
			float num2 = Mathf.Clamp(num, 0f - obiDistanceConstraintBatch.restLengths[constraintIndexAtNormalizedCoordinate], rope.InterparticleDistance - obiDistanceConstraintBatch.restLengths[constraintIndexAtNormalizedCoordinate]);
			obiDistanceConstraintBatch.restLengths[constraintIndexAtNormalizedCoordinate] += num2;
			num -= num2;
			int num3 = ((!(num > 0f)) ? Mathf.FloorToInt(num / rope.InterparticleDistance) : Mathf.CeilToInt(num / rope.InterparticleDistance));
			float value = ObiUtils.Mod(num, rope.InterparticleDistance);
			if (num3 > 0)
			{
				if (AddParticles(num3) == 0)
				{
					value = rope.InterparticleDistance;
				}
				constraintIndexAtNormalizedCoordinate = rope.GetConstraintIndexAtNormalizedCoordinate(normalizedCoord);
				obiDistanceConstraintBatch.restLengths[constraintIndexAtNormalizedCoordinate] = value;
			}
			else if (num3 < 0)
			{
				if (RemoveParticles(-num3) == 0)
				{
					value = 0f;
				}
				constraintIndexAtNormalizedCoordinate = rope.GetConstraintIndexAtNormalizedCoordinate(normalizedCoord);
				obiDistanceConstraintBatch.restLengths[constraintIndexAtNormalizedCoordinate] = value;
			}
			obiDistanceConstraintBatch.PushDataToSolver(rope.DistanceConstraints);
			rope.RecalculateLenght();
		}
	}
}
