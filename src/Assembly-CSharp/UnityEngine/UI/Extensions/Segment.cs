using UnityEngine.EventSystems;

namespace UnityEngine.UI.Extensions
{
	[RequireComponent(typeof(Selectable))]
	public class Segment : UIBehaviour, IPointerClickHandler, ISubmitHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, ISelectHandler, IDeselectHandler, IEventSystemHandler
	{
		internal int index;

		[SerializeField]
		private Color textColor;

		internal bool leftmost => index == 0;

		internal bool rightmost => index == segmentControl.segments.Length - 1;

		public bool selected
		{
			get
			{
				return segmentControl.selectedSegment == button;
			}
			set
			{
				SetSelected(value);
			}
		}

		internal SegmentedControl segmentControl => GetComponentInParent<SegmentedControl>();

		internal Selectable button => GetComponent<Selectable>();

		protected Segment()
		{
		}

		public virtual void OnPointerClick(PointerEventData eventData)
		{
			if (eventData.button == PointerEventData.InputButton.Left)
			{
				selected = true;
			}
		}

		public virtual void OnPointerEnter(PointerEventData eventData)
		{
			MaintainSelection();
		}

		public virtual void OnPointerExit(PointerEventData eventData)
		{
			MaintainSelection();
		}

		public virtual void OnPointerDown(PointerEventData eventData)
		{
			MaintainSelection();
		}

		public virtual void OnPointerUp(PointerEventData eventData)
		{
			MaintainSelection();
		}

		public virtual void OnSelect(BaseEventData eventData)
		{
			MaintainSelection();
		}

		public virtual void OnDeselect(BaseEventData eventData)
		{
			MaintainSelection();
		}

		public virtual void OnSubmit(BaseEventData eventData)
		{
			selected = true;
		}

		private void SetSelected(bool value)
		{
			if (value && button.IsActive() && button.IsInteractable())
			{
				if (segmentControl.selectedSegment == button)
				{
					if (segmentControl.allowSwitchingOff)
					{
						Deselect();
					}
					else
					{
						MaintainSelection();
					}
					return;
				}
				if ((bool)segmentControl.selectedSegment)
				{
					Segment component = segmentControl.selectedSegment.GetComponent<Segment>();
					segmentControl.selectedSegment = null;
					component.TransitionButton();
				}
				segmentControl.selectedSegment = button;
				StoreTextColor();
				TransitionButton();
				segmentControl.onValueChanged.Invoke(index);
			}
			else if (segmentControl.selectedSegment == button)
			{
				Deselect();
			}
		}

		private void Deselect()
		{
			segmentControl.selectedSegment = null;
			TransitionButton();
			segmentControl.onValueChanged.Invoke(-1);
		}

		private void MaintainSelection()
		{
			if (!(button != segmentControl.selectedSegment))
			{
				TransitionButton(true);
			}
		}

		internal void TransitionButton()
		{
			TransitionButton(false);
		}

		internal void TransitionButton(bool instant)
		{
			Color color = ((!selected) ? button.colors.normalColor : segmentControl.selectedColor);
			Color color2 = ((!selected) ? textColor : button.colors.normalColor);
			Sprite newSprite = ((!selected) ? null : button.spriteState.pressedSprite);
			string triggername = ((!selected) ? button.animationTriggers.normalTrigger : button.animationTriggers.pressedTrigger);
			switch (button.transition)
			{
			case Selectable.Transition.ColorTint:
				StartColorTween(color * button.colors.colorMultiplier, instant);
				ChangeTextColor(color2 * button.colors.colorMultiplier);
				break;
			case Selectable.Transition.SpriteSwap:
				DoSpriteSwap(newSprite);
				break;
			case Selectable.Transition.Animation:
				TriggerAnimation(triggername);
				break;
			}
		}

		private void StartColorTween(Color targetColor, bool instant)
		{
			if (!(button.targetGraphic == null))
			{
				button.targetGraphic.CrossFadeColor(targetColor, (!instant) ? button.colors.fadeDuration : 0f, true, true);
			}
		}

		internal void StoreTextColor()
		{
			Text componentInChildren = GetComponentInChildren<Text>();
			if ((bool)componentInChildren)
			{
				textColor = componentInChildren.color;
			}
		}

		private void ChangeTextColor(Color targetColor)
		{
			Text componentInChildren = GetComponentInChildren<Text>();
			if ((bool)componentInChildren)
			{
				componentInChildren.color = targetColor;
			}
		}

		private void DoSpriteSwap(Sprite newSprite)
		{
			if (!(button.image == null))
			{
				button.image.overrideSprite = newSprite;
			}
		}

		private void TriggerAnimation(string triggername)
		{
			if (!(button.animator == null) && button.animator.isActiveAndEnabled && button.animator.hasBoundPlayables && !string.IsNullOrEmpty(triggername))
			{
				button.animator.ResetTrigger(button.animationTriggers.normalTrigger);
				button.animator.ResetTrigger(button.animationTriggers.pressedTrigger);
				button.animator.ResetTrigger(button.animationTriggers.highlightedTrigger);
				button.animator.ResetTrigger(button.animationTriggers.disabledTrigger);
				button.animator.SetTrigger(triggername);
			}
		}
	}
}
