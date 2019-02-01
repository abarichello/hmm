using System;
using System.Collections.Generic;
using ClientAPI.Objects;
using FMod;
using HeavyMetalMachines.VFX.PlotKids;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class FriendFilterGuiItem : MonoBehaviour
	{
		public bool IsEnabled
		{
			get
			{
				return this._isEnabled;
			}
		}

		private void Start()
		{
			this._hub = GameHubBehaviour.Hub;
			this._playerPrefsPropertyName = string.Format("FriendFilter_State_{0}", (!string.IsNullOrEmpty(this._overridePlayerPrefsName)) ? this._overridePlayerPrefsName : this._targetFriendState.ToString());
			if (this._parentUI == null)
			{
				this._parentUI = base.GetComponentInParent<SocialModalGUI>();
			}
			this._isEnabled = (this._hub.PlayerPrefs.GetInt(this._playerPrefsPropertyName, 1) == 1);
			this.UpdateFriendFilterState(this._isEnabled, true);
			this.UpdateTitleText();
		}

		private void Update()
		{
			this.TryUpdateFriendCount();
		}

		public void TryUpdateFriendCount()
		{
			int num = this.CurrentFriendCount;
			for (int i = 0; i < this._childFriendFilters.Length; i++)
			{
				num += this._childFriendFilters[i].CurrentFriendCount;
			}
			if (num == this._currentFriendCount)
			{
				return;
			}
			this._currentFriendCount = num;
			this.UpdateTitleText();
		}

		private void UpdateTitleText()
		{
			this._textLabel.text = string.Format("{0} ({1})", Language.Get(this._i18Key, TranslationSheets.Friends), this._currentFriendCount);
		}

		public void onButtonClick_ToggleFilter()
		{
			this.FriendFilterToggleState(!this._isEnabled);
			this._hub.PlayerPrefs.SetInt(this._playerPrefsPropertyName, (!this._isEnabled) ? 0 : 1);
			this._hub.PlayerPrefs.Save();
		}

		private void UpdateFriendFilterState(bool targetEnabledState, bool ignoreAudioPlayback = false)
		{
			this._isEnabled = targetEnabledState;
			this._arrowSpriteState.transform.localEulerAngles = Vector3.forward * (float)((!this._isEnabled) ? 90 : 0);
			if (this._childFriendFilters != null && this._childFriendFilters.Length > 0)
			{
				for (int i = 0; i < this._childFriendFilters.Length; i++)
				{
					this._childFriendFilters[i].FriendFilterToggleState(this._isEnabled);
					if (!ignoreAudioPlayback)
					{
						if (this._childFriendFilters[i].gameObject.activeSelf)
						{
							FMODAudioManager.PlayOneShotAt(this._parentUI.sfx_ui_chat_filter_maximize, base.transform.position, 0);
						}
						else
						{
							FMODAudioManager.PlayOneShotAt(this._parentUI.sfx_ui_chat_filter_minimize, base.transform.position, 0);
						}
					}
				}
				return;
			}
			foreach (KeyValuePair<string, FriendGuiItem> keyValuePair in this._parentUI.FriendGuiItemByIDDictionary)
			{
				UserFriend referenceObject = keyValuePair.Value.ReferenceObject;
				if (this._targetFriendState == referenceObject.GetHmmFriendState())
				{
					keyValuePair.Value.gameObject.SetActive(this._isEnabled);
				}
			}
			this._parentUI.UpdatePlayerListSorting();
		}

		public void onTogleClick_SendInviteForFriends()
		{
			if (!this._inviteToggle.activeSprite)
			{
				return;
			}
		}

		public void FriendFilterToggleState(bool filterState)
		{
			this._isEnabled = filterState;
			this.UpdateFriendFilterState(this._isEnabled, false);
		}

		[SerializeField]
		private SocialModalGUI _parentUI;

		private bool _isEnabled = true;

		[SerializeField]
		private HmmFriendState _targetFriendState;

		[SerializeField]
		[Tooltip("We can use this player prefs name to Main Filters, such as: Steam, HMM, etc. \nSubfilters use Friend State as Player Prefs property name!")]
		private string _overridePlayerPrefsName;

		[SerializeField]
		private string _i18Key;

		[SerializeField]
		private UILabel _textLabel;

		[SerializeField]
		private UI2DSprite _arrowSpriteState;

		private string _playerPrefsPropertyName;

		private int _currentFriendCount;

		[NonSerialized]
		public int CurrentFriendCount;

		[SerializeField]
		private UIToggle _inviteToggle;

		private HMMHub _hub;

		[SerializeField]
		private FriendFilterGuiItem[] _childFriendFilters;
	}
}
