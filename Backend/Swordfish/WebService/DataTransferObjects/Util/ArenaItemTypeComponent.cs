using System;
using Assets.ClientApiObjects.Components;
using HeavyMetalMachines.Arena;
using HeavyMetalMachines.Arena.Infra;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Matches.DataTransferObjects;
using UnityEngine;

namespace HeavyMetalMachines.Backend.Swordfish.WebService.DataTransferObjects.Util
{
	public class ArenaItemTypeComponent : ItemTypeComponent, IGameArenaInfo, IGameplayGameArenaInfo, IMinimapGameArenaInfo, IUiGameArenaInfo
	{
		public override ItemTypeComponent.Type ComponentType
		{
			get
			{
				return ItemTypeComponent.Type.Arena;
			}
		}

		public override void CompleteComponentWithPlaceHolder(IItemTypeComponentPlaceHolderProvider placeHolderProvider)
		{
		}

		public float CursorLockTimeInSeconds
		{
			get
			{
				return this._cursorLockTimeInSeconds;
			}
		}

		public Vector2 CursorLockOffset
		{
			get
			{
				return this._cursorLockOffset;
			}
		}

		public float RespawnTimeSeconds
		{
			get
			{
				return this._respawnTimeSeconds;
			}
		}

		public float RespawningTimeSeconds
		{
			get
			{
				return this._respawningTimeSeconds;
			}
		}

		public float KillCamWaitTimeSeconds
		{
			get
			{
				return this._killCamWaitTimeSeconds;
			}
		}

		public int WarmupTimeSeconds
		{
			get
			{
				return this._warmupTimeSeconds;
			}
		}

		public float FirstShopExtraTimeSeconds
		{
			get
			{
				return this._firstShopExtraTimeSeconds;
			}
		}

		public int ShopPhaseSeconds
		{
			get
			{
				return this._shopPhaseSeconds;
			}
		}

		public Vector3 BombSpawnPoint
		{
			get
			{
				return this._bombSpawnPoint;
			}
		}

		public ArenaModifierConfiguration[] ModifiersToApply
		{
			get
			{
				return this._modifiersToApply;
			}
		}

		public int ArenaModifierDistanceToBombToApply
		{
			get
			{
				return this._arenaModiferDistanceToBombModifier;
			}
		}

		public int ReplayDelaySeconds
		{
			get
			{
				return this._replayDelaySeconds;
			}
		}

		public float RoundTimeSeconds
		{
			get
			{
				return this._roundTimeSeconds;
			}
		}

		public float OvertimeDurationSeconds
		{
			get
			{
				return this._overtimeDurationSeconds;
			}
		}

		public int TimeBeforeOvertime
		{
			get
			{
				return this._timeBeforeOvertime;
			}
		}

		public float OvertimeGuiDeliveryScaleModifier
		{
			get
			{
				return this._overtimeGuiDeliveryScaleModifier;
			}
		}

		public float NearGoalDistance
		{
			get
			{
				return this._nearGoalDistance;
			}
		}

		public bool IsGPSDisabled
		{
			get
			{
				return this._isGPSDisabled;
			}
		}

		public string MinimapTextureName
		{
			get
			{
				return this._minimapTextureName;
			}
		}

		public int MinimapScale
		{
			get
			{
				return this._minimapScale;
			}
		}

		public Vector2 MinimapTextureSize
		{
			get
			{
				return this._minimapTextureSize;
			}
		}

		public int MinimapTextureYOffset
		{
			get
			{
				return this._minimapTextureYOffset;
			}
		}

		public int MinimapTextureXOffset
		{
			get
			{
				return this._minimapTextureXOffset;
			}
		}

		public int MapSize
		{
			get
			{
				return this._mapSize;
			}
		}

		public int TeamBlueAngleY
		{
			get
			{
				return this._teamBlueAngleY;
			}
		}

		public int TeamRedAngleY
		{
			get
			{
				return this._teamRedAngleY;
			}
		}

		public Vector2 IconPositionOffset
		{
			get
			{
				return this._iconPositionOffset;
			}
		}

		public int TeamBlueArrowRotation
		{
			get
			{
				return this._teamBlueArrowRotation;
			}
		}

		public int TeamRedArrowRotation
		{
			get
			{
				return this._teamRedArrowRotation;
			}
		}

		public Vector2 TeamBlueBasePoint
		{
			get
			{
				return this._teamBlueBasePoint;
			}
		}

		public Vector2 TeamRedBasePoint
		{
			get
			{
				return this._teamRedBasePoint;
			}
		}

