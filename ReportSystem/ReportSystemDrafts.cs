using System;
using System.Collections.Generic;
using HeavyMetalMachines.ReportSystem.DataTransferObjects;
using HeavyMetalMachines.ReportSystem.Infra;

namespace HeavymetalMachines.ReportSystem
{
	public static class ReportSystemDrafts
	{
		public static string TitleWarningDraft = "REPORT_FEEDBACK_WARNING_TITLE";

		public static string TitlePunishmentDraft = "REPORT_FEEDBACK_PUNISH_TITLE";

		public static string DescriptionWarningDraft = "REPORT_FEEDBACK_WARNING_DESC_START";

		public static string DescriptionPunishmentDraft = "REPORT_FEEDBACK_PUNISH_DESC_START";

		public static readonly Dictionary<ReportMotive, string> MotiveDrafts = new Dictionary<ReportMotive, string>
		{
			{
				1,
				"REPORT_BANDERING_OR_HARASSING"
			},
			{
				2,
				"REPORT_OFFENSIVE_LANGUAGE"
			},
			{
				4,
				"REPORT_DISRESPECT_TOWARDS_STAFF"
			},
			{
				8,
				"REPORT_BAD_UNSPORTING_CONDUCT"
			},
			{
				16,
				"REPORT_INAPPROPRIATE_ACCOUNT"
			}
		};

		public static readonly Dictionary<PlayerFeedbackKind, string> PlayerFeedbackDrafts = new Dictionary<PlayerFeedbackKind, string>
		{
			{
				1,
				"REPORT_PUNISHMENT_WARNING"
			},
			{
				2,
				"REPORT_PUNISHMENT_RANKED_RESET"
			},
			{
				6,
				"REPORT_PUNISHMENT_ACCOUNT_BAN"
			},
			{
				5,
				"REPORT_PUNISHMENT_RANKED_BAN"
			},
			{
				4,
				"REPORT_PUNISHMENT_METAL_LEAGUE_BAN"
			}
		};
	}
}
