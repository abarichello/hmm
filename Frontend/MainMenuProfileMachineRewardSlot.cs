using System;
using HeavyMetalMachines.VFX;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class MainMenuProfileMachineRewardSlot : GameHubBehaviour
	{
		public void Setup(MainMenuProfileMachineRewardSlot.MachineRewardSlotInfo matchSlotInfo)
		{
			if (!string.Equals(this.IconSprite.SpriteName, matchSlotInfo.SpriteName, StringComparison.InvariantCultureIgnoreCase))
			{
				this.IconSprite.sprite2D = null;
			}
			this.IconSprite.SpriteName = matchSlotInfo.SpriteName;
			this.IconSprite.alpha = ((!matchSlotInfo.IsLocked) ? 1f : this.IconLockedAlpha);
			this.NameLabel.text = matchSlotInfo.RewardName;
			this.LockGameObject.SetActive(matchSlotInfo.IsLocked);
			this.LevelLabel.text = matchSlotInfo.LevelText;
		}

		[SerializeField]
		protected HMMUI2DDynamicSprite IconSprite;

		[SerializeField]
		protected float IconLockedAlpha = 0.4f;

		[SerializeField]
		protected UILabel NameLabel;

		[SerializeField]
		protected UILabel LevelLabel;

		[SerializeField]
		protected GameObject LockGameObject;

		private string _rewardName;

		public struct MachineRewardSlotInfo
		{
			public string RewardName;

			public string SpriteName;

			public string LevelText;

			public bool IsLocked;
		}
	}
}
