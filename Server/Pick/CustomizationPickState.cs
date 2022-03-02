using System;
using HeavyMetalMachines.Characters;
using HeavyMetalMachines.Server.Pick.Apis;
using UnityEngine;

namespace HeavyMetalMachines.Server.Pick
{
	public class CustomizationPickState : IPickModeState
	{
		public CustomizationPickState(CharacterService pickService)
		{
			this._pickService = pickService;
		}

		public bool IsInitialized { get; private set; }

		public void Initialize()
		{
			this._pickService.ServerSendCustomizationTime();
			this.IsInitialized = true;
		}

		public bool Update()
		{
			this._pickService.CustomizationTime -= Time.deltaTime;
			return this.IsCustomizationTimeout();
		}

		private bool IsCustomizationTimeout()
		{
			return this._pickService.CustomizationTime <= 0f;
		}

		private readonly CharacterService _pickService;
	}
}
