using System;
using System.Collections.Generic;
using AnimationOrTween;
using HeavyMetalMachines.Utils;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	[AddComponentMenu("NGUI/Interaction/Play Tween")]
	[ExecuteInEditMode]
	public class UIPlayTween : MonoBehaviour
	{
		protected bool IsAllowedToClick()
		{
			UIButtonColor.UIMouseButton uiMouseButton = UIPlayTween.GetUiMouseButton();
			if (uiMouseButton == UIButtonColor.UIMouseButton.None)
			{
				return true;
			}
			bool flag = uiMouseButton == (this.AllowedMouseClick & uiMouseButton);
			UnityEngine.Debug.LogFormat("[{0}] IsAllowedToClick: {1}", new object[]
			{
				base.gameObject.name,
				flag
			});
			return flag;
		}

		public static UIButtonColor.UIMouseButton GetUiMouseButton()
		{
			UIButtonColor.UIMouseButton result = UIButtonColor.UIMouseButton.None;
			int currentTouchID = UICamera.currentTouchID;
			if (currentTouchID != -1)
			{
				if (currentTouchID != -2)
				{
					if (currentTouchID == -3)
					{
						result = UIButtonColor.UIMouseButton.Middle;
					}
				}
				else
				{
					result = UIButtonColor.UIMouseButton.Right;
				}
			}
			else
			{
				result = UIButtonColor.UIMouseButton.Left;
			}
			return result;
		}

		private void Awake()
		{
			if (this.eventReceiver != null && EventDelegate.IsValid(this.onFinished))
			{
				this.eventReceiver = null;
				this.callWhenFinished = null;
			}
		}

		private void Start()
		{
			this.mStarted = true;
			if (this.tweenTarget == null)
			{
				this.tweenTarget = base.gameObject;
			}
		}

		private void OnEnable()
		{
			if (this.mStarted)
			{
				this.OnHover(UICamera.IsHighlighted(base.gameObject));
			}
			if (UICamera.currentTouch != null)
			{
				if (this.trigger == Trigger.OnPress || this.trigger == Trigger.OnPressTrue)
				{
					this.mActivated = (UICamera.currentTouch.pressed == base.gameObject);
				}
				if (this.trigger == Trigger.OnHover || this.trigger == Trigger.OnHoverTrue)
				{
					this.mActivated = (UICamera.currentTouch.current == base.gameObject);
				}
			}
			UIToggle component = base.GetComponent<UIToggle>();
			if (component != null)
			{
				EventDelegate.Add(component.onChange, new EventDelegate.Callback(this.OnToggle));
			}
		}

		private void OnDisable()
		{
			UIToggle component = base.GetComponent<UIToggle>();
			if (component != null)
			{
				EventDelegate.Remove(component.onChange, new EventDelegate.Callback(this.OnToggle));
			}
		}

		private void OnDragOver()
		{
			if (this.trigger == Trigger.OnHover)
			{
				this.OnHover(true);
			}
		}

		private void OnHover(bool isOver)
		{
			if (!base.enabled)
			{
				return;
			}
			if (!this.IsAllowedToClick())
			{
				return;
			}
			if (this.trigger == Trigger.OnHover || (this.trigger == Trigger.OnHoverTrue && isOver) || (this.trigger == Trigger.OnHoverFalse && !isOver))
			{
				this.mActivated = (isOver && this.trigger == Trigger.OnHover);
				this.Play(isOver);
			}
		}

		private void OnDragOut()
		{
			if (base.enabled && this.mActivated)
			{
				this.mActivated = false;
				this.Play(false);
			}
		}

		private void OnPress(bool isPressed)
		{
			if (base.enabled && (this.trigger == Trigger.OnPress || (this.trigger == Trigger.OnPressTrue && isPressed) || (this.trigger == Trigger.OnPressFalse && !isPressed)))
			{
				this.mActivated = (isPressed && this.trigger == Trigger.OnPress);
				this.Play(isPressed);
			}
		}

		private void OnClick()
		{
			if (!base.enabled || this.trigger != Trigger.OnClick)
			{
				return;
			}
			if (!this.IsAllowedToClick())
			{
				return;
			}
			this.Play(true);
		}

		private void OnDoubleClick()
		{
			if (!base.enabled || this.trigger != Trigger.OnDoubleClick)
			{
				return;
			}
			if (!this.IsAllowedToClick())
			{
				return;
			}
			this.Play(true);
		}

		private void OnSelect(bool isSelected)
		{
			if (base.enabled && (this.trigger == Trigger.OnSelect || (this.trigger == Trigger.OnSelectTrue && isSelected) || (this.trigger == Trigger.OnSelectFalse && !isSelected)))
			{
				this.mActivated = (isSelected && this.trigger == Trigger.OnSelect);
				this.Play(isSelected);
			}
		}

		private void OnToggle()
		{
			if (!base.enabled || UIToggle.current == null)
			{
				return;
			}
			if (this.trigger == Trigger.OnActivate || (this.trigger == Trigger.OnActivateTrue && UIToggle.current.value) || (this.trigger == Trigger.OnActivateFalse && !UIToggle.current.value))
			{
				this.Play(UIToggle.current.value);
			}
		}

		private void Update()
		{
			if (this.disableWhenFinished != DisableCondition.DoNotDisable && this.mTweens != null)
			{
				bool flag = true;
				bool flag2 = true;
				int i = 0;
				int num = this.mTweens.Length;
				while (i < num)
				{
					UITweener uitweener = this.mTweens[i];
					if (uitweener.tweenGroup == this.tweenGroup)
					{
						if (uitweener.enabled)
						{
							flag = false;
							break;
						}
						if (uitweener.direction != (Direction)this.disableWhenFinished)
						{
							flag2 = false;
						}
					}
					i++;
				}
				if (flag)
				{
					if (flag2)
					{
						NGUITools.SetActive(this.tweenTarget, false, false, false);
					}
					this.mTweens = null;
				}
			}
		}

		public virtual void Play(bool forward)
		{
			this.mActive = 0;
			GameObject gameObject = (!(this.tweenTarget == null)) ? this.tweenTarget : base.gameObject;
			if (!NGUITools.GetActive(gameObject))
			{
				if (this.ifDisabledOnPlay != EnableCondition.EnableThenPlay)
				{
					return;
				}
				NGUITools.SetActive(gameObject, true, false, false);
			}
			this.mTweens = ((!this.includeChildren) ? gameObject.GetComponents<UITweener>() : gameObject.GetComponentsInChildren<UITweener>());
			if (this.mTweens.Length == 0)
			{
				if (this.disableWhenFinished != DisableCondition.DoNotDisable)
				{
					NGUITools.SetActive(this.tweenTarget, false, false, false);
				}
				return;
			}
			bool flag = false;
			if (this.playDirection == Direction.Reverse)
			{
				forward = !forward;
			}
			int i = 0;
			int num = this.mTweens.Length;
			while (i < num)
			{
				UITweener uitweener = this.mTweens[i];
				if (uitweener.tweenGroup == this.tweenGroup)
				{
					if (!flag && !NGUITools.GetActive(gameObject))
					{
						flag = true;
						NGUITools.SetActive(gameObject, true, false, false);
					}
					this.mActive++;
					if (this.playDirection == Direction.Toggle)
					{
						EventDelegate.Add(uitweener.onFinished, new EventDelegate.Callback(this.OnFinished), true);
						uitweener.Toggle();
					}
					else
					{
						if (this.resetOnPlay || (this.resetIfDisabled && !uitweener.enabled))
						{
							uitweener.Play(forward);
							uitweener.ResetToBeginning();
						}
						EventDelegate.Add(uitweener.onFinished, new EventDelegate.Callback(this.OnFinished), true);
						uitweener.Play(forward);
					}
				}
				i++;
			}
		}

		private void OnFinished()
		{
			if (--this.mActive == 0 && UIPlayTween.current == null)
			{
				UIPlayTween.current = this;
				EventDelegate.Execute(this.onFinished);
				if (this.eventReceiver != null && !string.IsNullOrEmpty(this.callWhenFinished))
				{
					this.eventReceiver.SendMessage(this.callWhenFinished, SendMessageOptions.DontRequireReceiver);
				}
				this.eventReceiver = null;
				UIPlayTween.current = null;
			}
		}

		public static void AddUITweener(UITweener tweener)
		{
			if (UIPlayTween.UITweeners == null)
			{
				UIPlayTween.UITweeners = new List<UITweener>();
			}
			UIPlayTween.UITweeners.Add(tweener);
		}

		public static void RemoveUITweener(UITweener tweener)
		{
			if (UIPlayTween.UITweeners == null)
			{
				return;
			}
			UIPlayTween.UITweeners.Remove(tweener);
		}

		private static void VerifyTweeners()
		{
			if (UIPlayTween.UITweeners != null && UIPlayTween.UITweeners.Count > 0)
			{
				List<UITweener> list = new List<UITweener>();
				for (int i = 0; i < UIPlayTween.UITweeners.Count; i++)
				{
					UITweener uitweener = UIPlayTween.UITweeners[i];
					if (uitweener == null || uitweener.gameObject == null)
					{
						list.Add(uitweener);
					}
					else if (!uitweener.enabled || !uitweener.gameObject.activeInHierarchy)
					{
						list.Add(uitweener);
					}
				}
				for (int j = 0; j < list.Count; j++)
				{
					UITweener item = list[j];
					UIPlayTween.UITweeners.Remove(item);
				}
			}
		}

		public static bool IsAnimating()
		{
			UIPlayTween.VerifyTweeners();
			return UIPlayTween.UITweeners != null && UIPlayTween.UITweeners.Count > 0;
		}

		[BitMask(typeof(UIButtonColor.UIMouseButton))]
		[SerializeField]
		public UIButtonColor.UIMouseButton AllowedMouseClick = UIButtonColor.UIMouseButton.Left;

		public static UIPlayTween current;

		public GameObject tweenTarget;

		public int tweenGroup;

		public Trigger trigger;

		public Direction playDirection = Direction.Forward;

		public bool resetOnPlay;

		public bool resetIfDisabled;

		public EnableCondition ifDisabledOnPlay;

		public DisableCondition disableWhenFinished;

		public bool includeChildren;

		public List<EventDelegate> onFinished = new List<EventDelegate>();

		[HideInInspector]
		[SerializeField]
		private GameObject eventReceiver;

		[HideInInspector]
		[SerializeField]
		private string callWhenFinished;

		private UITweener[] mTweens;

		private bool mStarted;

		private int mActive;

		private bool mActivated;

		private static List<UITweener> UITweeners;
	}
}
