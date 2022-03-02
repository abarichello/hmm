using System;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using UnityEngine;

namespace HeavyMetalMachines.MuteSystem
{
	public class UnityMuteSystemSubtitleView : MonoBehaviour, IMuteSystemSubtitleView
	{
		public IActivatable MainActivatable
		{
			get
			{
				return this._mainGameObjectActivatable;
			}
		}

		public ISpriteImage IconSpriteImage
		{
			get
			{
				return this._iconImage;
			}
		}

		public ILabel TitleLabel
		{
			get
			{
				return this._titleLabel;
			}
		}

		public ILabel SubTitleLabel
		{
			get
			{
				return this._subTitleLabel;
			}
		}

		public void GetSubtitleDrafts(MuteSystemSubtitleType subtitleType, out string titleDraft, out string subTitleDraft)
		{
			MuteSystemSubtitleInfo subtitleInfo = this.GetSubtitleInfo(subtitleType);
			titleDraft = subtitleInfo.TitleDraft;
			subTitleDraft = subtitleInfo.SubtitleDraft;
		}

		public ISprite GetIconSprite(MuteSystemSubtitleType subtitleType)
		{
			MuteSystemSubtitleInfo subtitleInfo = this.GetSubtitleInfo(subtitleType);
			return new UnitySprite(subtitleInfo.IconSprite);
		}

		private MuteSystemSubtitleInfo GetSubtitleInfo(MuteSystemSubtitleType subtitleType)
		{
			foreach (MuteSystemSubtitleInfo muteSystemSubtitleInfo in this._subtitleInfos)
			{
				if (muteSystemSubtitleInfo.SubtitleType == subtitleType)
				{
					return muteSystemSubtitleInfo;
				}
			}
			throw new ArgumentException("Subtitle info not found for " + subtitleType);
		}

		[SerializeField]
		private GameObjectActivatable _mainGameObjectActivatable;

		[SerializeField]
		private UnityImage _iconImage;

		[SerializeField]
		private UnityLabel _titleLabel;

		[SerializeField]
		private UnityLabel _subTitleLabel;

		[SerializeField]
		private MuteSystemSubtitleInfo[] _subtitleInfos;
	}
}
