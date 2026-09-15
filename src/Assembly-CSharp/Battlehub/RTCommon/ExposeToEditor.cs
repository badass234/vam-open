using System.Collections.Generic;
using System.Linq;
using Battlehub.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Battlehub.RTCommon
{
	[DisallowMultipleComponent]
	public class ExposeToEditor : MonoBehaviour
	{
		private bool m_applicationQuit;

		[SerializeField]
		[HideInInspector]
		private Collider[] m_colliders;

		private SpriteRenderer m_spriteRenderer;

		private MeshFilter m_filter;

		private SkinnedMeshRenderer m_skinned;

		private static readonly Bounds m_none = default(Bounds);

		public ExposeToEditorUnityEvent Selected;

		public ExposeToEditorUnityEvent Unselected;

		public GameObject BoundsObject;

		public BoundsType BoundsType;

		public Bounds CustomBounds;

		[HideInInspector]
		public bool CanSelect = true;

		[HideInInspector]
		public bool CanSnap = true;

		public bool AddColliders = true;

		[SerializeField]
		[HideInInspector]
		private ExposeToEditorObjectType m_objectType;

		private bool m_markAsDestroyed;

		private BoundsType m_effectiveBoundsType;

		private HierarchyItem m_hierarchyItem;

		private List<ExposeToEditor> m_children = new List<ExposeToEditor>();

		private ExposeToEditor m_parent;

		private bool m_initialized;

		public Collider[] Colliders => m_colliders;

		public ExposeToEditorObjectType ObjectType
		{
			get
			{
				return m_objectType;
			}
			set
			{
				if (m_objectType == ExposeToEditorObjectType.Undefined || m_objectType != value)
				{
				}
				m_objectType = value;
			}
		}

		public bool MarkAsDestroyed
		{
			get
			{
				return m_markAsDestroyed;
			}
			set
			{
				if (m_markAsDestroyed != value)
				{
					m_markAsDestroyed = value;
					base.gameObject.SetActive(!m_markAsDestroyed);
					if (ExposeToEditor.MarkAsDestroyedChanged != null)
					{
						ExposeToEditor.MarkAsDestroyedChanged(this);
					}
				}
			}
		}

		public Bounds Bounds
		{
			get
			{
				if (m_effectiveBoundsType == BoundsType.Any)
				{
					if (m_filter != null && m_filter.sharedMesh != null)
					{
						return m_filter.sharedMesh.bounds;
					}
					if (m_skinned != null && m_skinned.sharedMesh != null)
					{
						return m_skinned.sharedMesh.bounds;
					}
					if (m_spriteRenderer != null)
					{
						return m_spriteRenderer.sprite.bounds;
					}
					return CustomBounds;
				}
				if (m_effectiveBoundsType == BoundsType.Mesh)
				{
					if (m_filter != null && m_filter.sharedMesh != null)
					{
						return m_filter.sharedMesh.bounds;
					}
					return m_none;
				}
				if (m_effectiveBoundsType == BoundsType.SkinnedMesh)
				{
					if (m_skinned != null && m_skinned.sharedMesh != null)
					{
						return m_skinned.sharedMesh.bounds;
					}
				}
				else if (m_effectiveBoundsType == BoundsType.Sprite)
				{
					if (m_spriteRenderer != null)
					{
						return m_spriteRenderer.sprite.bounds;
					}
				}
				else if (m_effectiveBoundsType == BoundsType.Custom)
				{
					return CustomBounds;
				}
				return m_none;
			}
		}

		public int ChildCount => m_children.Count;

		public int MarkedAsDestroyedChildCount => m_children.Where((ExposeToEditor e) => e.MarkAsDestroyed).Count();

		public ExposeToEditor Parent
		{
			get
			{
				return m_parent;
			}
			set
			{
				if (m_parent != value)
				{
					ExposeToEditor oldValue = ChangeParent(value);
					if (ExposeToEditor.ParentChanged != null)
					{
						ExposeToEditor.ParentChanged(this, oldValue, m_parent);
					}
				}
			}
		}

		public static event ExposeToEditorEvent Awaked;

		public static event ExposeToEditorEvent Destroying;

		public static event ExposeToEditorEvent Destroyed;

		public static event ExposeToEditorEvent MarkAsDestroyedChanged;

		public static event ExposeToEditorEvent NameChanged;

		public static event ExposeToEditorEvent TransformChanged;

		public static event ExposeToEditorEvent Started;

		public static event ExposeToEditorEvent Enabled;

		public static event ExposeToEditorEvent Disabled;

		public static event ExposeToEditorChangeEvent<ExposeToEditor> ParentChanged;

		public ExposeToEditor[] GetChildren()
		{
			return m_children.OrderBy((ExposeToEditor c) => c.transform.GetSiblingIndex()).ToArray();
		}

		public ExposeToEditor NextSibling()
		{
			if (Parent != null)
			{
				int num = Parent.m_children.IndexOf(this);
				if (num < Parent.m_children.Count - 1)
				{
					return Parent.m_children[num - 1];
				}
				return null;
			}
			IEnumerable<GameObject> enumerable = ((!RuntimeEditorApplication.IsPlaying) ? (from g in FindAll(ExposeToEditorObjectType.EditorMode)
				orderby g.transform.GetSiblingIndex()
				select g) : FindAll(ExposeToEditorObjectType.PlayMode));
			IEnumerator<GameObject> enumerator = enumerable.GetEnumerator();
			while (enumerator.MoveNext())
			{
				if (enumerator.Current == base.gameObject)
				{
					enumerator.MoveNext();
					return enumerator.Current.GetComponent<ExposeToEditor>();
				}
			}
			return null;
		}

		private ExposeToEditor ChangeParent(ExposeToEditor value)
		{
			ExposeToEditor parent = m_parent;
			m_parent = value;
			if (parent != null)
			{
				parent.m_children.Remove(this);
			}
			if (m_parent != null)
			{
				m_parent.m_children.Add(this);
			}
			return parent;
		}

		private void Awake()
		{
			RuntimeEditorApplication.IsOpenedChanged += OnEditorIsOpenedChanged;
			m_objectType = ExposeToEditorObjectType.Undefined;
			Init();
			m_hierarchyItem = base.gameObject.GetComponent<HierarchyItem>();
			if (m_hierarchyItem == null)
			{
				m_hierarchyItem = base.gameObject.AddComponent<HierarchyItem>();
			}
			if (base.hideFlags != HideFlags.HideAndDontSave && ExposeToEditor.Awaked != null)
			{
				ExposeToEditor.Awaked(this);
			}
		}

		public void Init()
		{
			if (!m_initialized)
			{
				FindChildren(base.transform);
				m_initialized = true;
			}
		}

		private void FindChildren(Transform parent)
		{
			foreach (Transform item in parent)
			{
				ExposeToEditor component = item.GetComponent<ExposeToEditor>();
				if (component == null)
				{
					FindChildren(item);
					continue;
				}
				component.m_parent = this;
				m_children.Add(component);
			}
		}

		private void OnEditorIsOpenedChanged()
		{
			if (RuntimeEditorApplication.IsOpened)
			{
				TryToAddColliders();
			}
			else
			{
				TryToDestroyColliders();
			}
		}

		private void Start()
		{
			if (BoundsObject == null)
			{
				BoundsObject = base.gameObject;
			}
			m_effectiveBoundsType = BoundsType;
			m_filter = BoundsObject.GetComponent<MeshFilter>();
			m_skinned = BoundsObject.GetComponent<SkinnedMeshRenderer>();
			if (m_filter == null && m_skinned == null)
			{
				m_spriteRenderer = BoundsObject.GetComponent<SpriteRenderer>();
			}
			if (RuntimeEditorApplication.IsOpened)
			{
				TryToAddColliders();
			}
			else
			{
				TryToDestroyColliders();
				m_colliders = null;
			}
			if (base.hideFlags != HideFlags.HideAndDontSave && ExposeToEditor.Started != null)
			{
				ExposeToEditor.Started(this);
			}
		}

		private void OnEnable()
		{
			if (base.hideFlags != HideFlags.HideAndDontSave && ExposeToEditor.Enabled != null)
			{
				ExposeToEditor.Enabled(this);
			}
		}

		private void OnDisable()
		{
			if (base.hideFlags != HideFlags.HideAndDontSave && ExposeToEditor.Disabled != null)
			{
				ExposeToEditor.Disabled(this);
			}
		}

		private void OnDestroy()
		{
			RuntimeEditorApplication.IsOpenedChanged -= OnEditorIsOpenedChanged;
			if (!m_applicationQuit)
			{
				if (base.hideFlags != HideFlags.HideAndDontSave && ExposeToEditor.Destroying != null)
				{
					ExposeToEditor.Destroying(this);
				}
				if (m_parent != null)
				{
					ChangeParent(null);
				}
				TryToDestroyColliders();
				if (m_hierarchyItem != null)
				{
					Object.Destroy(m_hierarchyItem);
				}
				if (base.hideFlags != HideFlags.HideAndDontSave && ExposeToEditor.Destroyed != null)
				{
					ExposeToEditor.Destroyed(this);
				}
			}
		}

		private void OnApplicationQuit()
		{
			m_applicationQuit = true;
		}

		private void Update()
		{
			if (ExposeToEditor.TransformChanged != null && base.transform.hasChanged)
			{
				base.transform.hasChanged = false;
				if (base.hideFlags != HideFlags.HideAndDontSave && ExposeToEditor.TransformChanged != null)
				{
					ExposeToEditor.TransformChanged(this);
				}
			}
		}

		private void TryToAddColliders()
		{
			if (this == null || (m_colliders != null && m_colliders.Length != 0))
			{
				return;
			}
			List<Collider> list = new List<Collider>();
			Rigidbody component = base.gameObject.GetComponent<Rigidbody>();
			bool flag = component != null;
			if (m_effectiveBoundsType == BoundsType.Any)
			{
				if (m_filter != null)
				{
					if (AddColliders && !flag)
					{
						MeshCollider meshCollider = base.gameObject.AddComponent<MeshCollider>();
						meshCollider.convex = flag;
						meshCollider.sharedMesh = m_filter.sharedMesh;
						list.Add(meshCollider);
					}
				}
				else if (m_skinned != null)
				{
					if (AddColliders && !flag)
					{
						MeshCollider meshCollider2 = base.gameObject.AddComponent<MeshCollider>();
						meshCollider2.convex = flag;
						meshCollider2.sharedMesh = m_skinned.sharedMesh;
						list.Add(meshCollider2);
					}
				}
				else if (m_spriteRenderer != null && AddColliders && !flag)
				{
					BoxCollider boxCollider = base.gameObject.AddComponent<BoxCollider>();
					boxCollider.size = m_spriteRenderer.sprite.bounds.size;
					list.Add(boxCollider);
				}
			}
			else if (m_effectiveBoundsType == BoundsType.Mesh)
			{
				if (m_filter != null && AddColliders && !flag)
				{
					MeshCollider meshCollider3 = base.gameObject.AddComponent<MeshCollider>();
					meshCollider3.convex = flag;
					meshCollider3.sharedMesh = m_filter.sharedMesh;
					list.Add(meshCollider3);
				}
			}
			else if (m_effectiveBoundsType == BoundsType.SkinnedMesh)
			{
				if (m_skinned != null && AddColliders && !flag)
				{
					MeshCollider meshCollider4 = base.gameObject.AddComponent<MeshCollider>();
					meshCollider4.convex = flag;
					meshCollider4.sharedMesh = m_skinned.sharedMesh;
					list.Add(meshCollider4);
				}
			}
			else if (m_effectiveBoundsType == BoundsType.Sprite)
			{
				if (m_spriteRenderer != null && AddColliders && !flag)
				{
					BoxCollider boxCollider2 = base.gameObject.AddComponent<BoxCollider>();
					boxCollider2.size = m_spriteRenderer.sprite.bounds.size;
					list.Add(boxCollider2);
				}
			}
			else if (m_effectiveBoundsType == BoundsType.Custom && AddColliders && !flag)
			{
				Mesh sharedMesh = RuntimeGraphics.CreateCubeMesh(Color.black, CustomBounds.center, CustomBounds.extents.x * 2f, CustomBounds.extents.y * 2f, CustomBounds.extents.z * 2f);
				MeshCollider meshCollider5 = base.gameObject.AddComponent<MeshCollider>();
				meshCollider5.convex = flag;
				meshCollider5.sharedMesh = sharedMesh;
				list.Add(meshCollider5);
			}
			m_colliders = list.ToArray();
		}

		private void TryToDestroyColliders()
		{
			if (m_colliders == null)
			{
				return;
			}
			for (int i = 0; i < m_colliders.Length; i++)
			{
				Collider collider = m_colliders[i];
				if (collider != null)
				{
					Object.Destroy(collider);
				}
			}
			m_colliders = null;
		}

		public void SetName(string name)
		{
			base.gameObject.name = name;
			if (base.hideFlags != HideFlags.HideAndDontSave && ExposeToEditor.NameChanged != null)
			{
				ExposeToEditor.NameChanged(this);
			}
		}

		private static bool IsExposedToEditor(GameObject go, ExposeToEditorObjectType type, bool roots)
		{
			ExposeToEditor component = go.GetComponent<ExposeToEditor>();
			return component != null && (!roots || component.transform.parent == null || component.transform.parent.GetComponentsInParent<ExposeToEditor>(true).Length == 0) && !component.MarkAsDestroyed && component.ObjectType == type && component.hideFlags != HideFlags.HideAndDontSave;
		}

		public static IEnumerable<GameObject> FindAll(ExposeToEditorObjectType type, bool roots = true)
		{
			if (SceneManager.GetActiveScene().isLoaded)
			{
				return FindAllUsingSceneManagement(type, roots);
			}
			List<GameObject> list = new List<GameObject>();
			GameObject[] array = Resources.FindObjectsOfTypeAll<GameObject>();
			foreach (GameObject gameObject in array)
			{
				if (!(gameObject == null) && !gameObject.IsPrefab())
				{
					list.Add(gameObject);
				}
			}
			return list.Where((GameObject f) => IsExposedToEditor(f, type, roots));
		}

		public static IEnumerable<GameObject> FindAllUsingSceneManagement(ExposeToEditorObjectType type, bool roots = true)
		{
			List<GameObject> list = new List<GameObject>();
			GameObject[] rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();
			for (int i = 0; i < rootGameObjects.Length; i++)
			{
				ExposeToEditor[] componentsInChildren = rootGameObjects[i].GetComponentsInChildren<ExposeToEditor>(true);
				foreach (ExposeToEditor exposeToEditor in componentsInChildren)
				{
					if (IsExposedToEditor(exposeToEditor.gameObject, type, roots) && !exposeToEditor.gameObject.IsPrefab())
					{
						list.Add(exposeToEditor.gameObject);
					}
				}
			}
			return list;
		}
	}
}
