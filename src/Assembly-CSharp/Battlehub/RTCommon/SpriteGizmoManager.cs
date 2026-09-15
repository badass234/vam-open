using System;
using System.Collections.Generic;
using Battlehub.Utils;
using UnityEngine;

namespace Battlehub.RTCommon
{
	public class SpriteGizmoManager : MonoBehaviour
	{
		private static readonly Dictionary<Type, string> m_typeToMaterialName = new Dictionary<Type, string>
		{
			{
				typeof(Light),
				"BattlehubLightGizmo"
			},
			{
				typeof(Camera),
				"BattlehubCameraGizmo"
			},
			{
				typeof(AudioSource),
				"BattlehubAudioSourceGizmo"
			}
		};

		private static Dictionary<Type, Material> m_typeToMaterial;

		private static Type[] m_types;

		private static SpriteGizmoManager m_instance;

		private void Awake()
		{
			if (m_instance != null)
			{
				Debug.LogWarning("Another instance of GizmoManager Exists");
			}
			m_instance = this;
			Cleanup();
			Initialize();
			AwakeOverride();
		}

		private void OnDestroy()
		{
			Cleanup();
			if (m_instance == this)
			{
				m_instance = null;
				m_typeToMaterial = null;
				m_types = null;
			}
			OnDestroyOverride();
		}

		protected virtual void AwakeOverride()
		{
		}

		protected virtual void OnDestroyOverride()
		{
		}

		protected virtual Type[] GetTypes(Type[] types)
		{
			return types;
		}

		protected virtual void GreateGizmo(GameObject go, Type type)
		{
			Material value;
			if (m_typeToMaterial.TryGetValue(type, out value))
			{
				SpriteGizmo spriteGizmo = go.GetComponent<SpriteGizmo>();
				if (!spriteGizmo)
				{
					spriteGizmo = go.AddComponent<SpriteGizmo>();
				}
				spriteGizmo.Material = value;
			}
		}

		protected virtual void DestroyGizmo(GameObject go)
		{
			SpriteGizmo component = go.GetComponent<SpriteGizmo>();
			if ((bool)component)
			{
				UnityEngine.Object.Destroy(component);
			}
		}

		private static void Initialize()
		{
			if (m_types != null)
			{
				Debug.LogWarning("Already initialized");
				return;
			}
			m_typeToMaterial = new Dictionary<Type, Material>();
			foreach (KeyValuePair<Type, string> item in m_typeToMaterialName)
			{
				Material material = Resources.Load<Material>(item.Value);
				if (material != null)
				{
					m_typeToMaterial.Add(item.Key, material);
				}
			}
			int num = 0;
			m_types = new Type[m_typeToMaterial.Count];
			foreach (Type key in m_typeToMaterial.Keys)
			{
				m_types[num] = key;
				num++;
			}
			m_types = m_instance.GetTypes(m_types);
			OnIsOpenedChanged();
			RuntimeEditorApplication.IsOpenedChanged += OnIsOpenedChanged;
		}

		private static void Cleanup()
		{
			m_types = null;
			m_typeToMaterial = null;
			RuntimeEditorApplication.IsOpenedChanged -= OnIsOpenedChanged;
			UnsubscribeAndDestroy();
		}

		private static void UnsubscribeAndDestroy()
		{
			Unsubscribe();
			SpriteGizmo[] array = Resources.FindObjectsOfTypeAll<SpriteGizmo>();
			foreach (SpriteGizmo spriteGizmo in array)
			{
				if (!spriteGizmo.gameObject.IsPrefab())
				{
					m_instance.DestroyGizmo(spriteGizmo.gameObject);
				}
			}
		}

		private static void OnIsOpenedChanged()
		{
			if (RuntimeEditorApplication.IsOpened)
			{
				for (int i = 0; i < m_types.Length; i++)
				{
					UnityEngine.Object[] array = Resources.FindObjectsOfTypeAll(m_types[i]);
					for (int j = 0; j < array.Length; j++)
					{
						Component component = array[j] as Component;
						if ((bool)component && !component.gameObject.IsPrefab())
						{
							ExposeToEditor component2 = component.gameObject.GetComponent<ExposeToEditor>();
							if (component2 != null)
							{
								m_instance.GreateGizmo(component.gameObject, m_types[i]);
							}
						}
					}
				}
				Subscribe();
			}
			else
			{
				UnsubscribeAndDestroy();
			}
		}

		private static void Subscribe()
		{
			ExposeToEditor.Awaked += OnAwaked;
			ExposeToEditor.Destroyed += OnDestroyed;
		}

		private static void Unsubscribe()
		{
			ExposeToEditor.Awaked -= OnAwaked;
			ExposeToEditor.Destroyed -= OnDestroyed;
		}

		private static void OnAwaked(ExposeToEditor obj)
		{
			for (int i = 0; i < m_types.Length; i++)
			{
				Component component = obj.GetComponent(m_types[i]);
				if (component != null)
				{
					m_instance.GreateGizmo(obj.gameObject, m_types[i]);
				}
			}
		}

		private static void OnDestroyed(ExposeToEditor obj)
		{
			for (int i = 0; i < m_types.Length; i++)
			{
				Component component = obj.GetComponent(m_types[i]);
				if (component != null)
				{
					m_instance.DestroyGizmo(obj.gameObject);
				}
			}
		}
	}
}
