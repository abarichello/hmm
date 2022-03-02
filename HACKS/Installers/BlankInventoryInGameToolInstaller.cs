using System;
using HeavyMetalMachines.DataTransferObjects.Battlepass;
using HeavyMetalMachines.VFX.HACKS.Business;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.HACKS.Installers
{
	public class BlankInventoryInGameToolInstaller : MonoInstaller<BlankInventoryInGameToolInstaller>
	{
		public override void InstallBindings()
		{
			throw new InvalidOperationException("BlankInventoryInGameToolInstaller outside UNITY EDITOR");
		}

		private class MockedSetLocalPlayerCustomization : ISetLocalPlayerCustomization
		{
			public void Set(PlayerCustomizationSlot slot, Guid itemTypeId)
			{
				Debug.LogFormat("{0}=>{1}", new object[]
				{
					slot,
					itemTypeId
				});
			}
		}
	}
}
