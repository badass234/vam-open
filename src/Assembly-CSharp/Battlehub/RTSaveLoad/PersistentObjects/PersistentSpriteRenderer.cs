using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentSpriteRenderer : PersistentRenderer
	{
		public long sprite;

		public uint drawMode;

		public Vector2 size;

		public float adaptiveModeThreshold;

		public uint tileMode;

		public Color color;

		public bool flipX;

		public bool flipY;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			SpriteRenderer spriteRenderer = (SpriteRenderer)obj;
			spriteRenderer.sprite = (Sprite)objects.Get(sprite);
			spriteRenderer.drawMode = (SpriteDrawMode)drawMode;
			spriteRenderer.size = size;
			spriteRenderer.adaptiveModeThreshold = adaptiveModeThreshold;
			spriteRenderer.tileMode = (SpriteTileMode)tileMode;
			spriteRenderer.color = color;
			spriteRenderer.flipX = flipX;
			spriteRenderer.flipY = flipY;
			return spriteRenderer;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				SpriteRenderer spriteRenderer = (SpriteRenderer)obj;
				sprite = spriteRenderer.sprite.GetMappedInstanceID();
				drawMode = (uint)spriteRenderer.drawMode;
				size = spriteRenderer.size;
				adaptiveModeThreshold = spriteRenderer.adaptiveModeThreshold;
				tileMode = (uint)spriteRenderer.tileMode;
				color = spriteRenderer.color;
				flipX = spriteRenderer.flipX;
				flipY = spriteRenderer.flipY;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			AddDependency(sprite, dependencies, objects, allowNulls);
		}

		protected override void GetDependencies(Dictionary<long, UnityEngine.Object> dependencies, object obj)
		{
			base.GetDependencies(dependencies, obj);
			if (obj != null)
			{
				SpriteRenderer spriteRenderer = (SpriteRenderer)obj;
				AddDependency(spriteRenderer.sprite, dependencies);
			}
		}
	}
}
