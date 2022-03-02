using System;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Input;
using HeavyMetalMachines.Input.ControllerInput;
using HeavyMetalMachines.Options;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using Hoplon.Input;
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
			base.transform.localPosition = Vector3.zero;
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
			if (GameHubBehaviour.Hub.BombManager.CurrentBombGameState != BombScoreboardState.BombDelivery && this.state != HudLifebarCounselor.State.Off)
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
			if (this._anim.clip.wrapMode != 2)
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
			if (this._controlParent.activeSelf)
			{
				this._controlParent.SetActive(false);
			}
			if (GameHubBehaviour.Hub.ClientCounselorController.CurrentAdviceConfig.InputAction > -1)
			{
				this.StopCurrentAnim();
				this.currentInputAction = GameHubBehaviour.Hub.ClientCounselorController.CurrentAdviceConfig.InputAction;
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
				this._controlParent.SetActive(true);
			}
		}

		private void UpdateInState()
		{
			if (GameHubBehaviour.Hub.ClientCounselorController.CurrentAdviceConfig.InputAction == -1 || !GameHubBehaviour.Hub.ClientCounselorController.IsPlaying)
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
			ISprite sprite;
			string text;
			if (this.TryToGetInputActionAssetOrFallbackToTranslation(this.currentInputAction, out sprite, out text))
			{
				this.substate = HudLifebarCounselor.SubState.Icon;
				this._iconImage.sprite = (sprite as UnitySprite).GetSprite();
			}
			else
			{
				this.substate = HudLifebarCounselor.SubState.Key;
				this._label.text = text;
			}
		}

		private bool TryToGetInputActionAssetOrFallbackToTranslation(ControllerInputActions controllerInputActions, out ISprite iconSprite, out string keyTranslation)
		{
			if (controllerInputActions == 4 && this._inputGetActiveDevicePoller.GetActiveDevice() == 3 && this._controlSetting.TryToGetJoystickKeyIconSprite(22, this._inputGetActiveDevicePoller.GetLastJoystickHardware(), out iconSprite))
			{
				keyTranslation = null;
				return true;
			}
			return this._inputTranslation.TryToGetInputActionActiveDeviceAssetOrFallbackToTranslation(this.currentInputAction, ref iconSprite, ref keyTranslation);
		}

		private void UpdateShowingState()
		{
			if (GameHubBehaviour.Hub.ClientCounselorController.CurrentAdviceConfig.InputAction == -1 || !GameHubBehaviour.Hub.ClientCounselorController.IsPlaying)
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
			if (this.currentInputAction != GameHubBehaviour.Hub.ClientCounselorController.CurrentAdviceConfig.InputAction)
			{
				this.currentInputAction = GameHubBehaviour.Hub.ClientCounselorController.CurrentAdviceConfig.InputAction;
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
			if (GameHubBehaviour.Hub.ClientCounselorController.CurrentAdviceConfig.InputAction == -1)
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

		private void OnValidate()
		{
			if (this._controlParent == null && base.transform.childCount > 0)
			{
				this._controlParent = base.transform.GetChild(0).gameObject;
			}
		}

		private HudLifebarCounselor.State state = HudLifebarCounselor.State.Off;

		private HudLifebarCounselor.SubState substate;

		private ControllerInputActions currentInputAction;

		[SerializeField]
		private GameObject _controlParent;

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

		[InjectOnClient]
		private IInputTranslation _inputTranslation;

		[InjectOnClient]
		private IInputGetActiveDevicePoller _inputGetActiveDevicePoller;

		[InjectOnClient]
		private IControlSetting _controlSetting;

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
