using System;
using System.Collections.Generic;
using TypeReferences;
using UnityEngine;

[RequireComponent(typeof(UIConnectorMaster))]
public class UIMultiConnector : MonoBehaviour
{
	[Serializable]
	public class Connector
	{
		public Transform receiverTransform;

		public string storeid;

		public JSONStorable receiver;
	}

	public bool altConnector;

	public bool disable;

	public Connector[] connectors;

	[ClassExtends(typeof(JSONStorable))]
	public ClassTypeReference typeToConnect;

	[ClassExtends(typeof(UIProvider))]
	public ClassTypeReference UITypeToConnect;

	public virtual void Connect()
	{
		Component[] componentsInChildren = GetComponentsInChildren(UITypeToConnect.Type);
		List<UIProvider> list = new List<UIProvider>();
		Component[] array = componentsInChildren;
		foreach (Component component in array)
		{
			UIProvider uIProvider = component as UIProvider;
			if (uIProvider != null && uIProvider.completeProvider)
			{
				list.Add(uIProvider);
			}
		}
		if (list.Count > connectors.Length)
		{
			Debug.LogError(string.Concat("Number of complete ", UITypeToConnect.Type, " objects ", list.Count, " is greater than number of connnectors ", connectors.Length));
		}
		for (int j = 0; j < connectors.Length; j++)
		{
			Connector connector = connectors[j];
			if (j >= list.Count)
			{
				continue;
			}
			UIProvider uIProvider2 = list[j];
			if (uIProvider2 != null)
			{
				if (connector.receiver == null && connector.receiverTransform != null)
				{
					JSONStorable[] components = connector.receiverTransform.GetComponents<JSONStorable>();
					JSONStorable[] array2 = components;
					foreach (JSONStorable jSONStorable in array2)
					{
						if (jSONStorable.storeId == connector.storeid)
						{
							connector.receiver = jSONStorable;
						}
					}
				}
				if (connector.receiver != null)
				{
					if (altConnector)
					{
						connector.receiver.SetUIAlt(uIProvider2.transform);
					}
					else
					{
						connector.receiver.SetUI(uIProvider2.transform);
					}
				}
				else
				{
					Debug.LogError("Could not get receiver on connector " + connector.storeid);
				}
			}
			else
			{
				Debug.LogError("UIProvider is null");
			}
		}
	}

	public virtual void Disconnect()
	{
		for (int i = 0; i < connectors.Length; i++)
		{
			Connector connector = connectors[i];
			if (connector.receiver != null)
			{
				if (altConnector)
				{
					connector.receiver.SetUIAlt(null);
				}
				else
				{
					connector.receiver.SetUI(null);
				}
			}
		}
	}

	public virtual void ClearConnectors()
	{
		connectors = new Connector[0];
	}

	public virtual void AddConnector(JSONStorable js)
	{
		bool flag = false;
		if (connectors != null)
		{
			for (int i = 0; i < connectors.Length; i++)
			{
				if (connectors[i].receiver == js)
				{
					flag = true;
					break;
				}
			}
		}
		if (!flag)
		{
			int num = 0;
			num = ((connectors == null) ? 1 : (connectors.Length + 1));
			Connector[] array = new Connector[num];
			for (int j = 0; j < num - 1; j++)
			{
				array[j] = connectors[j];
			}
			Connector connector = new Connector();
			connector.receiverTransform = js.transform;
			connector.receiver = js;
			connector.storeid = js.storeId;
			array[num - 1] = connector;
			connectors = array;
		}
	}

	public virtual void RemoveConnector(JSONStorable js)
	{
		List<Connector> list = new List<Connector>();
		if (connectors == null)
		{
			return;
		}
		bool flag = false;
		for (int i = 0; i < connectors.Length; i++)
		{
			if (connectors[i].receiver == js)
			{
				flag = true;
			}
			else
			{
				list.Add(connectors[i]);
			}
		}
		if (flag)
		{
			connectors = list.ToArray();
		}
	}
}
