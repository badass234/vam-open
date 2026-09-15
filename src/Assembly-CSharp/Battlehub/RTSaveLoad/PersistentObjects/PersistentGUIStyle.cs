using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentGUIStyle : PersistentData
	{
		public PersistentGUIStyleState normal;

		public PersistentGUIStyleState hover;

		public PersistentGUIStyleState active;

		public PersistentGUIStyleState onNormal;

		public PersistentGUIStyleState onHover;

		public PersistentGUIStyleState onActive;

		public PersistentGUIStyleState focused;

		public PersistentGUIStyleState onFocused;

		public RectOffset border;

		public RectOffset margin;

		public RectOffset padding;

		public RectOffset overflow;

		public long font;

		public string name;

		public uint imagePosition;

		public uint alignment;

		public bool wordWrap;

		public uint clipping;

		public Vector2 contentOffset;

		public float fixedWidth;

		public float fixedHeight;

		public bool stretchWidth;

		public bool stretchHeight;

		public int fontSize;

		public uint fontStyle;

		public bool richText;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			GUIStyle gUIStyle = (GUIStyle)obj;
			gUIStyle.normal = Write(gUIStyle.normal, normal, objects);
			gUIStyle.hover = Write(gUIStyle.hover, hover, objects);
			gUIStyle.active = Write(gUIStyle.active, active, objects);
			gUIStyle.onNormal = Write(gUIStyle.onNormal, onNormal, objects);
			gUIStyle.onHover = Write(gUIStyle.onHover, onHover, objects);
			gUIStyle.onActive = Write(gUIStyle.onActive, onActive, objects);
			gUIStyle.focused = Write(gUIStyle.focused, focused, objects);
			gUIStyle.onFocused = Write(gUIStyle.onFocused, onFocused, objects);
			gUIStyle.border = border;
			gUIStyle.margin = margin;
			gUIStyle.padding = padding;
			gUIStyle.overflow = overflow;
			gUIStyle.font = (Font)objects.Get(font);
			gUIStyle.name = name;
			gUIStyle.imagePosition = (ImagePosition)imagePosition;
			gUIStyle.alignment = (TextAnchor)alignment;
			gUIStyle.wordWrap = wordWrap;
			gUIStyle.clipping = (TextClipping)clipping;
			gUIStyle.contentOffset = contentOffset;
			gUIStyle.fixedWidth = fixedWidth;
			gUIStyle.fixedHeight = fixedHeight;
			gUIStyle.stretchWidth = stretchWidth;
			gUIStyle.stretchHeight = stretchHeight;
			gUIStyle.fontSize = fontSize;
			gUIStyle.fontStyle = (FontStyle)fontStyle;
			gUIStyle.richText = richText;
			return gUIStyle;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				GUIStyle gUIStyle = (GUIStyle)obj;
				normal = Read(normal, gUIStyle.normal);
				hover = Read(hover, gUIStyle.hover);
				active = Read(active, gUIStyle.active);
				onNormal = Read(onNormal, gUIStyle.onNormal);
				onHover = Read(onHover, gUIStyle.onHover);
				onActive = Read(onActive, gUIStyle.onActive);
				focused = Read(focused, gUIStyle.focused);
				onFocused = Read(onFocused, gUIStyle.onFocused);
				border = gUIStyle.border;
				margin = gUIStyle.margin;
				padding = gUIStyle.padding;
				overflow = gUIStyle.overflow;
				font = gUIStyle.font.GetMappedInstanceID();
				name = gUIStyle.name;
				imagePosition = (uint)gUIStyle.imagePosition;
				alignment = (uint)gUIStyle.alignment;
				wordWrap = gUIStyle.wordWrap;
				clipping = (uint)gUIStyle.clipping;
				contentOffset = gUIStyle.contentOffset;
				fixedWidth = gUIStyle.fixedWidth;
				fixedHeight = gUIStyle.fixedHeight;
				stretchWidth = gUIStyle.stretchWidth;
				stretchHeight = gUIStyle.stretchHeight;
				fontSize = gUIStyle.fontSize;
				fontStyle = (uint)gUIStyle.fontStyle;
				richText = gUIStyle.richText;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			FindDependencies(normal, dependencies, objects, allowNulls);
			FindDependencies(hover, dependencies, objects, allowNulls);
			FindDependencies(active, dependencies, objects, allowNulls);
			FindDependencies(onNormal, dependencies, objects, allowNulls);
			FindDependencies(onHover, dependencies, objects, allowNulls);
			FindDependencies(onActive, dependencies, objects, allowNulls);
			FindDependencies(focused, dependencies, objects, allowNulls);
			FindDependencies(onFocused, dependencies, objects, allowNulls);
			AddDependency(font, dependencies, objects, allowNulls);
		}

		protected override void GetDependencies(Dictionary<long, UnityEngine.Object> dependencies, object obj)
		{
			base.GetDependencies(dependencies, obj);
			if (obj != null)
			{
				GUIStyle gUIStyle = (GUIStyle)obj;
				GetDependencies(normal, gUIStyle.normal, dependencies);
				GetDependencies(hover, gUIStyle.hover, dependencies);
				GetDependencies(active, gUIStyle.active, dependencies);
				GetDependencies(onNormal, gUIStyle.onNormal, dependencies);
				GetDependencies(onHover, gUIStyle.onHover, dependencies);
				GetDependencies(onActive, gUIStyle.onActive, dependencies);
				GetDependencies(focused, gUIStyle.focused, dependencies);
				GetDependencies(onFocused, gUIStyle.onFocused, dependencies);
				AddDependency(gUIStyle.font, dependencies);
			}
		}
	}
}
