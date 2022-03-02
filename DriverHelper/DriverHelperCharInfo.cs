using System;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.VFX;
using UnityEngine;

namespace HeavyMetalMachines.DriverHelper
{
	[Serializable]
	public struct DriverHelperCharInfo
	{
		public Color RoleLabelSupportColor;

		public Color RoleLabelTacklerColor;

		public Color RoleLabelCarrierColor;

		public string RoleSupportDraft;

		public string RoleTacklerDraft;

		public string RoleCarrierDraft;

		public TranslationSheets RoleTranslationSheet;

		public TranslationSheets NamesSheet;

		public TranslationSheets DescriptionsSheet;

		public UILabel PilotNameLabel;

		public UILabel RoleNameLabel;

		public HMMUI2DDynamicSprite PilotIconSprite;
	}
}
