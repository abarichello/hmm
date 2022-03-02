using System;
using Pocketverse;

namespace HeavyMetalMachines.Options
{
	public class Options : GameHubBehaviour
	{
		private void Start()
		{
			if (GameHubBehaviour.Hub.Net.IsServer() && !GameHubBehaviour.Hub.Net.IsTest())
			{
				return;
			}
			BitLogger.Initialize(CppFileAppender.GetMainLogger());
			this.Game.Init();
			this.Controls.Init();
		}

		public ControlOptions Controls;

		public AudioOptions Audio;

		public GameOptions Game;
	}
}
