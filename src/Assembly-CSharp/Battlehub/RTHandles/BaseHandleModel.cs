using Battlehub.RTCommon;
using Battlehub.RTSaveLoad;
using UnityEngine;

namespace Battlehub.RTHandles
{
	public class BaseHandleModel : MonoBehaviour
	{
		[SerializeField]
		protected Color m_xColor = RTHColors.XColor;

		[SerializeField]
		protected Color m_yColor = RTHColors.YColor;

		[SerializeField]
		protected Color m_zColor = RTHColors.ZColor;

		[SerializeField]
		protected Color m_altColor = RTHColors.AltColor;

		[SerializeField]
		protected Color m_altColor2 = RTHColors.AltColor2;

		[SerializeField]
		protected Color m_disabledColor = RTHColors.DisabledColor;

		[SerializeField]
		protected Color m_selectionColor = RTHColors.SelectionColor;

		protected RuntimeHandleAxis m_selectedAxis;

		protected LockObject m_lockObj = new LockObject();

		protected RuntimeGraphicsLayer m_graphicsLayer;

		protected virtual void Awake()
		{
			m_graphicsLayer = Object.FindObjectOfType<RuntimeGraphicsLayer>();
			if (m_graphicsLayer == null)
			{
				GameObject gameObject = new GameObject();
				gameObject.AddComponent<PersistentIgnore>();
				m_graphicsLayer = gameObject.AddComponent<RuntimeGraphicsLayer>();
				m_graphicsLayer.name = "RuntimeGraphicsLayer";
			}
			SetLayer(base.transform, m_graphicsLayer.GraphicsLayer);
		}

		private void SetLayer(Transform t, int layer)
		{
			t.gameObject.layer = layer;
			foreach (Transform item in t)
			{
				SetLayer(item, layer);
			}
		}

		public virtual void SetLock(LockObject lockObj)
		{
			if (lockObj == null)
			{
				lockObj = new LockObject();
			}
			m_lockObj = lockObj;
		}

		public virtual void Select(RuntimeHandleAxis axis)
		{
			m_selectedAxis = axis;
		}

		public virtual void SetScale(Vector3 scale)
		{
		}

		protected virtual void Start()
		{
		}

		protected virtual void OnDestroy()
		{
		}

		protected virtual void Update()
		{
		}
	}
}
