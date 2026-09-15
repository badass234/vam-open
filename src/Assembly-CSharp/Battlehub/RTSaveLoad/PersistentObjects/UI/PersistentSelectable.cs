using System;
using System.Collections.Generic;
using Battlehub.RTSaveLoad.PersistentObjects.EventSystems;
using ProtoBuf;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTSaveLoad.PersistentObjects.UI
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	[ProtoInclude(1131, typeof(PersistentButton))]
	[ProtoInclude(1132, typeof(PersistentDropdown))]
	[ProtoInclude(1133, typeof(PersistentInputField))]
	[ProtoInclude(1134, typeof(PersistentScrollbar))]
	[ProtoInclude(1135, typeof(PersistentSlider))]
	[ProtoInclude(1136, typeof(PersistentToggle))]
	public class PersistentSelectable : PersistentUIBehaviour
	{
		public PersistentNavigation navigation;

		public Selectable.Transition transition;

		public ColorBlock colors;

		public PersistentSpriteState spriteState;

		public AnimationTriggers animationTriggers;

		public long targetGraphic;

		public bool interactable;

		public long image;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			Selectable selectable = (Selectable)obj;
			selectable.navigation = default(Navigation);
			navigation.WriteTo(selectable.navigation, objects);
			selectable.transition = transition;
			selectable.colors = colors;
			selectable.spriteState = default(SpriteState);
			spriteState.WriteTo(selectable.spriteState, objects);
			selectable.animationTriggers = animationTriggers;
			selectable.targetGraphic = (Graphic)objects.Get(targetGraphic);
			selectable.interactable = interactable;
			selectable.image = (Image)objects.Get(image);
			return selectable;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				Selectable selectable = (Selectable)obj;
				navigation = new PersistentNavigation();
				navigation.ReadFrom(selectable.navigation);
				transition = selectable.transition;
				colors = selectable.colors;
				spriteState = new PersistentSpriteState();
				spriteState.ReadFrom(selectable.spriteState);
				animationTriggers = selectable.animationTriggers;
				targetGraphic = selectable.targetGraphic.GetMappedInstanceID();
				interactable = selectable.interactable;
				image = selectable.image.GetMappedInstanceID();
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			AddDependency(targetGraphic, dependencies, objects, allowNulls);
			AddDependency(image, dependencies, objects, allowNulls);
			if (navigation != null)
			{
				navigation.FindDependencies(dependencies, objects, allowNulls);
			}
			if (spriteState != null)
			{
				spriteState.FindDependencies(dependencies, objects, allowNulls);
			}
		}

		protected override void GetDependencies(Dictionary<long, UnityEngine.Object> dependencies, object obj)
		{
			base.GetDependencies(dependencies, obj);
			if (obj != null)
			{
				Selectable selectable = (Selectable)obj;
				AddDependency(selectable.targetGraphic, dependencies);
				AddDependency(selectable.image, dependencies);
				PersistentNavigation persistentNavigation = new PersistentNavigation();
				persistentNavigation.GetDependencies(selectable.navigation, dependencies);
				PersistentSpriteState persistentSpriteState = new PersistentSpriteState();
				persistentSpriteState.GetDependencies(selectable.spriteState, dependencies);
			}
		}
	}
}
