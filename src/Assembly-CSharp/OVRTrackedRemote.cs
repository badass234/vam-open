using UnityEngine;

public class OVRTrackedRemote : MonoBehaviour
{
	public GameObject m_modelGearVrController;

	public GameObject m_modelOculusGoController;

	public OVRInput.Controller m_controller;

	private bool m_isOculusGo;

	private bool m_prevControllerConnected;

	private bool m_prevControllerConnectedCached;

	private void Start()
	{
		m_isOculusGo = OVRPlugin.productName == "Oculus Go";
	}

	private void Update()
	{
		bool flag = OVRInput.IsControllerConnected(m_controller);
		if (flag != m_prevControllerConnected || !m_prevControllerConnectedCached)
		{
			m_modelOculusGoController.SetActive(flag && m_isOculusGo);
			m_modelGearVrController.SetActive(flag && !m_isOculusGo);
			m_prevControllerConnected = flag;
			m_prevControllerConnectedCached = true;
		}
		if (flag)
		{
		}
	}
}
