using UnityEngine;

namespace MeshVR.Hands
{
	public class HandOutput : JSONStorable
	{
		public enum Hand
		{
			Left,
			Right
		}

		public JSONStorableBool inputConnectedJSON;

		[SerializeField]
		protected bool _inputConnected = true;

		public JSONStorableBool outputConnectedJSON;

		[SerializeField]
		protected bool _outputConnected = true;

		public Hand hand;

		public FingerOutput thumbProximal;

		public JSONStorableFloat thumbProximalBendJSON;

		public JSONStorableFloat thumbProximalSpreadJSON;

		public JSONStorableFloat thumbProximalTwistJSON;

		public FingerOutput thumbMiddle;

		public JSONStorableFloat thumbMiddleBendJSON;

		public FingerOutput thumbDistal;

		public JSONStorableFloat thumbDistalBendJSON;

		public FingerOutput indexProximal;

		public JSONStorableFloat indexProximalBendJSON;

		public JSONStorableFloat indexProximalSpreadJSON;

		public JSONStorableFloat indexProximalTwistJSON;

		public FingerOutput indexMiddle;

		public JSONStorableFloat indexMiddleBendJSON;

		public FingerOutput indexDistal;

		public JSONStorableFloat indexDistalBendJSON;

		public FingerOutput middleProximal;

		public JSONStorableFloat middleProximalBendJSON;

		public JSONStorableFloat middleProximalSpreadJSON;

		public JSONStorableFloat middleProximalTwistJSON;

		public FingerOutput middleMiddle;

		public JSONStorableFloat middleMiddleBendJSON;

		public FingerOutput middleDistal;

		public JSONStorableFloat middleDistalBendJSON;

		public FingerOutput ringProximal;

		public JSONStorableFloat ringProximalBendJSON;

		public JSONStorableFloat ringProximalSpreadJSON;

		public JSONStorableFloat ringProximalTwistJSON;

		public FingerOutput ringMiddle;

		public JSONStorableFloat ringMiddleBendJSON;

		public FingerOutput ringDistal;

		public JSONStorableFloat ringDistalBendJSON;

		public FingerOutput pinkyProximal;

		public JSONStorableFloat pinkyProximalBendJSON;

		public JSONStorableFloat pinkyProximalSpreadJSON;

		public JSONStorableFloat pinkyProximalTwistJSON;

		public FingerOutput pinkyMiddle;

		public JSONStorableFloat pinkyMiddleBendJSON;

		public FingerOutput pinkyDistal;

		public JSONStorableFloat pinkyDistalBendJSON;

		public bool inputConnected
		{
			get
			{
				if (inputConnectedJSON != null)
				{
					return inputConnectedJSON.val;
				}
				return _inputConnected;
			}
			set
			{
				if (inputConnectedJSON != null)
				{
					inputConnectedJSON.val = value;
				}
				else
				{
					_inputConnected = value;
				}
			}
		}

		public bool outputConnected
		{
			get
			{
				if (outputConnectedJSON != null)
				{
					return outputConnectedJSON.val;
				}
				return _outputConnected;
			}
			set
			{
				if (outputConnectedJSON != null)
				{
					outputConnectedJSON.val = value;
				}
				else
				{
					_outputConnected = value;
				}
			}
		}

		protected void SetInputConnected(bool b)
		{
			_inputConnected = b;
		}

		protected void SetOutputConnected(bool b)
		{
			_outputConnected = b;
			if (_outputConnected)
			{
				SyncAllOutputs();
			}
		}

		protected void SetThumbProximalBend(float f)
		{
			if (thumbProximal != null && _outputConnected)
			{
				thumbProximal.currentBend = f;
				thumbProximal.UpdateOutput();
			}
		}

		protected void SetThumbProximalSpread(float f)
		{
			if (thumbProximal != null && _outputConnected)
			{
				thumbProximal.currentSpread = f;
				thumbProximal.UpdateOutput();
			}
		}

		protected void SetThumbProximalTwist(float f)
		{
			if (thumbProximal != null && _outputConnected)
			{
				thumbProximal.currentTwist = f;
				thumbProximal.UpdateOutput();
			}
		}

		protected void SetThumbMiddleBend(float f)
		{
			if (thumbMiddle != null && _outputConnected)
			{
				thumbMiddle.currentBend = f;
				thumbMiddle.UpdateOutput();
			}
		}

		protected void SetThumbDistalBend(float f)
		{
			if (thumbDistal != null && _outputConnected)
			{
				thumbDistal.currentBend = f;
				thumbDistal.UpdateOutput();
			}
		}

