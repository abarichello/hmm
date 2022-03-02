using System;
using FMod;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class BombTargetTrigger : GameHubBehaviour
	{
		private void Start()
		{
			if (!GameHubBehaviour.Hub)
			{
				base.enabled = false;
				return;
			}
			this._isClient = GameHubBehaviour.Hub.Net.IsClient();
			this._isTutorial = GameHubBehaviour.Hub.Match.LevelIsTutorial();
			if (this._isClient)
			{
				return;
			}
			this._root = base.transform.parent;
			this._goingInReverse = (this._startProgress > this._endProgress);
			this._hits = new Vector3[2];
			this._overtimeDurationMsec = GameHubBehaviour.Hub.ArenaConfig.GetCurrentArena().OvertimeDurationSeconds * 1000f;
			this._saveTimeDurationMsec = (int)(GameHubBehaviour.Hub.BombManager.Rules.BombSaveTimeSeconds * 1000f);
			GameHubBehaviour.Hub.BombManager.ListenToBombCarrierChanged += this.OnBombCarrierChanged;
			this.Reset();
		}

		private void OnDestroy()
		{
			if (!this._isClient)
			{
				GameHubBehaviour.Hub.BombManager.ListenToBombCarrierChanged -= this.OnBombCarrierChanged;
			}
		}

		private void OnBombCarrierChanged(CombatObject carrier)
		{
			if (carrier == null || carrier.Team != this.TeamOwner)
			{
				this._lastTimeMyTeamPickedTheBombUp = 0;
				return;
			}
			this._lastTimeMyTeamPickedTheBombUp = GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
		}

		private void OnCollisionStay(Collision collision)
		{
			if (this._isClient)
			{
				return;
			}
			CombatObject combat = CombatRef.GetCombat(collision.collider);
			if (combat == null)
			{
				return;
			}
			if (!combat.IsBomb)
			{
				return;
			}
			if (GameHubBehaviour.Hub.BombManager.IsSomeoneCarryingBomb() && GameHubBehaviour.Hub.BombManager.TeamCarryingBomb() == this.TeamOwner)
			{
				return;
			}
			GameHubBehaviour.Hub.BombManager.CancelDispute();
			int playbackTime = GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			if (playbackTime - this._lastTimeMyTeamPickedTheBombUp > this._saveTimeDurationMsec)
			{
				GameHubBehaviour.Hub.BombManager.DropDetonator(combat, this.TeamOwner);
			}
		}

		public void Update()
		{
			if (this._isClient)
			{
				this.ClientUpdate();
				return;
			}
			if (this._bezierSpline == null)
			{
				return;
			}
			this.ServerUpdate();
		}

		public void OnDisable()
		{
			this.DisableOvertimeAudio();
		}

		private void ClientUpdate()
		{
			if (this._isTutorial)
			{
				return;
			}
			if (!this._isInOvertime && GameHubBehaviour.Hub.BombManager.ScoreBoard.IsInOvertime)
			{
				this.EnableOvertimeEffects();
			}
			else if (this._isInOvertime && (GameHubBehaviour.Hub.BombManager.CurrentBombGameState == BombScoreboardState.Shop || GameHubBehaviour.Hub.BombManager.CurrentBombGameState == BombScoreboardState.PreBomb))
			{
				this.DisableOvertimeEffects();
			}
			if (!this._isInOvertime)
			{
				return;
			}
			if (GameHubBehaviour.Hub.BombManager.CurrentBombGameState != BombScoreboardState.BombDelivery)
			{
				this.DisableOvertimeAudio();
			}
		}

		private void DisableOvertimeAudio()
		{
			if (this._overtimeLoopingAudioToken != null && !this._overtimeLoopingAudioToken.IsInvalidated())
			{
				this._overtimeLoopingAudioToken.Stop();
				this._overtimeLoopingAudioToken = null;
			}
		}

		private void ServerUpdate()
		{
			if (this._isTutorial)
			{
				return;
			}
			if (GameHubBehaviour.Hub.BombManager.CurrentBombGameState != BombScoreboardState.BombDelivery || !GameHubBehaviour.Hub.BombManager.ActiveBomb.IsSpawned)
			{
				return;
			}
			if (!GameHubBehaviour.Hub.BombManager.ScoreBoard.IsInOvertime)
			{
				return;
			}
			if (!this._isInOvertime)
			{
				this._overtimeElapsedTime = 0;
				this._overtimeLastTime = GameHubBehaviour.Hub.GameTime.MatchTimer.GetTime();
				this._isInOvertime = true;
				this._currentProgress = this._startProgress;
				this.EnableOvertimeEffects();
			}
			if ((this._goingInReverse && this._currentProgress <= this._endProgress) || (!this._goingInReverse && this._currentProgress >= this._endProgress))
			{
				return;
			}
			int time = GameHubBehaviour.Hub.GameTime.MatchTimer.GetTime();
			int num = time - this._overtimeLastTime;
			this._overtimeLastTime = time;
			this._overtimeElapsedTime += num;
			this._currentProgress = ((this._overtimeDurationMsec > 0f) ? Mathf.Lerp(this._startProgress, this._endProgress, (float)this._overtimeElapsedTime / this._overtimeDurationMsec) : 1f);
			this.UpdateTransformation();
		}

		private void UpdateTransformation()
		{
			this.EvaluateCurrentProgress(out this._evaluatedPosition, out this._evaluatedDirection);
			this._root.SetPositionAndRotation(this._evaluatedPosition, Quaternion.LookRotation(this._evaluatedDirection));
			Vector3 vector;
			vector..ctor(-this._evaluatedDirection.z, 0f, this._evaluatedDirection.x);
			this.CheckOvertimeColliderHit(0, this._evaluatedPosition, vector);
			vector *= -1f;
			this.CheckOvertimeColliderHit(1, this._evaluatedPosition, vector);
			Transform transform = base.transform;
			Vector3 localScale = transform.localScale;
			Vector3 vector2 = this._hits[1] - this._hits[0];
			localScale.x = vector2.magnitude;
			transform.localScale = localScale;
			vector2 *= 0.5f;
			transform.SetPositionAndRotation(this._hits[0] + vector2, Quaternion.LookRotation(Vector3.Cross(vector2, Vector3.up)));
		}

		private void EvaluateCurrentProgress(out Vector3 position, out Vector3 tangent)
		{
			this._bezierSpline.Evaluate(this._currentProgress, out position, out tangent, true);
			position.z = position.y;
			position.y = 0f;
			tangent.z = tangent.y;
			tangent.y = 0f;
			if (this._goingInReverse)
			{
				tangent = -tangent;
			}
		}

		private void CheckOvertimeColliderHit(int index, Vector3 origin, Vector3 direction)
		{
			RaycastHit2D raycastHit2D = PhysicsUtils.Raycast(origin, direction, this.RaycastSize, 524288);
			if (!raycastHit2D.collider)
			{
				return;
			}
			this._hits[index] = new Vector3(raycastHit2D.point.x, 0f, raycastHit2D.point.y);
		}

		public void Reset()
		{
			this._isInOvertime = false;
			this._overtimeElapsedTime = 0;
			this._currentProgress = this._startProgress;
			if (this._bezierSpline != null)
			{
				this.UpdateTransformation();
			}
			this._hits[0] = Vector3.zero;
			this._hits[1] = Vector3.zero;
			this.DisableOvertimeEffects();
		}

		private void DisableOvertimeEffects()
		{
			if (!this._isClient)
			{
				return;
			}
			this._isInOvertime = false;
		}

		public void EnableOvertimeEffects()
		{
			if (this._isClient)
			{
				this._isInOvertime = true;
				if (this._overtimeLoopingAudioToken == null && this._overtimeLoopingAudioAsset != null)
				{
					this._overtimeLoopingAudioToken = FMODAudioManager.PlayAt(this._overtimeLoopingAudioAsset, base.transform);
				}
			}
			else
			{
				GameHubBehaviour.Hub.BombManager.DispatchReliable(GameHubBehaviour.Hub.SendAll).ClientEnableOvertimeEffects(base.Id.ObjId);
			}
		}

		private void OnDrawGizmosSelected()
		{
			if (this._hits == null || this._hits.Length < 2)
			{
				return;
			}
			Gizmos.DrawCube(this._hits[0], Vector3.one * 2f);
			Gizmos.DrawCube(this._hits[1], Vector3.one * 2f);
		}

		public static readonly BitLogger Log = new BitLogger(typeof(BombTargetTrigger));

		public TeamKind TeamOwner;

		private bool _isInOvertime;

		public float RaycastSize = 500f;

		[SerializeField]
		[Tooltip("The spline that defines the path followed during overtime.")]
		private BezierSpline2D _bezierSpline;

		[SerializeField]
		[Tooltip("Where the delivery point must be when the round starts. This value represents the progress along Bezier Spline and ranges from 0 to 1.")]
		private float _startProgress;

		[SerializeField]
		[Tooltip("Where the overtime path ends. This value represents the progress along Bezier Spline and ranges from 0 to 1.")]
		private float _endProgress;

		[SerializeField]
		private AudioEventAsset _overtimeLoopingAudioAsset;

		private FMODAudioManager.FMODAudio _overtimeLoopingAudioToken;

		private float _currentProgress;

		private Vector3 _evaluatedPosition;

		private Vector3 _evaluatedDirection;

		private int _overtimeElapsedTime;

		private int _overtimeLastTime;

		private float _overtimeDurationMsec;

		private Vector3[] _hits;

		private bool _goingInReverse;

		private Transform _root;

		private bool _isClient;

		private bool _isTutorial;

		private int _lastTimeMyTeamPickedTheBombUp;

		private int _saveTimeDurationMsec;
	}
}
