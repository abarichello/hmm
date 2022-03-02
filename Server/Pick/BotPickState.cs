using System;
using HeavyMetalMachines.Server.Apis;
using HeavyMetalMachines.Server.Pick.Apis;
using UnityEngine;

namespace HeavyMetalMachines.Server.Pick
{
	public class BotPickState : IPickModeState
	{
		public BotPickState(IBotPickController botPick)
		{
			this._botPick = botPick;
		}

		public bool IsInitialized { get; private set; }

		public void Initialize()
		{
			this._botPick.DefineBotsDesires();
			this.IsInitialized = true;
		}

		public bool Update()
		{
			this.UpdateBots();
			return this.IsBotPickingFinished();
		}

		private void UpdateBots()
		{
			this._botPick.UpdateAllGridDesires();
			this._botPick.UpdateAllBot(Time.deltaTime);
		}

		private bool IsBotPickingFinished()
		{
			return !this._botPick.IsBotPicking;
		}

		private readonly IBotPickController _botPick;
	}
}
