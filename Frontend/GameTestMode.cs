using System;
using HeavyMetalMachines.Car;
using HeavyMetalMachines.Character;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Fog;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Server;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class GameTestMode : GameHubBehaviour
	{
		public GameTestMode()
		{
			bool[] array = new bool[3];
			array[0] = true;
			this._instanceUpgrades = array;
			this.botNames = new string[]
			{
				"Grachinsky",
				"AlyGattor",
				"Matador",
				"Pitcher",
				"Herald",
				"Botter",
				"BotMala",
				"Will"
			};
			this.SceneName = "Arena_Test";
			this.timescaleWaitingForBomb = 1f;
			this.timescaleWithActiveBomb = 1f;
			this._bombPosition = Vector3.zero;
			base..ctor();
		}

		private void SwitchInstance()
		{
			int num = -1;
			for (int i = 0; i < this._instanceUpgrades.Length; i++)
			{
				if (this._instanceUpgrades[i])
				{
					num = i;
					break;
				}
			}
			if (num >= 0)
			{
				GadgetBehaviour customGadget = this.combatObject.CustomGadget0;
				GadgetBehaviour.UpgradeInstance[] upgrades = this.combatObject.CustomGadget0.Upgrades;
				string value = string.Format("{0:00}", num + 1);
				for (int j = 0; j < upgrades.Length; j++)
				{
					string name = upgrades[j].Info.Name;
					if (name.Contains(value))
					{
						customGadget.Upgrade(name);
						this.playerController.SelectedInstance = name;
						Debug.Log(string.Format("Switched to instance {0}", name));
					}
					else
					{
						customGadget.Downgrade(name);
					}
				}
			}
			else
			{
				Debug.LogError("No instance was selected!");
			}
		}

		public const string TestModePlayerName = "PlayerBot";

		private bool[] _instanceUpgrades;

		public LoadingState loadingState;

		public ServerGame serverGame;

		public HeavyMetalMachines.Character.CharacterInfo character;

		public HeavyMetalMachines.Character.CharacterInfo[] botCharacters;

		private CarInput carInput;

		private PlayerController playerController;

		private CarMovement carMovement;

		private CombatObject combatObject;

		private TRCInterpolator interpolator;

		public int skinIdx;

		public int botCount;

		public CarInput.DrivingStyleKind drivingStyle;

		private string[] botNames;

		private float delay;

		public string SceneName;

		public float timescaleWaitingForBomb;

		public float timescaleWithActiveBomb;

		public Vector2 input;

		public static PlayerData playerData;

		private Vector3 _bombPosition;
	}
}
