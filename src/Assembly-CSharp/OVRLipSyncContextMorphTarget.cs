using UnityEngine;

public class OVRLipSyncContextMorphTarget : MonoBehaviour
{
	public SkinnedMeshRenderer skinnedMeshRenderer;

	public int[] VisemeToBlendTargets = new int[OVRLipSync.VisemeCount];

	public bool enableVisemeSignals;

	public int[] KeySendVisemeSignal = new int[10];

	public int SmoothAmount = 100;

	private OVRLipSyncContextBase lipsyncContext;

	private void Start()
	{
		if (skinnedMeshRenderer == null)
		{
			Debug.Log("LipSyncContextMorphTarget.Start WARNING: Please set required public components!");
			return;
		}
		lipsyncContext = GetComponent<OVRLipSyncContextBase>();
		if (lipsyncContext == null)
		{
			Debug.Log("LipSyncContextMorphTarget.Start WARNING: No phoneme context component set to object");
		}
		lipsyncContext.Smoothing = SmoothAmount;
	}

	private void Update()
	{
		if (lipsyncContext != null && skinnedMeshRenderer != null)
		{
			OVRLipSync.Frame currentPhonemeFrame = lipsyncContext.GetCurrentPhonemeFrame();
			if (currentPhonemeFrame != null)
			{
				SetVisemeToMorphTarget(currentPhonemeFrame);
			}
			CheckForKeys();
		}
	}

	private void CheckForKeys()
	{
		if (enableVisemeSignals)
		{
			CheckVisemeKey(KeyCode.Alpha1, 0, 100);
			CheckVisemeKey(KeyCode.Alpha2, 1, 100);
			CheckVisemeKey(KeyCode.Alpha3, 2, 100);
			CheckVisemeKey(KeyCode.Alpha4, 3, 100);
			CheckVisemeKey(KeyCode.Alpha5, 4, 100);
			CheckVisemeKey(KeyCode.Alpha6, 5, 100);
			CheckVisemeKey(KeyCode.Alpha7, 6, 100);
			CheckVisemeKey(KeyCode.Alpha8, 7, 100);
			CheckVisemeKey(KeyCode.Alpha9, 8, 100);
			CheckVisemeKey(KeyCode.Alpha0, 9, 100);
			CheckVisemeKey(KeyCode.Q, 10, 100);
			CheckVisemeKey(KeyCode.W, 11, 100);
			CheckVisemeKey(KeyCode.E, 12, 100);
			CheckVisemeKey(KeyCode.R, 13, 100);
			CheckVisemeKey(KeyCode.T, 14, 100);
		}
	}

	private void SetVisemeToMorphTarget(OVRLipSync.Frame frame)
	{
		for (int i = 0; i < VisemeToBlendTargets.Length; i++)
		{
			if (VisemeToBlendTargets[i] != -1)
			{
				skinnedMeshRenderer.SetBlendShapeWeight(VisemeToBlendTargets[i], frame.Visemes[i] * 100f);
			}
		}
	}

	private void CheckVisemeKey(KeyCode key, int viseme, int amount)
	{
		if (Input.GetKeyDown(key))
		{
			lipsyncContext.SetVisemeBlend(KeySendVisemeSignal[viseme], amount);
		}
		if (Input.GetKeyUp(key))
		{
			lipsyncContext.SetVisemeBlend(KeySendVisemeSignal[viseme], 0);
		}
	}
}
