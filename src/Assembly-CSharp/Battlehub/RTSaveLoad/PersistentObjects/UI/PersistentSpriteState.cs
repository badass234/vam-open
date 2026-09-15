using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTSaveLoad.PersistentObjects.UI
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentSpriteState : PersistentData
	{
		public long highlightedSprite;

		public long pressedSprite;

		public long disabledSprite;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			SpriteState spriteState = (SpriteState)obj;
			spriteState.highlightedSprite = (Sprite)objects.Get(highlightedSprite);
			spriteState.pressedSprite = (Sprite)objects.Get(pressedSprite);
			spriteState.disabledSprite = (Sprite)objects.Get(disabledSprite);
			return spriteState;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				SpriteState spriteState = (SpriteState)obj;
				highlightedSprite = spriteState.highlightedSprite.GetMappedInstanceID();
				pressedSprite = spriteState.pressedSprite.GetMappedInstanceID();
				disabledSprite = spriteState.disabledSprite.GetMappedInstanceID();
			}
		}
	}
}
