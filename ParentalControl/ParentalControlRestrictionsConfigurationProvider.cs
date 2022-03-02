using System;
using HeavyMetalMachines.ParentalControl.Restrictions;
using UnityEngine;

namespace HeavyMetalMachines.ParentalControl
{
	[CreateAssetMenu(menuName = "ParentalControlRestrictionsConfigurationProvider")]
	public class ParentalControlRestrictionsConfigurationProvider : ScriptableObject, IParentalControlRestrictionsConfigurationProvider
	{
		public string PlayerNameReplacement
		{
			get
			{
				return this._playerNameReplacement;
			}
		}

		public string TeamNameReplacement
		{
			get
			{
				return this._teamNameReplacement;
			}
		}

		public string TeamTagReplacement
		{
			get
			{
				return this._teamTagReplacement;
			}
		}

		[SerializeField]
		private string _playerNameReplacement;

		[SerializeField]
		private string _teamNameReplacement;

		[SerializeField]
		private string _teamTagReplacement;
	}
}