		public Vector2[] TeamBlueDeadZone
		{
			get
			{
				return this._teamBlueDeadZone;
			}
		}

		public Vector2[] TeamRedDeadZone
		{
			get
			{
				return this._teamRedDeadZone;
			}
		}

		public TeamKind BaseFlipTeam
		{
			get
			{
				return this._baseFlipTeam;
			}
		}

		public bool HideWhenInDeadZone
		{
			get
			{
				return this._hideWhenInDeadZone;
			}
		}

		public float MinimapCarriedBombOffset
		{
			get
			{
				return this._minimapCarriedBombOffset;
			}
		}

		public string PickBackgroundImageName
		{
			get
			{
				return this._pickBackgroundImageName;
			}
		}

		public string UnlockIconName
		{
			get
			{
				return this._unlockIconName;
			}
		}

		public string CustomMatchSelectionImageName
		{
			get
			{
				return this._customMatchSelectionImageName;
			}
		}

		public string ArenaSelectorImageName
		{
			get
			{
				return this._arenaSelectorImageName;
			}
		}

		public string GameModeDraft
		{
			get
			{
				return this._gameModeDraft;
			}
		}

		public string MatchesSlotIconName
		{
			get
			{
				return this._matchesSlotIconName;
			}
		}

		public bool LifebarShowIndestructibleFeedback
		{
			get
			{
				return this._lifebarShowIndestructibleFeedback;
			}
		}

		public bool DisableUINearBombFeedback
		{
			get
			{
				return this._disableUINearBombFeedback;
			}
		}

		public MatchKind[] ShowInMatchKindsSelector
		{
			get
			{
				return this._showInMatchKindsSelector;
			}
		}

		public TeamKind ArenaFlipTeam
		{
			get
			{
				return this._arenaFlipTeam;
			}
		}

		public int ArenaFlipScale
		{
			get
			{
				return this._arenaFlipScale;
			}
		}

		public int ArenaFlipRotation
		{
			get
			{
				return this._arenaFlipRotation;
			}
		}

		public TeamKind TugOfWarInvertProgressTeam
		{
			get
			{
				return this._tugOfWarInvertProgressTeam;
			}
		}

		public TeamKind TugOfWarFlipTeam
		{
			get
			{
				return this._tugOfWarFlipTeam;
			}
		}

		public int UnlockLevel
		{
			get
			{
				return this._unlockLevel;
			}
		}

		public bool IsTutorial
		{
			get
			{
				return this._isTutorial;
			}
		}

		public bool IsCustomOnly
		{
			get
			{
				return this._isCustomOnly;
			}
		}

		public bool IsOnCheatsEnabledOnly
		{
			get
			{
				return this._isOnCheatsEnabledOnly;
			}
		}

		public string SceneName
		{
			get
			{
				return this._sceneName;
			}
		}

		public string DraftName
		{
			get
			{
				return this._draftName;
			}
		}

		public string LoadingBackgroundRedTeamImageName
		{
			get
			{
				return this._loadingBackgroundRedTeamImageName;
			}
		}

		public string LoadingBackgroundBlueTeamImageName
		{
			get
			{
				return this._loadingBackgroundBlueTeamImageName;
			}
		}

		public PreCacheArenaObjects[] ObjectsToLoad
		{
			get
			{
				return this._objectsToLoad;
			}
		}

		public int CameraInversionTeamAAngleY
		{
			get
			{
				return this._cameraInversionTeamAAngleY;
			}
		}

		public int CameraInversionTeamBAngleY
		{
			get
			{
				return this._cameraInversionTeamBAngleY;
			}
		}

		public bool FlipPinVerticalPosition
		{
			get
			{
				return this._flipPinVerticalPosition;
			}
		}

		public bool FlipPinHorizontalPosition
		{
			get
			{
				return this._flipPinHorizontalPosition;
			}
		}

		[Header("[Flip]")]
		public TeamKind _arenaFlipTeam;

		[SerializeField]
		private int _arenaFlipScale;

		[SerializeField]
		private int _arenaFlipRotation;

		[SerializeField]
		private TeamKind _tugOfWarInvertProgressTeam;

		[SerializeField]
		private TeamKind _tugOfWarFlipTeam;

		[Header("MUST BE EDITED IN SharedConfigs: ArenaConfig AND PlayerProgression")]
		[SerializeField]
		private int _unlockLevel;

		[Header("MUST BE EDITED IN SharedConfigs: ArenaConfig")]
		[SerializeField]
		private bool _isTutorial;

		[SerializeField]
		private bool _isCustomOnly;