		protected void SetIndexProximalBend(float f)
		{
			if (indexProximal != null && _outputConnected)
			{
				indexProximal.currentBend = f;
				indexProximal.UpdateOutput();
			}
		}

		protected void SetIndexProximalSpread(float f)
		{
			if (indexProximal != null && _outputConnected)
			{
				indexProximal.currentSpread = f;
				indexProximal.UpdateOutput();
			}
		}

		protected void SetIndexProximalTwist(float f)
		{
			if (indexProximal != null && _outputConnected)
			{
				indexProximal.currentTwist = f;
				indexProximal.UpdateOutput();
			}
		}

		protected void SetIndexMiddleBend(float f)
		{
			if (indexMiddle != null && _outputConnected)
			{
				indexMiddle.currentBend = f;
				indexMiddle.UpdateOutput();
			}
		}

		protected void SetIndexDistalBend(float f)
		{
			if (indexDistal != null && _outputConnected)
			{
				indexDistal.currentBend = f;
				indexDistal.UpdateOutput();
			}
		}

		protected void SetMiddleProximalBend(float f)
		{
			if (middleProximal != null && _outputConnected)
			{
				middleProximal.currentBend = f;
				middleProximal.UpdateOutput();
			}
		}

		protected void SetMiddleProximalSpread(float f)
		{
			if (middleProximal != null && _outputConnected)
			{
				middleProximal.currentSpread = f;
				middleProximal.UpdateOutput();
			}
		}

		protected void SetMiddleProximalTwist(float f)
		{
			if (middleProximal != null && _outputConnected)
			{
				middleProximal.currentTwist = f;
				middleProximal.UpdateOutput();
			}
		}

		protected void SetMiddleMiddleBend(float f)
		{
			if (middleMiddle != null && _outputConnected)
			{
				middleMiddle.currentBend = f;
				middleMiddle.UpdateOutput();
			}
		}

		protected void SetMiddleDistalBend(float f)
		{
			if (middleDistal != null && _outputConnected)
			{
				middleDistal.currentBend = f;
				middleDistal.UpdateOutput();
			}
		}

		protected void SetRingProximalBend(float f)
		{
			if (ringProximal != null && _outputConnected)
			{
				ringProximal.currentBend = f;
				ringProximal.UpdateOutput();
			}
		}

		protected void SetRingProximalSpread(float f)
		{
			if (ringProximal != null && _outputConnected)
			{
				ringProximal.currentSpread = f;
				ringProximal.UpdateOutput();
			}
		}

		protected void SetRingProximalTwist(float f)
		{
			if (ringProximal != null && _outputConnected)
			{
				ringProximal.currentTwist = f;
				ringProximal.UpdateOutput();
			}
		}

		protected void SetRingMiddleBend(float f)
		{
			if (ringMiddle != null && _outputConnected)
			{
				ringMiddle.currentBend = f;
				ringMiddle.UpdateOutput();
			}
		}

		protected void SetRingDistalBend(float f)
		{
			if (ringDistal != null && _outputConnected)
			{
				ringDistal.currentBend = f;
				ringDistal.UpdateOutput();
			}
		}

		protected void SetPinkyProximalBend(float f)
		{
			if (pinkyProximal != null && _outputConnected)
			{
				pinkyProximal.currentBend = f;
				pinkyProximal.UpdateOutput();
			}
		}

		protected void SetPinkyProximalSpread(float f)
		{
			if (pinkyProximal != null && _outputConnected)
			{
				pinkyProximal.currentSpread = f;
				pinkyProximal.UpdateOutput();
			}
		}

		protected void SetPinkyProximalTwist(float f)
		{
			if (pinkyProximal != null && _outputConnected)
			{
				pinkyProximal.currentTwist = f;
				pinkyProximal.UpdateOutput();
			}
		}

		protected void SetPinkyMiddleBend(float f)
		{
			if (pinkyMiddle != null && _outputConnected)
			{
				pinkyMiddle.currentBend = f;
				pinkyMiddle.UpdateOutput();
			}
		}

		protected void SetPinkyDistalBend(float f)
		{
			if (pinkyDistal != null && _outputConnected)
			{
				pinkyDistal.currentBend = f;
				pinkyDistal.UpdateOutput();
			}
		}

