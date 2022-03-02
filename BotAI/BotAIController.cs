using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using HeavyMetalMachines.AI;
using HeavyMetalMachines.AI.Steering;
using HeavyMetalMachines.Car;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Combat.Gadget;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.BotAI
{
	public class BotAIController : GameHubBehaviour, IObjectSpawnListener, IPlayerController, ISerializationCallbackReceiver
	{
		public IAIAgent AIAgent { get; set; }

		public bool MovingCar
		{
			get
			{
				return this.AIAgent.SteeringContext.Steering.MovingCar;
			}
		}

		public bool AcceleratingForward
		{
			get
			{
				return this.AIAgent.SteeringContext.Steering.AcceleratingForward;
			}
		}

		public Vector3 MousePosition
		{
			get
			{
				return this.AIAgent.SteeringContext.Steering.MousePosition;
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
			}
		}

		public override void OnAfterDeserialize()
		{
			base.OnAfterDeserialize();
			this.Directives = ((!(this._directivesGoalManager != null)) ? null : this._directivesGoalManager);
		}

		public ISteeringContext GetContext()
		{
			return this.AIAgent.SteeringContext;
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
			this.BotPathFind = componentHub.AIAgent.PathFind;
			this.BotPathFind.TargetNode = null;
			this.BotPathFind.OnAIPathFound += this.OnPathFound;
			if (!this.Combat)
			{
				this.Combat = componentHub.combatObject;
			}
			componentHub.carInput.ForceDrivingStyle(CarInput.DrivingStyleKind.Bot);
		}

		private void OnDisable()
		{
			if (this.BotPathFind)
			{
				this.BotPathFind.OnAIPathFound -= this.OnPathFound;
			}
		}

		public void SetEnabled(bool value)
		{
			base.enabled = value;
			this.AIAgent.BotContext.IsBotControlled = value;
		}

		private void Update()
		{
			if (this._currentTransf != null)
			{
				this.GoToTarget();
			}
			this.ActivateUpdateInput();
		}

		private IEnumerator StuckWorkAround()
		{
			this._doingWorkAround = true;
			yield return UnityUtils.WaitForOneSecond;
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
			this.AIAgent.BotContext.DesiredDestination = new Vector3?(this._nextPointPosition);
		}

		public void MoveToDirectionCanGoStraight(Vector3 finalPosition, bool isTargetPosition)
		{
			if (this._doingWorkAround)
			{
				return;
			}
			Vector3 vector = finalPosition - this.Combat.Transform.position;
			if (isTargetPosition && this._currentCombatObject)
			{
				float sqrMagnitude = vector.sqrMagnitude;
				if (sqrMagnitude <= this._colisionDistance * this._colisionDistance)
				{
					this.AIAgent.BotContext.DesiredDestination = null;
					return;
				}
			}
			this.AIAgent.BotContext.DesiredDestination = new Vector3?(finalPosition);
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
				if (BotAIUtils.GetDistanceSqr(base.transform, targetTransform) < this.GetRangeSqr() && this.OnArrival != null)
				{
					this.OnArrival();
					this.OnArrival = null;
				}
			}
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
			Vector3 vector = (!flag) ? Vector3.zero : GameHubBehaviour.Hub.BombManager.BombMovement.transform.position;
			bool flag2 = false;
			Vector3 vector2 = this._nextPointPosition;
			int num = this.NextPointIndex;
			bool isCarryingBomb = this.AIAgent.BotContext.IsCarryingBomb;
			for (int i = this.NextPointIndex; i <= this.NextPointIndex + 2; i++)
			{
				if (i >= this._pathFound.Count)
				{
					break;
				}
				BotAINode.NodeRequiredForPathKind requirementKind = this._pathFound[i].RequirementKind;
				num = i;
				if (requirementKind == BotAINode.NodeRequiredForPathKind.Always || (requirementKind == BotAINode.NodeRequiredForPathKind.WithBomb && isCarryingBomb) || (requirementKind == BotAINode.NodeRequiredForPathKind.WithoutBomb && !isCarryingBomb))
				{
					break;
				}
			}
			for (int j = num; j >= this.NextPointIndex - 2; j--)
			{
				if (j >= 0 && j < this._pathFound.Count)
				{
					vector2 = this._pathFound[j].transform.position - base.transform.position;
					if (!Physics.Raycast(base.transform.position, vector2, vector2.magnitude, LayerManager.GetBombAndTeamSceneryMask(false, this.Combat.Team)))
					{
						if (!flag || !Physics.Raycast(vector, vector2, vector2.magnitude, LayerManager.GetBombAndTeamSceneryMask(true, this.Combat.Team)))
						{
							this.NextPointIndex = j;
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
				float sqrMagnitude = vector2.sqrMagnitude;
				bool flag3;
				if (this._nextPointNode && this._nextPointNode.HasRangeCollider)
				{
					Ray ray;
					ray..ctor(base.transform.position + Vector3.up * 1000f, Vector3.down);
					RaycastHit raycastHit;
					flag3 = this._nextPointNode.RangeCollider.Raycast(ray, ref raycastHit, 1000f);
				}
				else
				{
					float num2 = (!this._nextPointNode) ? this._colisionDistance : this._nextPointNode.Range;
					flag3 = (num2 <= 0f || sqrMagnitude < num2 * num2);
				}
				if (flag3)
				{
					this.GoToWaypoint();
				}
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
				this.AIAgent.BotContext.DesiredDestination = null;
			}
		}

		public void StartBot()
		{
			this._repath = true;
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
			if (this.IsJammed())
			{
				this.CancelCurrentAction();
			}
			this.AIAgent.BotContext.IsCarryingBomb = this.Combat.IsCarryingBomb;
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

		private void OnCollisionStay(Collision col)
		{
			if (GameHubBehaviour.Hub.Net.IsClient() || col.collider.gameObject.layer != 9)
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

		public CombatObject Combat;

		public List<BotAINode> _pathFound = new List<BotAINode>(10);

		private BotAINode _nextPointNode;

		private Vector3 _nextPointPosition;

		public BotAIPath AllNodesPath;

		public BotAIPathFind BotPathFind;

		private const float Radius = 10f;

		private float _colisionDistance;

		public Vector3 GizmoOffset;

		private Vector3 _finalPointNormalized;

		private TimedUpdater _timedUpdater;

		public int NextPointIndex;

		private bool _pathMoving;

		private Vector3 _offsetToTarget;

		[NonSerialized]
		private IBotAIDirectives _directives;

		private BotAIGoalManager _directivesGoalManager;

		public BotAIGoalManager goalManager;

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
