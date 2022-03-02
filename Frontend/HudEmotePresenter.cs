using System;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using Assets.Customization;
using FMod;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.DataTransferObjects.Battlepass;
using HeavyMetalMachines.Logs;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Players.Business;
using Hoplon.Logging;
using UniRx;

namespace HeavyMetalMachines.Frontend
{
	public class HudEmotePresenter : IHudEmotePresenter
	{
		public HudEmotePresenter(IHudEmoteView view, IMatchPlayers matchPlayers, ICustomizationAssets customizationAssets, IIsPlayerRestrictedByTextChat isPlayerRestrictedByTextChat)
		{
			this._customizationAssets = customizationAssets;
			this._isPlayerRestrictedByTextChat = isPlayerRestrictedByTextChat;
			this._logger = new BitLogLogger<HudEmotePresenter>();
			this._matchPlayers = matchPlayers;
			this._view = view;
			this._dueTime = TimeSpan.FromSeconds(1.7999999523162842);
		}

		public void Initialize(int playerCarId)
		{
			this._disposable = ObservableExtensions.Subscribe<Unit>(Observable.Switch<Unit>(Observable.Select<GadgetSlot, IObservable<Unit>>(Observable.Do<GadgetSlot>(this._onEmotePlayedSubject, delegate(GadgetSlot slot)
			{
				this.UpdateEmoteContent(playerCarId, slot);
			}), (GadgetSlot _) => Observable.ContinueWith<Unit, Unit>(this.PlayAnimationIn(), this.PlayAnimationOut()))));
		}

		public void PlayEmote(GadgetSlot slot, int playerCarId)
		{
			PlayerData playerByObjectId = this._matchPlayers.GetPlayerByObjectId(playerCarId);
			if (this.IsPlayerRestricted(playerByObjectId.PlayerId))
			{
				return;
			}
			this._onEmotePlayedSubject.OnNext(slot);
		}

		private IObservable<Unit> UpdateEmoteContent(int playerCarId, GadgetSlot slot)
		{
			PlayerData playerByObjectId = this._matchPlayers.GetPlayerByObjectId(playerCarId);
			PlayerCustomizationSlot playerCustomizationSlot = HudEmotePresenter.GetPlayerCustomizationSlot(slot);
			ItemTypeScriptableObject itemTypeScriptableObjectBySlot = this._customizationAssets.GetItemTypeScriptableObjectBySlot(playerCustomizationSlot, playerByObjectId.Customizations);
			EmoteItemTypeComponent component = itemTypeScriptableObjectBySlot.GetComponent<EmoteItemTypeComponent>();
			this._view.SpritesheetAnimator.TryToLoadAsset(component.spriteSheetName);
			this._view.SpritesheetAnimator.StartAnimation();
			this.PlayAudio(component, playerByObjectId);
			return Observable.ReturnUnit();
		}

		private IObservable<Unit> PlayAnimationIn()
		{
			return Observable.Merge<Unit>(new IObservable<Unit>[]
			{
				this._view.AnimationIn.Play(),
				Observable.Delay<Unit>(Observable.ReturnUnit(), this._dueTime)
			});
		}

		private IObservable<Unit> PlayAnimationOut()
		{
			return this._view.AnimationOut.Play();
		}

		private bool IsPlayerRestricted(long playerId)
		{
			return this._isPlayerRestrictedByTextChat.IsPlayerRestricted(playerId);
		}

		private void PlayAudio(EmoteItemTypeComponent emoteComponent, PlayerData playerData)
		{
			if (emoteComponent.audio == null || playerData.CharacterInstance == null)
			{
				return;
			}
			if (this._audioToken != null && !this._audioToken.IsInvalidated())
			{
				this._audioToken.Stop();
			}
			this._audioToken = FMODAudioManager.PlayAtVolume(emoteComponent.audio, playerData.CharacterInstance.transform, 1f, false);
		}

		private static PlayerCustomizationSlot GetPlayerCustomizationSlot(GadgetSlot slot)
		{
			switch (slot)
			{
			case GadgetSlot.EmoteGadget0:
				return 40;
			case GadgetSlot.EmoteGadget1:
				return 41;
			case GadgetSlot.EmoteGadget2:
				return 42;
			case GadgetSlot.EmoteGadget3:
				return 43;
			case GadgetSlot.EmoteGadget4:
				return 44;
			default:
				return 0;
			}
		}

		public void Stop()
		{
			this._view.SpritesheetAnimator.Stop();
		}

		~HudEmotePresenter()
		{
			this.Dispose();
		}

		public void Dispose()
		{
			if (this._timer != null)
			{
				this._timer.Dispose();
				this._timer = null;
			}
			this._disposable.Dispose();
		}

		private const string EmoteIn = "Emote_in";

		private const string EmoteOut = "Emote_out";

		private readonly IHudEmoteView _view;

		private readonly IMatchPlayers _matchPlayers;

		private readonly ICustomizationAssets _customizationAssets;

		private readonly IIsPlayerRestrictedByTextChat _isPlayerRestrictedByTextChat;

		private readonly ILogger<HudEmotePresenter> _logger;

		private readonly TimeSpan _dueTime;

		private IDisposable _timer;

		private FMODAudioManager.FMODAudio _audioToken;

		private IDisposable _disposable;

		private readonly Subject<GadgetSlot> _onEmotePlayedSubject = new Subject<GadgetSlot>();
	}
}