		protected void SyncAllOutputs()
		{
			SetThumbProximalBend(thumbProximalBendJSON.val);
			SetThumbProximalSpread(thumbProximalSpreadJSON.val);
			SetThumbProximalTwist(thumbProximalTwistJSON.val);
			SetThumbMiddleBend(thumbMiddleBendJSON.val);
			SetThumbDistalBend(thumbDistalBendJSON.val);
			SetIndexProximalBend(indexProximalBendJSON.val);
			SetIndexProximalSpread(indexProximalSpreadJSON.val);
			SetIndexProximalTwist(indexProximalTwistJSON.val);
			SetIndexMiddleBend(indexMiddleBendJSON.val);
			SetIndexDistalBend(indexDistalBendJSON.val);
			SetMiddleProximalBend(middleProximalBendJSON.val);
			SetMiddleProximalSpread(middleProximalSpreadJSON.val);
			SetMiddleProximalTwist(middleProximalTwistJSON.val);
			SetMiddleMiddleBend(middleMiddleBendJSON.val);
			SetMiddleDistalBend(middleDistalBendJSON.val);
			SetRingProximalBend(ringProximalBendJSON.val);
			SetRingProximalSpread(ringProximalSpreadJSON.val);
			SetRingProximalTwist(ringProximalTwistJSON.val);
			SetRingMiddleBend(ringMiddleBendJSON.val);
			SetRingDistalBend(ringDistalBendJSON.val);
			SetPinkyProximalBend(pinkyProximalBendJSON.val);
			SetPinkyProximalSpread(pinkyProximalSpreadJSON.val);
			SetPinkyProximalTwist(pinkyProximalTwistJSON.val);
			SetPinkyMiddleBend(pinkyMiddleBendJSON.val);
			SetPinkyDistalBend(pinkyDistalBendJSON.val);
		}

