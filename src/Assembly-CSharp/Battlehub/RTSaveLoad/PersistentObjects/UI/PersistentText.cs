using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTSaveLoad.PersistentObjects.UI
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentText : PersistentMaskableGraphic
	{
		public long font;

		public string text;

		public bool supportRichText;

		public bool resizeTextForBestFit;

		public int resizeTextMinSize;

		public int resizeTextMaxSize;

		public uint alignment;

		public bool alignByGeometry;

		public int fontSize;

		public uint horizontalOverflow;

		public uint verticalOverflow;

		public float lineSpacing;

		public uint fontStyle;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			Text text = (Text)obj;
			text.font = (Font)objects.Get(font);
			text.text = this.text;
			text.supportRichText = supportRichText;
			text.resizeTextForBestFit = resizeTextForBestFit;
			text.resizeTextMinSize = resizeTextMinSize;
			text.resizeTextMaxSize = resizeTextMaxSize;
			text.alignment = (TextAnchor)alignment;
			text.alignByGeometry = alignByGeometry;
			text.fontSize = fontSize;
			text.horizontalOverflow = (HorizontalWrapMode)horizontalOverflow;
			text.verticalOverflow = (VerticalWrapMode)verticalOverflow;
			text.lineSpacing = lineSpacing;
			text.fontStyle = (FontStyle)fontStyle;
			return text;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				Text text = (Text)obj;
				font = text.font.GetMappedInstanceID();
				this.text = text.text;
				supportRichText = text.supportRichText;
				resizeTextForBestFit = text.resizeTextForBestFit;
				resizeTextMinSize = text.resizeTextMinSize;
				resizeTextMaxSize = text.resizeTextMaxSize;
				alignment = (uint)text.alignment;
				alignByGeometry = text.alignByGeometry;
				fontSize = text.fontSize;
				horizontalOverflow = (uint)text.horizontalOverflow;
				verticalOverflow = (uint)text.verticalOverflow;
				lineSpacing = text.lineSpacing;
				fontStyle = (uint)text.fontStyle;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			AddDependency(font, dependencies, objects, allowNulls);
		}

		protected override void GetDependencies(Dictionary<long, UnityEngine.Object> dependencies, object obj)
		{
			base.GetDependencies(dependencies, obj);
			if (obj != null)
			{
				Text text = (Text)obj;
				AddDependency(text.font, dependencies);
			}
		}
	}
}
