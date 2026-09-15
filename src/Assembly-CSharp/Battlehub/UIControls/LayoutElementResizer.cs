using System;
using Battlehub.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Battlehub.UIControls
{
	public class LayoutElementResizer : MonoBehaviour, IBeginDragHandler, IDragHandler, IDropHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IEventSystemHandler
	{
		public LayoutElement Target;

		public RectTransform Parent;

		public LayoutElement SecondaryTarget;

		public Texture2D CursorTexture;

		public float XSign = 1f;

		public float YSign;

		public float MaxSize;

		public bool HasMaxSize;

		private bool m_pointerInside;

		private bool m_pointerDown;

		private float m_midX;

		private float m_midY;

		void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
		{
			if (Parent != null && SecondaryTarget != null)
			{
				if (XSign != 0f)
				{
					Target.flexibleWidth = Mathf.Clamp01(Target.flexibleWidth);
					SecondaryTarget.flexibleWidth = Mathf.Clamp01(SecondaryTarget.flexibleWidth);
				}
				if (YSign != 0f)
				{
					Target.flexibleHeight = Mathf.Clamp01(Target.flexibleHeight);
					SecondaryTarget.flexibleHeight = Mathf.Clamp01(SecondaryTarget.flexibleHeight);
				}
				m_midY = Target.flexibleHeight / (Target.flexibleHeight + SecondaryTarget.flexibleHeight);
				m_midY *= Math.Max(Parent.rect.height - Target.minHeight - SecondaryTarget.minHeight, 0f);
				m_midX = Target.flexibleWidth / (Target.flexibleWidth + SecondaryTarget.flexibleWidth);
				m_midX *= Math.Max(Parent.rect.width - Target.minWidth - SecondaryTarget.minWidth, 0f);
			}
		}

		void IDragHandler.OnDrag(PointerEventData eventData)
		{
			if (Parent != null && SecondaryTarget != null)
			{
				if (XSign != 0f)
				{
					float num = m_midX + eventData.delta.x * (float)Math.Sign(XSign);
					float num2 = num / (Parent.rect.width - Target.minWidth - SecondaryTarget.minWidth);
					Target.flexibleWidth = num2;
					SecondaryTarget.flexibleWidth = 1f - num2;
					m_midX = num;
				}
				if (YSign != 0f)
				{
					float num3 = m_midY + eventData.delta.y * (float)Math.Sign(YSign);
					float num4 = num3 / (Parent.rect.height - Target.minHeight - SecondaryTarget.minHeight);
					Target.flexibleHeight = num4;
					SecondaryTarget.flexibleHeight = 1f - num4;
					m_midY = num3;
				}
				if (XSign != 0f)
				{
					Target.flexibleWidth = Mathf.Clamp01(Target.flexibleWidth);
					SecondaryTarget.flexibleWidth = Mathf.Clamp01(SecondaryTarget.flexibleWidth);
				}
				if (YSign != 0f)
				{
					Target.flexibleHeight = Mathf.Clamp01(Target.flexibleHeight);
					SecondaryTarget.flexibleHeight = Mathf.Clamp01(SecondaryTarget.flexibleHeight);
				}
				return;
			}
			if (XSign != 0f)
			{
				Target.preferredWidth += eventData.delta.x * (float)Math.Sign(XSign);
				if (HasMaxSize && Target.preferredWidth > MaxSize)
				{
					Target.preferredWidth = MaxSize;
				}
			}
			if (YSign != 0f)
			{
				Target.preferredHeight += eventData.delta.y * (float)Math.Sign(YSign);
				if (HasMaxSize && Target.preferredHeight > MaxSize)
				{
					Target.preferredHeight = MaxSize;
				}
			}
		}

		void IDropHandler.OnDrop(PointerEventData eventData)
		{
		}

		void IEndDragHandler.OnEndDrag(PointerEventData eventData)
		{
		}

		void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
		{
			m_pointerInside = true;
			CursorHelper.SetCursor(this, CursorTexture, new Vector2(CursorTexture.width / 2, CursorTexture.height / 2), CursorMode.Auto);
		}

		void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
		{
			m_pointerInside = false;
			if (!m_pointerDown)
			{
				CursorHelper.ResetCursor(this);
			}
		}

		void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
		{
			m_pointerDown = true;
			if (Target.preferredWidth < -1f)
			{
				Target.preferredWidth = Target.minWidth;
			}
			if (Target.preferredHeight < -1f)
			{
				Target.preferredHeight = Target.minHeight;
			}
		}

		void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
		{
			m_pointerDown = false;
			if (!m_pointerInside)
			{
				CursorHelper.ResetCursor(this);
			}
		}
	}
}
