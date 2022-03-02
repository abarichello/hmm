using System;
using HeavyMetalMachines.Customization.Business;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Presenting;
using UnityEngine;

namespace HeavyMetalMachines.RadialMenu.View
{
	public interface IRadialCustomizationView
	{
		Animation WindowAnimation { get; }

		IUnequipItem[] UnequipItems { get; }

		ITextureMappingUpdater[] SpritesheetAnimators { get; }

		ILabel EmoteNameLabel { get; }

		IActivatable EquipGroupActivatable { get; }

		ILabel EquipLabel { get; }

		void SetupEquipShortcutImage(ISprite shortcutImageSprite);
	}
}
