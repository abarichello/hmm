using System;
using System.Collections;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.CustomMatch;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Infra;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Social;
using Assets.Standard_Assets.Scripts.Infra.GUI.Hints;
using ClientAPI.Objects;
using HeavyMetalMachines.Frontend;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.VFX.PlotKids
{
	public class SocialController : SingletonMonoBehaviour<SocialController>
	{
		public ChatUiFeedbackDispatcher ChatUiFeedbackDispatcher
		{
			get
			{
				return this._chatUiFeedbackDispatcher;
			}
		}

		private void OnEnable()
		{
			this._hub = GameHubBehaviour.Hub;
			if (this._hub == null)
			{
				SingletonMonoBehaviour<SocialController>.Log.Error("[Social Controller] Hub should not be null!");
			}
			this._socialUiFeedbackDispatcher = ScriptableObject.CreateInstance<SocialUiFeedbackDispatcher>();
			this._socialUiFeedbackDispatcher.Init(this._hub, SingletonMonoBehaviour<PanelController>.Instance, ManagerController.Get<GroupManager>());
			this._matchUiFeedbackDispatcher = ScriptableObject.CreateInstance<MatchUiFeedbackDispatcher>();
			this._matchUiFeedbackDispatcher.Init(this._hub, SingletonMonoBehaviour<PanelController>.Instance, ManagerController.Get<MatchManager>());
			this._chatUiFeedbackDispatcher = ScriptableObject.CreateInstance<ChatUiFeedbackDispatcher>();
			base.StartCoroutine(this.WaitHudWindowInit());
		}

		private IEnumerator WaitHudWindowInit()
		{
			while (!HudWindowManager.DoesInstanceExist())
			{
				yield return null;
			}
			this._chatUiFeedbackDispatcher.Init(SingletonMonoBehaviour<PanelController>.Instance, HudWindowManager.Instance, ManagerController.Get<ChatManager>());
			yield break;
		}

		public void OpenSteamFriendInvite(string universalID)
		{
			if (!this.CheckOverlayEnabled())
			{
				return;
			}
			this._hub.ClientApi.overlay.AddFriend.Show(universalID);
		}

		public void OpenSteamChatWithFriend(UserFriend userFriend)
		{
			if (!this.CheckOverlayEnabled())
			{
				return;
			}
			this._hub.ClientApi.overlay.Chat.Show(userFriend.UniversalID);
		}

		public void OpenSteamPlayerProfile(string universalID)
		{
			if (!this.CheckOverlayEnabled())
			{
				return;
			}
			this._hub.ClientApi.overlay.Profile.Show(universalID);
		}

		private bool CheckOverlayEnabled()
		{
			if (this._hub.ClientApi.overlay.IsOverlayEnabled())
			{
				return true;
			}
			SingletonMonoBehaviour<PanelController>.Instance.SendSystemMessage(Language.Get("STEAM_OVERLAY_NOT_ENABLED", TranslationSheets.Friends), "SystemMessage", true, false, StackableHintKind.None, HintColorScheme.Refused);
			return false;
		}

		private HMMHub _hub;

		public SocialConfig SocialConfiguration;

		private MatchUiFeedbackDispatcher _matchUiFeedbackDispatcher;

		private SocialUiFeedbackDispatcher _socialUiFeedbackDispatcher;

		private ChatUiFeedbackDispatcher _chatUiFeedbackDispatcher;
	}
}
