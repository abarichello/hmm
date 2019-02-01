using System;
using HeavyMetalMachines.Character;
using HeavyMetalMachines.VFX;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class MainMenuProfileCharacterCard : MonoBehaviour
	{
		public void SetCharacterName(string characterName)
		{
			this.CharacterNameLabel.text = characterName;
		}

		public void SetCharacterSprite(string assetName)
		{
			this.CharacterSprite.SpriteName = assetName;
		}

		public void SetCharacterLevelProgressBarVisibility(bool visible)
		{
			this.CharacterLevelProgressBar.transform.parent.gameObject.SetActive(visible);
		}

		public void SetInfo(float level, float normalizedLevelInfo, HeavyMetalMachines.Character.CharacterInfo.DriverRoleKind role)
		{
			this.CharacterLevelProgressBar.value = normalizedLevelInfo;
			level += 1f;
			this.CharacterLevelLabel.text = level.ToString("0");
			this._level = level;
			this._normalizedLevelInfo = normalizedLevelInfo;
			this._role = role;
		}

		public void SetCornerVisibility(bool visible)
		{
			this.StampCornerCircleSprite.transform.parent.gameObject.SetActive(false);
		}

		public void SetCornerInfo(Sprite circle, Sprite icon)
		{
			this.StampCornerCircleSprite.sprite2D = circle;
			this.StampCornerIconSprite.sprite2D = icon;
		}

		public void SetLockGroupVisibility(bool visible)
		{
			if (this.LockGroupGameObject == null)
			{
				return;
			}
			this.LockGroupGameObject.SetActive(visible);
		}

		public void SetButtonEventListener(int id)
		{
			this.ButtonEventListener.IntParameter = id;
		}

		public HeavyMetalMachines.Character.CharacterInfo.DriverRoleKind GetRole()
		{
			return this._role;
		}

		public float GetLevel()
		{
			return this._level + this._normalizedLevelInfo;
		}

		public int Compare(MainMenuProfileMachines.FilterType filterType, MainMenuProfileCharacterCard otherCard)
		{
			switch (filterType)
			{
			case MainMenuProfileMachines.FilterType.ZtoA:
				return -string.Compare(this.CharacterNameLabel.text, otherCard.CharacterNameLabel.text, StringComparison.OrdinalIgnoreCase);
			case MainMenuProfileMachines.FilterType.Role:
			{
				int num = string.Compare(this.GetRole().ToString(), otherCard.GetRole().ToString(), StringComparison.OrdinalIgnoreCase);
				if (num != 0)
				{
					return num;
				}
				break;
			}
			case MainMenuProfileMachines.FilterType.Level:
				return -this.GetLevel().CompareTo(otherCard.GetLevel());
			}
			return string.Compare(this.CharacterNameLabel.text, otherCard.CharacterNameLabel.text, StringComparison.OrdinalIgnoreCase);
		}

		[SerializeField]
		protected UILabel CharacterNameLabel;

		[SerializeField]
		protected HMMUI2DDynamicSprite CharacterSprite;

		[SerializeField]
		protected UIProgressBar CharacterLevelProgressBar;

		[SerializeField]
		protected UILabel CharacterLevelLabel;

		[SerializeField]
		protected UI2DSprite StampCornerCircleSprite;

		[SerializeField]
		protected UI2DSprite StampCornerIconSprite;

		[SerializeField]
		protected GameObject LockGroupGameObject;

		[SerializeField]
		protected GUIEventListener ButtonEventListener;

		private float _level;

		private float _normalizedLevelInfo;

		private HeavyMetalMachines.Character.CharacterInfo.DriverRoleKind _role;
	}
}
