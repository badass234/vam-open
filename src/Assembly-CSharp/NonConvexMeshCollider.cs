using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class NonConvexMeshCollider : MonoBehaviour
{
	public class BoundingBox
	{
		public Interval IntervalX { get; private set; }

		public Interval IntervalY { get; private set; }

		public Interval IntervalZ { get; private set; }

		public BoundingBox(params Interval[] intervalsXyz)
			: this(intervalsXyz[0], intervalsXyz[1], intervalsXyz[2])
		{
		}

		public BoundingBox(Interval intervalX, Interval intervalY, Interval intervalZ)
		{
			IntervalX = intervalX;
			IntervalY = intervalY;
			IntervalZ = intervalZ;
		}

		public bool IntersectsRayToPositiveX(Vector3 origin)
		{
			float val = IntervalX.Min - origin.x;
			float val2 = IntervalX.Max - origin.x;
			float val3 = Math.Min(val, val2);
			float val4 = Math.Max(val, val2);
			val3 = Math.Max(val3, 0f);
			val4 = Math.Min(val4, 0f);
			return val4 >= val3;
		}

		public bool Intersects(Ray r)
		{
			float num = 1f / r.direction.x;
			float num2 = 1f / r.direction.y;
			float num3 = 1f / r.direction.z;
			float val = (IntervalX.Min - r.origin.x) * num;
			float val2 = (IntervalX.Max - r.origin.x) * num;
			float val3 = (IntervalY.Min - r.origin.y) * num2;
			float val4 = (IntervalY.Max - r.origin.y) * num2;
			float val5 = (IntervalZ.Min - r.origin.z) * num3;
			float val6 = (IntervalZ.Max - r.origin.z) * num3;
			float num4 = Math.Max(Math.Max(Math.Min(val, val2), Math.Min(val3, val4)), Math.Min(val5, val6));
			float num5 = Math.Min(Math.Min(Math.Max(val, val2), Math.Max(val3, val4)), Math.Max(val5, val6));
			if (num5 < 0f)
			{
				return false;
			}
			if (num4 > num5)
			{
				return false;
			}
			return true;
		}

		public bool Intersects(BoundingBox other)
		{
			return IntervalX.Intersects(other.IntervalX) && IntervalY.Intersects(other.IntervalY) && IntervalZ.Intersects(other.IntervalZ);
		}

		public override string ToString()
		{
			return $"X: {IntervalX.Min:N4}-{IntervalX.Max:N4}, Y: {IntervalY.Min:N4}-{IntervalY.Max:N4}, Z: {IntervalZ.Min:N4}-{IntervalZ.Max:N4}";
		}
	}

	public class Tri
	{
		private BoundingBox bounds;

		public Vector3 A { get; set; }

		public Vector3 B { get; set; }

		public Vector3 C { get; set; }

		public BoundingBox Bounds
		{
			get
			{
				if (bounds == null)
				{
					BoundingBox boundingBox = new BoundingBox(Interval.From(A.x, B.x, C.x), Interval.From(A.y, B.y, C.y), Interval.From(A.z, B.z, C.z));
					bounds = boundingBox;
				}
				return bounds;
			}
		}

		public Tri(Vector3 a, Vector3 b, Vector3 c)
		{
			A = a;
			B = b;
			C = c;
		}

		public bool Intersect(Ray ray)
		{
			Vector3 vector = B - A;
			Vector3 vector2 = C - A;
			Vector3 rhs = Vector3.Cross(ray.direction, vector2);
			float num = Vector3.Dot(vector, rhs);
			if (num > -1E-06f && num < 1E-06f)
			{
				return false;
			}
			float num2 = 1f / num;
			Vector3 lhs = ray.origin - A;
			float num3 = Vector3.Dot(lhs, rhs) * num2;
			if (num3 < 0f || num3 > 1f)
			{
				return false;
			}
			Vector3 rhs2 = Vector3.Cross(lhs, vector);
			float num4 = Vector3.Dot(ray.direction, rhs2) * num2;
			if (num4 < 0f || num3 + num4 > 1f)
			{
				return false;
			}
			if (Vector3.Dot(vector2, rhs2) * num2 > 1E-06f)
			{
				return true;
			}
			return false;
		}

		public static bool Intersect(Vector3 p1, Vector3 p2, Vector3 p3, Ray ray)
		{
			Vector3 vector = p2 - p1;
			Vector3 vector2 = p3 - p1;
			Vector3 rhs = Vector3.Cross(ray.direction, vector2);
			float num = Vector3.Dot(vector, rhs);
			if (num > -1E-06f && num < 1E-06f)
			{
				return false;
			}
			float num2 = 1f / num;
			Vector3 lhs = ray.origin - p1;
			float num3 = Vector3.Dot(lhs, rhs) * num2;
			if (num3 < 0f || num3 > 1f)
			{
				return false;
			}
			Vector3 rhs2 = Vector3.Cross(lhs, vector);
			float num4 = Vector3.Dot(ray.direction, rhs2) * num2;
			if (num4 < 0f || num3 + num4 > 1f)
			{
				return false;
			}
			if (Vector3.Dot(vector2, rhs2) * num2 > 1E-06f)
			{
				return true;
			}
			return false;
		}
	}

	public class SpatialBinaryTree
	{
		private readonly SpatialBinaryTreeNode root;

		public SpatialBinaryTree(Mesh m, int maxLevels)
		{
			BoundingBox bounds = new BoundingBox(new Interval(m.vertices.Min((Vector3 v) => v.x), m.vertices.Max((Vector3 v) => v.x)), new Interval(m.vertices.Min((Vector3 v) => v.y), m.vertices.Max((Vector3 v) => v.y)), new Interval(m.vertices.Min((Vector3 v) => v.z), m.vertices.Max((Vector3 v) => v.z)));
			root = new SpatialBinaryTreeNode(0, maxLevels, bounds);
			int num = m.triangles.Length / 3;
			for (int i = 0; i < num; i++)
			{
				Vector3 a = m.vertices[m.triangles[i * 3]];
				Vector3 b = m.vertices[m.triangles[i * 3 + 1]];
				Vector3 c = m.vertices[m.triangles[i * 3 + 2]];
				Tri t = new Tri(a, b, c);
				Add(t);
			}
		}

		public void Add(Tri t)
		{
			root.Add(t);
		}

		public IEnumerable<Tri> GetTris(Ray r)
		{
			return new HashSet<Tri>(root.GetTris(r));
		}
	}

	public class SpatialBinaryTreeNode
	{
		private readonly int level;

		private readonly int maxLevels;

		private SpatialBinaryTreeNode childA;

		private SpatialBinaryTreeNode childB;

		private readonly List<Tri> tris;

		private readonly BoundingBox bounds;

		private readonly BoundingBox boundsChildA;

		private readonly BoundingBox boundsChildB;

		public SpatialBinaryTreeNode(int level, int maxLevels, BoundingBox bounds)
		{
			this.level = level;
			this.maxLevels = maxLevels;
			this.bounds = bounds;
			if (level >= maxLevels)
			{
				tris = new List<Tri>();
				return;
			}
			int num = level % 3;
			boundsChildA = new BoundingBox((num != 0) ? bounds.IntervalX : bounds.IntervalX.LowerHalf, (num != 1) ? bounds.IntervalY : bounds.IntervalY.LowerHalf, (num != 2) ? bounds.IntervalZ : bounds.IntervalZ.LowerHalf);
			boundsChildB = new BoundingBox((num != 0) ? bounds.IntervalX : bounds.IntervalX.UpperHalf, (num != 1) ? bounds.IntervalY : bounds.IntervalY.UpperHalf, (num != 2) ? bounds.IntervalZ : bounds.IntervalZ.UpperHalf);
		}

		public void Add(Tri t)
		{
			if (tris != null)
			{
				tris.Add(t);
				return;
			}
			if (boundsChildA.Intersects(t.Bounds))
			{
				if (childA == null)
				{
					childA = new SpatialBinaryTreeNode(level + 1, maxLevels, boundsChildA);
				}
				childA.Add(t);
			}
			if (boundsChildB.Intersects(t.Bounds))
			{
				if (childB == null)
				{
					childB = new SpatialBinaryTreeNode(level + 1, maxLevels, boundsChildB);
				}
				childB.Add(t);
			}
		}

		public IEnumerable<Tri> GetTris(Ray r)
		{
			if (!bounds.Intersects(r))
			{
				yield break;
			}
			if (tris != null)
			{
				foreach (Tri tri in tris)
				{
					yield return tri;
				}
				yield break;
			}
			if (childA != null)
			{
				foreach (Tri tri2 in childA.GetTris(r))
				{
					yield return tri2;
				}
			}
			if (childB == null)
			{
				yield break;
			}
			foreach (Tri tri3 in childB.GetTris(r))
			{
				yield return tri3;
			}
		}

		public override string ToString()
		{
			if (tris != null)
			{
				return "Leaf node: " + tris.Count + " tris";
			}
			return bounds.ToString();
		}
	}

	public class Interval
	{
		public float Min { get; set; }

		public float Max { get; set; }

		public float Center { get; private set; }

		public float Size => Max - Min;

		public Interval LowerHalf => new Interval(Min, Center);

		public Interval UpperHalf => new Interval(Center, Max);

		public Interval(float min, float max)
		{
			Min = min;
			Max = max;
			Center = (min + max) / 2f;
		}

		public bool Contains(float v)
		{
			if (v < Min)
			{
				return false;
			}
			if (v >= Max)
			{
				return false;
			}
			return true;
		}

		public bool IsInLeftHalf(float v)
		{
			return v >= Min && v < Center;
		}

		public bool IsInRightHalf(float v)
		{
			return v > Center && v < Max;
		}

		public bool Intersects(Interval other)
		{
			return Min <= other.Max && other.Min <= Max;
		}

		public static Interval From(float a, float b, float c)
		{
			return new Interval(Math.Min(Math.Min(a, b), c), Math.Max(Math.Max(a, b), c));
		}

		public static Interval From(float a, float b)
		{
			return new Interval(Math.Min(a, b), Math.Max(a, b));
		}
	}

	public class Box
	{
		private readonly Box[,,] boxes;

		private readonly Vector3Int lastLevelGridPos;

		private Box[] lastLevelBoxes;

		private Vector3Int minGridPos;

		private Vector3Int maxGridPos;

		private Vector3Int gridSize;

		private Vector3? center;

		private Vector3? size;

		public Vector3 Center
		{
			get
			{
				Vector3? vector = center;
				if (!vector.HasValue)
				{
					if (Children == null)
					{
						throw new Exception("Last level child box needs a center position");
					}
					Vector3 zero = Vector3.zero;
					Box[] array = LastLevelBoxes;
					foreach (Box box in array)
					{
						zero += box.Center;
					}
					zero /= (float)LastLevelBoxes.Length;
					center = zero;
				}
				return center.Value;
			}
		}

		public Vector3 Size
		{
			get
			{
				Vector3? vector = size;
				if (!vector.HasValue)
				{
					if (Children == null)
					{
						throw new Exception("Last level child box needs a size");
					}
					Vector3 vector2 = LastLevelBoxes[0].Size;
					size = new Vector3((float)GridSize.X * vector2.x, (float)GridSize.Y * vector2.y, (float)GridSize.Z * vector2.z);
				}
				return size.Value;
			}
		}

		public Box Parent { get; set; }

		public Box[] Children { get; set; }

		public IEnumerable<Box> Parents
		{
			get
			{
				Box b = this;
				while (b.Parent != null)
				{
					yield return b.Parent;
					b = b.Parent;
				}
			}
		}

		public IEnumerable<Box> SelfAndParents
		{
			get
			{
				yield return this;
				foreach (Box parent in Parents)
				{
					yield return parent;
				}
			}
		}

		public Box Root => (Parent != null) ? Parent.Root : this;

		public IEnumerable<Box> ChildrenRecursive
		{
			get
			{
				if (Children == null)
				{
					yield break;
				}
				Box[] children = Children;
				foreach (Box c in children)
				{
					yield return c;
					foreach (Box item in c.ChildrenRecursive)
					{
						yield return item;
					}
				}
			}
		}

		public IEnumerable<Box> SelfAndChildrenRecursive
		{
			get
			{
				yield return this;
				foreach (Box item in ChildrenRecursive)
				{
					yield return item;
				}
			}
		}

		public Box[] LastLevelBoxes
		{
			get
			{
				if (lastLevelBoxes == null)
				{
					lastLevelBoxes = SelfAndChildrenRecursive.Where((Box c) => c.Children == null).ToArray();
				}
				return lastLevelBoxes;
			}
		}

		private IEnumerable<Vector3Int> CoveredGridPositions => LastLevelBoxes.Select((Box c) => c.lastLevelGridPos);

		private int MinGridPosX => (Children != null) ? CoveredGridPositions.Min((Vector3Int p) => p.X) : lastLevelGridPos.X;

		private int MinGridPosY => (Children != null) ? CoveredGridPositions.Min((Vector3Int p) => p.Y) : lastLevelGridPos.Y;

		private int MinGridPosZ => (Children != null) ? CoveredGridPositions.Min((Vector3Int p) => p.Z) : lastLevelGridPos.Z;

		private int MaxGridPosX => (Children != null) ? CoveredGridPositions.Max((Vector3Int p) => p.X) : lastLevelGridPos.X;

		private int MaxGridPosY => (Children != null) ? CoveredGridPositions.Max((Vector3Int p) => p.Y) : lastLevelGridPos.Y;

		private int MaxGridPosZ => (Children != null) ? CoveredGridPositions.Max((Vector3Int p) => p.Z) : lastLevelGridPos.Z;

		private Vector3Int MinGridPos => minGridPos ?? (minGridPos = new Vector3Int(MinGridPosX, MinGridPosY, MinGridPosZ));

		private Vector3Int MaxGridPos => maxGridPos ?? (maxGridPos = new Vector3Int(MaxGridPosX, MaxGridPosY, MaxGridPosZ));

		private Vector3Int GridSize
		{
			get
			{
				if (gridSize == null)
				{
					gridSize = ((Children != null) ? new Vector3Int(MaxGridPos.X - MinGridPos.X + 1, MaxGridPos.Y - MinGridPos.Y + 1, MaxGridPos.Z - MinGridPos.Z + 1) : Vector3Int.One);
				}
				return gridSize;
			}
		}

		public Box(Box[,,] boxes, Vector3? center = null, Vector3? size = null, Vector3Int lastLevelGridPos = null)
		{
			this.boxes = boxes;
			this.lastLevelGridPos = lastLevelGridPos;
			this.center = center;
			this.size = size;
		}

		private void MergeWith(Box other)
		{
			Box box = new Box(boxes, null, null);
			Box[] array = new Box[2] { this, other };
			foreach (Box box2 in array)
			{
				box2.Parent = box;
			}
			box.Children = new Box[2] { this, other };
		}

		public bool TryMerge(Vector3Int direction)
		{
			if (Parent != null)
			{
				return false;
			}
			foreach (Vector3Int coveredGridPosition in CoveredGridPositions)
			{
				Vector3Int vector3Int = new Vector3Int(coveredGridPosition.X + direction.X, coveredGridPosition.Y + direction.Y, coveredGridPosition.Z + direction.Z);
				if (vector3Int.X < 0 || vector3Int.Y < 0 || vector3Int.Z < 0 || vector3Int.X >= boxes.GetLength(0) || vector3Int.Y >= boxes.GetLength(1) || vector3Int.Z >= boxes.GetLength(2))
				{
					continue;
				}
				Box box = boxes[vector3Int.X, vector3Int.Y, vector3Int.Z];
				if (box != null)
				{
					box = box.Root;
					if (box != this && (direction.X != 0 || box.GridSize.X == GridSize.X) && (direction.Y != 0 || box.GridSize.Y == GridSize.Y) && (direction.Z != 0 || box.GridSize.Z == GridSize.Z) && (direction.X != 0 || MinGridPos.X == box.MinGridPos.X) && (direction.Y != 0 || MinGridPos.Y == box.MinGridPos.Y) && (direction.Z != 0 || MinGridPos.Z == box.MinGridPos.Z))
					{
						MergeWith(box);
						return true;
					}
				}
			}
			return false;
		}
	}

	public class Vector3Int
	{
		public static readonly Vector3Int One = new Vector3Int(1, 1, 1);

		public int X { get; set; }

		public int Y { get; set; }

		public int Z { get; set; }

		public Vector3Int(int x, int y, int z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		protected bool Equals(Vector3Int other)
		{
			return X == other.X && Y == other.Y && Z == other.Z;
		}

		public override bool Equals(object obj)
		{
			if (object.ReferenceEquals(null, obj))
			{
				return false;
			}
			if (object.ReferenceEquals(this, obj))
			{
				return true;
			}
			if (obj.GetType() != GetType())
			{
				return false;
			}
			return Equals((Vector3Int)obj);
		}

		public override int GetHashCode()
		{
			int x = X;
			x = (x * 397) ^ Y;
			return (x * 397) ^ Z;
		}

		public override string ToString()
		{
			return $"X: {X}, Y: {Y}, Z: {Z}";
		}
	}

	private bool mergeBoxesToReduceNumber = true;

	private int spatialTreeLevelDepth = 9;

	public bool outputTimeMeasurements;

	[Tooltip("Will create a child game object called 'Colliders' to store the generated colliders in. \n\rThis leads to a cleaner and more organized structure. \n\rPlease note that collisions will then report the child game object. So you may want to check for transform.parent.gameObject on your collision check.")]
	public bool createChildGameObject = true;

	[Tooltip("Takes a bit more time to compute, but leads to more performance optimized colliders (less boxes).")]
	public bool avoidGapsInside;

	[Tooltip("Makes sure all box colliders are generated completely on the inside of the mesh. More expensive to compute, but desireable if you need to avoid false collisions of objects very close to another, like rings of a chain for example.")]
	public bool avoidExceedingMesh;

	[Tooltip("The number of boxes your mesh will be segmented into, on each axis (x, y and z). \n\rHigher values lead to more accurate colliders but on the other hand makes computation and collision checks more expensive.")]
	public int boxesPerEdge = 20;

	[Tooltip("The physics material to apply to the generated compound colliders.")]
	public PhysicMaterial physicsMaterialForColliders;

	public const bool DebugOutput = false;

	public void Calculate()
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		if (boxesPerEdge > 100)
		{
			boxesPerEdge = 100;
		}
		if (avoidExceedingMesh && boxesPerEdge > 50)
		{
			boxesPerEdge = 50;
		}
		if (boxesPerEdge < 1)
		{
			boxesPerEdge = 3;
		}
		GameObject gameObject = base.gameObject;
		MeshFilter component = gameObject.GetComponent<MeshFilter>();
		if (component == null || component.sharedMesh == null)
		{
			return;
		}
		Rigidbody component2 = gameObject.GetComponent<Rigidbody>();
		bool flag = false;
		if (component2 != null && !component2.isKinematic)
		{
			flag = true;
			component2.isKinematic = true;
		}
		bool flag2 = false;
		if (component2 != null && component2.useGravity)
		{
			flag2 = true;
			component2.useGravity = false;
		}
		if (!createChildGameObject)
		{
			BoxCollider[] components = gameObject.GetComponents<BoxCollider>();
			foreach (BoxCollider obj in components)
			{
				UnityEngine.Object.DestroyImmediate(obj);
			}
		}
		int layer = gameObject.layer;
		int firstEmptyLayer = GetFirstEmptyLayer();
		gameObject.layer = firstEmptyLayer;
		Transform parent = gameObject.transform.parent;
		Vector3 localPosition = gameObject.transform.localPosition;
		Quaternion localRotation = gameObject.transform.localRotation;
		Vector3 localScale = gameObject.transform.localScale;
		GameObject gameObject2 = new GameObject("Temp_CompoundColliderParent");
		gameObject.transform.parent = gameObject2.transform;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localRotation = Quaternion.Euler(Vector3.zero);
		gameObject.transform.localScale = Vector3.one;
		try
		{
			GameObject gameObject3 = CreateColliderChildGameObject(gameObject, component);
			Box[] array = CreateMeshIntersectingBoxes(gameObject3).ToArray();
			Box[] array2 = ((!mergeBoxesToReduceNumber) ? array : MergeBoxes(array.ToArray()));
			Box[] array3 = array2;
			foreach (Box box in array3)
			{
				BoxCollider boxCollider = ((!createChildGameObject) ? gameObject : gameObject3).AddComponent<BoxCollider>();
				boxCollider.size = box.Size;
				boxCollider.center = box.Center;
				if (physicsMaterialForColliders != null)
				{
					boxCollider.material = physicsMaterialForColliders;
				}
			}
			UnityEngine.Debug.Log("NonConvexMeshCollider: " + array2.Length + " box colliders created");
			UnityEngine.Object.DestroyImmediate(gameObject3.GetComponent<MeshFilter>());
			UnityEngine.Object.DestroyImmediate(gameObject3.GetComponent<MeshCollider>());
			UnityEngine.Object.DestroyImmediate(gameObject3.GetComponent<Rigidbody>());
			if (!createChildGameObject)
			{
				UnityEngine.Object.DestroyImmediate(gameObject3);
			}
			else if ((bool)gameObject3)
			{
				gameObject3.layer = layer;
			}
		}
		finally
		{
			gameObject.transform.parent = parent;
			gameObject.transform.localPosition = localPosition;
			gameObject.transform.localRotation = localRotation;
			gameObject.transform.localScale = localScale;
			gameObject.layer = layer;
			if (flag)
			{
				component2.isKinematic = false;
			}
			if (flag2)
			{
				component2.useGravity = true;
			}
			UnityEngine.Object.DestroyImmediate(gameObject2);
		}
		stopwatch.Stop();
		if (outputTimeMeasurements)
		{
			UnityEngine.Debug.Log("Total duration: " + stopwatch.Elapsed);
		}
	}

	private Box[] MergeBoxes(Box[] boxes)
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		Vector3Int[] array = new Vector3Int[6]
		{
			new Vector3Int(1, 0, 0),
			new Vector3Int(0, 1, 0),
			new Vector3Int(0, 0, 1),
			new Vector3Int(-1, 0, 0),
			new Vector3Int(0, -1, 0),
			new Vector3Int(0, 0, -1)
		};
		bool flag = false;
		do
		{
			Vector3Int[] array2 = array;
			foreach (Vector3Int direction in array2)
			{
				flag = false;
				Box[] array3 = boxes;
				foreach (Box box in array3)
				{
					if (box.TryMerge(direction))
					{
						flag = true;
					}
				}
				boxes = boxes.Select((Box b) => b.Root).Distinct().ToArray();
			}
		}
		while (flag);
		Box[] result = boxes.Select((Box b) => b.Root).Distinct().ToArray();
		stopwatch.Stop();
		if (outputTimeMeasurements)
		{
			UnityEngine.Debug.Log("Merged in " + stopwatch.Elapsed);
		}
		return result;
	}

	private static GameObject CreateColliderChildGameObject(GameObject go, MeshFilter meshFilter)
	{
		Transform transform = go.transform.Find("Colliders");
		GameObject gameObject;
		if (transform != null)
		{
			gameObject = transform.gameObject;
		}
		else
		{
			gameObject = new GameObject("Colliders");
			gameObject.transform.parent = go.transform;
			gameObject.transform.localRotation = Quaternion.Euler(Vector3.zero);
			gameObject.transform.localPosition = Vector3.zero;
		}
		gameObject.layer = go.layer;
		BoxCollider[] components = gameObject.GetComponents<BoxCollider>();
		foreach (BoxCollider obj in components)
		{
			UnityEngine.Object.DestroyImmediate(obj);
		}
		MeshFilter component = gameObject.GetComponent<MeshFilter>();
		if (component != null)
		{
			UnityEngine.Object.DestroyImmediate(component);
		}
		MeshCollider component2 = gameObject.GetComponent<MeshCollider>();
		if (component2 != null)
		{
			UnityEngine.Object.DestroyImmediate(component2);
		}
		Rigidbody component3 = gameObject.GetComponent<Rigidbody>();
		if (component3 != null)
		{
			UnityEngine.Object.DestroyImmediate(component3);
		}
		component3 = gameObject.AddComponent<Rigidbody>();
		component3.isKinematic = true;
		component3.useGravity = false;
		component = gameObject.AddComponent<MeshFilter>();
		component.sharedMesh = meshFilter.sharedMesh;
		component2 = gameObject.AddComponent<MeshCollider>();
		component2.convex = false;
		return gameObject;
	}

	private IEnumerable<Box> CreateMeshIntersectingBoxes(GameObject colliderGo)
	{
		GameObject go = colliderGo.transform.parent.gameObject;
		int colliderLayer = colliderGo.layer;
		LayerMask colliderLayerMask = 1 << colliderLayer;
		Bounds bounds = CalculateLocalBounds(go);
		Mesh mesh = colliderGo.GetComponent<MeshFilter>().sharedMesh;
		Stopwatch swTree = Stopwatch.StartNew();
		SpatialBinaryTree tree = new SpatialBinaryTree(mesh, spatialTreeLevelDepth);
		swTree.Stop();
		if (outputTimeMeasurements)
		{
			UnityEngine.Debug.Log("SpatialTree Built in " + swTree.Elapsed);
		}
		Box[,,] boxes = new Box[boxesPerEdge, boxesPerEdge, boxesPerEdge];
		bool[,,] boxColliderPositions = new bool[boxesPerEdge, boxesPerEdge, boxesPerEdge];
		Vector3 s = bounds.size / boxesPerEdge;
		Vector3 halfExtent = s / 2f;
		Vector3[] directionsFromBoxCenterToCorners = new Vector3[8]
		{
			new Vector3(1f, 1f, 1f),
			new Vector3(1f, 1f, -1f),
			new Vector3(1f, -1f, 1f),
			new Vector3(1f, -1f, -1f),
			new Vector3(-1f, 1f, 1f),
			new Vector3(-1f, 1f, -1f),
			new Vector3(-1f, -1f, 1f),
			new Vector3(-1f, -1f, -1f)
		};
		Dictionary<Vector3, bool> pointInsideMeshCache = new Dictionary<Vector3, bool>();
		Stopwatch sw = Stopwatch.StartNew();
		Collider[] colliders = new Collider[1000];
		for (int i = 0; i < boxesPerEdge; i++)
		{
			for (int j = 0; j < boxesPerEdge; j++)
			{
				for (int k = 0; k < boxesPerEdge; k++)
				{
					Vector3 center = new Vector3(bounds.center.x - bounds.size.x / 2f + s.x * (float)i + halfExtent.x, bounds.center.y - bounds.size.y / 2f + s.y * (float)j + halfExtent.y, bounds.center.z - bounds.size.z / 2f + s.z * (float)k + halfExtent.z);
					if (!avoidExceedingMesh)
					{
						if (avoidGapsInside)
						{
							bool flag = IsInsideMesh(center, tree, pointInsideMeshCache);
							boxColliderPositions[i, j, k] = flag;
						}
						else
						{
							bool flag2 = Physics.OverlapBoxNonAlloc(center, halfExtent, colliders, Quaternion.identity, colliderLayerMask) > 0;
							boxColliderPositions[i, j, k] = flag2;
						}
					}
					else
					{
						bool flag3 = directionsFromBoxCenterToCorners.Select((Vector3 d) => new Vector3(center.x + halfExtent.x * d.x, center.y + halfExtent.y * d.y, center.z + halfExtent.z * d.z)).All((Vector3 cornerPoint) => IsInsideMesh(cornerPoint, tree, pointInsideMeshCache));
						boxColliderPositions[i, j, k] = flag3;
					}
				}
			}
		}
		sw.Stop();
		if (outputTimeMeasurements)
		{
			UnityEngine.Debug.Log("Boxes analyzed in " + sw.Elapsed);
		}
		for (int x = 0; x < boxesPerEdge; x++)
		{
			for (int y = 0; y < boxesPerEdge; y++)
			{
				for (int z = 0; z < boxesPerEdge; z++)
				{
					if (boxColliderPositions[x, y, z])
					{
						Vector3 center2 = new Vector3(bounds.center.x - bounds.size.x / 2f + s.x * (float)x + s.x / 2f, bounds.center.y - bounds.size.y / 2f + s.y * (float)y + s.y / 2f, bounds.center.z - bounds.size.z / 2f + s.z * (float)z + s.z / 2f);
						yield return boxes[x, y, z] = new Box(boxes, center2, s, new Vector3Int(x, y, z));
					}
				}
			}
		}
	}

	private bool IsInsideMesh(Vector3 p, SpatialBinaryTree tree, Dictionary<Vector3, bool> pointInsideMeshCache)
	{
		bool value;
		if (pointInsideMeshCache.TryGetValue(Vector3.one, out value))
		{
			return value;
		}
		Ray r = new Ray(p, new Vector3(1f, 0f, 0f));
		int num = tree.GetTris(r).Count((Tri t) => t.Intersect(r));
		return pointInsideMeshCache[p] = num % 2 != 0;
	}

	private int GetFirstEmptyLayer()
	{
		for (int i = 8; i <= 31; i++)
		{
			string text = LayerMask.LayerToName(i);
			if (text.Length == 0)
			{
				return i;
			}
		}
		throw new Exception("Didn't find unused layer for temporary assignment");
	}

	private static Bounds CalculateLocalBounds(GameObject go)
	{
		Bounds result = new Bounds(go.transform.position, Vector3.zero);
		Renderer[] componentsInChildren = go.GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			result.Encapsulate(renderer.bounds);
		}
		Vector3 center = result.center - go.transform.position;
		result.center = center;
		return result;
	}
}
