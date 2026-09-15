using System;
using UnityEngine;

namespace ZenFulcrum.EmbeddedBrowser
{
	public class Door : MonoBehaviour
	{
		public enum OpenState
		{
			Open,
			Closed,
			Opening,
			Closing
		}

		public Vector3 openOffset = new Vector3(0f, -6.1f, 0f);

		[Tooltip("Time to open or close, in seconds.")]
		public float openSpeed = 2f;

		[Tooltip("Number of coins needed to open the door.")]
		public int numCoins;

		private Vector3 closedPos;

		private Vector3 openPos;

		private OpenState _state;

		public OpenState State
		{
			get
			{
				return _state;
			}
			set
			{
				_state = value;
				this.stateChange(_state);
			}
		}

		public event Action<OpenState> stateChange = delegate
		{
		};

		public void Start()
		{
			closedPos = base.transform.position;
			openPos = base.transform.position + openOffset;
			State = OpenState.Closed;
			Browser browser = GetComponentInChildren<Browser>();
			browser.CallFunction("setRequiredCoins", numCoins);
			browser.RegisterFunction("toggleDoor", delegate(JSONNode args)
			{
				switch ((string)args[0].Check())
				{
				case "open":
					Open();
					break;
				case "close":
					Close();
					break;
				case "toggle":
					Toggle();
					break;
				}
			});
			PlayerInventory.Instance.coinCollected += delegate(int coinCount)
			{
				browser.CallFunction("setCoinCoint", coinCount);
			};
		}

		public void Toggle()
		{
			if (State == OpenState.Open || State == OpenState.Opening)
			{
				Close();
			}
			else
			{
				Open();
			}
		}

		public void Open()
		{
			if (State != 0)
			{
				State = OpenState.Opening;
			}
		}

		public void Close()
		{
			if (State != OpenState.Closed)
			{
				State = OpenState.Closing;
			}
		}

		public void Update()
		{
			if (State == OpenState.Opening)
			{
				float num = Vector3.Distance(base.transform.position, closedPos) / openOffset.magnitude;
				num = Mathf.Min(1f, num + Time.deltaTime / openSpeed);
				base.transform.position = Vector3.Lerp(closedPos, openPos, num);
				if (num >= 1f)
				{
					State = OpenState.Open;
				}
			}
			else if (State == OpenState.Closing)
			{
				float num2 = Vector3.Distance(base.transform.position, openPos) / openOffset.magnitude;
				num2 = Mathf.Min(1f, num2 + Time.deltaTime / openSpeed);
				base.transform.position = Vector3.Lerp(openPos, closedPos, num2);
				if (num2 >= 1f)
				{
					State = OpenState.Closed;
				}
			}
		}
	}
}
