using System;
using Assets.ClientApiObjects.Components;
using Assets.Standard_Assets.Scripts.HMM.PlotKids;
using FMod;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.GameCamera.Behaviour;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Render;
using NewParticleSystem;
using Pocketverse;
using Pocketverse.Util;
using UnityEngine;
using UnityEngine.Serialization;

namespace HeavyMetalMachines.Combat
{
	public class BombVisualController : GameHubBehaviour
	{
		private CombatObject CurrentCombatObject
		{
			get
			{
				if (this._currentPlayerCombatObject != null)
				{
					return this._currentPlayerCombatObject;
				}
				this._currentPlayerCombatObject = GameHubBehaviour.Hub.Players.CurrentPlayerData.CharacterInstance.GetBitComponent<CombatObject>();
				return this._currentPlayerCombatObject;
			}
		}

		private static string BombPrefabName
		{
			get
			{
				if (!string.IsNullOrEmpty(BombVisualController._bombPrefabName))
				{
					return BombVisualController._bombPrefabName;
				}
				BombSkinItemTypeComponent component = GameHubBehaviour.Hub.BombManager.Rules.BombInfo.Skin.GetComponent<BombSkinItemTypeComponent>();
				BombVisualController._bombPrefabName = component.BombPrefabName;
				return BombVisualController._bombPrefabName;
			}
		}

		public static BombVisualController GetInstance()
		{
			return BombVisualController._instance;
		}

		public static BombVisualController CreateInstance()
		{
			if (BombVisualController._instance != null)
			{
				if (!GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.HORTA))
				{
					BombVisualController.Log.ErrorFormat("This should not happen during normal client playback", new object[0]);
				}
				BombVisualController.DestroyInstance();
			}
			Transform transform = (Transform)GameHubBehaviour.Hub.Resources.CacheInstantiate(BombVisualController.BombPrefabName, typeof(Transform), Vector3.zero, Quaternion.identity);
			transform.gameObject.SetActive(false);
			BombVisualController._instance = transform.GetComponent<BombVisualController>();
			return BombVisualController._instance;
		}

