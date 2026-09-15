using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using VikingCrewTools.UI;

namespace VikingCrewDevelopment.Demos
{
	public class UISpeechController : MonoBehaviour
	{
		public InputField txtMessage;

		public SayRandomThingsBehaviour talkBehaviour;

		public ToggleGroup toggles;

		private void Start()
		{
		}

		private void Update()
		{
		}

		public void OnTalk()
		{
			talkBehaviour.SaySomething(txtMessage.text, (SpeechBubbleManager.SpeechbubbleType)toggles.ActiveToggles().First().transform.GetSiblingIndex());
		}
	}
}
