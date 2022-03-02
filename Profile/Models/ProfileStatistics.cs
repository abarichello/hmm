using System;
using System.Collections.Generic;
using HeavyMetalMachines.Presenting.Unity;
using HeavyMetalMachines.Social.Profile.Models;
using Hoplon.Logging;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Profile.Models
{
	[CreateAssetMenu(menuName = "Scriptable Object/Profile/Statistics")]
	public class ProfileStatistics : ScriptableObject
	{
		public ProfileStatisticViewModel[] ViewModels
		{
			get
			{
				return this._viewModels;
			}
		}

		private Dictionary<ProfileStatistic, UnitySprite> SpriteDictionary
		{
			get
			{
				if (this._spriteDictionary == null)
				{
					this.BuildDictionary();
				}
				return this._spriteDictionary;
			}
		}

		public UnitySprite GetUnitySpriteByStatistic(ProfileStatistic statistic)
		{
			if (this.SpriteDictionary.ContainsKey(statistic))
			{
				return this.SpriteDictionary[statistic];
			}
			string message = string.Format("There is no icon defined to statistic '{0}'.", statistic);
			throw new KeyNotFoundException(message);
		}

		private void BuildDictionary()
		{
			this._spriteDictionary = new Dictionary<ProfileStatistic, UnitySprite>();
			foreach (ProfileStatisticViewModel profileStatisticViewModel in this._viewModels)
			{
				if (!this._spriteDictionary.ContainsKey(profileStatisticViewModel.Statistic))
				{
					this._spriteDictionary.Add(profileStatisticViewModel.Statistic, profileStatisticViewModel.Icon);
				}
				else
				{
					string text = string.Format("Duplicated icon is defined as '{0}'.", profileStatisticViewModel.Statistic);
					this._logger.Error(text);
				}
			}
		}

		[SerializeField]
		private ProfileStatisticViewModel[] _viewModels;

		[Inject]
		private ILogger<ProfileStatistics> _logger;

		private Dictionary<ProfileStatistic, UnitySprite> _spriteDictionary;
	}
}
