using System;
using HeavyMetalMachines.CompetitiveMode.Divisions;
using HeavyMetalMachines.CompetitiveMode.Players;
using HeavyMetalMachines.Localization;
using Hoplon.Localization.TranslationTable;

namespace HeavyMetalMachines.CompetitiveMode.View
{
	public class CompetitiveDivisionsBadgeNameBuilder : ICompetitiveDivisionsBadgeNameBuilder
	{
		public CompetitiveDivisionsBadgeNameBuilder(IGetCompetitiveDivisions getCompetitiveDivisions, ILocalizeKey translation)
		{
			this._getCompetitiveDivisions = getCompetitiveDivisions;
			this._translation = translation;
		}

		private Division GetDivision(CompetitiveRank rank)
		{
			if (rank.TopPlacementPosition != null)
			{
				return this._topDivision;
			}
			Division[] array = this._getCompetitiveDivisions.Get();
			return array[rank.Division];
		}

		public string GetDivisionBadgeFileNameFromCompetitiveRank(CompetitiveRank rank, CompetitiveBadgeSize badgeSize)
		{
			Division division = this.GetDivision(rank);
			return this.GetDivisionBadgeFileName(division, badgeSize);
		}

		public string GetDivisionBadgeFileName(Division division, CompetitiveBadgeSize badgeSize)
		{
			string sizeFromBadgeSize = this.GetSizeFromBadgeSize(badgeSize);
			return string.Format("{0}_icon_{1}", division.NameDraft, sizeFromBadgeSize);
		}

		public string GetDivisionBadgeBackgroundFileName(Division division)
		{
			return string.Format("{0}_background", division.NameDraft);
		}

		public string GetSubdivisionBadgeFileName(CompetitiveRank rank, CompetitiveBadgeSize badgeSize)
		{
			Division division = this.GetDivision(rank);
			if (division == this._topDivision)
			{
				return this.GetTopDivisionBadgeFileName(badgeSize);
			}
			return this.GetSubdivisionBadgeFileName(division, rank.Subdivision, badgeSize);
		}

		public string GetSubdivisionBadgeFileName(Division division, int subdivisionIndex, CompetitiveBadgeSize badgeSize)
		{
			string subDivisionFromIndex = this.GetSubDivisionFromIndex(division.Subdivisions, subdivisionIndex);
			string sizeFromBadgeSize = this.GetSizeFromBadgeSize(badgeSize);
			return string.Format("{0}_{1}_{2}", division.NameDraft, subDivisionFromIndex, sizeFromBadgeSize);
		}

		public string GetSubdivisionNumberFileName(Division division, int subdivisionIndex)
		{
			string subDivisionFromIndex = this.GetSubDivisionFromIndex(division.Subdivisions, subdivisionIndex);
			return string.Format("{0}_number_{1}", division.NameDraft, subDivisionFromIndex);
		}

		public string GetTopDivisionBadgeFileName(CompetitiveBadgeSize badgeSize)
		{
			return this.GetDivisionBadgeFileName(this._topDivision, badgeSize);
		}

		public string GetDivisionNameTranslated(CompetitiveRank rank)
		{
			if (rank.TopPlacementPosition != null)
			{
				return this._translation.Get(this._topDivision.NameDraft, TranslationContext.Ranked);
			}
			Division division = this.GetDivision(rank);
			return this._translation.Get(division.NameDraft, TranslationContext.Ranked);
		}

		public string GetSubdivisionNumberTranslated(CompetitiveRank rank)
		{
			int subdivision = rank.Subdivision;
			return CompetitiveDivisionsBadgeNameBuilder.RomanNumbers[subdivision];
		}

		public string GetDivisionWithSubdivisionNameTranslated(CompetitiveRank rank)
		{
			string divisionNameTranslated = this.GetDivisionNameTranslated(rank);
			if (rank.TopPlacementPosition != null)
			{
				return divisionNameTranslated;
			}
			int subdivision = rank.Subdivision;
			return string.Format("{0} {1}", divisionNameTranslated, CompetitiveDivisionsBadgeNameBuilder.RomanNumbers[subdivision]);
		}

		private string GetSubDivisionFromIndex(Subdivision[] subdivisions, int subdivisionIndex)
		{
			int num = subdivisions.Length;
			return (num - subdivisionIndex).ToString();
		}

		private string GetSizeFromBadgeSize(CompetitiveBadgeSize badgeSize)
		{
			int num = badgeSize;
			return num.ToString();
		}

		private readonly IGetCompetitiveDivisions _getCompetitiveDivisions;

		private readonly ILocalizeKey _translation;

		private static readonly string[] RomanNumbers = new string[]
		{
			"V",
			"IV",
			"III",
			"II",
			"I"
		};

		private readonly Division _topDivision = new Division
		{
			NameDraft = "RANKING_HEAVYMETAL_LEAGUE",
			Subdivisions = new Subdivision[0]
		};
	}
}
