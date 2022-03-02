using System;
using HeavyMetalMachines.Matches.DataTransferObjects;
using UnityEngine;

namespace HeavyMetalMachines.Pick
{
	[CreateAssetMenu(fileName = "PickModeConfig", menuName = "Config/PickModeConfig")]
	public class PickModeConfig : ScriptableObject, IPickModeConfigProvider
	{
		public MatchPickModeConfig Get(MatchKind kind)
		{
			for (int i = 0; i < this._specificConfiguration.Length; i++)
			{
				PickModeConfig.MatchPickConfigAggregation matchPickConfigAggregation = this._specificConfiguration[i];
				if (matchPickConfigAggregation.MatchKind == kind)
				{
					return matchPickConfigAggregation.Configuration;
				}
			}
			return this._defaultConfiguration;
		}

		[SerializeField]
		private MatchPickModeConfig _defaultConfiguration;

		[SerializeField]
		private PickModeConfig.MatchPickConfigAggregation[] _specificConfiguration;

		[Serializable]
		public struct MatchPickConfigAggregation
		{
			public MatchKind MatchKind;

			public MatchPickModeConfig Configuration;
		}
	}
}
