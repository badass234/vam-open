using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Leap.Unity.RuntimeGizmos
{
	[ExecuteInEditMode]
	public class RuntimeGizmoManager : MonoBehaviour
	{
		public const string DEFAULT_SHADER_NAME = "Hidden/Runtime Gizmos";

		public const int CIRCLE_RESOLUTION = 32;

		[Tooltip("Should the gizmos be visible in the game view.")]
		[SerializeField]
		protected bool _displayInGameView = true;

		[Tooltip("Should the gizmos be visible in a build.")]
		[SerializeField]
		protected bool _enabledForBuild = true;

		[Tooltip("The mesh to use for the filled sphere gizmo.")]
		[SerializeField]
		protected Mesh _sphereMesh;

		[Tooltip("The shader to use for rendering gizmos.")]
		[SerializeField]
		protected Shader _gizmoShader;

		protected Mesh _cubeMesh;

		protected Mesh _wireCubeMesh;

		protected Mesh _wireSphereMesh;

		protected static RuntimeGizmoDrawer _backDrawer;

		protected static RuntimeGizmoDrawer _frontDrawer;

		private bool _readyForSwap;

		private List<GameObject> _objList = new List<GameObject>();

		private List<IRuntimeGizmoComponent> _gizmoList = new List<IRuntimeGizmoComponent>();

		public static event Action<RuntimeGizmoDrawer> OnPostRenderGizmos;

		public static bool TryGetGizmoDrawer(out RuntimeGizmoDrawer drawer)
		{
			drawer = _backDrawer;
			if (drawer != null)
			{
				drawer.ResetMatrixAndColorState();
				return true;
			}
			return false;
		}

		public static bool TryGetGizmoDrawer(GameObject attatchedGameObject, out RuntimeGizmoDrawer drawer)
		{
			drawer = _backDrawer;
			if (drawer != null && !areGizmosDisabled(attatchedGameObject.transform))
			{
				drawer.ResetMatrixAndColorState();
				return true;
			}
			return false;
		}

		protected virtual void OnValidate()
		{
			if (_gizmoShader == null)
			{
				_gizmoShader = Shader.Find("Hidden/Runtime Gizmos");
			}
			Material material = new Material(_gizmoShader);
			material.hideFlags = HideFlags.HideAndDontSave;
			if (material.passCount != 4)
			{
				Debug.LogError(string.Concat("Shader ", _gizmoShader, " does not have 4 passes and cannot be used as a gizmo shader."));
				_gizmoShader = Shader.Find("Hidden/Runtime Gizmos");
			}
			if (_frontDrawer != null && _backDrawer != null)
			{
				assignDrawerParams();
			}
		}

		protected virtual void Reset()
		{
			_gizmoShader = Shader.Find("Hidden/Runtime Gizmos");
		}

		protected virtual void OnEnable()
		{
			if (!_enabledForBuild)
			{
				base.enabled = false;
				return;
			}
			_frontDrawer = new RuntimeGizmoDrawer();
			_backDrawer = new RuntimeGizmoDrawer();
			_frontDrawer.BeginGuard();
			if (_gizmoShader == null)
			{
				_gizmoShader = Shader.Find("Hidden/Runtime Gizmos");
			}
			generateMeshes();
			assignDrawerParams();
			Camera.onPostRender = (Camera.CameraCallback)Delegate.Remove(Camera.onPostRender, new Camera.CameraCallback(onPostRender));
			Camera.onPostRender = (Camera.CameraCallback)Delegate.Combine(Camera.onPostRender, new Camera.CameraCallback(onPostRender));
		}

		protected virtual void OnDisable()
		{
			_frontDrawer = null;
			_backDrawer = null;
			Camera.onPostRender = (Camera.CameraCallback)Delegate.Remove(Camera.onPostRender, new Camera.CameraCallback(onPostRender));
		}

		protected virtual void Update()
		{
			SceneManager.GetActiveScene().GetRootGameObjects(_objList);
			for (int i = 0; i < _objList.Count; i++)
			{
				GameObject gameObject = _objList[i];
				gameObject.GetComponentsInChildren(false, _gizmoList);
				for (int j = 0; j < _gizmoList.Count; j++)
				{
					if (!areGizmosDisabled((_gizmoList[j] as Component).transform))
					{
						_backDrawer.ResetMatrixAndColorState();
						try
						{
							_gizmoList[j].OnDrawRuntimeGizmos(_backDrawer);
						}
						catch (Exception exception)
						{
							Debug.LogException(exception);
						}
					}
				}
			}
			_readyForSwap = true;
		}

		protected void onPostRender(Camera camera)
		{
			if (_readyForSwap)
			{
				if (RuntimeGizmoManager.OnPostRenderGizmos != null)
				{
					_backDrawer.ResetMatrixAndColorState();
					RuntimeGizmoManager.OnPostRenderGizmos(_backDrawer);
				}
				RuntimeGizmoDrawer backDrawer = _backDrawer;
				_backDrawer = _frontDrawer;
				_frontDrawer = backDrawer;
				_frontDrawer.BeginGuard();
				_backDrawer.EndGuard();
				_readyForSwap = false;
				_backDrawer.ClearAllGizmos();
			}
			_frontDrawer.DrawAllGizmosToScreen();
		}

		protected static bool areGizmosDisabled(Transform transform)
		{
			bool result = false;
			do
			{
				RuntimeGizmoToggle componentInParent = transform.GetComponentInParent<RuntimeGizmoToggle>();
				if (componentInParent == null)
				{
					break;
				}
				if (!componentInParent.enabled)
				{
					result = true;
					break;
				}
				transform = transform.parent;
			}
			while (transform != null);
			return result;
		}

		private void assignDrawerParams()
		{
			if (_gizmoShader != null)
			{
				_frontDrawer.gizmoShader = _gizmoShader;
				_backDrawer.gizmoShader = _gizmoShader;
			}
			_frontDrawer.sphereMesh = _sphereMesh;
			_frontDrawer.cubeMesh = _cubeMesh;
			_frontDrawer.wireSphereMesh = _wireSphereMesh;
			_frontDrawer.wireCubeMesh = _wireCubeMesh;
			_backDrawer.sphereMesh = _sphereMesh;
			_backDrawer.cubeMesh = _cubeMesh;
			_backDrawer.wireSphereMesh = _wireSphereMesh;
			_backDrawer.wireCubeMesh = _wireCubeMesh;
		}

		private void generateMeshes()
		{
			_cubeMesh = new Mesh();
			_cubeMesh.name = "RuntimeGizmoCube";
			_cubeMesh.hideFlags = HideFlags.HideAndDontSave;
			List<Vector3> list = new List<Vector3>();
			List<int> list2 = new List<int>();
			Vector3[] array = new Vector3[3]
			{
				Vector3.forward,
				Vector3.right,
				Vector3.up
			};
			for (int i = 0; i < 3; i++)
			{
				addQuad(list, list2, array[i % 3], -array[(i + 1) % 3], array[(i + 2) % 3]);
				addQuad(list, list2, -array[i % 3], array[(i + 1) % 3], array[(i + 2) % 3]);
			}
			_cubeMesh.SetVertices(list);
			_cubeMesh.SetIndices(list2.ToArray(), MeshTopology.Quads, 0);
			_cubeMesh.RecalculateNormals();
			_cubeMesh.RecalculateBounds();
			_cubeMesh.UploadMeshData(true);
			_wireCubeMesh = new Mesh();
			_wireCubeMesh.name = "RuntimeWireCubeMesh";
			_wireCubeMesh.hideFlags = HideFlags.HideAndDontSave;
			list.Clear();
			list2.Clear();
			for (int num = 1; num >= -1; num -= 2)
			{
				for (int num2 = 1; num2 >= -1; num2 -= 2)
				{
					for (int num3 = 1; num3 >= -1; num3 -= 2)
					{
						list.Add(0.5f * new Vector3(num, num2, num3));
					}
				}
			}
			addCorner(list2, 0, 1, 2, 4);
			addCorner(list2, 3, 1, 2, 7);
			addCorner(list2, 5, 1, 4, 7);
			addCorner(list2, 6, 2, 4, 7);
			_wireCubeMesh.SetVertices(list);
			_wireCubeMesh.SetIndices(list2.ToArray(), MeshTopology.Lines, 0);
			_wireCubeMesh.RecalculateBounds();
			_wireCubeMesh.UploadMeshData(true);
			_wireSphereMesh = new Mesh();
			_wireSphereMesh.name = "RuntimeWireSphereMesh";
			_wireSphereMesh.hideFlags = HideFlags.HideAndDontSave;
			list.Clear();
			list2.Clear();
			int num4 = 96;
			for (int j = 0; j < 32; j++)
			{
				float f = (float)Math.PI * 2f * (float)j / 32f;
				float num5 = 0.5f * Mathf.Cos(f);
				float num6 = 0.5f * Mathf.Sin(f);
				for (int k = 0; k < 3; k++)
				{
					list2.Add((j * 3 + k) % num4);
					list2.Add((j * 3 + k + 3) % num4);
				}
				list.Add(new Vector3(num5, num6, 0f));
				list.Add(new Vector3(0f, num5, num6));
				list.Add(new Vector3(num5, 0f, num6));
			}
			_wireSphereMesh.SetVertices(list);
			_wireSphereMesh.SetIndices(list2.ToArray(), MeshTopology.Lines, 0);
			_wireSphereMesh.RecalculateBounds();
			_wireSphereMesh.UploadMeshData(true);
		}

		private void addQuad(List<Vector3> verts, List<int> indexes, Vector3 normal, Vector3 axis1, Vector3 axis2)
		{
			indexes.Add(verts.Count);
			indexes.Add(verts.Count + 1);
			indexes.Add(verts.Count + 2);
			indexes.Add(verts.Count + 3);
			verts.Add(0.5f * (normal + axis1 + axis2));
			verts.Add(0.5f * (normal + axis1 - axis2));
			verts.Add(0.5f * (normal - axis1 - axis2));
			verts.Add(0.5f * (normal - axis1 + axis2));
		}

		private void addCorner(List<int> indexes, int a, int b, int c, int d)
		{
			indexes.Add(a);
			indexes.Add(b);
			indexes.Add(a);
			indexes.Add(c);
			indexes.Add(a);
			indexes.Add(d);
		}
	}
}
