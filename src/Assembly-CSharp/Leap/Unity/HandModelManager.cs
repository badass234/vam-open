using System;
using System.Collections;
using System.Collections.Generic;
using Leap.Unity.Attributes;
using UnityEngine;

namespace Leap.Unity
{
	public class HandModelManager : MonoBehaviour
	{
		[Serializable]
		public class ModelGroup
		{
			public string GroupName;

			[HideInInspector]
			public HandModelManager _handModelManager;

			public HandModelBase LeftModel;

			[HideInInspector]
			public bool IsLeftToBeSpawned;

			public HandModelBase RightModel;

			[HideInInspector]
			public bool IsRightToBeSpawned;

			[NonSerialized]
			public List<HandModelBase> modelList = new List<HandModelBase>();

			[NonSerialized]
			public List<HandModelBase> modelsCheckedOut = new List<HandModelBase>();

			public bool IsEnabled = true;

			public bool CanDuplicate;

			public HandModelBase TryGetModel(Chirality chirality, ModelType modelType)
			{
				for (int i = 0; i < modelList.Count; i++)
				{
					if (modelList[i].HandModelType == modelType && modelList[i].Handedness == chirality)
					{
						HandModelBase handModelBase = modelList[i];
						modelList.RemoveAt(i);
						modelsCheckedOut.Add(handModelBase);
						return handModelBase;
					}
				}
				if (CanDuplicate)
				{
					for (int j = 0; j < modelsCheckedOut.Count; j++)
					{
						if (modelsCheckedOut[j].HandModelType == modelType && modelsCheckedOut[j].Handedness == chirality)
						{
							HandModelBase original = modelsCheckedOut[j];
							HandModelBase handModelBase2 = UnityEngine.Object.Instantiate(original);
							handModelBase2.transform.parent = _handModelManager.transform;
							_handModelManager.modelGroupMapping.Add(handModelBase2, this);
							modelsCheckedOut.Add(handModelBase2);
							return handModelBase2;
						}
					}
				}
				return null;
			}

			public void ReturnToGroup(HandModelBase model)
			{
				modelsCheckedOut.Remove(model);
				modelList.Add(model);
				_handModelManager.modelToHandRepMapping.Remove(model);
			}
		}

		protected Dictionary<int, HandRepresentation> graphicsHandReps = new Dictionary<int, HandRepresentation>();

		protected Dictionary<int, HandRepresentation> physicsHandReps = new Dictionary<int, HandRepresentation>();

		protected bool graphicsEnabled = true;

		protected bool physicsEnabled = true;

		[Tooltip("The LeapProvider to use to drive hand representations in the defined model pool groups.")]
		[SerializeField]
		[OnEditorChange("leapProvider")]
		private LeapProvider _leapProvider;

		[SerializeField]
		[Tooltip("To add a new set of Hand Models, first add the Left and Right objects as children of this object. Then create a new Model Pool entry referencing the new Hand Models and configure it as desired. Once configured, the Hand Model Manager object pipes Leap tracking data to the Hand Models as hands are tracked, and spawns duplicates as needed if \"Can Duplicate\" is enabled.")]
		private List<ModelGroup> ModelPool = new List<ModelGroup>();

		private List<HandRepresentation> activeHandReps = new List<HandRepresentation>();

		private Dictionary<HandModelBase, ModelGroup> modelGroupMapping = new Dictionary<HandModelBase, ModelGroup>();

		private Dictionary<HandModelBase, HandRepresentation> modelToHandRepMapping = new Dictionary<HandModelBase, HandRepresentation>();

		public bool GraphicsEnabled
		{
			get
			{
				return graphicsEnabled;
			}
			set
			{
				graphicsEnabled = value;
			}
		}

		public bool PhysicsEnabled
		{
			get
			{
				return physicsEnabled;
			}
			set
			{
				physicsEnabled = value;
			}
		}

		public LeapProvider leapProvider
		{
			get
			{
				return _leapProvider;
			}
			set
			{
				if (_leapProvider != null)
				{
					_leapProvider.OnFixedFrame -= OnFixedFrame;
					_leapProvider.OnUpdateFrame -= OnUpdateFrame;
				}
				_leapProvider = value;
				if (_leapProvider != null)
				{
					_leapProvider.OnFixedFrame += OnFixedFrame;
					_leapProvider.OnUpdateFrame += OnUpdateFrame;
				}
			}
		}

		protected virtual void OnUpdateFrame(Frame frame)
		{
			if (frame != null && graphicsEnabled)
			{
				UpdateHandRepresentations(graphicsHandReps, ModelType.Graphics, frame);
			}
		}

		protected virtual void OnFixedFrame(Frame frame)
		{
			if (frame != null && physicsEnabled)
			{
				UpdateHandRepresentations(physicsHandReps, ModelType.Physics, frame);
			}
		}

