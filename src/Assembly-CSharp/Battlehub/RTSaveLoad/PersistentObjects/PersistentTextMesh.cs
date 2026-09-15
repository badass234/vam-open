using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentTextMesh : PersistentComponent
	{
		public string text;

		public long font;

		public int fontSize;

		public uint fontStyle;

		public float offsetZ;

		public uint alignment;

		public uint anchor;

		public float characterSize;

		public float lineSpacing;

		public float tabSize;

		public bool richText;

		public Color color;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			TextMesh textMesh = (TextMesh)obj;
			textMesh.text = text;
			textMesh.font = (Font)objects.Get(font);
			textMesh.fontSize = fontSize;
			textMesh.fontStyle = (FontStyle)fontStyle;
			textMesh.offsetZ = offsetZ;
			textMesh.alignment = (TextAlignment)alignment;
			textMesh.anchor = (TextAnchor)anchor;
			textMesh.characterSize = characterSize;
			textMesh.lineSpacing = lineSpacing;
			textMesh.tabSize = tabSize;
			textMesh.richText = richText;
			textMesh.color = color;
			return textMesh;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				TextMesh textMesh = (TextMesh)obj;
				text = textMesh.text;
				font = textMesh.font.GetMappedInstanceID();
				fontSize = textMesh.fontSize;
				fontStyle = (uint)textMesh.fontStyle;
				offsetZ = textMesh.offsetZ;
				alignment = (uint)textMesh.alignment;
				anchor = (uint)textMesh.anchor;
				characterSize = textMesh.characterSize;
				lineSpacing = textMesh.lineSpacing;
				tabSize = textMesh.tabSize;
				richText = textMesh.richText;
				color = textMesh.color;
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
				TextMesh textMesh = (TextMesh)obj;
				AddDependency(textMesh.font, dependencies);
			}
		}
	}
}
