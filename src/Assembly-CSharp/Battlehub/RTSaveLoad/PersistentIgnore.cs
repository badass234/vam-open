using System;
using System.Collections.Generic;
using System.Linq;
using Battlehub.Utils;
using UnityEngine;

namespace Battlehub.RTSaveLoad
{
	[DisallowMultipleComponent]
	[ExecuteInEditMode]
	public class PersistentIgnore : MonoBehaviour
	{
		[ReadOnly]
		[SerializeField]
		private string m_guid;

		private static Dictionary<Guid, PersistentIgnore> m_instances = new Dictionary<Guid, PersistentIgnore>();

		[ReadOnly]
		[SerializeField]
		private bool m_isRuntime = true;

		public bool IsRuntime => m_isRuntime;

		private void Awake()
		{
			if (!Application.isPlaying)
			{
				m_isRuntime = false;
				if (!string.IsNullOrEmpty(m_guid))
				{
					Guid key = new Guid(m_guid);
					if (m_instances.ContainsKey(key))
					{
						if (m_instances[key] != this)
						{
							key = Guid.NewGuid();
							m_instances.Add(key, this);
							m_guid = key.ToString();
						}
					}
					else
					{
						m_instances.Add(key, this);
					}
				}
				else
				{
					Guid key2 = Guid.NewGuid();
					m_instances.Add(key2, this);
					m_guid = key2.ToString();
				}
			}
			else if (string.IsNullOrEmpty(m_guid))
			{
				m_isRuntime = true;
			}
			else
			{
				if (m_instances == null)
				{
					m_instances = new Dictionary<Guid, PersistentIgnore>();
				}
				Guid key3 = new Guid(m_guid);
				if (m_instances.ContainsKey(key3))
				{
					m_isRuntime = true;
					return;
				}
				m_isRuntime = false;
				m_instances.Add(key3, null);
			}
		}

		private void Reset()
		{
			if (!Application.isPlaying)
			{
				Guid key = (from kvp in m_instances
					where kvp.Value == this
					select kvp.Key).FirstOrDefault();
				if (m_instances.ContainsKey(key))
				{
					m_guid = key.ToString();
				}
				m_isRuntime = false;
			}
		}

		private void Start()
		{
		}

		private void OnDestroy()
		{
			if (!string.IsNullOrEmpty(m_guid))
			{
				m_instances.Remove(new Guid(m_guid));
			}
		}

		public GameObject InstantiatePrefab(GameObject prefab, Vector3 position, Quaternion rotation)
		{
			if (!prefab.IsPrefab())
			{
				throw new ArgumentException("is not a prefab", "prefab");
			}
			return UnityEngine.Object.Instantiate(prefab, position, rotation);
		}
	}
}
