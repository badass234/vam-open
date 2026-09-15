using UnityEngine;

namespace Weelco.VRInput
{
	public abstract class IUIHitPointer : IUIPointer
	{
		public float LaserHitScale;

		public Color LaserColor;

		public bool HitAlwaysOn;

		public bool UseCustomLaserPointer;

		private GameObject hitPoint;

		public abstract bool ButtonDown();

		public abstract bool ButtonUp();

		public override void Initialize()
		{
			if (UseCustomLaserPointer)
			{
				hitPoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				hitPoint.transform.SetParent(target.transform, false);
				hitPoint.transform.localScale = new Vector3(LaserHitScale, LaserHitScale, LaserHitScale);
				hitPoint.transform.localPosition = new Vector3(0f, 0f, 100f);
				Object.DestroyImmediate(hitPoint.GetComponent<SphereCollider>());
				Material material = new Material(Shader.Find("Unlit/Color"));
				material.SetColor("_Color", LaserColor);
				hitPoint.GetComponent<MeshRenderer>().material = material;
			}
			else if ((bool)target)
			{
				Transform transform = target.Find("LaserPointer/LaserBeamDot");
				if ((bool)transform)
				{
					hitPoint = transform.gameObject;
				}
			}
			if ((bool)hitPoint && !HitAlwaysOn)
			{
				hitPoint.SetActive(false);
			}
		}

		protected override void UpdateRaycasting(bool isHit, float distance)
		{
			if ((bool)hitPoint)
			{
				if (isHit)
				{
					hitPoint.transform.localPosition = new Vector3(0f, 0f, distance);
				}
				if (!HitAlwaysOn)
				{
					hitPoint.SetActive(isHit);
				}
			}
		}
	}
}
