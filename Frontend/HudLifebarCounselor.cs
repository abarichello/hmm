using System;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Options;
using Pocketverse;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.Frontend
{
	public class HudLifebarCounselor : GameHubBehaviour
	{
		public void Setup(Transform lifebarParentTransform)
		{
			base.transform.SetParent(lifebarParentTransform);
			this.Reset();
		}

		private void Reset()
		{
			this.state = HudLifebarCounselor.State.Off;
			this.substate = HudLifebarCounselor.SubState.None;
			this._anim.Stop();
			this._anim.Sample();
			this._anim.Play(this.animationOffName);
		}

		private void OnEnable()
		{
			RectTransform component = base.GetComponent<RectTransform>();
			component.localScale = Vector3.one;
			component.localPosition = Vector3.zero;
			this.Reset();
		}

		public void Update()
		{
			if (GameHubBehaviour.Hub.BombManager.CurrentBombGameState != BombScoreBoard.State.BombDelivery && this.state != HudLifebarCounselor.State.Off)
			{
				this.Reset();
				return;
			}
			switch (this.state)
			{
			case HudLifebarCounselor.State.Off:
				this.UpdateOffState();
				break;
			case HudLifebarCounselor.State.In:
				this.UpdateInState();
				break;
			case HudLifebarCounselor.State.Showing:
				this.UpdateShowingState();
				break;
			case HudLifebarCounselor.State.ShowingTransition:
				this.UpdateShowingTransitionState();
				break;
			}
		}

		private void StopCurrentAnim()
		{
			if (this._anim.clip.wrapMode != WrapMode.Loop)
			{
				return;
			}
			this._anim.Stop(this._anim.clip.name);
			this._anim.Rewind(this._anim.clip.name);
			this._anim.Sample();
		}

		private void UpdateOffState()
		{
			if (this._anim.isPlaying || !GameHubBehaviour.Hub.ClientCounselorController.IsPlaying)
			{
				return;
			}
			if (GameHubBehaviour.Hub.ClientCounselorController.CurrentAdviceConfig.ControlAction > ControlAction.None)
			{
				this.StopCurrentAnim();
				this.currentControlAction = GameHubBehaviour.Hub.ClientCounselorController.CurrentAdviceConfig.ControlAction;
				this.ConfigureCurrentAdvice();
				HudLifebarCounselor.SubState subState = this.substate;
				if (subState != HudLifebarCounselor.SubState.Key)
				{
					if (subState == HudLifebarCounselor.SubState.Icon)
					{
						this._anim.Play(this.animationIconInName);
					}
				}
				else
				{
					this._anim.Play(this.animationKeyInName);
				}
				this.state = HudLifebarCounselor.State.In;
			}
		}

		private void UpdateInState()
		{
			if (GameHubBehaviour.Hub.ClientCounselorController.CurrentAdviceConfig.ControlAction == ControlAction.None || !GameHubBehaviour.Hub.ClientCounselorController.IsPlaying)
			{
				this.StopCurrentAnim();
				HudLifebarCounselor.SubState subState = this.substate;
				if (subState != HudLifebarCounselor.SubState.Key)
				{
					if (subState == HudLifebarCounselor.SubState.Icon)
					{
						this._anim.Play(this.animationIconOutName);
					}
				}
				else
				{
					this._anim.Play(this.animationKeyOutName);
				}
				this.state = HudLifebarCounselor.State.Off;
				this.substate = HudLifebarCounselor.SubState.None;
				return;
			}
			if (!this._anim.isPlaying)
			{
				this.StopCurrentAnim();
				HudLifebarCounselor.SubState subState2 = this.substate;
				if (subState2 != HudLifebarCounselor.SubState.Key)
				{
					if (subState2 == HudLifebarCounselor.SubState.Icon)
					{
						this._anim.Play(this.animationIconIdleName);
					}
				}
				else
				{
					this._anim.Play(this.animationKeyIdleName);
				}
				this.state = HudLifebarCounselor.State.Showing;
			}
		}

		private void ConfigureCurrentAdvice()
		{
			KeyCode keyCode;
			if (ControlOptions.IsMouseInput(this.currentControlAction, out keyCode))
			{
				this.substate = HudLifebarCounselor.SubState.Icon;
				Sprite sprite = null;
				switch (keyCode)
				{
				case KeyCode.Mouse0:
					sprite = GameHubBehaviour.Hub.CounselorConfig.ShortcutMouse0Sprite;
					break;
				case KeyCode.Mouse1:
					sprite = GameHubBehaviour.Hub.CounselorConfig.ShortcutMouse1Sprite;
					break;
				case KeyCode.Mouse2:
					sprite = GameHubBehaviour.Hub.CounselorConfig.ShortcutMouse2Sprite;
					break;
				}
				this._iconImage.sprite = sprite;
			}
			else
			{
				this.substate = HudLifebarCounselor.SubState.Key;
				this._label.text = ControlOptions.GetTextlocalized(this.currentControlAction, ControlOptions.ControlActionInputType.Primary);
			}
		}

		private void UpdateShowingState()
		{
			if (GameHubBehaviour.Hub.ClientCounselorController.CurrentAdviceConfig.ControlAction == ControlAction.None || !GameHubBehaviour.Hub.ClientCounselorController.IsPlaying)
			{
				this.StopCurrentAnim();
				HudLifebarCounselor.SubState subState = this.substate;
				if (subState != HudLifebarCounselor.SubState.Key)
				{
					if (subState == HudLifebarCounselor.SubState.Icon)
					{
						this._anim.Play(this.animationIconOutName);
					}
				}
				else
				{
					this._anim.Play(this.animationKeyOutName);
				}
				this.state = HudLifebarCounselor.State.Off;
				this.substate = HudLifebarCounselor.SubState.None;
				return;
			}
			if (this.currentControlAction != GameHubBehaviour.Hub.ClientCounselorController.CurrentAdviceConfig.ControlAction)
			{
				this.currentControlAction = GameHubBehaviour.Hub.ClientCounselorController.CurrentAdviceConfig.ControlAction;
				HudLifebarCounselor.SubState subState2 = this.substate;
				this.ConfigureCurrentAdvice();
				if (subState2 != this.substate)
				{
					HudLifebarCounselor.SubState subState3 = this.substate;
					if (subState3 != HudLifebarCounselor.SubState.Key)
					{
						if (subState3 == HudLifebarCounselor.SubState.Icon)
						{
							this._anim.Play(this.animationKeyToIconName);
						}
					}
					else
					{
						this._anim.Play(this.animationIconToKeyName);
					}
					this.state = HudLifebarCounselor.State.ShowingTransition;
				}
			}
		}

		private void UpdateShowingTransitionState()
		{
			if (GameHubBehaviour.Hub.ClientCounselorController.CurrentAdviceConfig.ControlAction == ControlAction.None)
			{
				this.StopCurrentAnim();
				HudLifebarCounselor.SubState subState = this.substate;
				if (subState != HudLifebarCounselor.SubState.Key)
				{
					if (subState == HudLifebarCounselor.SubState.Icon)
					{
						this._anim.Play(this.animationIconOutName);
					}
				}
				else
				{
					this._anim.Play(this.animationKeyOutName);
				}
				this.state = HudLifebarCounselor.State.Off;
				this.substate = HudLifebarCounselor.SubState.None;
				return;
			}
			if (!this._anim.isPlaying)
			{
				this.StopCurrentAnim();
				HudLifebarCounselor.SubState subState2 = this.substate;
				if (subState2 != HudLifebarCounselor.SubState.Key)
				{
					if (subState2 == HudLifebarCounselor.SubState.Icon)
					{
						this._anim.Play(this.animationIconIdleName);
					}
				}
				else
				{
					this._anim.Play(this.animationKeyIdleName);
				}
				this.state = HudLifebarCounselor.State.Showing;
			}
		}

		private HudLifebarCounselor.State state = HudLifebarCounselor.State.Off;

		private HudLifebarCounselor.SubState substate;

		private ControlAction currentControlAction;

		[SerializeField]
		private Text _label;

		[SerializeField]
		private Image _iconImage;

		[SerializeField]
		private Animation _anim;

		[SerializeField]
		private string animationOffName;

		[SerializeField]
		private string animationKeyInName;

		[SerializeField]
		private string animationKeyIdleName;

		[SerializeField]
		private string animationKeyOutName;

		[SerializeField]
		private string animationIconInName;

		[SerializeField]
		private string animationIconIdleName;

		[SerializeField]
		private string animationIconOutName;

		[SerializeField]
		private string animationIconToKeyName;

		[SerializeField]
		private string animationKeyToIconName;

		public enum State
		{
			Off = 1,
			In,
			Showing,
			ShowingTransition
		}

		public enum SubState
		{
			None,
			Key,
			Icon
		}
	}
}
