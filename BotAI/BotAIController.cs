using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using HeavyMetalMachines.Car;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Combat.Gadget;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.BotAI
{
	public class BotAIController : GameHubBehaviour, IObjectSpawnListener, IPlayerController, ISerializationCallbackReceiver
	{
		public bool MovingCar
		{
			get
			{
				return this.Vertical != 0f;
			}
		}

		public bool AcceleratingForward
		{
			get
			{
				return this.Vertical > 0f;
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action ServerListenToReverseUse;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event CancelActionListener ListenToCancelAction;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action OnArrival;

		public IBotAIDirectives Directives
		{
			get
			{
				return this._directives;
			}
			set
			{
				this._directives = value;
				if (this._directives is BotAIGoalManager)
				{
					this._directivesGoalManager = (BotAIGoalManager)this._directives;
				}
				else
				{
					this._directivesCreepBotController = (CreepBotController)this._directives;
				}
			}
		}

		public override void OnAfterDeserialize()
		{
			base.OnAfterDeserialize();
			this.Directives = ((!(this._directivesGoalManager != null)) ? this._directivesCreepBotController : this._directivesGoalManager);
		}

		private void Awake()
		{
			this._timedUpdater = new TimedUpdater(150, true, false);
			this._checkAggroUpdater = new TimedUpdater(200, true, false);
			this._repathUpdater = new TimedUpdater(5000, true, false);
			if (BotAIPath.Terrain != null && BotAIPath.Terrain.Nodes != null)
			{
				BotAIPathFind.Acquire(BotAIPath.Terrain.Nodes);
			}
		}

		private void OnDestroy()
		{
			if (BotAIPath.Terrain != null && BotAIPath.Terrain.Nodes != null)
			{
				BotAIPathFind.Release();
			}
		}

		private void OnEnable()
		{
			if (!GameHubBehaviour.Hub || GameHubBehaviour.Hub.Net.IsClient())
			{
				base.enabled = false;
				return;
			}
			this._colisionDistance = 10f;
			CarComponentHub componentHub = base.Id.GetComponentHub<CarComponentHub>();
			this.AllNodesPath = BotAIPath.Terrain;
			this.BotPathFind = componentHub.botAIPathFind;
			this.BotPathFind.TargetNode = null;
			this.BotPathFind.OnAIPathFound += this.OnPathFound;
			if (!this.Combat)
			{
				this.Combat = componentHub.combatObject;
			}
			if (!this.CarInput)
			{
				this.CarInput = componentHub.carInput;
				this.CarInput.ForceDrivingStyle(CarInput.DrivingStyleKind.Bot);
			}
		}

		private void OnDisable()
		{
			if (this.BotPathFind)
			{
				this.BotPathFind.OnAIPathFound -= this.OnPathFound;
			}
		}

		private void Update()
		{
		}

		private IEnumerator StuckWorkAround()
		{
			this._doingWorkAround = true;
			float oldHorizontalValue = this.Horizontal;
			this.Horizontal = (float)UnityEngine.Random.Range(-1, 2);
			this.Vertical = -1f;
			yield return UnityUtils.WaitForOneSecond;
			this._repath = true;
			this.Vertical = 1f;
			this.Horizontal = oldHorizontalValue;
			this._doingWorkAround = false;
			yield break;
		}

		public float MaxDistanceSqr
		{
			get
			{
				if (this._maxDistanceSqr > 0f)
				{
					return this._maxDistanceSqr;
				}
				this._maxDistanceSqr = Mathf.Max(new float[]
				{
					this.Combat.CustomGadget0.GetRangeSqr(),
					this.Combat.CustomGadget1.GetRangeSqr(),
					this.Combat.CustomGadget2.GetRangeSqr()
				});
				this._maxDistanceSqr = Mathf.Min(this._maxDistanceSqr, this.MaxAttackDistance * this.MaxAttackDistance);
				return this._maxDistanceSqr;
			}
		}

		public CombatObject GetNearestEnemy(out float distance, bool charsOnly)
		{
			return BotAIUtils.SearchNearestCombatObject(this.Directives.GetEnemies(), this.Combat.transform.position, out distance, charsOnly);
		}

		private void MoveToNextPath()
		{
			this.CheckDistance();
			this.SetDirectionCanBeStraight(this._nextPointPosition - base.transform.position);
		}

		public void MoveToDirectionCanGoStraight(Vector3 finalPosition, bool isTargetPosition)
		{
			if (this._doingWorkAround)
			{
				return;
			}
			this.Vertical = this.rate;
			Vector3 directionCanBeStraight = finalPosition - this.Combat.Transform.position;
			if (isTargetPosition && this._currentCombatObject)
			{
				float sqrMagnitude = directionCanBeStraight.sqrMagnitude;
				if (sqrMagnitude <= this._colisionDistance * this._colisionDistance)
				{
					this.Vertical = 0f;
					this.Horizontal = 0f;
					this.Drift = false;
					this.Turbo = false;
					return;
				}
			}
			this.Turbo = false;
			this.SetDirectionCanBeStraight(directionCanBeStraight);
		}

		private void SetDirectionCanBeStraight(Vector3 direction)
		{
			this.Turbo = false;
			Vector3 vector = this.Combat.Transform.InverseTransformDirection(direction);
			this.Horizontal = ((Mathf.Abs(vector.x) <= 1f) ? vector.x : vector.normalized.x);
		}

		public CombatObject currentCombatObject
		{
			get
			{
				return this._currentCombatObject;
			}
			set
			{
				this._currentCombatObject = value;
				this._currentTransf = ((!(value != null)) ? null : value.Transform);
			}
		}

		private float GetRangeSqr()
		{
			return (this.rangeDesiredSqrDistance < 0f) ? BotAIController.rangeSqrDistanceDefault : this.rangeDesiredSqrDistance;
		}

		private void GoToFixedPath()
		{
			if (!this.Spawned)
			{
				return;
			}
			if (this._timedUpdater.ShouldHalt())
			{
				return;
			}
			if (this._currentFixedPath == null)
			{
				this.FindNextFixedPath();
			}
			if (this._currentFixedPath == null)
			{
				return;
			}
			this.GoToNode(this._currentFixedPath, this._currentFixedPath.transform, this.creepBotController.PatrolController.Path);
		}

		private void GoToTarget()
		{
			if (!this.Spawned || this._currentTransf == null)
			{
				return;
			}
			if (this._timedUpdater.ShouldHalt())
			{
				return;
			}
			BotAINode closestNode = BotAIPathFind.GetClosestNode(this.AllNodesPath, this._currentTransf.TransformPoint(this._offsetToTarget), this.Combat.Team, this.Combat.BombBlocked);
			this.GoToNode(closestNode, this._currentTransf, this.AllNodesPath);
			if (this._repathUpdater.ShouldHalt())
			{
				return;
			}
			if (closestNode == this._lastNode)
			{
				this._repath = true;
			}
			this._lastNode = closestNode;
		}

		private void GoToNode(BotAINode targetNode, Transform targetTransform, BotAIPath path)
		{
			if (!this.Directives.IsPathFixed() && (BotAIPathFind.GetClosestNode(path, this.Combat.transform.position, this.Combat.Team, this.Combat.BombBlocked) != targetNode || this._repath))
			{
				if (this.BotPathFind.TargetNode != targetNode || this._repath)
				{
					this._repath = false;
					this.BotPathFind.TargetNode = targetNode;
					Vector3 startingPosition = (this.goalManager.CurrentState != BotAIGoalManager.State.DeliverBomb) ? base.transform.position : GameHubBehaviour.Hub.BombManager.BombMovement.transform.position;
					this.BotPathFind.FindPath(path, startingPosition);
				}
				else if (this._pathMoving)
				{
					this.MoveToNextPath();
				}
			}
			else
			{
				if (targetTransform == this._currentTransf)
				{
					Vector3 finalPosition = targetTransform.TransformPoint(this._offsetToTarget);
					this.MoveToDirectionCanGoStraight(finalPosition, true);
				}
				else
				{
					this.MoveToDirectionCanGoStraight(targetTransform.position, false);
				}
				if (BotAIUtils.GetDistanceSqr(base.transform, targetTransform) < this.GetRangeSqr())
				{
					if (this.OnArrival != null)
					{
						this.OnArrival();
						this.OnArrival = null;
					}
					if (this.currentCombatObject == null && this.Directives.IsPathFixed())
					{
						this.FindNextFixedPath();
					}
				}
			}
		}

		private void FindNextFixedPath()
		{
			if (this._currentFixedPath == null)
			{
				this._currentFixedPathIndex = -1;
			}
			this._currentFixedPathIndex = (this._currentFixedPathIndex + 1) % this.creepBotController.PatrolController.Path.Nodes.Count;
			this._currentFixedPath = this.creepBotController.PatrolController.Path.Nodes[this._currentFixedPathIndex];
		}

		private void GoToWaypoint()
		{
			this.NextPointIndex++;
			if (this._pathFound.Count == 0 || this._pathFound.Count == this.NextPointIndex)
			{
				this.StopBot(false);
				return;
			}
			this._nextPointNode = this._pathFound[this.NextPointIndex];
			this._nextPointPosition = this._nextPointNode.transform.position;
			this.MoveToDirectionCanGoStraight(this._nextPointPosition, false);
			this._pathMoving = true;
		}

		private void OnPathFound(List<BotAINode> pathFound)
		{
			if (pathFound != null && pathFound.Count > 0)
			{
				this._pathFound = pathFound;
			}
			else
			{
				this._pathFound.Clear();
			}
			this.NextPointIndex = 0;
			this.GoToWaypoint();
		}

		private void CheckDistance()
		{
			bool flag = GameHubBehaviour.Hub.BombManager.ActiveBomb.IsSpawned && GameHubBehaviour.Hub.BombManager.IsCarryingBomb(base.Id.ObjId);
			Vector3 origin = (!flag) ? Vector3.zero : GameHubBehaviour.Hub.BombManager.BombMovement.transform.position;
			bool flag2 = false;
			Vector3 direction = this._nextPointPosition;
			for (int i = this.NextPointIndex + 2; i >= this.NextPointIndex - 2; i--)
			{
				if (i >= 0 && i < this._pathFound.Count)
				{
					direction = this._pathFound[i].transform.position - base.transform.position;
					if (!Physics.Raycast(base.transform.position, direction, direction.magnitude, LayerManager.GetBombAndTeamSceneryMask(false, this.Combat.Team)))
					{
						if (!flag || !Physics.Raycast(origin, direction, direction.magnitude, LayerManager.GetBombAndTeamSceneryMask(true, this.Combat.Team)))
						{
							this.NextPointIndex = i;
							this._nextPointNode = this._pathFound[this.NextPointIndex];
							this._nextPointPosition = this._nextPointNode.transform.position;
							this.MoveToDirectionCanGoStraight(this._nextPointPosition, false);
							this._pathMoving = true;
							flag2 = true;
							break;
						}
					}
				}
			}
			if (!flag2)
			{
				this._repath = true;
			}
			else
			{
				float sqrMagnitude = direction.sqrMagnitude;
				float num = (!this._nextPointNode) ? this._colisionDistance : this._nextPointNode.Range;
				if (num > 0f && sqrMagnitude >= num * num)
				{
					return;
				}
				this.GoToWaypoint();
			}
		}

		public void StopBot(bool stopAcceleration = false)
		{
			this._nextPointNode = null;
			this._nextPointPosition = Vector3.zero;
			this.NextPointIndex = 0;
			this._pathMoving = false;
			if (stopAcceleration)
			{
				this.CarInput.BotInput(0f, 0f, false);
			}
		}

		protected bool IsJammed()
		{
			return this.Combat.Attributes.CurrentStatus.HasFlag(StatusKind.Jammed);
		}

		protected bool IsDisarmed()
		{
			return this.Combat.Attributes.CurrentStatus.HasFlag(StatusKind.Disarmed);
		}

		public void ActivateUpdateInput()
		{
			StatusKind currentStatus = this.Combat.Attributes.CurrentStatus;
			bool flag = this.IsJammed();
			bool flag2 = currentStatus.HasFlag(StatusKind.Immobilized);
			if (flag2)
			{
				this.CarInput.BotInput(0f, 0f, false);
			}
			else if (this.rate < 0f)
			{
				this.CarInput.BotInput(-this.Horizontal, this.Vertical, this.UseDrift && this.Drift);
			}
			else
			{
				this.CarInput.BotInput(this.Horizontal, this.Vertical, this.UseDrift && this.Drift);
			}
			if (flag)
			{
				this.CancelCurrentAction();
			}
		}

		public void ActionExecuted(GadgetBehaviour gadget)
		{
			if (gadget != this.Combat.CustomGadget0 && gadget != this.Combat.CustomGadget1 && gadget != this.Combat.CustomGadget2 && gadget != this.Combat.BombGadget && gadget != this.Combat.BoostGadget)
			{
				return;
			}
			if (this.ListenToCancelAction != null)
			{
				this.ListenToCancelAction(gadget);
			}
		}

		public void OnObjectSpawned(SpawnEvent msg)
		{
			this.Spawned = true;
			this._doingWorkAround = false;
		}

		public void OnObjectUnspawned(UnspawnEvent msg)
		{
			this.Spawned = false;
		}

		public void CancelCurrentAction()
		{
			this.rangeDesiredSqrDistance = -1f;
			this.FireSponsorGadget = false;
		}

		public bool SetGoal(Transform transf)
		{
			this._offsetToTarget = Vector3.zero;
			this.currentCombatObject = null;
			this._currentTransf = transf;
			this.BotPathFind.StartingNode = null;
			BotAINode closestNode = BotAIPathFind.GetClosestNode(this.AllNodesPath, this._currentTransf.position, this.Combat.Team, this.Combat.BombBlocked);
			this.BotPathFind.TargetNode = closestNode;
			Vector3 startingPosition = (this.goalManager.CurrentState != BotAIGoalManager.State.DeliverBomb) ? base.transform.position : GameHubBehaviour.Hub.BombManager.BombMovement.transform.position;
			this.BotPathFind.FindPath(this.AllNodesPath, startingPosition);
			return this.BotPathFind.StartingNode;
		}

		public void SetGoal(CombatObject combatObject)
		{
			this._offsetToTarget = Vector3.zero;
			this.currentCombatObject = combatObject;
		}

		public void SetCreepController(CreepBotController botController)
		{
			this.Directives = botController;
			this.creepBotController = botController;
		}

		private void OnCollisionStay(Collision col)
		{
			if (GameHubBehaviour.Hub.Net.IsClient() || col.collider.gameObject.layer != 9 || this.Horizontal != 0f)
			{
				return;
			}
			float time = Time.time;
			if (this._lastStuckTime > 0f && time - this._lastStuckTime > this.StuckTime)
			{
				if (Vector3.SqrMagnitude(this.Combat.Transform.position - this._lastStuckPos) < this.StuckPosDelta * this.StuckPosDelta)
				{
					base.StartCoroutine(this.StuckWorkAround());
				}
				this._lastStuckTime = -1f;
				this._lastStuckPos = Vector3.zero;
				return;
			}
			if (this._lastStuckTime < 0f)
			{
				this._lastStuckTime = time;
				this._lastStuckPos = this.Combat.Transform.position;
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(BotAIController));

		public bool Spawned;

		public CarInput CarInput;

		public CombatObject Combat;

		public List<BotAINode> _pathFound = new List<BotAINode>(10);

		private BotAINode _nextPointNode;

		private Vector3 _nextPointPosition;

		public BotAIPath AllNodesPath;

		public BotAIPathFind BotPathFind;

		private const float Radius = 10f;

		private float _colisionDistance;

		public Color GizmoSphereColliderColor = new Color(0f, 1f, 0f, 1f);

		public Vector3 GizmoOffset;

		private Vector3 _finalPointNormalized;

		private TimedUpdater _timedUpdater;

		public float Horizontal;

		public float Vertical;

		public bool Drift;

		public bool UseDrift;

		public bool Turbo;

		public float rate = 1f;

		public int NextPointIndex;

		private bool _pathMoving;

		private Vector3 _offsetToTarget;

		[NonSerialized]
		private IBotAIDirectives _directives;

		private BotAIGoalManager _directivesGoalManager;

		private CreepBotController _directivesCreepBotController;

		public BotAIGoalManager goalManager;

		public CreepBotController creepBotController;

		private TimedUpdater _repathUpdater;

		private TimedUpdater _checkAggroUpdater;

		public bool _doingWorkAround;

		public bool PauseOnStuck;

		public bool SetTarget = true;

		public float MaxAttackDistance = 60f;

		private float _maxDistanceSqr;

		public float driftValuePos = 0.7f;

		public float driftValueNeg = -0.7f;

		public float crossProdCheck = 0.15f;

		public CombatObject _currentCombatObject;

		public Transform _currentTransf;

		public static float rangeSqrDistanceDefault = 150f;

		protected float rangeDesiredSqrDistance = -1f;

		private bool _repath;

		private BotAINode _lastNode;

		private BotAINode _currentFixedPath;

		private int _currentFixedPathIndex;

		public bool FireSponsorGadget;

		public Vector3 GenericGadgetTargetPosition;

		public Vector3 LiftGadgetTargetPosition;

		public float StuckTime = 2f;

		public float StuckPosDelta = 2f;

		private float _lastStuckTime = -1f;

		private Vector3 _lastStuckPos;

		public float curveRateScale = 3f;
	}
}
