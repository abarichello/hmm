using System;

namespace HeavyMetalMachines.Server.Apis
{
	public interface IBotPickController
	{
		bool IsBotPicking { get; }

		void Initialize();

		void UpdateBotFakeSelection(float deltaTime);

		void DefineBotsDesires();

		void UpdateAllGridDesires();

		void UpdateAllBot(float deltaTime);
	}
}
