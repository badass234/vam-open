using UnityEngine;
using VikingCrewTools.UI;

namespace VikingCrewDevelopment.Demos
{
	public class SayRandomThingsBehaviour : MonoBehaviour
	{
		[Multiline]
		public string[] thingsToSay = new string[1] { "Hello world" };

		[Header("Leave as null if you just want center of character to emit speechbubbles")]
		public Transform mouth;

		public float timeBetweenSpeak = 5f;

		public bool doTalkOnYourOwn = true;

		private float timeToNextSpeak;

		private void Start()
		{
			timeToNextSpeak = timeBetweenSpeak;
		}

		private void Update()
		{
			timeToNextSpeak -= Time.deltaTime;
			if (doTalkOnYourOwn && timeToNextSpeak <= 0f && thingsToSay.Length > 0)
			{
				SaySomething();
			}
		}

		public void SaySomething()
		{
			string message = thingsToSay[Random.Range(0, thingsToSay.Length)];
			SaySomething(message);
		}

		public void SaySomething(string message)
		{
			SaySomething(message, SpeechBubbleManager.Instance.GetRandomSpeechbubbleType());
		}

		public void SaySomething(string message, SpeechBubbleManager.SpeechbubbleType speechbubbleType)
		{
			if (mouth == null)
			{
				SpeechBubbleManager.Instance.AddSpeechBubble(base.transform, message, speechbubbleType);
			}
			else
			{
				SpeechBubbleManager.Instance.AddSpeechBubble(mouth, message, speechbubbleType);
			}
			timeToNextSpeak = timeBetweenSpeak;
		}
	}
}
