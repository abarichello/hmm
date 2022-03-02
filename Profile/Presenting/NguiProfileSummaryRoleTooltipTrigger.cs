using System;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.ToggleableFeatures;
using Hoplon.ToggleableFeatures;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Profile.Presenting
{
	public class NguiProfileSummaryRoleTooltipTrigger : MonoBehaviour
	{
		private void Awake()
		{
			if (!this._isFeatureToggled.Check(Features.ProfileRefactor))
			{
				Object.Destroy(this);
				return;
			}
			this._tooltipInfo = this.CreateTooltipInfo();
			this._onHoverBeginEventDelegate = new EventDelegate(new EventDelegate.Callback(this.OnHoverBegin));
			this._onHoverEndEventDelegate = new EventDelegate(new EventDelegate.Callback(this.OnHoverEnd));
		}

		private void OnEnable()
		{
			this._eventTrigger.onHoverOver.Add(this._onHoverBeginEventDelegate);
			this._eventTrigger.onHoverOut.Add(this._onHoverEndEventDelegate);
		}

		private void OnDisable()
		{
			this._eventTrigger.onHoverOver.Remove(this._onHoverBeginEventDelegate);
			this._eventTrigger.onHoverOut.Add(this._onHoverEndEventDelegate);
		}

		private void OnHoverBegin()
		{
			if (!this._hub.GuiScripts.TooltipController.IsVisible())
			{
				this._hub.GuiScripts.TooltipController.ToggleOpenWindow(this._tooltipInfo);
			}
		}

		private void OnHoverEnd()
		{
			this._hub.GuiScripts.TooltipController.HideWindow();
		}

		private TooltipInfo CreateTooltipInfo()
		{
			return new TooltipInfo(TooltipInfo.TooltipType.Normal, TooltipInfo.DescriptionSummaryType.None, PreferredDirection.Left, this._sprite, string.Empty, Language.Get(this._titleDraft, this._titleTranslationSheet), string.Empty, Language.Get(this._descriptionDraft, this._descriptionTranslationSheet), string.Empty, string.Empty, string.Empty, this._targetPosition.position, string.Empty);
		}

		[SerializeField]
		private UIEventTrigger _eventTrigger;

		[SerializeField]
		private Transform _targetPosition;

		[SerializeField]
		private Sprite _sprite;

		[SerializeField]
		private string _titleDraft;

		[SerializeField]
		private TranslationSheets _titleTranslationSheet;

		[SerializeField]
		private string _descriptionDraft;

		[SerializeField]
		private TranslationSheets _descriptionTranslationSheet;

		[Inject]
		private HMMHub _hub;

		[Inject]
		private IIsFeatureToggled _isFeatureToggled;

		private TooltipInfo _tooltipInfo;

		private EventDelegate _onHoverBeginEventDelegate;

		private EventDelegate _onHoverEndEventDelegate;
	}
}