		protected virtual void UpdateHandRepresentations(Dictionary<int, HandRepresentation> all_hand_reps, ModelType modelType, Frame frame)
		{
			for (int i = 0; i < frame.Hands.Count; i++)
			{
				Hand hand = frame.Hands[i];
				HandRepresentation value;
				if (!all_hand_reps.TryGetValue(hand.Id, out value))
				{
					value = MakeHandRepresentation(hand, modelType);
					all_hand_reps.Add(hand.Id, value);
				}
				if (value != null)
				{
					value.IsMarked = true;
					value.UpdateRepresentation(hand);
					value.LastUpdatedTime = (int)frame.Timestamp;
				}
			}
			HandRepresentation handRepresentation = null;
			Dictionary<int, HandRepresentation>.Enumerator enumerator = all_hand_reps.GetEnumerator();
			while (enumerator.MoveNext())
			{
				KeyValuePair<int, HandRepresentation> current = enumerator.Current;
				if (current.Value != null)
				{
					if (current.Value.IsMarked)
					{
						current.Value.IsMarked = false;
					}
					else
					{
						handRepresentation = current.Value;
					}
				}
			}
			if (handRepresentation != null)
			{
				all_hand_reps.Remove(handRepresentation.HandID);
				handRepresentation.Finish();
			}
		}

		public void ReturnToPool(HandModelBase model)
		{
			ModelGroup value;
			bool flag = modelGroupMapping.TryGetValue(model, out value);
			for (int i = 0; i < activeHandReps.Count; i++)
			{
				HandRepresentation handRepresentation = activeHandReps[i];
				if (handRepresentation.RepChirality != model.Handedness || handRepresentation.RepType != model.HandModelType)
				{
					continue;
				}
				bool flag2 = false;
				if (handRepresentation.handModels != null)
				{
					for (int j = 0; j < value.modelsCheckedOut.Count; j++)
					{
						HandModelBase handModelBase = value.modelsCheckedOut[j];
						for (int k = 0; k < handRepresentation.handModels.Count; k++)
						{
							if (handRepresentation.handModels[k] == handModelBase)
							{
								flag2 = true;
							}
						}
					}
				}
				if (!flag2)
				{
					handRepresentation.AddModel(model);
					modelToHandRepMapping[model] = handRepresentation;
					return;
				}
			}
			value.ReturnToGroup(model);
		}

		public HandRepresentation MakeHandRepresentation(Hand hand, ModelType modelType)
		{
			Chirality chirality = (hand.IsRight ? Chirality.Right : Chirality.Left);
			HandRepresentation handRepresentation = new HandRepresentation(this, hand, chirality, modelType);
			for (int i = 0; i < ModelPool.Count; i++)
			{
				ModelGroup modelGroup = ModelPool[i];
				if (!modelGroup.IsEnabled)
				{
					continue;
				}
				HandModelBase handModelBase = modelGroup.TryGetModel(chirality, modelType);
				if (handModelBase != null)
				{
					handRepresentation.AddModel(handModelBase);
					if (!modelToHandRepMapping.ContainsKey(handModelBase))
					{
						handModelBase.group = modelGroup;
						modelToHandRepMapping.Add(handModelBase, handRepresentation);
					}
				}
			}
			activeHandReps.Add(handRepresentation);
			return handRepresentation;
		}

		public void RemoveHandRepresentation(HandRepresentation handRepresentation)
		{
			activeHandReps.Remove(handRepresentation);
		}

		protected virtual void OnEnable()
		{
			if (_leapProvider == null)
			{
				_leapProvider = Hands.Provider;
			}
			_leapProvider.OnUpdateFrame -= OnUpdateFrame;
			_leapProvider.OnUpdateFrame += OnUpdateFrame;
			_leapProvider.OnFixedFrame -= OnFixedFrame;
			_leapProvider.OnFixedFrame += OnFixedFrame;
		}

		protected virtual void OnDisable()
		{
			_leapProvider.OnUpdateFrame -= OnUpdateFrame;
			_leapProvider.OnFixedFrame -= OnFixedFrame;
		}

		private void Start()
		{
			for (int i = 0; i < ModelPool.Count; i++)
			{
				InitializeModelGroup(ModelPool[i]);
			}
		}

		private void InitializeModelGroup(ModelGroup collectionGroup)
		{
			if (!modelGroupMapping.ContainsValue(collectionGroup))
			{
				collectionGroup._handModelManager = this;
				HandModelBase handModelBase;
				if (collectionGroup.IsLeftToBeSpawned)
				{
					HandModelBase leftModel = collectionGroup.LeftModel;
					GameObject gameObject = UnityEngine.Object.Instantiate(leftModel.gameObject);
					handModelBase = gameObject.GetComponent<HandModelBase>();
					handModelBase.transform.parent = base.transform;
				}
				else
				{
					handModelBase = collectionGroup.LeftModel;
				}
				if (handModelBase != null)
				{
					collectionGroup.modelList.Add(handModelBase);
					modelGroupMapping.Add(handModelBase, collectionGroup);
				}
				HandModelBase handModelBase2;
				if (collectionGroup.IsRightToBeSpawned)
				{
					HandModelBase rightModel = collectionGroup.RightModel;
					GameObject gameObject2 = UnityEngine.Object.Instantiate(rightModel.gameObject);
					handModelBase2 = gameObject2.GetComponent<HandModelBase>();
					handModelBase2.transform.parent = base.transform;
				}
				else
				{
					handModelBase2 = collectionGroup.RightModel;
				}
				if (handModelBase2 != null)
				{
					collectionGroup.modelList.Add(handModelBase2);
					modelGroupMapping.Add(handModelBase2, collectionGroup);
				}
			}
		}

