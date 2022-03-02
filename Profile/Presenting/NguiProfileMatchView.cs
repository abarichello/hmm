using System;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.NGui;
using HeavyMetalMachines.Social.Profile.Presenting;
using UnityEngine;

namespace HeavyMetalMachines.Profile.Presenting
{
	public class NguiProfileMatchView : MonoBehaviour, IProfileMatchView
	{
		public ISpriteImage ResultIndicatorImage
		{
			get
			{
				return this._resultIndicatorImage;
			}
		}

		public ILabel ResultLabel
		{
			get
			{
				return this._resultLabel;
			}
		}

		public IImage ResultGlowImage
		{
			get
			{
				return this._resultGlowImage;
			}
		}

		public IDynamicImage CharacterIconImage
		{
			get
			{
				return this._characterIconImage;
			}
		}

		public ILabel CharacterNameLabel
		{
			get
			{
				return this._characterNameLabel;
			}
		}

		public ILabel GameModelLabel
		{
			get
			{
				return this._gameModelLabel;
			}
		}

		public ILabel ArenaLabel
		{
			get
			{
				return this._arenaLabel;
			}
		}

		public IDynamicImage PerformanceIconImage
		{
			get
			{
				return this._performanceIconImage;
			}
		}

		public ILabel DateLabel
		{
			get
			{
				return this._dateLabel;
			}
		}

		public ILabel TimeLabel
		{
			get
			{
				return this._timeLabel;
			}
		}

		public void Activate()
		{
			base.gameObject.SetActive(true);
		}

		public void Deactivate()
		{
			base.gameObject.SetActive(false);
		}

		[SerializeField]
		private NGuiImage _resultIndicatorImage;

		[SerializeField]
		private NGuiLabel _resultLabel;

		[SerializeField]
		private NGuiImage _resultGlowImage;

		[SerializeField]
		private NGuiDynamicTextureOrSprite _characterIconImage;

		[SerializeField]
		private NGuiLabel _characterNameLabel;

		[SerializeField]
		private NGuiLabel _gameModelLabel;

		[SerializeField]
		private NGuiLabel _arenaLabel;

		[SerializeField]
		private NGuiDynamicTextureOrSprite _performanceIconImage;

		[SerializeField]
		private NGuiLabel _dateLabel;

		[SerializeField]
		private NGuiLabel _timeLabel;
	}
}