		protected virtual void Init()
		{
			inputConnectedJSON = new JSONStorableBool("inputConnected", _inputConnected, SetInputConnected);
			outputConnectedJSON = new JSONStorableBool("outputConnected", _outputConnected, SetOutputConnected);
			thumbProximalBendJSON = new JSONStorableFloat("thumbProximalBend", 0f, SetThumbProximalBend, -100f, 100f);
			thumbProximalBendJSON.storeType = JSONStorableParam.StoreType.Physical;
			RegisterFloat(thumbProximalBendJSON);
			thumbProximalSpreadJSON = new JSONStorableFloat("thumbProximalSpread", 0f, SetThumbProximalSpread, -100f, 100f);
			thumbProximalSpreadJSON.storeType = JSONStorableParam.StoreType.Physical;
			RegisterFloat(thumbProximalSpreadJSON);
			thumbProximalTwistJSON = new JSONStorableFloat("thumbProximalTwist", 0f, SetThumbProximalTwist, -100f, 100f);
			thumbProximalTwistJSON.storeType = JSONStorableParam.StoreType.Physical;
			RegisterFloat(thumbProximalTwistJSON);
			thumbMiddleBendJSON = new JSONStorableFloat("thumbMiddleBend", 0f, SetThumbMiddleBend, -100f, 100f);
			thumbMiddleBendJSON.storeType = JSONStorableParam.StoreType.Physical;
			RegisterFloat(thumbMiddleBendJSON);
			thumbDistalBendJSON = new JSONStorableFloat("thumbDistalBend", 0f, SetThumbDistalBend, -100f, 100f);
			thumbDistalBendJSON.storeType = JSONStorableParam.StoreType.Physical;
			RegisterFloat(thumbDistalBendJSON);
			indexProximalBendJSON = new JSONStorableFloat("indexProximalBend", 0f, SetIndexProximalBend, -100f, 100f);
			indexProximalBendJSON.storeType = JSONStorableParam.StoreType.Physical;
			RegisterFloat(indexProximalBendJSON);
			indexProximalSpreadJSON = new JSONStorableFloat("indexProximalSpread", 0f, SetIndexProximalSpread, -100f, 100f);
			indexProximalSpreadJSON.storeType = JSONStorableParam.StoreType.Physical;
			RegisterFloat(indexProximalSpreadJSON);
			indexProximalTwistJSON = new JSONStorableFloat("indexProximalTwist", 0f, SetIndexProximalTwist, -100f, 100f);
			indexProximalTwistJSON.storeType = JSONStorableParam.StoreType.Physical;
			RegisterFloat(indexProximalTwistJSON);
			indexMiddleBendJSON = new JSONStorableFloat("indexMiddleBend", 0f, SetIndexMiddleBend, -100f, 100f);
			indexMiddleBendJSON.storeType = JSONStorableParam.StoreType.Physical;
			RegisterFloat(indexMiddleBendJSON);
			indexDistalBendJSON = new JSONStorableFloat("indexDistalBend", 0f, SetIndexDistalBend, -100f, 100f);
			indexDistalBendJSON.storeType = JSONStorableParam.StoreType.Physical;
			RegisterFloat(indexDistalBendJSON);
			middleProximalBendJSON = new JSONStorableFloat("middleProximalBend", 0f, SetMiddleProximalBend, -100f, 100f);
			middleProximalBendJSON.storeType = JSONStorableParam.StoreType.Physical;
			RegisterFloat(middleProximalBendJSON);
			middleProximalSpreadJSON = new JSONStorableFloat("middleProximalSpread", 0f, SetMiddleProximalSpread, -100f, 100f);
			middleProximalSpreadJSON.storeType = JSONStorableParam.StoreType.Physical;
			RegisterFloat(middleProximalSpreadJSON);
			middleProximalTwistJSON = new JSONStorableFloat("middleProximalTwist", 0f, SetMiddleProximalTwist, -100f, 100f);
			middleProximalTwistJSON.storeType = JSONStorableParam.StoreType.Physical;
			RegisterFloat(middleProximalTwistJSON);
			middleMiddleBendJSON = new JSONStorableFloat("middleMiddleBend", 0f, SetMiddleMiddleBend, -100f, 100f);
			middleMiddleBendJSON.storeType = JSONStorableParam.StoreType.Physical;
			RegisterFloat(middleMiddleBendJSON);
			middleDistalBendJSON = new JSONStorableFloat("middleDistalBend", 0f, SetMiddleDistalBend, -100f, 100f);
			middleDistalBendJSON.storeType = JSONStorableParam.StoreType.Physical;
			RegisterFloat(middleDistalBendJSON);
			ringProximalBendJSON = new JSONStorableFloat("ringProximalBend", 0f, SetRingProximalBend, -100f, 100f);
			ringProximalBendJSON.storeType = JSONStorableParam.StoreType.Physical;
			RegisterFloat(ringProximalBendJSON);
			ringProximalSpreadJSON = new JSONStorableFloat("ringProximalSpread", 0f, SetRingProximalSpread, -100f, 100f);
			ringProximalSpreadJSON.storeType = JSONStorableParam.StoreType.Physical;
			RegisterFloat(ringProximalSpreadJSON);
			ringProximalTwistJSON = new JSONStorableFloat("ringProximalTwist", 0f, SetRingProximalTwist, -100f, 100f);
			ringProximalTwistJSON.storeType = JSONStorableParam.StoreType.Physical;
			RegisterFloat(ringProximalTwistJSON);
			ringMiddleBendJSON = new JSONStorableFloat("ringMiddleBend", 0f, SetRingMiddleBend, -100f, 100f);
			ringMiddleBendJSON.storeType = JSONStorableParam.StoreType.Physical;
			RegisterFloat(ringMiddleBendJSON);
			ringDistalBendJSON = new JSONStorableFloat("ringDistalBend", 0f, SetRingDistalBend, -100f, 100f);
			ringDistalBendJSON.storeType = JSONStorableParam.StoreType.Physical;
			RegisterFloat(ringDistalBendJSON);
			pinkyProximalBendJSON = new JSONStorableFloat("pinkyProximalBend", 0f, SetPinkyProximalBend, -100f, 100f);
			pinkyProximalBendJSON.storeType = JSONStorableParam.StoreType.Physical;
			RegisterFloat(pinkyProximalBendJSON);
			pinkyProximalSpreadJSON = new JSONStorableFloat("pinkyProximalSpread", 0f, SetPinkyProximalSpread, -100f, 100f);
			pinkyProximalSpreadJSON.storeType = JSONStorableParam.StoreType.Physical;
			RegisterFloat(pinkyProximalSpreadJSON);
			pinkyProximalTwistJSON = new JSONStorableFloat("pinkyProximalTwist", 0f, SetPinkyProximalTwist, -100f, 100f);
			pinkyProximalTwistJSON.storeType = JSONStorableParam.StoreType.Physical;
			RegisterFloat(pinkyProximalTwistJSON);
			pinkyMiddleBendJSON = new JSONStorableFloat("pinkyMiddleBend", 0f, SetPinkyMiddleBend, -100f, 100f);
			pinkyMiddleBendJSON.storeType = JSONStorableParam.StoreType.Physical;
			RegisterFloat(pinkyMiddleBendJSON);
			pinkyDistalBendJSON = new JSONStorableFloat("pinkyDistalBend", 0f, SetPinkyDistalBend, -100f, 100f);
			pinkyDistalBendJSON.storeType = JSONStorableParam.StoreType.Physical;
			RegisterFloat(pinkyDistalBendJSON);
		}

