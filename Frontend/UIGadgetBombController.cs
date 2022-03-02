using System;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.VFX;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class UIGadgetBombController : GameHubBehaviour, PlayerBuildComplete.IPlayerBuildCompleteListener
	{
		public void OnPlayerBuildComplete(PlayerBuildComplete evt)
		{
			if (GameHubBehaviour.Hub.Players.CurrentPlayerData == null)
			{
				return;
			}
			if (GameHubBehaviour.Hub.Players.CurrentPlayerData.CharacterInstance == null)
			{
				return;
			}
			if (evt.Id != GameHubBehaviour.Hub.Players.CurrentPlayerData.CharacterInstance.ObjId)
			{
				return;
			}
			GameHubBehaviour.Hub.BombManager.ListenToBombCarrierChanged += this.BombManagerOnListenToBombCarrierChanged;
		}

		private void Awake()
		{
			this.UpdateInterface(false);
		}

		private void OnDestroy()
		{
			GameHubBehaviour.Hub.BombManager.ListenToBombCarrierChanged -= this.BombManagerOnListenToBombCarrierChanged;
		}

		private void BombManagerOnListenToBombCarrierChanged(CombatObject carrier)
		{
			this.UpdateInterface(carrier != null && GameHubBehaviour.Hub.Players.CurrentPlayerData.PlayerId == carrier.Player.PlayerId);
		}

		private void UpdateInterface(bool isPlayerCarryingBomb)
		{
			this._isPlayerCarryingBomb = isPlayerCarryingBomb;
			this.BombDisabledSprite.gameObject.SetActive(!isPlayerCarryingBomb);
			this.BombSpriteAnimation.gameObject.SetActive(isPlayerCarryingBomb);
			this.BorderSprite.color = ((!isPlayerCarryingBomb) ? this.BombDisabledSprite.color : this.BombEnabledSprite.color);
		}

		private void Update()
		{
			if (!this._isPlayerCarryingBomb)
			{
				return;
			}
			this._spriteAnimationTimeInSec += Time.deltaTime;
			this.BombSpriteAnimation.animationTime = this._spriteAnimationTimeInSec / this.SpriteAnimationTime;
			if (this._spriteAnimationTimeInSec > this.SpriteAnimationTime)
			{
				this._spriteAnimationTimeInSec = 0f;
			}
		}

		public void UpdateKey(string keyText)
		{
			this.KeyLabel.text = keyText;
			this.KeyLabel.gameObject.SetActive(true);
			this.KeySprite.gameObject.SetActive(false);
		}

		public void UpdateKey(Sprite keySprite)
		{
			this.KeySprite.sprite2D = keySprite;
			this.KeySprite.gameObject.SetActive(true);
			this.KeyLabel.gameObject.SetActive(false);
		}

		public float SpriteAnimationTime = 0.5f;

		public UILabel KeyLabel;

		public UI2DSprite KeySprite;

		public UI2DSprite BombEnabledSprite;

		public UI2DSprite BombDisabledSprite;

		public HMM2DSpriteAnimation BombSpriteAnimation;

		public UI2DSprite BorderSprite;

		private bool _isPlayerCarryingBomb;

		private float _spriteAnimationTimeInSec;
	}
}
