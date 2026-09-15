using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentGUISkin : PersistentScriptableObject
	{
		public long font;

		public PersistentGUIStyle box;

		public PersistentGUIStyle label;

		public PersistentGUIStyle textField;

		public PersistentGUIStyle textArea;

		public PersistentGUIStyle button;

		public PersistentGUIStyle toggle;

		public PersistentGUIStyle window;

		public PersistentGUIStyle horizontalSlider;

		public PersistentGUIStyle horizontalSliderThumb;

		public PersistentGUIStyle verticalSlider;

		public PersistentGUIStyle verticalSliderThumb;

		public PersistentGUIStyle horizontalScrollbar;

		public PersistentGUIStyle horizontalScrollbarThumb;

		public PersistentGUIStyle horizontalScrollbarLeftButton;

		public PersistentGUIStyle horizontalScrollbarRightButton;

		public PersistentGUIStyle verticalScrollbar;

		public PersistentGUIStyle verticalScrollbarThumb;

		public PersistentGUIStyle verticalScrollbarUpButton;

		public PersistentGUIStyle verticalScrollbarDownButton;

		public PersistentGUIStyle scrollView;

		public PersistentGUIStyle[] customStyles;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			GUISkin gUISkin = (GUISkin)obj;
			gUISkin.font = (Font)objects.Get(font);
			gUISkin.box = Write(gUISkin.box, box, objects);
			gUISkin.label = Write(gUISkin.label, label, objects);
			gUISkin.textField = Write(gUISkin.textField, textField, objects);
			gUISkin.textArea = Write(gUISkin.textArea, textArea, objects);
			gUISkin.button = Write(gUISkin.button, button, objects);
			gUISkin.toggle = Write(gUISkin.toggle, toggle, objects);
			gUISkin.window = Write(gUISkin.window, window, objects);
			gUISkin.horizontalSlider = Write(gUISkin.horizontalSlider, horizontalSlider, objects);
			gUISkin.horizontalSliderThumb = Write(gUISkin.horizontalSliderThumb, horizontalSliderThumb, objects);
			gUISkin.verticalSlider = Write(gUISkin.verticalSlider, verticalSlider, objects);
			gUISkin.verticalSliderThumb = Write(gUISkin.verticalSliderThumb, verticalSliderThumb, objects);
			gUISkin.horizontalScrollbar = Write(gUISkin.horizontalScrollbar, horizontalScrollbar, objects);
			gUISkin.horizontalScrollbarThumb = Write(gUISkin.horizontalScrollbarThumb, horizontalScrollbarThumb, objects);
			gUISkin.horizontalScrollbarLeftButton = Write(gUISkin.horizontalScrollbarLeftButton, horizontalScrollbarLeftButton, objects);
			gUISkin.horizontalScrollbarRightButton = Write(gUISkin.horizontalScrollbarRightButton, horizontalScrollbarRightButton, objects);
			gUISkin.verticalScrollbar = Write(gUISkin.verticalScrollbar, verticalScrollbar, objects);
			gUISkin.verticalScrollbarThumb = Write(gUISkin.verticalScrollbarThumb, verticalScrollbarThumb, objects);
			gUISkin.verticalScrollbarUpButton = Write(gUISkin.verticalScrollbarUpButton, verticalScrollbarUpButton, objects);
			gUISkin.verticalScrollbarDownButton = Write(gUISkin.verticalScrollbarDownButton, verticalScrollbarDownButton, objects);
			gUISkin.scrollView = Write(gUISkin.scrollView, scrollView, objects);
			gUISkin.customStyles = Write(gUISkin.customStyles, customStyles, objects);
			return gUISkin;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				GUISkin gUISkin = (GUISkin)obj;
				font = gUISkin.font.GetMappedInstanceID();
				box = Read(box, gUISkin.box);
				label = Read(label, gUISkin.label);
				textField = Read(textField, gUISkin.textField);
				textArea = Read(textArea, gUISkin.textArea);
				button = Read(button, gUISkin.button);
				toggle = Read(toggle, gUISkin.toggle);
				window = Read(window, gUISkin.window);
				horizontalSlider = Read(horizontalSlider, gUISkin.horizontalSlider);
				horizontalSliderThumb = Read(horizontalSliderThumb, gUISkin.horizontalSliderThumb);
				verticalSlider = Read(verticalSlider, gUISkin.verticalSlider);
				verticalSliderThumb = Read(verticalSliderThumb, gUISkin.verticalSliderThumb);
				horizontalScrollbar = Read(horizontalScrollbar, gUISkin.horizontalScrollbar);
				horizontalScrollbarThumb = Read(horizontalScrollbarThumb, gUISkin.horizontalScrollbarThumb);
				horizontalScrollbarLeftButton = Read(horizontalScrollbarLeftButton, gUISkin.horizontalScrollbarLeftButton);
				horizontalScrollbarRightButton = Read(horizontalScrollbarRightButton, gUISkin.horizontalScrollbarRightButton);
				verticalScrollbar = Read(verticalScrollbar, gUISkin.verticalScrollbar);
				verticalScrollbarThumb = Read(verticalScrollbarThumb, gUISkin.verticalScrollbarThumb);
				verticalScrollbarUpButton = Read(verticalScrollbarUpButton, gUISkin.verticalScrollbarUpButton);
				verticalScrollbarDownButton = Read(verticalScrollbarDownButton, gUISkin.verticalScrollbarDownButton);
				scrollView = Read(scrollView, gUISkin.scrollView);
				customStyles = Read(customStyles, gUISkin.customStyles);
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			AddDependency(font, dependencies, objects, allowNulls);
			FindDependencies(box, dependencies, objects, allowNulls);
			FindDependencies(label, dependencies, objects, allowNulls);
			FindDependencies(textField, dependencies, objects, allowNulls);
			FindDependencies(textArea, dependencies, objects, allowNulls);
			FindDependencies(button, dependencies, objects, allowNulls);
			FindDependencies(toggle, dependencies, objects, allowNulls);
			FindDependencies(window, dependencies, objects, allowNulls);
			FindDependencies(horizontalSlider, dependencies, objects, allowNulls);
			FindDependencies(horizontalSliderThumb, dependencies, objects, allowNulls);
			FindDependencies(verticalSlider, dependencies, objects, allowNulls);
			FindDependencies(verticalSliderThumb, dependencies, objects, allowNulls);
			FindDependencies(horizontalScrollbar, dependencies, objects, allowNulls);
			FindDependencies(horizontalScrollbarThumb, dependencies, objects, allowNulls);
			FindDependencies(horizontalScrollbarLeftButton, dependencies, objects, allowNulls);
			FindDependencies(horizontalScrollbarRightButton, dependencies, objects, allowNulls);
			FindDependencies(verticalScrollbar, dependencies, objects, allowNulls);
			FindDependencies(verticalScrollbarThumb, dependencies, objects, allowNulls);
			FindDependencies(verticalScrollbarUpButton, dependencies, objects, allowNulls);
			FindDependencies(verticalScrollbarDownButton, dependencies, objects, allowNulls);
			FindDependencies(scrollView, dependencies, objects, allowNulls);
			FindDependencies(customStyles, dependencies, objects, allowNulls);
		}

		protected override void GetDependencies(Dictionary<long, UnityEngine.Object> dependencies, object obj)
		{
			base.GetDependencies(dependencies, obj);
			if (obj != null)
			{
				GUISkin gUISkin = (GUISkin)obj;
				AddDependency(gUISkin.font, dependencies);
				GetDependencies(box, gUISkin.box, dependencies);
				GetDependencies(label, gUISkin.label, dependencies);
				GetDependencies(textField, gUISkin.textField, dependencies);
				GetDependencies(textArea, gUISkin.textArea, dependencies);
				GetDependencies(button, gUISkin.button, dependencies);
				GetDependencies(toggle, gUISkin.toggle, dependencies);
				GetDependencies(window, gUISkin.window, dependencies);
				GetDependencies(horizontalSlider, gUISkin.horizontalSlider, dependencies);
				GetDependencies(horizontalSliderThumb, gUISkin.horizontalSliderThumb, dependencies);
				GetDependencies(verticalSlider, gUISkin.verticalSlider, dependencies);
				GetDependencies(verticalSliderThumb, gUISkin.verticalSliderThumb, dependencies);
				GetDependencies(horizontalScrollbar, gUISkin.horizontalScrollbar, dependencies);
				GetDependencies(horizontalScrollbarThumb, gUISkin.horizontalScrollbarThumb, dependencies);
				GetDependencies(horizontalScrollbarLeftButton, gUISkin.horizontalScrollbarLeftButton, dependencies);
				GetDependencies(horizontalScrollbarRightButton, gUISkin.horizontalScrollbarRightButton, dependencies);
				GetDependencies(verticalScrollbar, gUISkin.verticalScrollbar, dependencies);
				GetDependencies(verticalScrollbarThumb, gUISkin.verticalScrollbarThumb, dependencies);
				GetDependencies(verticalScrollbarUpButton, gUISkin.verticalScrollbarUpButton, dependencies);
				GetDependencies(verticalScrollbarDownButton, gUISkin.verticalScrollbarDownButton, dependencies);
				GetDependencies(scrollView, gUISkin.scrollView, dependencies);
				GetDependencies(customStyles, gUISkin.customStyles, dependencies);
			}
		}
	}
}
