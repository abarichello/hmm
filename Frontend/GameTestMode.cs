using System;
using HeavyMetalMachines.Car;
using HeavyMetalMachines.Characters;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.GameCamera;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Playback;
using HeavyMetalMachines.Server;
using Pocketverse;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Frontend
{
	public sealed class GameTestMode : GameHubBehaviour
	{
		private static readonly string[] botNames = new string[]
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

		public const string TestModePlayerName = "PlayerBot";

		[Inject]
		private IPlayback _playback;

		public LoadingState loadingState;

		public ServerGame serverGame;

		public CharacterInfo character;

		public CharacterInfo[] botCharacters;

		private CarInput carInput;

		private PlayerController playerController;

		private CarMovement carMovement;

		private CombatObject combatObject;

		public int skinIdx;

		public int botCount;

		public CarInput.DrivingStyleKind drivingStyle;

		private float delay;

		public string SceneName = "Arena_Test";

		public float timescaleWaitingForBomb = 1f;

		public float timescaleWithActiveBomb = 1f;

		public Vector2 input;

		public static PlayerData playerData;

		private IGameCamera _gameCamera;

		private IGameCameraEngine _gameCameraEngine;

		private IGameCameraInversion _gameCameraInversion;
	}
}
