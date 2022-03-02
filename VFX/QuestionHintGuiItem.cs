using System;
using HeavyMetalMachines.Input;
using HeavyMetalMachines.Input.ControllerInput;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using Hoplon.Input;
using Hoplon.Input.UiNavigation;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.VFX
{
	public class QuestionHintGuiItem : BaseHintGuiItem<QuestionHintGuiItem, QuestionHintContent>
	{
		protected override void SetPropertiesTasks(QuestionHintContent referenceObject)
		{
			base.SetPropertiesTasks(referenceObject);
			this.ObserveInputNavigation();
		}

		private void ObserveInputNavigation()
		{
			IDisposable disposable = ObservableExtensions.Subscribe<long>(Observable.Do<long>(Observable.EveryUpdate(), delegate(long _)
			{
				this.CheckForGetInputNavigationFocus();
			}));
			this._compositeDisposable.Add(disposable);
			IDisposable disposable2 = ObservableExtensions.Subscribe<InputDevice>(Observable.Do<InputDevice>(this._activeDeviceChangeNotifier.GetAndObserveActiveDeviceChange(), new Action<InputDevice>(this.CheckForActiveDevice)));
			this._compositeDisposable.Add(disposable2);
		}

		private void CheckForGetInputNavigationFocus()
		{
			if (this._controllerInputActionPoller.GetButtonDown(this._selectInputAction))
			{
				this._uiNavigationGroupHolder.AddHighPriorityGroup();
				this._highlightGroupGameObject.SetActive(true);
			}
		}

		private void CheckForActiveDevice(InputDevice device)
		{
			this.UpdateJoystickButtonToFocusImage();
			this._focusShortcutSprite.gameObject.SetActive(device == 3);
		}

		private void UpdateJoystickButtonToFocusImage()
		{
			int selectInputAction = this._selectInputAction;
			ISprite sprite;
			string text;
			if (this._inputTranslation.TryToGetInputActionJoystickAssetOrFallbackToTranslation(selectInputAction, ref sprite, ref text))
			{
				this._focusShortcutSprite.sprite2D = (sprite as UnitySprite).GetSprite();
			}
		}

		public void onButtonClick_Approve()
		{
			this.SetAsFinished(true, false);
		}

		public void onButtonClick_Refuse()
		{
			this.SetAsFinished(false, false);
		}

		protected override void SetAsFinished()
		{
			this.SetAsFinished(false, true);
		}

		private void SetAsFinished(bool isApproved, bool hasTimedOut)
		{
			if (this._isFinished)
			{
				return;
			}
			base.DismissQuestionHint();
			if (isApproved)
			{
				base.ReferenceObject.ApproveAction();
				return;
			}
			if (!hasTimedOut)
			{
				base.ReferenceObject.RefuseAction();
				return;
			}
			if (base.ReferenceObject.ExecuteRefuseActionOnTimeout && base.ReferenceObject.RefuseAction != null)
			{
				base.ReferenceObject.RefuseAction();
			}
		}

		private void OnDestroy()
		{
			this._compositeDisposable.Dispose();
		}

		[SerializeField]
		private UIButton _accept_Button;

		[SerializeField]
		private UIButton _refuse_Button;

		[SerializeField]
		private UI2DSprite _focusShortcutSprite;

		[SerializeField]
		private GameObject _highlightGroupGameObject;

		[SerializeField]
		private UiNavigationGroupHolder _uiNavigationGroupHolder;

		[SerializeField]
		private ControllerInputActions _selectInputAction = 44;

		[Inject]
		private IInputTranslation _inputTranslation;

		[Inject]
		private IInputActiveDeviceChangeNotifier _activeDeviceChangeNotifier;

		[Inject]
		private IControllerInputActionPoller _controllerInputActionPoller;

		private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();
	}
}
