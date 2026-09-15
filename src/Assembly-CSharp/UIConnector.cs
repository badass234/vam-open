using UnityEngine;

[RequireComponent(typeof(UIConnectorMaster))]
public class UIConnector : MonoBehaviour
{
	public bool altConnector;

	public bool disable;

	public Transform receiverTransform;

	public string storeid;

	public JSONStorable receiver;

	public virtual void Connect()
	{
		if (receiver == null && receiverTransform != null)
		{
			JSONStorable[] components = receiverTransform.GetComponents<JSONStorable>();
			JSONStorable[] array = components;
			foreach (JSONStorable jSONStorable in array)
			{
				if (jSONStorable.storeId == storeid)
				{
					receiver = jSONStorable;
				}
			}
		}
		if (receiver != null)
		{
			if (altConnector)
			{
				receiver.SetUIAlt(base.transform);
			}
			else
			{
				receiver.SetUI(base.transform);
			}
		}
	}

	public virtual void Disconnect()
	{
		if (receiver != null)
		{
			if (altConnector)
			{
				receiver.SetUIAlt(null);
			}
			else
			{
				receiver.SetUI(null);
			}
		}
	}
}
