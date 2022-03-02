using System;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using UnityEngine;

namespace HeavymetalMachines.ReportSystem
{
	public class UnityReportSystemPlayerView : MonoBehaviour, IReportSystemPlayerView
	{
		public ILabel PlayerNameLabel
		{
			get
			{
				return this._playerNameLabel;
			}
		}

		public ILabel PlayerTagLabel
		{
			get
			{
				return this._playerTagLabel;
			}
		}

		public IDynamicImage PlayerAvatarImage
		{
			get
			{
				return this._avatarImage;
			}
		}

		public IDynamicImage PlayerPortraitImage
		{
			get
			{
				return this._portraitImage;
			}
		}

		public IActivatable PsnIdIconActivatable
		{
			get
			{
				return this._psnIdIconActivatable;
			}
		}

		public ILabel PsnIdLabel
		{
			get
			{
				return this._psnIdLabel;
			}
		}

		[SerializeField]
		private UnityDynamicImage _avatarImage;

		[SerializeField]
		private UnityDynamicImage _portraitImage;

		[SerializeField]
		private UnityLabel _playerNameLabel;

		[SerializeField]
		private UnityLabel _playerTagLabel;

		[SerializeField]
		private GameObjectActivatable _psnIdIconActivatable;

		[SerializeField]
		private UnityLabel _psnIdLabel;
	}
}