		protected override void InitUI(Transform t, bool isAlt)
		{
			if (!(t != null))
			{
			}
		}

		protected override void Awake()
		{
			if (!awakecalled)
			{
				base.Awake();
				Init();
				InitUI();
				InitUIAlt();
			}
		}

		protected void Update()
		{
			if (_inputConnected)
			{
				if (hand == Hand.Left)
				{
					thumbProximalBendJSON.val = HandInput.leftThumbProximalBend;
					thumbProximalSpreadJSON.val = HandInput.leftThumbProximalSpread;
					thumbProximalTwistJSON.val = HandInput.leftThumbProximalTwist;
					thumbMiddleBendJSON.val = HandInput.leftThumbMiddleBend;
					thumbDistalBendJSON.val = HandInput.leftThumbDistalBend;
					indexProximalBendJSON.val = HandInput.leftIndexProximalBend;
					indexProximalSpreadJSON.val = HandInput.leftIndexProximalSpread;
					indexProximalTwistJSON.val = HandInput.leftIndexProximalTwist;
					indexMiddleBendJSON.val = HandInput.leftIndexMiddleBend;
					indexDistalBendJSON.val = HandInput.leftIndexDistalBend;
					middleProximalBendJSON.val = HandInput.leftMiddleProximalBend;
					middleProximalSpreadJSON.val = HandInput.leftMiddleProximalSpread;
					middleProximalTwistJSON.val = HandInput.leftMiddleProximalTwist;
					middleMiddleBendJSON.val = HandInput.leftMiddleMiddleBend;
					middleDistalBendJSON.val = HandInput.leftMiddleDistalBend;
					ringProximalBendJSON.val = HandInput.leftRingProximalBend;
					ringProximalSpreadJSON.val = HandInput.leftRingProximalSpread;
					ringProximalTwistJSON.val = HandInput.leftRingProximalTwist;
					ringMiddleBendJSON.val = HandInput.leftRingMiddleBend;
					ringDistalBendJSON.val = HandInput.leftRingDistalBend;
					pinkyProximalBendJSON.val = HandInput.leftPinkyProximalBend;
					pinkyProximalSpreadJSON.val = HandInput.leftPinkyProximalSpread;
					pinkyProximalTwistJSON.val = HandInput.leftPinkyProximalTwist;
					pinkyMiddleBendJSON.val = HandInput.leftPinkyMiddleBend;
					pinkyDistalBendJSON.val = HandInput.leftPinkyDistalBend;
				}
				else
				{
					thumbProximalBendJSON.val = HandInput.rightThumbProximalBend;
					thumbProximalSpreadJSON.val = HandInput.rightThumbProximalSpread;
					thumbProximalTwistJSON.val = HandInput.rightThumbProximalTwist;
					thumbMiddleBendJSON.val = HandInput.rightThumbMiddleBend;
					thumbDistalBendJSON.val = HandInput.rightThumbDistalBend;
					indexProximalBendJSON.val = HandInput.rightIndexProximalBend;
					indexProximalSpreadJSON.val = HandInput.rightIndexProximalSpread;
					indexProximalTwistJSON.val = HandInput.rightIndexProximalTwist;
					indexMiddleBendJSON.val = HandInput.rightIndexMiddleBend;
					indexDistalBendJSON.val = HandInput.rightIndexDistalBend;
					middleProximalBendJSON.val = HandInput.rightMiddleProximalBend;
					middleProximalSpreadJSON.val = HandInput.rightMiddleProximalSpread;
					middleProximalTwistJSON.val = HandInput.rightMiddleProximalTwist;
					middleMiddleBendJSON.val = HandInput.rightMiddleMiddleBend;
					middleDistalBendJSON.val = HandInput.rightMiddleDistalBend;
					ringProximalBendJSON.val = HandInput.rightRingProximalBend;
					ringProximalSpreadJSON.val = HandInput.rightRingProximalSpread;
					ringProximalTwistJSON.val = HandInput.rightRingProximalTwist;
					ringMiddleBendJSON.val = HandInput.rightRingMiddleBend;
					ringDistalBendJSON.val = HandInput.rightRingDistalBend;
					pinkyProximalBendJSON.val = HandInput.rightPinkyProximalBend;
					pinkyProximalSpreadJSON.val = HandInput.rightPinkyProximalSpread;
					pinkyProximalTwistJSON.val = HandInput.rightPinkyProximalTwist;
					pinkyMiddleBendJSON.val = HandInput.rightPinkyMiddleBend;
					pinkyDistalBendJSON.val = HandInput.rightPinkyDistalBend;
				}
			}
		}
	}
}