		private void Awake()
		{
			this._maxSpeedSqr = this._indicator.MaximumSpeedThreshold * this._indicator.MaximumSpeedThreshold;
			this._minSpeedSqr = this._indicator.MinimumSpeedThreshold * this._indicator.MinimumSpeedThreshold;
			MeshRenderer[] componentsInChildren = base.GetComponentsInChildren<MeshRenderer>();
			if (componentsInChildren != null && componentsInChildren.Length > 0)
			{
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					if (componentsInChildren[i] != this._indicator.ArrowRenderer)
					{
						this._bombMaterial = Object.Instantiate<Material>(componentsInChildren[i].material);
						break;
					}
				}
				for (int j = 0; j < componentsInChildren.Length; j++)
				{
					if (componentsInChildren[j] != this._indicator.ArrowRenderer)
					{
						componentsInChildren[j].material = this._bombMaterial;
					}
				}
			}
			this._propertyIds.BombPosition = Shader.PropertyToID("_BombPosition");
			this._propertyIds.OpenRadius = Shader.PropertyToID("_OpenRadius");
			this._propertyIds.TintColor = Shader.PropertyToID("_TintColor");
			this._propertyIds.GlowColor = Shader.PropertyToID("_GlowColor");
		}

		private void OnDestroy()
		{
			Object.Destroy(this._bombMaterial);
		}

		private void OnEnable()
		{
			this._gameGUI = GameHubBehaviour.Hub.State.Current.GetStateGuiController<GameGui>();
			this._idleLoopToken = FMODAudioManager.PlayAt(this._idleLoopAudio, base.transform);
			GameHubBehaviour.Hub.BombManager.ListenToMatchUpdate += this.OnMatchUpdated;
			GameHubBehaviour.Hub.BombManager.ListenToPhaseChange += this.OnPhaseChange;
			GameHubBehaviour.Hub.BombManager.ClientListenToBombDrop += this.OnBombDropped;
			GameHubBehaviour.Hub.BombManager.OnSlowMotionToggled += this.OnSlowToggled;
			GameHubBehaviour.Hub.BombManager.OnDisputeStarted += this.OnDisputeStarted;
			GameHubBehaviour.Hub.BombManager.OnDisputeFinished += this.OnDisputeFinished;
		}

		private void OnDisable()
		{
			if (this._borderLine)
			{
				this._openRadius = 0f;
				Shader.SetGlobalFloat(this._propertyIds.OpenRadius, this._openRadius);
				this.StopVisualTeam();
			}
			if (this._idleLoopToken != null)
			{
				this._idleLoopToken.Stop();
				this._idleLoopToken = null;
			}
			GameHubBehaviour.Hub.BombManager.ListenToPhaseChange -= this.OnPhaseChange;
			GameHubBehaviour.Hub.BombManager.ListenToMatchUpdate -= this.OnMatchUpdated;
			GameHubBehaviour.Hub.BombManager.ClientListenToBombDrop -= this.OnBombDropped;
			GameHubBehaviour.Hub.BombManager.OnSlowMotionToggled -= this.OnSlowToggled;
			GameHubBehaviour.Hub.BombManager.OnDisputeStarted -= this.OnDisputeStarted;
			GameHubBehaviour.Hub.BombManager.OnDisputeFinished -= this.OnDisputeFinished;
		}

		public static void DestroyInstance()
		{
			if (!BombVisualController._instance)
			{
				return;
			}
			BombVisualController._instance.SetCombatObject(null);
			BombVisualController._instance.StopVisualTeam();
			BombVisualController._instance.OnDisable();
			GameHubBehaviour.Hub.Resources.ReturnToCache(BombVisualController.BombPrefabName, BombVisualController._instance.transform);
			BombVisualController._instance = null;
		}

		private static ModifierData[] CreateBombModData()
		{
			ModifierInfo modifierInfo = new ModifierInfo
			{
				HitOwner = true,
				HitBomb = true,
				Status = StatusKind.Invulnerable
			};
			ModifierInfo[] infos = new ModifierInfo[]
			{
				modifierInfo
			};
			return ModifierData.CreateData(infos);
		}

		public void SetCombatObject(Transform bombTransform)
		{
			if (bombTransform)
			{
				this._bombCombatObject = bombTransform.GetComponent<Identifiable>();
				base.gameObject.SetActive(true);
				ModifierData[] datas = BombVisualController.CreateBombModData();
				CombatObject component = bombTransform.GetComponent<CombatObject>();
				component.Controller.AddPassiveModifiers(datas, component, -1);
			}
			else
			{
				base.gameObject.SetActive(false);
			}
			this._currentState = BombVisualController.State.FollowingTarget;
			if (this._borderLine)
			{
				this.SetVisualTeam(TeamType.None);
			}
		}

		private void OnDisputeStarted()
		{
			this.SetVisualTeam(TeamType.None);
		}

		private void OnDisputeFinished(TeamKind team)
		{
			if (team == TeamKind.Zero)
			{
				this.SetVisualTeam(TeamType.None);
			}
		}

		public void SetVisualTeam(TeamType newTeam)
		{
			for (int i = 0; i < this._teamVisuals[(int)this._currentVisualTeam].Particles.Length; i++)
			{
				this._teamVisuals[(int)this._currentVisualTeam].Particles[i].Stop();
			}
			for (int j = 0; j < this._teamVisuals[(int)newTeam].Particles.Length; j++)
			{
				this._teamVisuals[(int)newTeam].Particles[j].Play();
			}
			this._bombMaterial.SetColor(this._propertyIds.GlowColor, this._teamVisuals[(int)newTeam].ColorA);
			this._indicator.ArrowRenderer.material.SetColor(this._propertyIds.TintColor, this._teamVisuals[(int)newTeam].ColorB);
			this._borderLine.SetColors(this._teamVisuals[(int)newTeam].ColorA, this._teamVisuals[(int)newTeam].ColorB);
			if (this._jammingArea)
			{
				this._jammingArea.SetActive(newTeam == TeamType.None && !GameHubBehaviour.Hub.BombManager.IsDisputeStarted);
			}
			this._currentVisualTeam = newTeam;
		}

		public void StopVisualTeam()
		{
			for (int i = 0; i < this._teamVisuals[(int)this._currentVisualTeam].Particles.Length; i++)
			{
				this._teamVisuals[(int)this._currentVisualTeam].Particles[i].Stop();
			}
			this._jammingArea.SetActive(false);
		}

		public void AudioLoopChanged(float loopState)
		{
			if (this._idleLoopToken == null)
			{
				return;
			}
			if (this._idleLoopToken.IsInvalidated())
			{
				return;
			}
			this._idleLoopToken.SetParameter("state", loopState);
		}

		public void Detonate(int deliveryScore)
		{
			this._timing = 0f;
			this._bombScoreCamera.LookAtExplosion(base.transform);
			this._currentState = BombVisualController.State.ChangingToDetonator;
			BombVisualController.DestroyInstance();
		}

		private void LateUpdate()
		{
			this.ProcessStateMachine();
			Shader.SetGlobalVector(this._propertyIds.BombPosition, base.transform.position);
			Vector3 lastVelocity = GameHubBehaviour.Hub.BombManager.BombMovement.LastVelocity;
			float sqrMagnitude = lastVelocity.sqrMagnitude;
			if (sqrMagnitude >= this._minSpeedSqr)
			{
				this._indicator.Pivot.transform.localRotation = Quaternion.LookRotation(lastVelocity);
			}
			Color color = this._indicator.ArrowRenderer.material.GetColor(this._propertyIds.TintColor);
			color.a = Mathf.Clamp01((sqrMagnitude - this._minSpeedSqr) / (this._maxSpeedSqr - this._minSpeedSqr));
			color.a = Mathf.SmoothStep(0f, 1f, color.a);
			this._indicator.ArrowRenderer.material.SetColor(this._propertyIds.TintColor, color);
			if (this.rollOnIdle && GameHubBehaviour.Hub.BombManager.ActiveBomb.State == BombInstance.BombState.Idle)
			{
				this.RotateBomb();
			}
			else if (this.rollOnCarried && GameHubBehaviour.Hub.BombManager.ActiveBomb.State == BombInstance.BombState.Carried)
			{
				this.RotateBomb();
			}
			else if (this.rollOnMeteor && GameHubBehaviour.Hub.BombManager.ActiveBomb.State == BombInstance.BombState.Meteor)
			{
				this.RotateBomb();
			}
			else if (this.rollOnSpinning && GameHubBehaviour.Hub.BombManager.ActiveBomb.State == BombInstance.BombState.Spinning)
			{
				this.RotateBomb();
			}
			else if (this.lookAtCamera)
			{
				this.LookAtCamera();
			}
		}

		private void LookAtCamera()
		{
			if (this.rotationCameraFixed)
			{
				return;
			}
			this._bombMesh.transform.LookAt(Camera.main.transform, Vector3.up);
			this._bombMesh.transform.localEulerAngles = new Vector3(0f, this._bombMesh.transform.localEulerAngles.y + this.lookAtRotationOffset, 0f);
			this.rotationCameraFixed = true;
		}

		private void RotateBomb()
		{
			Vector3 vector;
			vector..ctor(GameHubBehaviour.Hub.BombManager.BombMovement.LastVelocity.z, 0f, -GameHubBehaviour.Hub.BombManager.BombMovement.LastVelocity.x);
			Vector3 vector2 = vector / GameHubBehaviour.Hub.BombManager.BombRules.BombInfo.VisualRadius * 57.29578f;
			vector2 = Vector3.ClampMagnitude(vector2, GameHubBehaviour.Hub.BombManager.BombRules.BombInfo.MaxVisualRotationSpeed);
			this._bombMesh.transform.Rotate(vector2 * Time.deltaTime, 0);
			this.rotationCameraFixed = false;
		}

		private void ProcessStateMachine()
		{
			if (this._bombCombatObject == null || !this._bombCombatObject.gameObject.activeInHierarchy)
			{
				return;
			}
			bool flag;
			if (SpectatorController.IsSpectating)
			{
				flag = false;
			}
			else
			{
				flag = GameHubBehaviour.Hub.BombManager.IsCarryingBomb(GameHubBehaviour.Hub.Players.CurrentPlayerData.CharacterInstance.ObjId);
				if (flag)
				{
					float num = Vector3.SqrMagnitude(this.CurrentCombatObject.transform.position - base.transform.position);
					if (this._idleLoopToken == null)
					{
						BombVisualController.Log.Error("null idleLoopToken! HOW?!? Please link this with QAHMM-15908");
					}
					else
					{
						this._idleLoopToken.SetParameter("cord", Mathf.Clamp01(num / this._bombTensionAudioNormalizer));
					}
				}
			}
			this._openRadius = ((!flag) ? Mathf.Clamp01(this._openRadius - Time.deltaTime * 2f) : Mathf.Clamp01(this._openRadius + Time.deltaTime * 2f));
			Shader.SetGlobalFloat(this._propertyIds.OpenRadius, this._openRadius);
			switch (this._currentState)
			{
			case BombVisualController.State.WaitingForTargetChange:
				this._currentState = BombVisualController.State.ChangingTarget;
				break;
			case BombVisualController.State.ChangingTarget:
				break;
			case BombVisualController.State.FastChangeTarget:
				this._currentState = BombVisualController.State.FollowingTarget;
				return;
			case BombVisualController.State.FollowingTarget:
				return;
			case BombVisualController.State.ChangingToDetonator:
				this._timing += Time.deltaTime;
				if (this._timing > 2f)
				{
					this._timing = 0f;
					this._currentState = BombVisualController.State.Exploding;
				}
				return;
			case BombVisualController.State.Exploding:
				BombVisualController.DestroyInstance();
				this._currentState = BombVisualController.State.Idle;
				return;
			case BombVisualController.State.Idle:
				return;
			default:
				return;
			}
			this._currentState = BombVisualController.State.FollowingTarget;
		}

		private void OnPhaseChange(BombScoreboardState state)
		{
			if (state != BombScoreboardState.BombDelivery)
			{
				this._openRadius = 0f;
				Shader.SetGlobalFloat(this._propertyIds.OpenRadius, this._openRadius);
			}
		}

		private void OnSlowToggled(bool enable)
		{
			if (!enable)
			{
				return;
			}
			FMODAudioManager.PlayOneShotAt(this._bombCompetitionAudio, base.transform.position, 0);
		}

		private void OnBombDropped(BombInstance bombInstance, SpawnReason reason, int causer)
		{
			bool flag = GameHubBehaviour.Hub.BombManager.ActiveBomb.State == BombInstance.BombState.Meteor;
			Vector3 position = base.transform.position;
			AudioEventAsset asset = (!flag) ? this._releaseAudio : this._releasePegasusAudio;
			FMODAudioManager.PlayOneShotAt(asset, position, 0);
			this._idleLoopToken.SetParameter("cord", 0f);
		}

		public void OnCollisionEvent(Vector3 position, Vector3 direction, float intensity, byte otherLayer)
		{
			switch (otherLayer)
			{
			case 19:
			case 22:
			case 24:
				FMODAudioManager.PlayOneShotAt(this._hitBombBlockerAudio, position, 0);
				return;
			}
			FMODAudioManager.PlayOneShotAt(this._hitWallAudio, position, 0);
		}

		public void OnMatchUpdated()
		{
			bool flag = this._currentBombState != GameHubBehaviour.Hub.BombManager.ActiveBomb.State;
			CombatObject combatObject = null;
			if (GameHubBehaviour.Hub.BombManager.IsSomeoneCarryingBomb())
			{
				combatObject = CombatRef.GetCombat(GameHubBehaviour.Hub.BombManager.ActiveBomb.BombCarriersIds[0]);
				flag |= (combatObject.Id != this._carrierCombatObject);
				this._carrierCombatObject = combatObject.Id;
			}
			else
			{
				this.AudioLoopChanged(1f);
			}
			if (!flag)
			{
				return;
			}
			switch (GameHubBehaviour.Hub.BombManager.ActiveBomb.State)
			{
			case BombInstance.BombState.Idle:
				this.HandleIdleBombState();
				break;
			case BombInstance.BombState.Carried:
				this.HandleCarriedBombState(combatObject);
				break;
			case BombInstance.BombState.Spinning:
				this.HandleSpinningBombState();
				break;
			case BombInstance.BombState.Meteor:
				this.HandleMeteorBombState();
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			this._currentBombState = GameHubBehaviour.Hub.BombManager.ActiveBomb.State;
		}

		private void HandleIdleBombState()
		{
			this.SetVisualTeam(TeamType.None);
			this.AudioLoopChanged(1f);
		}

		private void HandleCarriedBombState(CombatObject currentCarrier)
		{
			if (currentCarrier == null)
			{
				BombVisualController.Log.DebugFormat(string.Format("Current carrier null why!? carriers={0} carry={1} sp={2}", Arrays.ToStringWithComma(GameHubBehaviour.Hub.BombManager.ActiveBomb.BombCarriersIds.ToArray()), GameHubBehaviour.Hub.BombManager.IsSomeoneCarryingBomb(), GameHubBehaviour.Hub.BombManager.ActiveBomb.IsSpawned), new object[0]);
				return;
			}
			this.SetVisualTeam((!currentCarrier.IsSameTeamAsCurrentPlayer()) ? TeamType.Enemy : TeamType.Ally);
			this.AudioLoopChanged(2f);
			this._bombAnimator.SetBool(BombVisualController.SpinActivate, true);
			this._bombAnimator.SetBool(BombVisualController.SpinDeactivate, false);
			this._bombAnimator.SetBool(BombVisualController.FightOn, false);
			this._bombAnimator.SetBool(BombVisualController.FightOff, false);
			this._bombAnimator.SetBool(BombVisualController.SpinAttachedOn, false);
			this._bombAnimator.SetBool(BombVisualController.SpinAttachedOff, false);
			if (this._currentBombState == BombInstance.BombState.Idle)
			{
				FMODAudioManager.PlayOneShotAt(this._pickAudio, base.transform.position, 0);
			}
		}

		private void HandleSpinningBombState()
		{
			this.AudioLoopChanged(3f);
			if (this._currentBombState == BombInstance.BombState.Idle)
			{
				FMODAudioManager.PlayOneShotAt(this._pickAudio, base.transform.position, 0);
			}
		}

		private void HandleMeteorBombState()
		{
			this.AudioLoopChanged(3f);
			if (this._currentBombState == BombInstance.BombState.Spinning)
			{
				this._bombAnimator.SetTrigger(this.isMeteorTrigger);
			}
		}

		[InjectOnClient]
		private IBombScoreCameraBehaviour _bombScoreCamera;

		private static readonly BitLogger Log = new BitLogger(typeof(BombVisualController));

		private static readonly int SpinActivate = Animator.StringToHash("Spin_activate");

		private static readonly int SpinDeactivate = Animator.StringToHash("Spin_deactivate");

		private static readonly int FightOn = Animator.StringToHash("fight_On");

		private static readonly int FightOff = Animator.StringToHash("fight_Off");

		private static readonly int SpinAttachedOn = Animator.StringToHash("spin_atached_on");

		private static readonly int SpinAttachedOff = Animator.StringToHash("spin_atached_off");

		private static BombVisualController _instance;

		private static string _bombPrefabName;

		private const float BombLoopNormalState = 1f;

		private const float BombLoopTakenState = 2f;

		private const float BombLoopPegasusState = 3f;

		private const string AudioLoopParameter = "state";

		private const string AudioPitchParameter = "cord";

		private GameGui _gameGUI;

		private BombVisualController.State _currentState;

		private float _openRadius;

		private float _timing;

		private float _minSpeedSqr;

		private float _maxSpeedSqr;

		private Vector3 _offset;

		private Material _bombMaterial;

		private BombInstance.BombState _currentBombState;

		private BombVisualController.PropertyIds _propertyIds;

		private CombatObject _currentPlayerCombatObject;

		private Identifiable _bombCombatObject;

		private Identifiable _carrierCombatObject;

		private TeamType _currentVisualTeam;

		private FMODAudioManager.FMODAudio _idleLoopToken;

		[FormerlySerializedAs("idleLoopAudio")]
		[SerializeField]
		private AudioEventAsset _idleLoopAudio;

		[FormerlySerializedAs("pickAudio")]
		[SerializeField]
		private AudioEventAsset _pickAudio;

		[FormerlySerializedAs("releaseAudio")]
		[SerializeField]
		private AudioEventAsset _releaseAudio;

		[FormerlySerializedAs("releasePegasusAudio")]
		[SerializeField]
		private AudioEventAsset _releasePegasusAudio;

		[FormerlySerializedAs("hitWallAudio")]
		[SerializeField]
		private AudioEventAsset _hitWallAudio;

		[FormerlySerializedAs("hitBombBlockerAudio")]
		[SerializeField]
		private AudioEventAsset _hitBombBlockerAudio;

		[FormerlySerializedAs("bombCompetitionAudio")]
		[SerializeField]
		private AudioEventAsset _bombCompetitionAudio;

		[FormerlySerializedAs("JammingArea")]
		[SerializeField]
		private GameObject _jammingArea;

		[FormerlySerializedAs("borderLine")]
		[SerializeField]
		private ObjectOverlay _borderLine;

		[FormerlySerializedAs("BombAnimator")]
		[SerializeField]
		private Animator _bombAnimator;

		[FormerlySerializedAs("BombMesh")]
		[SerializeField]
		private GameObject _bombMesh;

		[FormerlySerializedAs("TeamVisuals")]
		[SerializeField]
		private BombVisualController.TeamData[] _teamVisuals = new BombVisualController.TeamData[3];

		[FormerlySerializedAs("Indicator")]
		[SerializeField]
		private BombVisualController.BombIndicator _indicator;

		[SerializeField]
		private float _bombTensionAudioNormalizer;

		[SerializeField]
		private string isMeteorTrigger = "isMeteor";

		[Header("Not rolling rotation fix")]
		[SerializeField]
		private bool lookAtCamera;

		[SerializeField]
		private float lookAtRotationOffset;

		[Header("Set rolling states")]
		[SerializeField]
		private bool rollOnIdle;

		[SerializeField]
		private bool rollOnCarried;

		[SerializeField]
		private bool rollOnSpinning;

		[SerializeField]
		private bool rollOnMeteor;

		private bool rotationCameraFixed;

		private enum State
		{
			WaitingForTargetChange,
			ChangingTarget,
			FastChangeTarget,
			FollowingTarget,
			ChangingToDetonator,
			Exploding,
			Idle
		}

		[Serializable]
		public struct TeamData
		{
			public TeamType Team;

			public Color ColorA;

			public Color ColorB;

			public HoplonParticleSystem[] Particles;
		}

		[Serializable]
		public struct BombIndicator
		{
			public GameObject Pivot;

			public MeshRenderer ArrowRenderer;

			public float MinimumSpeedThreshold;

			public float MaximumSpeedThreshold;
		}

		private struct PropertyIds
		{
			public int BombPosition;

			public int OpenRadius;

			public int TintColor;

			public int GlowColor;
		}
	}
}
