using System;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Input;
using HeavyMetalMachines.Input.ControllerInput;
using HeavyMetalMachines.Presenting;
using Hoplon.Input;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.Tutorial.UnityUI
{
	public class UnityUiTutorialBottomPanel : MonoBehaviour, ITutorialPanelView
	{
		protected void Awake()
		{
			this._isVisible = false;
			TutorialUIController tutorialUIController = Object.FindObjectOfType<TutorialUIController>();
			this._tutorialBottomComponent = tutorialUIController.BottomPanelComponent;
		}

		protected void Start()
		{
			this._tutorialBottomComponent.RegisterView(this);
			this._activeDeviceChangeNotifierDisposable = ObservableExtensions.Subscribe<InputDevice>(this._inputActiveDeviceChangeNotifier.ObserveActiveDeviceChange(), delegate(InputDevice activeDevice)
			{
				if (this._isVisible)
				{
					this.UpdateInputDescriptionCellViewComponents();
				}
			});
		}

		private void OnDestroy()
		{
			if (this._activeDeviceChangeNotifierDisposable != null)
			{
				this._activeDeviceChangeNotifierDisposable.Dispose();
				this._activeDeviceChangeNotifierDisposable = null;
			}
		}

		public void SetVisibility(bool visibility)
		{
			this._isVisible = visibility;
			if (visibility)
			{
				base.GetComponent<Canvas>().enabled = true;
				this._animation.Play(this._animationInName);
				this.TrySetLabel(this._tutorialBottomComponent.TitleLabelDraft, this._titleLabel);
				this.UpdateDescriptionCellViewComponents(this._tutorialBottomComponent.DataDescriptions);
				this.UpdateInputDescriptionCellViewComponents();
			}
			else
			{
				this._animation.Play(this._animationOutName);
			}
		}

		private void TrySetLabel(string textTranslated, Text uiLabel)
		{
			if (string.IsNullOrEmpty(textTranslated))
			{
				uiLabel.enabled = false;
			}
			else
			{
				uiLabel.enabled = true;
				uiLabel.text = textTranslated;
			}
		}

		private void UpdateDescriptionCellViewComponents(TutorialBottomDataDescription[] dataDescriptions)
		{
			int i = 0;
			while (i < dataDescriptions.Length && i < this._descriptionCellViewComponents.Length)
			{
				this.SetDescriptionCellViewLabel(this._descriptionCellViewComponents[i], dataDescriptions[i].DescriptionTranslated);
				i++;
			}
			while (i < this._descriptionCellViewComponents.Length)
			{
				this.SetDescriptionCellViewLabel(this._descriptionCellViewComponents[i], string.Empty);
				i++;
			}
		}

		private void SetDescriptionCellViewLabel(ITutorialBottomPanelDescriptionCellView descriptionCellViewComponent, string descriptionTranslated)
		{
			descriptionCellViewComponent.DescriptionLabel.Text = descriptionTranslated;
		}

		private void UpdateInputDescriptionCellViewComponents()
		{
			TutorialBottomDataDescription[] dataDescriptions = this._tutorialBottomComponent.DataDescriptions;
			int i = 0;
			while (i < dataDescriptions.Length && i < this._descriptionCellViewComponents.Length)
			{
				ControllerInputActions controllerInputAction = dataDescriptions[i].ControllerInputAction;
				ISprite iconSprite = null;
				string empty = string.Empty;
				if (controllerInputAction != -1)
				{
					if (this._inputGetActiveDevicePoller.GetActiveDevice() == 3 && controllerInputAction == 4)
					{
						this._inputTranslation.TryToGetInputJoystickAssetOrFallbackToTranslation(22, ref iconSprite, ref empty);
					}
					else
					{
						this._inputTranslation.TryToGetInputActionActiveDeviceAssetOrFallbackToTranslation(controllerInputAction, ref iconSprite, ref empty);
					}
				}
				this.UpdateDescriptionCellViewComponent(this._descriptionCellViewComponents[i], iconSprite, empty);
				i++;
			}
			while (i < this._descriptionCellViewComponents.Length)
			{
				ActivatableExtensions.Deactivate(this._descriptionCellViewComponents[i].MainActivatable);
				i++;
			}
		}

		private void UpdateDescriptionCellViewComponent(ITutorialBottomPanelDescriptionCellView tutorialBottomPanelDescriptionCellView, ISprite iconSprite, string inputTranslation)
		{
			ActivatableExtensions.Activate(tutorialBottomPanelDescriptionCellView.MainActivatable);
			if (iconSprite != null)
			{
				ActivatableExtensions.Activate(tutorialBottomPanelDescriptionCellView.InputIconActivatable);
				tutorialBottomPanelDescriptionCellView.InputIconImage.Sprite = iconSprite;
			}
			else
			{
				ActivatableExtensions.Deactivate(tutorialBottomPanelDescriptionCellView.InputIconActivatable);
			}
			if (iconSprite == null && !string.IsNullOrEmpty(inputTranslation))
			{
				ActivatableExtensions.Activate(tutorialBottomPanelDescriptionCellView.KeyboardActivatable);
				tutorialBottomPanelDescriptionCellView.KeyboardLabel.Text = inputTranslation;
			}
			else
			{
				ActivatableExtensions.Deactivate(tutorialBottomPanelDescriptionCellView.KeyboardActivatable);
			}
		}

		private ITutorialBottomComponent _tutorialBottomComponent;

		[SerializeField]
		private Text _titleLabel;

		[SerializeField]
		private TutorialBottomPanelDescriptionCellView[] _descriptionCellViewComponents;

		[SerializeField]
		private Animation _animation;

		[SerializeField]
		private string _animationInName;

		[SerializeField]
		private string _animationOutName;

		[InjectOnClient]
		private IInputGetActiveDevicePoller _inputGetActiveDevicePoller;

		[InjectOnClient]
		private IInputActiveDeviceChangeNotifier _inputActiveDeviceChangeNotifier;

		[InjectOnClient]
		private IInputTranslation _inputTranslation;

		private bool _isVisible;

		private IDisposable _activeDeviceChangeNotifierDisposable;
	}
}
