using System;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using HeavyMetalMachines.Utils;
using UnityEngine;

namespace HeavyMetalMachines.CompetitiveMode.View.Matches
{
	public class UnityCompetitiveMatchResultCalibrationMatchView : MonoBehaviour, ICompetitiveMatchResultCalibrationMatchView
	{
		public IActivatable MatchWonGroup
		{
			get
			{
				return this._matchWonGroup;
			}
		}

		public IActivatable MatchLostGroup
		{
			get
			{
				return this._matchLostGroup;
			}
		}

		public IAnimation RaiseAnimation
		{
			get
			{
				return this._raiseAnimation;
			}
		}

		public IAnimation ShowAnimation
		{
			get
			{
				return this._showAnimation;
			}
		}

		public IAnimation HideAnimation
		{
			get
			{
				return this._hideAnimation;
			}
		}

		public IAlpha WonAlpha
		{
			get
			{
				return this._wonAlpha;
			}
		}

		public IAlpha LostAlpha
		{
			get
			{
				return this._lostAlpha;
			}
		}

		public void SetWonImage()
		{
			this._image.mainTexture = this._wonTexture;
		}

		public void SetLostImage()
		{
			this._image.mainTexture = this._lostTexture;
		}

		[SerializeField]
		private UITexture _image;

		[SerializeField]
		private GameObjectActivatable _matchWonGroup;

		[SerializeField]
		private GameObjectActivatable _matchLostGroup;

		[SerializeField]
		private UnityAnimation _raiseAnimation;

		[SerializeField]
		private UnityAnimation _showAnimation;

		[SerializeField]
		private UnityAnimation _hideAnimation;

		[SerializeField]
		private Texture _wonTexture;

		[SerializeField]
		private Texture _lostTexture;

		[SerializeField]
		private NGUIWidgetAlpha _wonAlpha;

		[SerializeField]
		private NGUIWidgetAlpha _lostAlpha;
	}
}
