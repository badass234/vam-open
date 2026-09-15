using UnityEngine;

public class OVRLipSyncContextTextureFlip : MonoBehaviour
{
	public Material material;

	public Texture[] Textures = new Texture[OVRLipSync.VisemeCount];

	public float smoothing;

	private OVRLipSyncContextBase lipsyncContext;

	private OVRLipSync.Frame oldFrame = new OVRLipSync.Frame();

	private void Start()
	{
		lipsyncContext = GetComponent<OVRLipSyncContextBase>();
		if (lipsyncContext == null)
		{
			Debug.Log("LipSyncContextTextureFlip.Start WARNING: No lip sync context component set to object");
		}
	}

	private void Update()
	{
		if (!(lipsyncContext != null) || !(material != null))
		{
			return;
		}
		OVRLipSync.Frame currentPhonemeFrame = lipsyncContext.GetCurrentPhonemeFrame();
		if (currentPhonemeFrame != null)
		{
			for (int i = 0; i < currentPhonemeFrame.Visemes.Length; i++)
			{
				oldFrame.Visemes[i] = oldFrame.Visemes[i] * smoothing + currentPhonemeFrame.Visemes[i] * (1f - smoothing);
			}
			SetVisemeToTexture();
		}
	}

	private void SetVisemeToTexture()
	{
		int num = -1;
		float num2 = 0f;
		for (int i = 0; i < oldFrame.Visemes.Length; i++)
		{
			if (oldFrame.Visemes[i] > num2)
			{
				num = i;
				num2 = oldFrame.Visemes[i];
			}
		}
		if (num != -1 && num < Textures.Length)
		{
			Texture texture = Textures[num];
			if (texture != null)
			{
				material.SetTexture("_MainTex", texture);
			}
		}
	}
}
