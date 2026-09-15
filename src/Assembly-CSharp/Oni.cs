using System;
using System.Runtime.InteropServices;
using UnityEngine;

public static class Oni
{
	public enum ConstraintType
	{
		Tether,
		Pin,
		Volume,
		Bending,
		Distance,
		ParticleCollision,
		Density,
		Collision,
		Skin,
		Aerodynamics,
		Stitch,
		ShapeMatching
	}

	public enum ParticlePhase
	{
		SelfCollide = 0x1000000,
		Fluid = 0x2000000
	}

	public enum ShapeType
	{
		Sphere,
		Box,
		Capsule,
		Heightmap,
		TriangleMesh,
		EdgeMesh
	}

	public enum MaterialCombineMode
	{
		Average,
		Minimium,
		Multiply,
		Maximum
	}

	public enum NormalsUpdate
	{
		Recalculate,
		Skin
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct ProfileInfo
	{
		public double start;

		public double end;

		public double childDuration;

		public int threadID;

		public int level;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
		public string name;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct GridCell
	{
		public Vector3 center;

		public Vector3 size;

		public int count;
	}

	[Serializable]
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct SolverParameters
	{
		public enum Interpolation
		{
			None,
			Interpolate
		}

		public enum Mode
		{
			Mode3D,
			Mode2D
		}

		[Tooltip("In 2D mode, particles are simulated on the XY plane only. For use in conjunction with Unity's 2D mode.")]
		public Mode mode;

		[Tooltip("Same as Rigidbody.interpolation. Set to INTERPOLATE for cloth that is applied on a main character or closely followed by a camera. NONE for everything else.")]
		public Interpolation interpolation;

		public Vector3 gravity;

		[Tooltip("Percentage of velocity lost per second, between 0% (0) and 100% (1).")]
		[Range(0f, 1f)]
		public float damping;

		[Tooltip("Intensity of laplacian smoothing applied to fluids. High values yield more uniform fluid particle distributions.")]
		[Range(0f, 1f)]
		public float fluidDenoising;

		[Tooltip("Radius of diffuse particle advection. Large values yield better quality but are more expensive.")]
		public float advectionRadius;

		[Tooltip("Kinetic energy below which particle positions arent updated. Energy values are mass-normalized, so all particles in the solver have the same threshold.")]
		public float sleepThreshold;

		public SolverParameters(Interpolation interpolation, Vector4 gravity)
		{
			mode = Mode.Mode3D;
			this.gravity = gravity;
			this.interpolation = interpolation;
			damping = 0f;
			fluidDenoising = 0f;
			advectionRadius = 0.5f;
			sleepThreshold = 0.001f;
		}
	}

	[Serializable]
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct ConstraintParameters
	{
		public enum EvaluationOrder
		{
			Sequential,
			Parallel
		}

		[MarshalAs(UnmanagedType.I1)]
		[Tooltip("Whether this constraint group is solved or not.")]
		public bool enabled;

		[Tooltip("Order in which constraints are evaluated. SEQUENTIAL converges faster but is not very stable. PARALLEL is very stable but converges slowly, requiring more iterations to achieve the same result.")]
		public EvaluationOrder evaluationOrder;

		[Tooltip("Number of relaxation iterations performed by the constraint solver. A low number of iterations will perform better, but be less accurate.")]
		public int iterations;

		[Tooltip("Over (or under if < 1) relaxation factor used. At 1, no overrelaxation is performed. At 2, constraints double their relaxation rate. High values reduce stability but improve convergence.")]
		[Range(0.1f, 2f)]
		public float SORFactor;

		public ConstraintParameters(bool enabled, EvaluationOrder order, int iterations)
		{
			this.enabled = enabled;
			this.iterations = iterations;
			evaluationOrder = order;
			SORFactor = 1f;
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 112)]
	public struct Contact
	{
		public Vector4 point;

		public Vector4 normal;

		public Vector4 tangent;

		public Vector4 bitangent;

		public float distance;

		public float normalImpulse;

		public float tangentImpulse;

		public float bitangentImpulse;

		public float stickImpulse;

		public int particle;

		public int other;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct BoneWeights
	{
		public int bone0;

		public int bone1;

		public int bone2;

		public int bone3;

		public float weight0;

		public float weight1;

		public float weight2;

		public float weight3;

		public BoneWeights(BoneWeight weight)
		{
			bone0 = weight.boneIndex0;
			bone1 = weight.boneIndex1;
			bone2 = weight.boneIndex2;
			bone3 = weight.boneIndex3;
			weight0 = weight.weight0;
			weight1 = weight.weight1;
			weight2 = weight.weight2;
			weight3 = weight.weight3;
		}
	}

	[Serializable]
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct Rigidbody
	{
		public Quaternion rotation;

		public Vector3 linearVelocity;

		public Vector3 angularVelocity;

		public Vector3 centerOfMass;

		public Vector3 inertiaTensor;

		public float inverseMass;

		public void Set(UnityEngine.Rigidbody source, bool kinematicForParticles)
		{
			bool flag = source.isKinematic || kinematicForParticles;
			rotation = source.rotation;
			linearVelocity = ((!kinematicForParticles) ? source.velocity : Vector3.zero);
			angularVelocity = ((!kinematicForParticles) ? source.angularVelocity : Vector3.zero);
			centerOfMass = source.transform.position + source.transform.rotation * source.centerOfMass;
			Vector3 vector = new Vector3(((source.constraints & RigidbodyConstraints.FreezeRotationX) == 0) ? (1f / source.inertiaTensor.x) : 0f, ((source.constraints & RigidbodyConstraints.FreezeRotationY) == 0) ? (1f / source.inertiaTensor.y) : 0f, ((source.constraints & RigidbodyConstraints.FreezeRotationZ) == 0) ? (1f / source.inertiaTensor.z) : 0f);
			inertiaTensor = ((!flag) ? (source.inertiaTensorRotation * vector) : Vector3.zero);
			inverseMass = ((!flag) ? (1f / source.mass) : 0f);
		}

		public void Set(Rigidbody2D source, bool kinematicForParticles)
		{
			rotation = Quaternion.AngleAxis(source.rotation, Vector3.forward);
			linearVelocity = source.velocity;
			angularVelocity = new Vector4(0f, 0f, source.angularVelocity * ((float)Math.PI / 180f), 0f);
			centerOfMass = source.transform.position + source.transform.rotation * source.centerOfMass;
			inertiaTensor = ((!source.isKinematic && !kinematicForParticles) ? new Vector3(0f, 0f, ((source.constraints & RigidbodyConstraints2D.FreezeRotation) == 0) ? (1f / source.inertia) : 0f) : Vector3.zero);
			inverseMass = ((!source.isKinematic && !kinematicForParticles) ? (1f / source.mass) : 0f);
		}
	}

	[Serializable]
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct RigidbodyVelocities
	{
		public Vector3 linearVelocity;

		public Vector3 angularVelocity;
	}

	[Serializable]
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct Collider
	{
		public Quaternion rotation;

		public Vector3 translation;

		public Vector3 scale;

		public Vector3 boundsMin;

		public Vector3 boundsMax;

		public int id;

		public float contactOffset;

		public int collisionGroup;

		[MarshalAs(UnmanagedType.I1)]
		public bool trigger;

		public void Set(UnityEngine.Collider source, int phase, float thickness)
		{
			boundsMin = source.bounds.min - Vector3.one * (thickness + source.contactOffset);
			boundsMax = source.bounds.max + Vector3.one * (thickness + source.contactOffset);
			translation = source.transform.position;
			rotation = source.transform.rotation;
			scale = new Vector4(source.transform.lossyScale.x, source.transform.lossyScale.y, source.transform.lossyScale.z, 1f);
			contactOffset = thickness;
			collisionGroup = phase;
			trigger = source.isTrigger;
			id = source.GetInstanceID();
		}

		public void Set(Collider2D source, int phase, float thickness)
		{
			boundsMin = source.bounds.min - Vector3.one * (thickness + 0.01f);
			boundsMax = source.bounds.max + Vector3.one * (thickness + 0.01f);
			translation = source.transform.position;
			rotation = source.transform.rotation;
			scale = new Vector4(source.transform.lossyScale.x, source.transform.lossyScale.y, source.transform.lossyScale.z, 1f);
			contactOffset = thickness;
			collisionGroup = phase;
			trigger = source.isTrigger;
			id = source.GetInstanceID();
		}

		public void SetSpaceTransform(Transform space)
		{
			Matrix4x4 worldToLocalMatrix = space.worldToLocalMatrix;
			Vector4 vector = worldToLocalMatrix.GetColumn(0) * boundsMin.x;
			Vector4 vector2 = worldToLocalMatrix.GetColumn(0) * boundsMax.x;
			Vector4 vector3 = worldToLocalMatrix.GetColumn(1) * boundsMin.y;
			Vector4 vector4 = worldToLocalMatrix.GetColumn(1) * boundsMax.y;
			Vector4 vector5 = worldToLocalMatrix.GetColumn(2) * boundsMin.z;
			Vector4 vector6 = worldToLocalMatrix.GetColumn(2) * boundsMax.z;
			Vector3 vector7 = worldToLocalMatrix.GetColumn(3);
			boundsMin = Vector3.Min(vector, vector2) + Vector3.Min(vector3, vector4) + Vector3.Min(vector5, vector6) + vector7;
			boundsMax = Vector3.Max(vector, vector2) + Vector3.Max(vector3, vector4) + Vector3.Max(vector5, vector6) + vector7;
			translation = space.worldToLocalMatrix.MultiplyPoint3x4(translation);
			rotation = Quaternion.Inverse(space.rotation) * rotation;
			scale.Scale(new Vector4(1f / space.lossyScale.x, 1f / space.lossyScale.y, 1f / space.lossyScale.z, 1f));
		}
	}

	[Serializable]
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct Shape
	{
		private Vector3 center;

		private Vector3 size;

		private IntPtr data;

		private IntPtr indices;

		private int dataCount;

		private int indexCount;

		private int resolutionU;

		private int resolutionV;

		[MarshalAs(UnmanagedType.I1)]
		public bool is2D;

		public void Set(Vector3 center, float radius)
		{
			this.center = center;
			size = Vector3.one * radius;
		}

		public void Set(Vector3 center, Vector3 size)
		{
			this.center = center;
			this.size = size;
		}

		public void Set(Vector3 center, float radius, float height, int direction)
		{
			this.center = center;
			size = new Vector3(radius, height, direction);
		}

		public void Set(Vector3 size, int resolutionU, int resolutionV, IntPtr data)
		{
			this.size = size;
			this.resolutionU = resolutionU;
			this.resolutionV = resolutionV;
			this.data = data;
			dataCount = resolutionU * resolutionV;
		}

		public void Set(IntPtr data, IntPtr indices, int dataCount, int indicesCount)
		{
			this.data = data;
			this.indices = indices;
			this.dataCount = dataCount;
			indexCount = indicesCount;
		}
	}

	[Serializable]
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct CollisionMaterial
	{
		public float friction;

		public float stickiness;

		public float stickDistance;

		public MaterialCombineMode frictionCombine;

		public MaterialCombineMode stickinessCombine;
	}

	[Serializable]
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct FluidMaterial
	{
		public float smoothingRadius;

		public float restDensity;

		public float viscosity;

		public float surfaceTension;

		public float buoyancy;

		public float atmosphericDrag;

		public float atmosphericPressure;

		public float vorticity;

		public float elasticRange;

		public float plasticCreep;

		public float plasticThreshold;
	}

	[Serializable]
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct HalfEdge
	{
		public int index;

		public int indexInFace;

		public int face;

		public int nextHalfEdge;

		public int pair;

		public int endVertex;

		public HalfEdge(int index)
		{
			this.index = index;
			indexInFace = -1;
			face = -1;
			nextHalfEdge = -1;
			pair = -1;
			endVertex = -1;
		}
	}

	[Serializable]
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct Vertex
	{
		public int index;

		public int halfEdge;

		public Vector3 position;

		public Vertex(Vector3 position, int index, int halfEdge)
		{
			this.index = index;
			this.halfEdge = halfEdge;
			this.position = position;
		}
	}

	[Serializable]
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct Face
	{
		public int index;

		public int halfEdge;

		public Face(int index)
		{
			this.index = index;
			halfEdge = -1;
		}
	}

	[Serializable]
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct MeshInformation
	{
		public float volume;

		public float area;

		public int borderEdgeCount;

		[MarshalAs(UnmanagedType.I1)]
		public bool closed;

		[MarshalAs(UnmanagedType.I1)]
		public bool nonManifold;
	}

	public static GCHandle PinMemory(object data)
	{
		return GCHandle.Alloc(data, GCHandleType.Pinned);
	}

	public static void UnpinMemory(GCHandle handle)
	{
		if (handle.IsAllocated)
		{
			handle.Free();
		}
	}

	[DllImport("libOni")]
	public static extern IntPtr CreateCollider();

	[DllImport("libOni")]
	public static extern void DestroyCollider(IntPtr collider);

	[DllImport("libOni")]
	public static extern IntPtr CreateShape(ShapeType shapeType);

	[DllImport("libOni")]
	public static extern void DestroyShape(IntPtr shape);

	[DllImport("libOni")]
	public static extern IntPtr CreateRigidbody();

	[DllImport("libOni")]
	public static extern void DestroyRigidbody(IntPtr rigidbody);

	[DllImport("libOni")]
	public static extern void UpdateCollider(IntPtr collider, ref Collider adaptor);

	[DllImport("libOni")]
	public static extern void UpdateShape(IntPtr shape, ref Shape adaptor);

	[DllImport("libOni")]
	public static extern void UpdateRigidbody(IntPtr rigidbody, ref Rigidbody adaptor);

	[DllImport("libOni")]
	public static extern void GetRigidbodyVelocity(IntPtr rigidbody, ref RigidbodyVelocities velocities);

	[DllImport("libOni")]
	public static extern void SetColliderShape(IntPtr collider, IntPtr shape);

	[DllImport("libOni")]
	public static extern void SetColliderRigidbody(IntPtr collider, IntPtr rigidbody);

	[DllImport("libOni")]
	public static extern void SetColliderMaterial(IntPtr collider, IntPtr material);

	[DllImport("libOni")]
	public static extern IntPtr CreateCollisionMaterial();

	[DllImport("libOni")]
	public static extern void DestroyCollisionMaterial(IntPtr material);

	[DllImport("libOni")]
	public static extern void UpdateCollisionMaterial(IntPtr material, ref CollisionMaterial adaptor);

	[DllImport("libOni")]
	public static extern IntPtr CreateSolver(int maxParticles, int maxNeighbours);

	[DllImport("libOni")]
	public static extern void DestroySolver(IntPtr solver);

	[DllImport("libOni")]
	public static extern void AddCollider(IntPtr solver, IntPtr collider);

	[DllImport("libOni")]
	public static extern void RemoveCollider(IntPtr solver, IntPtr collider);

	[DllImport("libOni")]
	public static extern void GetBounds(IntPtr solver, ref Vector3 min, ref Vector3 max);

	[DllImport("libOni")]
	public static extern int GetParticleGridSize(IntPtr solver);

	[DllImport("libOni")]
	public static extern void GetParticleGrid(IntPtr solver, GridCell[] cells);

	[DllImport("libOni")]
	public static extern void SetSolverParameters(IntPtr solver, ref SolverParameters parameters);

	[DllImport("libOni")]
	public static extern void GetSolverParameters(IntPtr solver, ref SolverParameters parameters);

	[DllImport("libOni")]
	public static extern int SetActiveParticles(IntPtr solver, int[] active, int num);

	[DllImport("libOni")]
	public static extern void AddSimulationTime(IntPtr solver, float step_dt);

	[DllImport("libOni")]
	public static extern void ResetSimulationTime(IntPtr solver);

	[DllImport("libOni")]
	public static extern void UpdateSolver(IntPtr solver, float substep_dt);

	[DllImport("libOni")]
	public static extern void ApplyPositionInterpolation(IntPtr solver, float substep_dt);

	[DllImport("libOni")]
	public static extern void UpdateSkeletalAnimation(IntPtr solver);

	[DllImport("libOni")]
	public static extern void GetConstraintsOrder(IntPtr solver, int[] order);

	[DllImport("libOni")]
	public static extern void SetConstraintsOrder(IntPtr solver, int[] order);

	[DllImport("libOni")]
	public static extern int GetConstraintCount(IntPtr solver, int type);

	[DllImport("libOni")]
	public static extern void GetActiveConstraintIndices(IntPtr solver, int[] indices, int num, int type);

	[DllImport("libOni")]
	public static extern int SetRenderableParticlePositions(IntPtr solver, Vector4[] positions, int num, int destOffset);

	[DllImport("libOni")]
	public static extern int GetRenderableParticlePositions(IntPtr solver, Vector4[] positions, int num, int sourceOffset);

	[DllImport("libOni")]
	public static extern int SetParticlePhases(IntPtr solver, int[] phases, int num, int destOffset);

	[DllImport("libOni")]
	public static extern int SetParticlePositions(IntPtr solver, Vector4[] positions, int num, int destOffset);

	[DllImport("libOni")]
	public static extern int GetParticlePositions(IntPtr solver, Vector4[] positions, int num, int sourceOffset);

	[DllImport("libOni")]
	public static extern int SetParticleInverseMasses(IntPtr solver, float[] invMasses, int num, int destOffset);

	[DllImport("libOni")]
	public static extern int SetParticleSolidRadii(IntPtr solver, float[] radii, int num, int destOffset);

	[DllImport("libOni")]
	public static extern int SetParticleVelocities(IntPtr solver, Vector4[] velocities, int num, int destOffset);

	[DllImport("libOni")]
	public static extern int GetParticleVelocities(IntPtr solver, Vector4[] velocities, int num, int sourceOffset);

	[DllImport("libOni")]
	public static extern void AddParticleExternalForces(IntPtr solver, Vector4[] forces, int[] indices, int num);

	[DllImport("libOni")]
	public static extern void AddParticleExternalForce(IntPtr solver, ref Vector4 force, int[] indices, int num);

	[DllImport("libOni")]
	public static extern int GetParticleVorticities(IntPtr solver, Vector4[] vorticities, int num, int sourceOffset);

	[DllImport("libOni")]
	public static extern int GetParticleDensities(IntPtr solver, float[] densities, int num, int sourceOffset);

	[DllImport("libOni")]
	public static extern int GetDeformableTriangleCount(IntPtr solver);

	[DllImport("libOni")]
	public static extern void SetDeformableTriangles(IntPtr solver, int[] indices, int num, int destOffset);

	[DllImport("libOni")]
	public static extern int RemoveDeformableTriangles(IntPtr solver, int num, int sourceOffset);

	[DllImport("libOni")]
	public static extern void SetConstraintGroupParameters(IntPtr solver, int type, ref ConstraintParameters parameters);

	[DllImport("libOni")]
	public static extern void GetConstraintGroupParameters(IntPtr solver, int type, ref ConstraintParameters parameters);

	[DllImport("libOni")]
	public static extern void SetCollisionMaterials(IntPtr solver, IntPtr[] materials, int[] indices, int num);

	[DllImport("libOni")]
	public static extern int SetRestPositions(IntPtr solver, Vector4[] positions, int num, int destOffset);

	[DllImport("libOni")]
	public static extern void SetFluidMaterials(IntPtr solver, FluidMaterial[] materials, int num, int destOffset);

	[DllImport("libOni")]
	public static extern int SetFluidMaterialIndices(IntPtr solver, int[] indices, int num, int destOffset);

	[DllImport("libOni")]
	public static extern IntPtr CreateDeformableMesh(IntPtr solver, IntPtr halfEdge, IntPtr skinConstraintBatch, [MarshalAs(UnmanagedType.LPArray, SizeConst = 16)] float[] worldToLocal, IntPtr particleIndices, int vertexCapacity, int vertexCount);

	[DllImport("libOni")]
	public static extern void DestroyDeformableMesh(IntPtr solver, IntPtr mesh);

	[DllImport("libOni")]
	public static extern bool TearDeformableMeshAtVertex(IntPtr mesh, int vertexIndex, ref Vector3 planePoint, ref Vector3 planeNormal, int[] updated_edges, ref int num_edges);

	[DllImport("libOni")]
	public static extern void SetDeformableMeshTBNUpdate(IntPtr mesh, NormalsUpdate normalsUpdate, [MarshalAs(UnmanagedType.I1)] bool skinTangents);

	[DllImport("libOni")]
	public static extern void SetDeformableMeshTransform(IntPtr mesh, [MarshalAs(UnmanagedType.LPArray, SizeConst = 16)] float[] worldToLocal);

	[DllImport("libOni")]
	public static extern void SetDeformableMeshSkinMap(IntPtr mesh, IntPtr sourceMesh, IntPtr triangleSkinMap);

	[DllImport("libOni")]
	public static extern void SetDeformableMeshParticleIndices(IntPtr mesh, IntPtr particleIndices);

	[DllImport("libOni")]
	public static extern void SetDeformableMeshData(IntPtr mesh, IntPtr triangles, IntPtr vertices, IntPtr normals, IntPtr tangents, IntPtr colors, IntPtr uv1, IntPtr uv2, IntPtr uv3, IntPtr uv4);

	[DllImport("libOni")]
	public static extern void SetDeformableMeshAnimationData(IntPtr mesh, float[] bindPoses, BoneWeights[] weights, int numBones);

	[DllImport("libOni")]
	public static extern void SetDeformableMeshBoneTransforms(IntPtr mesh, float[] boneTransforms);

	[DllImport("libOni")]
	public static extern void ForceDeformableMeshSkeletalSkinning(IntPtr mesh);

	[DllImport("libOni")]
	public static extern IntPtr CreateBatch(int type, [MarshalAs(UnmanagedType.I1)] bool cooked);

	[DllImport("libOni")]
	public static extern void DestroyBatch(IntPtr batch);

	[DllImport("libOni")]
	public static extern IntPtr AddBatch(IntPtr solver, IntPtr batch, [MarshalAs(UnmanagedType.I1)] bool sharesParticles);

	[DllImport("libOni")]
	public static extern void RemoveBatch(IntPtr solver, IntPtr batch);

	[DllImport("libOni")]
	public static extern bool EnableBatch(IntPtr batch, [MarshalAs(UnmanagedType.I1)] bool enabled);

	[DllImport("libOni")]
	public static extern int GetBatchConstraintCount(IntPtr batch);

	[DllImport("libOni")]
	public static extern int GetBatchConstraintForces(IntPtr batch, float[] forces, int num, int destOffset);

	[DllImport("libOni")]
	public static extern int GetBatchPhaseCount(IntPtr batch);

	[DllImport("libOni")]
	public static extern void GetBatchPhaseSizes(IntPtr batch, int[] phaseSizes);

	[DllImport("libOni")]
	public static extern void SetBatchPhaseSizes(IntPtr batch, int[] phaseSizes, int num);

	[DllImport("libOni")]
	public static extern bool CookBatch(IntPtr batch);

	[DllImport("libOni")]
	public static extern int SetActiveConstraints(IntPtr batch, int[] active, int num);

	[DllImport("libOni")]
	public static extern void SetDistanceConstraints(IntPtr batch, int[] indices, float[] restLengths, Vector2[] stiffnesses, int num);

	[DllImport("libOni")]
	public static extern void GetDistanceConstraints(IntPtr batch, int[] indices, float[] restLengths, Vector2[] stiffnesses);

	[DllImport("libOni")]
	public static extern void SetBendingConstraints(IntPtr batch, int[] indices, float[] restBends, Vector2[] bendingStiffnesses, int num);

	[DllImport("libOni")]
	public static extern void GetBendingConstraints(IntPtr batch, int[] indices, float[] restBends, Vector2[] bendingStiffnesses);

	[DllImport("libOni")]
	public static extern void SetSkinConstraints(IntPtr batch, int[] indices, Vector4[] points, Vector4[] normals, float[] radiiBackstops, float[] stiffnesses, int num);

	[DllImport("libOni")]
	public static extern void GetSkinConstraints(IntPtr batch, int[] indices, Vector4[] points, Vector4[] normals, float[] radiiBackstops, float[] stiffnesses);

	[DllImport("libOni")]
	public static extern void SetAerodynamicConstraints(IntPtr batch, int[] particleIndices, float[] aerodynamicCoeffs, int num);

	[DllImport("libOni")]
	public static extern void SetVolumeConstraints(IntPtr batch, int[] triangleIndices, int[] firstTriangle, int[] numTriangles, float[] restVolumes, Vector2[] pressureStiffnesses, int num);

	[DllImport("libOni")]
	public static extern void SetTetherConstraints(IntPtr batch, int[] indices, Vector2[] maxLenghtsScales, float[] stiffnesses, int num);

	[DllImport("libOni")]
	public static extern void GetTetherConstraints(IntPtr batch, int[] indices, Vector2[] maxLenghtsScales, float[] stiffnesses);

	[DllImport("libOni")]
	public static extern void SetPinConstraints(IntPtr batch, int[] indices, Vector4[] pinOffsets, IntPtr[] colliders, float[] stiffnesses, int num);

	[DllImport("libOni")]
	public static extern void SetStitchConstraints(IntPtr batch, int[] indices, float[] stiffnesses, int num);

	[DllImport("libOni")]
	public static extern void GetCollisionContacts(IntPtr solver, Contact[] contacts, int n);

	[DllImport("libOni")]
	public static extern void ClearDiffuseParticles(IntPtr solver);

	[DllImport("libOni")]
	public static extern int SetDiffuseParticles(IntPtr solver, Vector4[] positions, int num);

	[DllImport("libOni")]
	public static extern int GetDiffuseParticleVelocities(IntPtr solver, Vector4[] velocities, int num, int sourceOffset);

	[DllImport("libOni")]
	public static extern void SetDiffuseParticleNeighbourCounts(IntPtr solver, IntPtr neighbourCounts);

	[DllImport("libOni")]
	public static extern IntPtr CreateHalfEdgeMesh();

	[DllImport("libOni")]
	public static extern void DestroyHalfEdgeMesh(IntPtr mesh);

	[DllImport("libOni")]
	public static extern void SetVertices(IntPtr mesh, IntPtr vertices, int n);

	[DllImport("libOni")]
	public static extern void SetHalfEdges(IntPtr mesh, IntPtr halfedges, int n);

	[DllImport("libOni")]
	public static extern void SetFaces(IntPtr mesh, IntPtr faces, int n);

	[DllImport("libOni")]
	public static extern void SetNormals(IntPtr mesh, IntPtr normals);

	[DllImport("libOni")]
	public static extern void SetTangents(IntPtr mesh, IntPtr tangents);

	[DllImport("libOni")]
	public static extern void SetInverseOrientations(IntPtr mesh, IntPtr orientations);

	[DllImport("libOni")]
	public static extern void SetVisualMap(IntPtr mesh, IntPtr map);

	[DllImport("libOni")]
	public static extern int GetVertexCount(IntPtr mesh);

	[DllImport("libOni")]
	public static extern int GetHalfEdgeCount(IntPtr mesh);

	[DllImport("libOni")]
	public static extern int GetFaceCount(IntPtr mesh);

	[DllImport("libOni")]
	public static extern int GetHalfEdgeMeshInfo(IntPtr mesh, ref MeshInformation meshInfo);

	[DllImport("libOni")]
	public static extern void CalculatePrimitiveCounts(IntPtr mesh, Vector3[] vertices, int[] triangles, int vertexCount, int triangleCount);

	[DllImport("libOni")]
	public static extern void Generate(IntPtr mesh, Vector3[] vertices, int[] triangles, int vertexCount, int triangleCount, ref Vector3 scale);

	[DllImport("libOni")]
	public static extern int MakePhase(int group, ParticlePhase flags);

	[DllImport("libOni")]
	public static extern int GetGroupFromPhase(int phase);

	[DllImport("libOni")]
	public static extern float BendingConstraintRest(float[] constraintCoordinates);

	[DllImport("libOni")]
	public static extern IntPtr CreateTriangleSkinMap();

	[DllImport("libOni")]
	public static extern void DestroyTriangleSkinMap(IntPtr skinmap);

	[DllImport("libOni")]
	public static extern void Bind(IntPtr skinmap, IntPtr sourcemesh, IntPtr targetmesh, uint[] sourceMasterFlags, uint[] targetSlaveFlags);

	[DllImport("libOni")]
	public static extern int GetSkinnedVertexCount(IntPtr skinmap);

	[DllImport("libOni")]
	public static extern void GetSkinInfo(IntPtr skinmap, int[] skinIndices, int[] sourceTriIndices, Vector3[] baryPositions, Vector3[] baryNormals, Vector3[] baryTangents);

	[DllImport("libOni")]
	public static extern void SetSkinInfo(IntPtr skinmap, int[] skinIndices, int[] sourceTriIndices, Vector3[] baryPositions, Vector3[] baryNormals, Vector3[] baryTangents, int num);

	[DllImport("libOni")]
	public static extern void WaitForAllTasks();

	[DllImport("libOni")]
	public static extern void ClearTasks();

	[DllImport("libOni")]
	public static extern int GetMaxSystemConcurrency();

	[DllImport("libOni")]
	public static extern void SignalFrameStart();

	[DllImport("libOni")]
	public static extern double SignalFrameEnd();

	[DllImport("libOni")]
	public static extern void EnableProfiler([MarshalAs(UnmanagedType.I1)] bool cooked);

	[DllImport("libOni")]
	public static extern int GetProfilingInfoCount();

	[DllImport("libOni")]
	public static extern void GetProfilingInfo([Out] ProfileInfo[] info, int num);
}
