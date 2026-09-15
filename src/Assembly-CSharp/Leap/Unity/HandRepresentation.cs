using System.Collections.Generic;

namespace Leap.Unity
{
	public class HandRepresentation
	{
		private HandModelManager parent;

		public List<HandModelBase> handModels;

		public int HandID { get; private set; }

		public int LastUpdatedTime { get; set; }

		public bool IsMarked { get; set; }

		public Chirality RepChirality { get; protected set; }

		public ModelType RepType { get; protected set; }

		public Hand MostRecentHand { get; protected set; }

		public HandRepresentation(HandModelManager parent, Hand hand, Chirality repChirality, ModelType repType)
		{
			this.parent = parent;
			HandID = hand.Id;
			RepChirality = repChirality;
			RepType = repType;
			MostRecentHand = hand;
		}

		public void Finish()
		{
			if (handModels != null)
			{
				for (int i = 0; i < handModels.Count; i++)
				{
					handModels[i].FinishHand();
					parent.ReturnToPool(handModels[i]);
					handModels[i] = null;
				}
			}
			parent.RemoveHandRepresentation(this);
		}

		public void AddModel(HandModelBase model)
		{
			if (handModels == null)
			{
				handModels = new List<HandModelBase>();
			}
			handModels.Add(model);
			if (model.GetLeapHand() == null)
			{
				model.SetLeapHand(MostRecentHand);
				model.InitHand();
				model.BeginHand();
				model.UpdateHand();
			}
			else
			{
				model.SetLeapHand(MostRecentHand);
				model.BeginHand();
			}
		}

		public void RemoveModel(HandModelBase model)
		{
			if (handModels != null)
			{
				model.FinishHand();
				handModels.Remove(model);
			}
		}

		public void UpdateRepresentation(Hand hand)
		{
			MostRecentHand = hand;
			if (handModels != null)
			{
				for (int i = 0; i < handModels.Count; i++)
				{
					handModels[i].SetLeapHand(hand);
					handModels[i].UpdateHand();
				}
			}
		}
	}
}