		public void EnableGroup(string groupName)
		{
			StartCoroutine(enableGroup(groupName));
		}

		private IEnumerator enableGroup(string groupName)
		{
			yield return new WaitForEndOfFrame();
			ModelGroup group = null;
			for (int i = 0; i < ModelPool.Count; i++)
			{
				if (!(ModelPool[i].GroupName == groupName))
				{
					continue;
				}
				group = ModelPool[i];
				for (int j = 0; j < activeHandReps.Count; j++)
				{
					HandRepresentation handRepresentation = activeHandReps[j];
					HandModelBase handModelBase = group.TryGetModel(handRepresentation.RepChirality, handRepresentation.RepType);
					if (handModelBase != null)
					{
						handRepresentation.AddModel(handModelBase);
						modelToHandRepMapping.Add(handModelBase, handRepresentation);
					}
				}
				group.IsEnabled = true;
			}
			if (group == null)
			{
				Debug.LogWarning("A group matching that name does not exisit in the modelPool");
			}
		}

		public void DisableGroup(string groupName)
		{
			StartCoroutine(disableGroup(groupName));
		}

		private IEnumerator disableGroup(string groupName)
		{
			yield return new WaitForEndOfFrame();
			ModelGroup group = null;
			for (int i = 0; i < ModelPool.Count; i++)
			{
				if (!(ModelPool[i].GroupName == groupName))
				{
					continue;
				}
				group = ModelPool[i];
				for (int j = 0; j < group.modelsCheckedOut.Count; j++)
				{
					HandModelBase handModelBase = group.modelsCheckedOut[j];
					HandRepresentation value;
					if (modelToHandRepMapping.TryGetValue(handModelBase, out value))
					{
						value.RemoveModel(handModelBase);
						group.ReturnToGroup(handModelBase);
						j--;
					}
				}
				group.IsEnabled = false;
			}
			if (group == null)
			{
				Debug.LogWarning("A group matching that name does not exisit in the modelPool");
			}
		}

		public void ToggleGroup(string groupName)
		{
			StartCoroutine(toggleGroup(groupName));
		}

		private IEnumerator toggleGroup(string groupName)
		{
			yield return new WaitForEndOfFrame();
			ModelGroup modelGroup = ModelPool.Find((ModelGroup i) => i.GroupName == groupName);
			if (modelGroup != null)
			{
				if (modelGroup.IsEnabled)
				{
					DisableGroup(groupName);
					modelGroup.IsEnabled = false;
				}
				else
				{
					EnableGroup(groupName);
					modelGroup.IsEnabled = true;
				}
			}
			else
			{
				Debug.LogWarning("A group matching that name does not exisit in the modelPool");
			}
		}

		public void AddNewGroup(string groupName, HandModelBase leftModel, HandModelBase rightModel)
		{
			ModelGroup modelGroup = new ModelGroup();
			modelGroup.LeftModel = leftModel;
			modelGroup.RightModel = rightModel;
			modelGroup.GroupName = groupName;
			modelGroup.CanDuplicate = false;
			modelGroup.IsEnabled = true;
			ModelPool.Add(modelGroup);
			InitializeModelGroup(modelGroup);
		}

		private IEnumerator addNewGroupWait(string groupName, HandModelBase leftModel, HandModelBase rightModel)
		{
			yield return new WaitForEndOfFrame();
			AddNewGroup(groupName, leftModel, rightModel);
		}

		public void AddNewGroupWait(string groupName, HandModelBase leftModel, HandModelBase rightModel)
		{
			StartCoroutine(addNewGroupWait(groupName, leftModel, rightModel));
		}

		public void RemoveGroup(string groupName)
		{
			while (ModelPool.Find((ModelGroup i) => i.GroupName == groupName) != null)
			{
				ModelGroup modelGroup = ModelPool.Find((ModelGroup i) => i.GroupName == groupName);
				if (modelGroup == null)
				{
					continue;
				}
				ModelPool.Remove(modelGroup);
				foreach (HandModelBase model in modelGroup.modelList)
				{
					modelGroupMapping.Remove(model);
				}
			}
		}

		private IEnumerator removeGroupWait(string groupName)
		{
			yield return new WaitForEndOfFrame();
			RemoveGroup(groupName);
		}

		public void RemoveGroupWait(string groupName)
		{
			StartCoroutine(removeGroupWait(groupName));
		}

		public T GetHandModel<T>(int handId) where T : HandModelBase
		{
			foreach (ModelGroup item in ModelPool)
			{
				foreach (HandModelBase item2 in item.modelsCheckedOut)
				{
					if (item2.GetLeapHand().Id == handId && item2 is T)
					{
						return item2 as T;
					}
				}
			}
			return (T)null;
		}
	}
}
