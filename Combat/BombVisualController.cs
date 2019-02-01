using System;
using Assets.Standard_Assets.Scripts.HMM.PlotKids;
using FMod;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Render;
using NewParticleSystem;
using Pocketverse;
using Pocketverse.Util;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class BombVisualController : GameHubBehaviour
	{
		private CombatObject _CurrentCombatObject
		{
			get
			{
				if (this._currentPlayerCombatObject == null)
				{
					this._currentPlayerCombatObject = GameHubBehaviour.Hub.Players.CurrentPlayerData.CharacterInstance.GetBitComponent<CombatObject>();
				}
				return this._currentPlayerCombatObject;
			}
		}

		public static BombVisualController GetInstance(bool allowInstantiate = false)
		{
			BombVisualController result = null;
			if (BombVisualController.instance)
			{
				result = BombVisualController.instance;
			}
			if (!BombVisualController.instance && allowInstantiate)
			{
				Transform transform = (Transform)GameHubBehaviour.Hub.Resources.CacheInstantiate(BombVisualController.BombAssetName, typeof(Transform), Vector3.zero, Quaternion.identity);
				if (transform)
				{
					BombVisualController.instance = transform.GetComponent<BombVisualController>();
					result = BombVisualController.instance;
					transform.gameObject.SetActive(false);
				}
			}
			return result;
		}

		private void Awake()
		{
			this._maxSpeedSqr = this.Indicator.MaximumSpeedThreshold * this.Indicator.MaximumSpeedThreshold;
			this._minSpeedSqr = this.Indicator.MinimumSpeedThreshold * this.Indicator.MinimumSpeedThreshold;
			MeshRenderer[] componentsInChildren = base.GetComponentsInChildren<MeshRenderer>();
			if (componentsInChildren != null && componentsInChildren.Length > 0)
			{
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					if (componentsInChildren[i] != this.Indicator.ArrowRenderer)
					{
						this._bombMaterialInstance = UnityEngine.Object.Instantiate<Material>(componentsInChildren[i].material);
						break;
					}
				}
				for (int j = 0; j < componentsInChildren.Length; j++)
				{
					if (componentsInChildren[j] != this.Indicator.ArrowRenderer)
					{
						componentsInChildren[j].material = this._bombMaterialInstance;
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
			UnityEngine.Object.Destroy(this._bombMaterialInstance);
		}

		private void OnEnable()
		{
			this.gameGUI = GameHubBehaviour.Hub.State.Current.GetStateGuiController<GameGui>();
			this.idleLoopToken = FMODAudioManager.PlayAt(this.idleLoopAudio, base.transform);
			GameHubBehaviour.Hub.BombManager.ListenToMatchUpdate += this.OnMatchUpdated;
			GameHubBehaviour.Hub.BombManager.ListenToPhaseChange += this.OnPhaseChange;
			GameHubBehaviour.Hub.BombManager.ClientListenToBombDrop += this.OnBombDropped;
			GameHubBehaviour.Hub.BombManager.OnSlowMotionToggled += this.OnSlowToggled;
			GameHubBehaviour.Hub.BombManager.OnDisputeStarted += this.OnDisputeStarted;
			GameHubBehaviour.Hub.BombManager.OnDisputeFinished += this.OnDisputeFinished;
		}

		private void OnDisable()
		{
			if (this.borderLine)
			{
				this.openRadius = 0f;
				Shader.SetGlobalFloat(this._propertyIds.OpenRadius, this.openRadius);
				this.StopVisualTeam();
			}
			if (this.idleLoopToken != null)
			{
				this.idleLoopToken.Stop();
				this.idleLoopToken = null;
			}
			GameHubBehaviour.Hub.BombManager.ListenToPhaseChange -= this.OnPhaseChange;
			GameHubBehaviour.Hub.BombManager.ListenToMatchUpdate -= this.OnMatchUpdated;
			GameHubBehaviour.Hub.BombManager.ClientListenToBombDrop -= this.OnBombDropped;
			GameHubBehaviour.Hub.BombManager.OnSlowMotionToggled -= this.OnSlowToggled;
			GameHubBehaviour.Hub.BombManager.OnDisputeStarted -= this.OnDisputeStarted;
			GameHubBehaviour.Hub.BombManager.OnDisputeFinished -= this.OnDisputeFinished;
		}

		private static void Destroy()
		{
			if (BombVisualController.instance)
			{
				BombVisualController.instance.SetCombatObject(null);
				BombVisualController.instance.StopVisualTeam();
				GameHubBehaviour.Hub.Resources.ReturnToCache(BombVisualController.BombAssetName, BombVisualController.instance.transform);
				BombVisualController.instance = null;
			}
		}

		private ModifierData[] CreateBombModData()
		{
			ModifierInfo[] infos = new ModifierInfo[]
			{
				new ModifierInfo
				{
					HitOwner = true,
					HitBomb = true,
					Status = StatusKind.Invulnerable
				}
			};
			return ModifierData.CreateData(infos);
		}

		public void SetCombatObject(Transform bombTransform)
		{
			if (bombTransform)
			{
				this.BombCombatObject = bombTransform.GetComponent<Identifiable>();
				base.gameObject.SetActive(true);
				ModifierData[] datas = this.CreateBombModData();
				CombatObject component = bombTransform.GetComponent<CombatObject>();
				component.Controller.AddPassiveModifiers(datas, component, -1);
			}
			else
			{
				base.gameObject.SetActive(false);
			}
			this.currentState = BombVisualController.State.FollowingTarget;
			if (this.borderLine)
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
			for (int i = 0; i < this.TeamVisuals[(int)this.CurrentVisualTeam].Particles.Length; i++)
			{
				this.TeamVisuals[(int)this.CurrentVisualTeam].Particles[i].Stop();
			}
			for (int j = 0; j < this.TeamVisuals[(int)newTeam].Particles.Length; j++)
			{
				this.TeamVisuals[(int)newTeam].Particles[j].Play();
			}
			this._bombMaterialInstance.SetColor(this._propertyIds.GlowColor, this.TeamVisuals[(int)newTeam].ColorA);
			this.Indicator.ArrowRenderer.material.SetColor(this._propertyIds.TintColor, this.TeamVisuals[(int)newTeam].ColorB);
			this.borderLine.SetColors(this.TeamVisuals[(int)newTeam].ColorA, this.TeamVisuals[(int)newTeam].ColorB);
			if (this.JammingArea)
			{
				this.JammingArea.SetActive(newTeam == TeamType.None && !GameHubBehaviour.Hub.BombManager.IsDisputeStarted);
			}
			this.CurrentVisualTeam = newTeam;
		}

		public void StopVisualTeam()
		{
			for (int i = 0; i < this.TeamVisuals[(int)this.CurrentVisualTeam].Particles.Length; i++)
			{
				this.TeamVisuals[(int)this.CurrentVisualTeam].Particles[i].Stop();
			}
			this.JammingArea.SetActive(false);
		}

		public void AudioLoopChanged(int loopState)
		{
			if (this.idleLoopToken != null && !this.idleLoopToken.IsInvalidated())
			{
				this.idleLoopToken.SetParameter(this.AudioLoopParameter, (float)loopState);
			}
		}

		public void Detonate(int deliveryScore)
		{
			this.timming = 0f;
			if (GameHubBehaviour.Hub.BombManager.ScoreBoard.CurrentState == BombScoreBoard.State.Replay)
			{
				CarCamera.Singleton.SetTarget("BombExplosionReplay", delegate()
				{
					bool flag = LogoTransition.IsPlaying() && !LogoTransition.HasTriggeredMiddleEvent();
					bool flag2 = GameHubBehaviour.Hub.BombManager.ScoreBoard.CurrentState == BombScoreBoard.State.Replay;
					return flag || flag2;
				}, base.transform, false, false, false);
			}
			else
			{
				CarCamera.Singleton.SetTarget("BombExplosion", delegate()
				{
					BombScoreBoard.State state = GameHubBehaviour.Hub.BombManager.ScoreBoard.CurrentState;
					return state == BombScoreBoard.State.BombDelivery || state == BombScoreBoard.State.PreReplay;
				}, base.transform, false, false, false);
			}
			this.currentState = BombVisualController.State.ChangingToDetonator;
			BombVisualController.Destroy();
		}

		private void LateUpdate()
		{
			this.ProcessStateMachine();
			Shader.SetGlobalVector(this._propertyIds.BombPosition, base.transform.position);
			Vector3 lastVelocity = GameHubBehaviour.Hub.BombManager.BombMovement.LastVelocity;
			float sqrMagnitude = lastVelocity.sqrMagnitude;
			if (sqrMagnitude >= this._minSpeedSqr)
			{
				this.Indicator.Pivot.transform.localRotation = Quaternion.LookRotation(lastVelocity);
			}
			Color color = this.Indicator.ArrowRenderer.material.GetColor(this._propertyIds.TintColor);
			color.a = Mathf.Clamp01((sqrMagnitude - this._minSpeedSqr) / (this._maxSpeedSqr - this._minSpeedSqr));
			color.a = Mathf.SmoothStep(0f, 1f, color.a);
			this.Indicator.ArrowRenderer.material.SetColor(this._propertyIds.TintColor, color);
		}

		private void ProcessStateMachine()
		{
			if (this.BombCombatObject == null || !this.BombCombatObject.gameObject.activeInHierarchy)
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
					float num = Vector3.SqrMagnitude(this._CurrentCombatObject.transform.position - base.transform.position);
					if (this.idleLoopToken == null)
					{
						BombVisualController.Log.Error("null idleLoopToken! HOW?!? Please link this with QAHMM-15908");
					}
					else
					{
						this.idleLoopToken.SetParameter(this.AudioPitchParameter, Mathf.Clamp01(num / this._bombTensionAudioNormalizer));
					}
				}
			}
			this.openRadius = ((!flag) ? Mathf.Clamp01(this.openRadius - Time.deltaTime * 2f) : Mathf.Clamp01(this.openRadius + Time.deltaTime * 2f));
			Shader.SetGlobalFloat(this._propertyIds.OpenRadius, this.openRadius);
			switch (this.currentState)
			{
			case BombVisualController.State.WaitingForTargetChange:
				this.currentState = BombVisualController.State.ChangingTarget;
				break;
			case BombVisualController.State.ChangingTarget:
				break;
			case BombVisualController.State.FastChangeTarget:
				this.currentState = BombVisualController.State.FollowingTarget;
				return;
			case BombVisualController.State.FollowingTarget:
				return;
			case BombVisualController.State.ChangingToDetonator:
				this.timming += Time.deltaTime;
				if (this.timming > 2f)
				{
					this.timming = 0f;
					this.currentState = BombVisualController.State.Exploding;
				}
				return;
			case BombVisualController.State.Exploding:
				BombVisualController.Destroy();
				this.currentState = BombVisualController.State.Idle;
				return;
			case BombVisualController.State.Idle:
				return;
			default:
				return;
			}
			this.currentState = BombVisualController.State.FollowingTarget;
		}

		private void OnPhaseChange(BombScoreBoard.State state)
		{
			if (state != BombScoreBoard.State.BombDelivery)
			{
				this.openRadius = 0f;
				Shader.SetGlobalFloat(this._propertyIds.OpenRadius, this.openRadius);
			}
		}

		private void OnSlowToggled(bool enable)
		{
			if (!enable)
			{
				return;
			}
			FMODAudioManager.PlayOneShotAt(this.bombCompetitionAudio, base.transform.position, 0);
		}

		private void OnBombDropped(BombInstance bombInstance, SpawnReason reason, int causer)
		{
			if (GameHubBehaviour.Hub.BombManager.ActiveBomb.State == BombInstance.BombState.Meteor)
			{
				FMODAudioManager.PlayOneShotAt(this.releasePegasusAudio, base.transform.position, 0);
			}
			else
			{
				FMODAudioManager.PlayOneShotAt(this.releaseAudio, base.transform.position, 0);
			}
			this.idleLoopToken.SetParameter(this.AudioPitchParameter, 0f);
		}

		public void OnCollisionEvent(Vector3 position, Vector3 direction, float intensity, byte otherLayer)
		{
			switch (otherLayer)
			{
			case 19:
			case 22:
			case 24:
				FMODAudioManager.PlayOneShotAt(this.hitBombBlockerAudio, position, 0);
				return;
			}
			FMODAudioManager.PlayOneShotAt(this.hitWallAudio, position, 0);
		}

		public void OnMatchUpdated()
		{
			bool flag = this._currentBombState != GameHubBehaviour.Hub.BombManager.ActiveBomb.State;
			CombatObject combatObject = null;
			if (GameHubBehaviour.Hub.BombManager.IsSomeoneCarryingBomb())
			{
				combatObject = CombatRef.GetCombat(GameHubBehaviour.Hub.BombManager.ActiveBomb.BombCarriersIds[0]);
				flag |= (combatObject.Id != this.CarrierCombatObject);
				this.CarrierCombatObject = combatObject.Id;
			}
			else
			{
				this.AudioLoopChanged(1);
			}
			if (flag)
			{
				switch (GameHubBehaviour.Hub.BombManager.ActiveBomb.State)
				{
				case BombInstance.BombState.Idle:
					this.SetVisualTeam(TeamType.None);
					this.AudioLoopChanged(1);
					break;
				case BombInstance.BombState.Carried:
					if (combatObject == null)
					{
						Debug.Log(string.Format("Current carrier null why!? carriers={0} carry={1} sp={2}", Arrays.ToStringWithComma(GameHubBehaviour.Hub.BombManager.ActiveBomb.BombCarriersIds.ToArray()), GameHubBehaviour.Hub.BombManager.IsSomeoneCarryingBomb(), GameHubBehaviour.Hub.BombManager.ActiveBomb.IsSpawned));
					}
					else
					{
						this.SetVisualTeam((!combatObject.IsSameTeamAsCurrentPlayer()) ? TeamType.Enemy : TeamType.Ally);
						this.AudioLoopChanged(2);
						this.BombAnimator.SetBool("Spin_activate", false);
						this.BombAnimator.SetBool("Spin_deactivate", false);
						this.BombAnimator.SetBool("fight_On", false);
						this.BombAnimator.SetBool("fight_Off", false);
						this.BombAnimator.SetBool("spin_atached_on", false);
						this.BombAnimator.SetBool("spin_atached_off", false);
						if (this._currentBombState == BombInstance.BombState.Idle)
						{
							FMODAudioManager.PlayOneShotAt(this.pickAudio, base.transform.position, 0);
						}
					}
					break;
				case BombInstance.BombState.Spinning:
					this.AudioLoopChanged(3);
					if (this._currentBombState == BombInstance.BombState.Idle)
					{
						FMODAudioManager.PlayOneShotAt(this.pickAudio, base.transform.position, 0);
					}
					break;
				case BombInstance.BombState.Meteor:
					this.AudioLoopChanged(3);
					break;
				}
				this._currentBombState = GameHubBehaviour.Hub.BombManager.ActiveBomb.State;
			}
		}

		private static readonly BitLogger Log = new BitLogger(typeof(BombVisualController));

		[NonSerialized]
		public static readonly string BombAssetName = "SK_bomb_PF";

		private const int BombLoopNormal = 1;

		private const int BombLoopTaken = 2;

		private const int BombLoopPegasus = 3;

		private readonly byte[] AudioLoopParameter = FMODAudioManager.GetBytes("state");

		private readonly byte[] AudioPitchParameter = FMODAudioManager.GetBytes("cord");

		public FMODAsset idleLoopAudio;

		public FMODAsset pickAudio;

		public FMODAsset releaseAudio;

		public FMODAsset releasePegasusAudio;

		public FMODAsset hitWallAudio;

		public FMODAsset hitBombBlockerAudio;

		public FMODAsset bombCompetitionAudio;

		public GameObject JammingArea;

		public ObjectOverlay borderLine;

		public Animator BombAnimator;

		public BombVisualController.TeamData[] TeamVisuals = new BombVisualController.TeamData[3];

		internal Identifiable BombCombatObject;

		internal Identifiable CarrierCombatObject;

		internal TeamType CurrentVisualTeam;

		private static BombVisualController instance;

		private FMODAudioManager.FMODAudio idleLoopToken;

		[SerializeField]
		private float _bombTensionAudioNormalizer;

		private float openRadius;

		private GameGui gameGUI;

		private BombVisualController.State currentState;

		private float timming;

		private Vector3 offset;

		private Material _bombMaterialInstance;

		private BombInstance.BombState _currentBombState;

		private CombatObject _currentPlayerCombatObject;

		public BombVisualController.BombIndicator Indicator;

		private float _minSpeedSqr;

		private float _maxSpeedSqr;

		private BombVisualController.PropertyIds _propertyIds;

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