		[SerializeField]
		private bool _isOnCheatsEnabledOnly;

		[Header("------------------------------------------------------------------")]
		[Header("[Loading]")]
		[SerializeField]
		private string _sceneName;

		[SerializeField]
		private string _draftName;

		[SerializeField]
		private string _loadingBackgroundRedTeamImageName;

		[SerializeField]
		private string _loadingBackgroundBlueTeamImageName;

		[SerializeField]
		private PreCacheArenaObjects[] _objectsToLoad;

		[Header("[UI]")]
		[SerializeField]
		private string _pickBackgroundImageName;

		[SerializeField]
		private string _unlockIconName;

		[SerializeField]
		private string _customMatchSelectionImageName;

		[SerializeField]
		private string _arenaSelectorImageName;

		[SerializeField]
		private string _gameModeDraft;

		[SerializeField]
		private string _matchesSlotIconName;

		[SerializeField]
		private bool _disableUINearBombFeedback;

		[SerializeField]
		private MatchKind[] _showInMatchKindsSelector;

		[Header("[Camera Config]")]
		[SerializeField]
		private int _cameraInversionTeamAAngleY = 120;

		[SerializeField]
		private int _cameraInversionTeamBAngleY = 300;

		[Header("[Minimap]")]
		[Header("[Alt+Shift+M to update values running in-game - Editor Only]")]
		[SerializeField]
		private string _minimapTextureName;

		[SerializeField]
		private int _minimapScale = 1;

		[SerializeField]
		private Vector2 _minimapTextureSize;

		[SerializeField]
		private int _minimapTextureYOffset;

		[SerializeField]
		private int _minimapTextureXOffset;

		[SerializeField]
		private int _mapSize;

		[SerializeField]
		private int _teamBlueAngleY;

		[SerializeField]
		private int _teamRedAngleY;

		[SerializeField]
		private Vector2 _iconPositionOffset;

		[SerializeField]
		private int _teamBlueArrowRotation;

		[SerializeField]
		private int _teamRedArrowRotation;

		[SerializeField]
		private Vector2 _teamBlueBasePoint;

		[SerializeField]
		private Vector2 _teamRedBasePoint;

		[SerializeField]
		private Vector2[] _teamBlueDeadZone;

		[SerializeField]
		private Vector2[] _teamRedDeadZone;

		[SerializeField]
		private TeamKind _baseFlipTeam;

		[SerializeField]
		private bool _hideWhenInDeadZone;

		[SerializeField]
		private bool _flipPinVerticalPosition;

		[SerializeField]
		private bool _flipPinHorizontalPosition;

		[SerializeField]
		private float _minimapCarriedBombOffset = 50f;

		[Range(0f, 1f)]
		[SerializeField]
		private float _overtimeGuiDeliveryScaleModifier = 1f;

		[Header("[Counselor]")]
		[SerializeField]
		private float _nearGoalDistance;

		[Header("[Lifebar]")]
		[SerializeField]
		private bool _lifebarShowIndestructibleFeedback = true;

		[Header("[Match Take-off Warmup]")]
		[SerializeField]
		private float _cursorLockTimeInSeconds;

		[Tooltip("Offset from screen middle. Eg. an offset of (0, -0.5) in temple of sacrifice will align the cursor in from of player's car.")]
		[SerializeField]
		private Vector2 _cursorLockOffset;

		[Header("[Gameplay]")]
		[SerializeField]
		private float _respawnTimeSeconds;

		[SerializeField]
		private float _respawningTimeSeconds = 3f;

		[SerializeField]
		private float _killCamWaitTimeSeconds;

		[SerializeField]
		private int _warmupTimeSeconds = 1;

		[SerializeField]
		private float _firstShopExtraTimeSeconds = 10f;

		[SerializeField]
		private int _shopPhaseSeconds = 10;

		[SerializeField]
		private Vector3 _bombSpawnPoint = Vector3.zero;

		[SerializeField]
		private bool _isGPSDisabled;

		[Tooltip("Time between delivering the bomb, and the beginning of the replay")]
		[SerializeField]
		private int _replayDelaySeconds = 3;

		[SerializeField]
		private float _roundTimeSeconds;

		[SerializeField]
		private float _overtimeDurationSeconds;

		[SerializeField]
		private int _timeBeforeOvertime = 11;

		[Header("[Arena Modifiers]")]
		[SerializeField]
		private int _arenaModiferDistanceToBombModifier;

		[SerializeField]
		private ArenaModifierConfiguration[] _modifiersToApply;
	}
}
