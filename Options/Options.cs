using System;
using Pocketverse;

namespace HeavyMetalMachines.Options
{
	public class Options : GameHubBehaviour
	{
		private void Awake()
		{
			this.Game.Init();
			this.Controls.Init();
		}

		public ControlOptions Controls;

		public AudioOptions Audio;

		public GameOptions Game;
	}
}
