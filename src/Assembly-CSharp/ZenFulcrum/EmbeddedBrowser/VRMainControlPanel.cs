using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ZenFulcrum.EmbeddedBrowser
{
	public class VRMainControlPanel : MonoBehaviour
	{
		public static VRMainControlPanel instance;

		public GameObject browserPrefab;

		public float moveSpeed = 0.01f;

		public float height = 1.5f;

		public float radius = 2f;

		public int browsersToFit = 8;

		protected List<VRBrowserPanel> allBrowsers = new List<VRBrowserPanel>();

		public ExternalKeyboard keyboard;

		private Vector3 baseKeyboardScale;

		private VRBrowserPanel keyboardTarget;

		public void Awake()
		{
			instance = this;
			Browser component = GetComponent<Browser>();
			component.RegisterFunction("openNewTab", delegate
			{
				OpenNewTab();
			});
			component.RegisterFunction("shiftTabs", delegate(JSONNode args)
			{
				ShiftTabs(args[0]);
			});
			baseKeyboardScale = keyboard.transform.localScale;
			OpenNewTab();
		}

		private void ShiftTabs(int direction)
		{
			allBrowsers = allBrowsers.Select((VRBrowserPanel t, int i) => allBrowsers[(i + direction + allBrowsers.Count) % allBrowsers.Count]).ToList();
		}

		public VRBrowserPanel OpenNewTab(VRBrowserPanel nextTo = null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(browserPrefab);
			VRBrowserPanel component = gameObject.GetComponent<VRBrowserPanel>();
			int num = -1;
			if ((bool)nextTo)
			{
				num = allBrowsers.FindIndex((VRBrowserPanel x) => x == nextTo);
			}
			if (num > 0)
			{
				allBrowsers.Insert(num + 1, component);
			}
			else
			{
				allBrowsers.Insert(allBrowsers.Count / 2, component);
			}
			component.transform.position = base.transform.position;
			component.transform.rotation = base.transform.rotation;
			component.transform.localScale = Vector3.zero;
			return component;
		}

		public void MoveKeyboardUnder(VRBrowserPanel panel)
		{
			keyboardTarget = panel;
		}

		public void DestroyPane(VRBrowserPanel pane)
		{
			StartCoroutine(_DestroyBrowser(pane));
		}

		private IEnumerator _DestroyBrowser(VRBrowserPanel pane)
		{
			allBrowsers.Remove(pane);
			if (!pane)
			{
				yield break;
			}
			Vector3 targetPos = pane.transform.position;
			targetPos.y = 0f;
			float t0 = Time.time;
			while (Time.time < t0 + 3f)
			{
				if (!pane)
				{
					yield break;
				}
				MoveToward(pane.transform, targetPos, pane.transform.rotation, Vector3.zero);
				yield return null;
			}
			UnityEngine.Object.Destroy(pane.gameObject);
		}

		private void MoveToward(Transform t, Vector3 pos, Quaternion rot, Vector3 scale)
		{
			t.position = Vector3.Lerp(t.position, pos, moveSpeed);
			t.rotation = Quaternion.Slerp(t.rotation, rot, moveSpeed);
			t.localScale = Vector3.Lerp(t.localScale, scale, moveSpeed);
		}

		public void Update()
		{
			float t = Mathf.Clamp01((float)(allBrowsers.Count - 1) / (float)browsersToFit);
			float num = Mathf.Lerp(0f, 360f, t);
			float num2 = Mathf.Lerp(0f, 180f, t);
			float num3 = Mathf.Lerp(1f, 0f, t);
			for (int i = 0; i < allBrowsers.Count; i++)
			{
				float num4 = (float)i / ((float)allBrowsers.Count - num3) * num - num2;
				if (i == 0 && allBrowsers.Count == 1)
				{
					num4 = num / 2f - num2;
				}
				num4 *= (float)Math.PI / 180f;
				Vector3 pos = new Vector3(Mathf.Sin(num4) * radius, height, Mathf.Cos(num4) * radius);
				Quaternion rot = Quaternion.LookRotation(new Vector3(pos.x, 0f, pos.z), Vector3.up);
				MoveToward(allBrowsers[i].transform, pos, rot, Vector3.one);
			}
			Vector3 pos2 = ((!keyboardTarget) ? Vector3.zero : keyboardTarget.keyboardLocation.position);
			Quaternion rot2 = ((!keyboardTarget) ? Quaternion.LookRotation(Vector3.down, Vector3.forward) : keyboardTarget.keyboardLocation.rotation);
			Vector3 scale = ((!keyboardTarget) ? Vector3.zero : baseKeyboardScale);
			MoveToward(keyboard.transform, pos2, rot2, scale);
		}
	}
}
