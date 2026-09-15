using UnityEngine;

namespace Weelco.VRInput
{
	public abstract class IUILaserPointer : IUIHitPointer
	{
		public bool IsRightHand;

		public bool UseHapticPulse;

		public float LaserThickness;

		private GameObject pointer;

		private float _distanceLimit;

		public override void Initialize()
		{
			if (UseCustomLaserPointer)
			{
				pointer = GameObject.CreatePrimitive(PrimitiveType.Cube);
				pointer.transform.SetParent(target.transform, false);
				pointer.transform.localScale = new Vector3(LaserThickness, LaserThickness, 100f);
				pointer.transform.localPosition = new Vector3(0f, 0f, 50f);
				Object.DestroyImmediate(pointer.GetComponent<BoxCollider>());
				Material material = new Material(Shader.Find("Unlit/Color"));
				material.SetColor("_Color", LaserColor);
				pointer.GetComponent<MeshRenderer>().material = material;
			}
			else if ((bool)target)
			{
				Transform transform = target.Find("LaserPointer/LaserBeam");
				if ((bool)transform)
				{
					pointer = transform.gameObject;
				}
			}
			base.Initialize();
		}

		protected override void UpdateRaycasting(bool isHit, float distance)
		{
			if ((bool)pointer)
			{
				pointer.transform.localScale = new Vector3(pointer.transform.localScale.x, pointer.transform.localScale.y, distance);
				pointer.transform.localPosition = new Vector3(0f, 0f, distance * 0.5f);
			}
			base.UpdateRaycasting(isHit, distance);
		}
	}
}
