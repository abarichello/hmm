using System;
using System.Collections;
using HeavyMetalMachines.Utils;
using HeavyMetalMachines.VFX;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class HudInstancesMatchHighlightsPlayer : GameHubBehaviour
	{
		public void Setup(string title, string value, string playerName, Color playerColor, string playerPortraitSpriteName, Sprite roleSprite, bool isReplace)
		{
			this.NewHighlightPlayer.TitleLabel.text = title;
			this.NewHighlightPlayer.ValueLabel.text = value;
			this.NewHighlightPlayer.PlayerNameLabel.text = playerName;
			this.NewHighlightPlayer.PlayerNameLabel.color = playerColor;
			this.NewHighlightPlayer.PlayerPortraitSprite.SpriteName = playerPortraitSpriteName;
			this.NewHighlightPlayer.RoleSprite.sprite2D = roleSprite;
			this._isReplace = isReplace;
			if (!isReplace)
			{
				this.CopyNewDataToCurrent();
			}
		}

		private void CopyNewDataToCurrent()
		{
			this.HighlightPlayer.TitleLabel.text = this.NewHighlightPlayer.TitleLabel.text;
			this.HighlightPlayer.ValueLabel.text = this.NewHighlightPlayer.ValueLabel.text;
			this.HighlightPlayer.PlayerNameLabel.text = this.NewHighlightPlayer.PlayerNameLabel.text;
			this.HighlightPlayer.PlayerNameLabel.color = this.NewHighlightPlayer.PlayerNameLabel.color;
			this.HighlightPlayer.PlayerPortraitSprite.SpriteName = this.NewHighlightPlayer.PlayerPortraitSprite.SpriteName;
			this.HighlightPlayer.RoleSprite.sprite2D = this.NewHighlightPlayer.RoleSprite.sprite2D;
		}

		public void PlayInAnimation()
		{
			this.InAnimation.Play();
		}

		public bool TryToPlayReplaceAnimation()
		{
			if (!this._isReplace)
			{
				return false;
			}
			this.InAnimation.GetComponent<NGUIWidgetAlpha>().alpha = 1f;
			base.StartCoroutine(this.WaitReplaceAnimation());
			return true;
		}

		private IEnumerator WaitReplaceAnimation()
		{
			this.ReplaceInAnimation.GetComponent<NGUIWidgetAlpha>().alpha = 0f;
			this.ReplaceOutAnimation.GetComponent<NGUIWidgetAlpha>().alpha = 0f;
			this.ReplaceInAnimation.gameObject.SetActive(true);
			this.ReplaceInAnimation.Play();
			this.ReplaceOutAnimation.Play();
			while (this.ReplaceInAnimation.isPlaying || this.ReplaceOutAnimation.isPlaying)
			{
				yield return null;
				if (!base.gameObject.activeInHierarchy)
				{
					yield break;
				}
			}
			this.CopyNewDataToCurrent();
			this.ReplaceOutAnimation.GetComponent<NGUIWidgetAlpha>().alpha = 1f;
			this.ReplaceInAnimation.gameObject.SetActive(false);
			yield break;
		}

		[SerializeField]
		private Animation InAnimation;

		[SerializeField]
		private Animation ReplaceOutAnimation;

		[SerializeField]
		private Animation ReplaceInAnimation;

		[SerializeField]
		private HudInstancesMatchHighlightsPlayer.HighlightPlayerGuiComponent HighlightPlayer;

		[SerializeField]
		private HudInstancesMatchHighlightsPlayer.HighlightPlayerGuiComponent NewHighlightPlayer;

		private bool _isReplace;

		[Serializable]
		private struct HighlightPlayerGuiComponent
		{
			public UILabel TitleLabel;

			public UILabel ValueLabel;

			public UILabel PlayerNameLabel;

			public HMMUI2DDynamicSprite PlayerPortraitSprite;

			public UI2DSprite RoleSprite;
		}
	}
}
