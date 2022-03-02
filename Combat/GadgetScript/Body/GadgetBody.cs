using System;
using System.Collections.Generic;
using HeavyMetalMachines.Combat.GadgetScript.Block;
using HeavyMetalMachines.Combat.GadgetScript.Body.Filter;
using HeavyMetalMachines.Infra.Context;
using Hoplon.GadgetScript;
using Pocketverse;
using UnityEngine;
using UnityEngine.Serialization;

namespace HeavyMetalMachines.Combat.GadgetScript.Body
{
	public class GadgetBody : BaseGadgetBody
	{
		public override IGadgetContext Context
		{
			get
			{
				return this._context;
			}
		}

		public override Identifiable Identifiable
		{
			get
			{
				return this._identifiable;
			}
		}

		public override Transform Transform
		{
			get
			{
				return this._transform;
			}
		}

		public void Initialize(GadgetBody.InitializationParameters parameters)
		{
			base.Initialize();
			this._accumulatedDisplacement = 0f;
			this._lastPosition = base.transform.position;
			this._displacementInterval = (this._nextEventDisplacement = parameters.DisplacementEventInterval);
			this._isDisplacementEventBlockAssigned = (null != parameters.EventBlocks.OnDisplacementIntervalBlock);
			this._nonCombatCollisionLayers = parameters.NonCombatCollisionLayers;
			this.Initialize(parameters.GadgetContext, parameters.EventBlocks, parameters.EventParameters, parameters.TimedEventInterval, parameters.CollisionCheckTimeInterval, parameters.CollisionCheckCount, parameters.Filters, parameters.EventContext, parameters.Prefab, parameters.HitOverBarrier);
		}

		private void Initialize(IGadgetContext context, GadgetBody.BodyEventsBlocks eventBlocks, GadgetBody.BodyEventsParameters eventParameters, float eventTime, float collisionEventTime, int collisionTestsToPerform, List<ICombatFilter> filters, IEventContext eventContext, Transform prefab, bool hitOverBarrier)
		{
			this._context = (IHMMGadgetContext)context;
			if (this._context.IsClient && this._body != null)
			{
				Collider[] componentsInChildren = base.gameObject.GetComponentsInChildren<Collider>(true);
				Object.Destroy(this._body);
				this._body = null;
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					if (componentsInChildren[i])
					{
						Object.Destroy(componentsInChildren[i]);
					}
				}
			}
			this._prefab = prefab;
			this._eventsBlocks = eventBlocks;
			this._eventsParameters = eventParameters;
			this._eventInterval = eventTime;
			this._nextEventTime = eventTime;
			this._eventTimeBlockAssigned = (this._eventsBlocks.OnTimeIntervalBlock != null);
			this._collisionEventTimeBlockAssigned = (this._eventsBlocks.OnCheckCollisionBlock != null);
			this._movementFinishedBlockAssigned = (this._eventsBlocks.OnMovementFinishedBlock != null && this._movement != null);
			this._onEnterBlockAssigned = (this._eventsBlocks.OnEnterBlock != null);
			this._onStayBlockAssigned = (this._eventsBlocks.OnStayBlock != null);
			this._onExitBlockAssigned = (this._eventsBlocks.OnExitBlock != null);
			this._collisionEventInterval = collisionEventTime;
			this._nextCollisionEventTime = collisionEventTime;
			this._collisionTestsToPerform = collisionTestsToPerform;
			base.Id = this._context.GetNewBodyId();
			base.CreationEventId = eventContext.Id;
			this._creationTime = eventContext.CreationTime;
			base.WasSentToClient = ((IHMMEventContext)eventContext).ShouldBeSent;
			this._destroyReason = BaseFX.EDestroyReason.Default;
			this._filters = filters;
			this._hitOverBarrier = hitOverBarrier;
			this._numOfCollision = 0;
			this._movement.Initialize(this, context, eventContext);
			this._transform.position = this._movement.GetPosition(this._lastTime);
			this.SetBasicEventParameters(this._elapsedTime);
			this._parameters.Clear();
			this._eventsParameters.AddParametersToList(this._parameters);
			this._hitObjects.Clear();
			base.IsAlive = true;
			this.CheckCollisionAndDecrementCounter();
			base.RaiseBodyInitialized();
		}

		public override void Destroy()
		{
			base.Destroy();
			this.TestCollision(this._eventsBlocks.OnExitBlock);
			base.IsAlive = false;
			base.RaiseBodyDestroyed();
			this._movement.Destroy();
			this._elapsedTime = (this._lastTime = 0f);
			if (this._collisionTestsToPerform > 0 && this._context.IsServer)
			{
				GadgetBody.Log.WarnFormat("Body {0} destroyed before all collision tests were performed ({1} remaining).", new object[]
				{
					base.name,
					this._collisionTestsToPerform
				});
			}
			this._collidersCollided.Clear();
			this._combatCollisionIsBarrier.Clear();
			ResourceLoader.Instance.ReturnToPrefabCache(this._prefab, this);
			if (this._context.IsServer && this._eventsBlocks.OnDestroyed != null)
			{
				this._context.TriggerEvent(GadgetEvent.GetInstance(this._eventsBlocks.OnDestroyed.Id, this._context, this._parameters));
			}
		}

		public float GetRemainingTime()
		{
			return this._nextEventTime - this._elapsedTime;
		}

		public List<BaseParameter> GetEventParameters()
		{
			this.SetBasicEventParameters(this._elapsedTime);
			return this._parameters;
		}

		private void Awake()
		{
			this._transform = base.transform;
			this._body = base.GetComponent<Rigidbody>();
			this._identifiable = base.GetComponent<Identifiable>();
			this._movement = base.GetComponent<IGadgetBodyMovement>();
			if (this._movement == null)
			{
				this._movement = new NullBodyMovement(base.transform);
			}
			this._gadgetBoomerangMovement = (this._movement as GadgetBodyBoomerangMovement);
		}

		private void Update()
		{
			if (this._context == null)
			{
				return;
			}
			this._transform.LookAt(this._transform.position + this._movement.GetDirection());
			if (this._context.IsServer)
			{
				this.ProcessCollisionEvents();
				this.TestEventTime();
				this.TestMovementFinished();
				this.CheckDisplacement();
				return;
			}
			this.UpdateElapsedTime((float)(this._context.CurrentTime - this._creationTime) * 0.001f);
			this._transform.position = this._movement.GetPosition(this._lastTime);
			if (this._gadgetBoomerangMovement && this._gadgetBoomerangMovement.isReturning)
			{
				this._transform.Rotate(0f, 180f, 0f);
			}
		}

		private void FixedUpdate()
		{
			if (!base.IsAlive || this._context == null || this._context.IsClient)
			{
				return;
			}
			this.UpdateElapsedTime((float)(this._context.CurrentTime - this._creationTime) * 0.001f);
			this.UpdateVelocity(this._movement.GetPosition(this._elapsedTime));
			this.TestCollisionEventTime();
		}

		private void UpdateElapsedTime(float newElapsedTime)
		{
			if (!Mathf.Approximately(this._elapsedTime, newElapsedTime))
			{
				this._lastTime = this._elapsedTime;
				this._elapsedTime = newElapsedTime;
			}
		}

		private void TestMovementFinished()
		{
			if (!this._movementFinishedBlockAssigned || !this._movement.Finished)
			{
				return;
			}
			this.SetBasicEventParameters(this._elapsedTime);
			this._transform.position = this._movement.GetPosition(this._elapsedTime);
			this._destroyReason = BaseFX.EDestroyReason.Lifetime;
			this._context.TriggerEvent(GadgetEvent.GetInstance(this._eventsBlocks.OnMovementFinishedBlock.Id, this._context, this._parameters));
		}

		private void TestEventTime()
		{
			if (!this._eventTimeBlockAssigned || this._nextEventTime - this._elapsedTime > Mathf.Epsilon)
			{
				return;
			}
			this.SetBasicEventParameters(this._elapsedTime);
			this._destroyReason = BaseFX.EDestroyReason.Lifetime;
			this._nextEventTime += this._eventInterval;
			this._context.TriggerEvent(GadgetEvent.GetInstance(this._eventsBlocks.OnTimeIntervalBlock.Id, this._context, this._parameters));
		}

		private void TestCollisionEventTime()
		{
			if (!this._collisionEventTimeBlockAssigned || this._collisionTestsToPerform <= 0 || this._nextCollisionEventTime - this._elapsedTime > Mathf.Epsilon)
			{
				return;
			}
			this.CheckCollisionAndDecrementCounter();
			this._nextCollisionEventTime += this._collisionEventInterval;
		}

		private void CheckCollisionAndDecrementCounter()
		{
			this.TestCollision(this._eventsBlocks.OnCheckCollisionBlock);
			this._collisionTestsToPerform--;
		}

		private void UpdateVelocity(Vector3 newPos)
		{
			this._body.velocity = Vector3.zero;
			if (Time.deltaTime > 0f)
			{
				this._body.velocity = (newPos - this._body.position) / Time.deltaTime;
			}
		}

		private void SetBasicEventParameters(float eventTime)
		{
			this._eventsParameters.SetParameter<GadgetBody>(this._context, this._eventsParameters.Body, this);
			if (null != this._eventsParameters.ElapsedTime)
			{
				IParameterTomate<float> parameterTomate = this._eventsParameters.ElapsedTime.ParameterTomate as IParameterTomate<float>;
				parameterTomate.SetValue(this._context, eventTime);
			}
			this._eventsParameters.SetParameter<Vector3>(this._context, this._eventsParameters.Direction, this._movement.GetDirection());
			this._eventsParameters.SetParameter<Vector3>(this._context, this._eventsParameters.Position, this._movement.GetPosition(eventTime));
		}

		private void ProcessCollisionEvents()
		{
			for (int i = 0; i < this._numOfCollision; i++)
			{
				if (!base.IsAlive)
				{
					break;
				}
				Collider collider = this._collisions[i].collider;
				IBlock block = this._collisions[i].block;
				Vector3 normal = this._collisions[i].normal;
				Vector3 position = this._collisions[i].position;
				float time = this._collisions[i].time;
				ICombatObject combat = this._collisions[i].combat;
				bool isCombat = this._collisions[i].isCombat;
				Vector3 targetPosition = this._collisions[i].targetPosition;
				if (!isCombat || this._combatCollisionIsBarrier.ContainsKey(combat))
				{
					bool flag = this._filters.Count == 0;
					for (int j = 0; j < this._filters.Count; j++)
					{
						flag |= this._filters[j].Match(combat, this._context.Owner as ICombatObject, collider);
					}
					if (flag)
					{
						int id = block.Id;
						this.SetBasicEventParameters(time);
						if (isCombat)
						{
							this._eventsParameters.SetParameter<bool>(this._context, this._eventsParameters.IsBarrier, this._combatCollisionIsBarrier[combat]);
						}
						this._eventsParameters.SetParameter<Vector3>(this._context, this._eventsParameters.Position, this._body.position);
						this._eventsParameters.SetParameter<Vector3>(this._context, this._eventsParameters.CollisionNormal, normal);
						this._eventsParameters.SetParameter<Vector3>(this._context, this._eventsParameters.TargetPosition, targetPosition);
						this._eventsParameters.SetParameter<Vector3>(this._context, this._eventsParameters.CollisionPosition, position);
						if (isCombat)
						{
							this._combatCollisionIsBarrier.Remove(combat);
						}
						if (combat != null)
						{
							this._destroyReason = BaseFX.EDestroyReason.HitIdentifiable;
							this._eventsParameters.SetParameter<ICombatObject>(this._context, this._eventsParameters.Target, combat);
							if (!this._hitObjects.Contains(combat))
							{
								this._eventsParameters.SetParameter<ICombatObject>(this._context, this._eventsParameters.UniqueCombatCollision, combat);
							}
							this._hitObjects.Add(combat);
							this._context.TriggerEvent(GadgetEvent.GetInstance(id, this._context, this._parameters));
						}
						else if (this.IsOnExpectedNonCombatCollisionLayer(collider))
						{
							this._destroyReason = BaseFX.EDestroyReason.HitScenery;
							this._eventsParameters.SetParameter<ICombatObject>(this._context, this._eventsParameters.Target, null);
							this._context.TriggerEvent(GadgetEvent.GetInstance(id, this._context, this._parameters));
						}
					}
				}
			}
			this._numOfCollision = 0;
			this._collidersCollided.Clear();
		}

		private bool IsOnExpectedNonCombatCollisionLayer(Collider col)
		{
			int num = 1 << col.gameObject.layer;
			return (this._nonCombatCollisionLayers & num) > 0;
		}

		private void OnCollisionEvent(IBlock block, Collider col, Vector3 normal, Vector3 position)
		{
			if (!base.IsAlive || this._context.IsClient || this._collidersCollided.Contains(col))
			{
				return;
			}
			this._collidersCollided.Add(col);
			ICombatObject combatObject = this._context.GetCombatObject(col);
			bool flag = combatObject != null;
			if (flag && combatObject.NoHit)
			{
				return;
			}
			if (flag && (!this._combatCollisionIsBarrier.ContainsKey(combatObject) || (!this._combatCollisionIsBarrier[combatObject] && !this._hitOverBarrier) || (this._combatCollisionIsBarrier[combatObject] && this._hitOverBarrier)))
			{
				this._combatCollisionIsBarrier[combatObject] = BarrierUtils.IsBarrier(col);
			}
			Vector3 position2 = col.transform.position;
			float lastTime = this._lastTime;
			this._collisions[this._numOfCollision].block = block;
			this._collisions[this._numOfCollision].collider = col;
			this._collisions[this._numOfCollision].normal = normal;
			this._collisions[this._numOfCollision].position = position;
			this._collisions[this._numOfCollision].combat = combatObject;
			this._collisions[this._numOfCollision].isCombat = flag;
			this._collisions[this._numOfCollision].targetPosition = position2;
			this._numOfCollision++;
		}

		private void TestCollision(IBlock eventBlock)
		{
			if (eventBlock == null || this._body == null)
			{
				return;
			}
			this._body.position += GadgetBody.Translation;
			RaycastHit[] array = this._body.SweepTestAll(Vector3.down, 100f, 2);
			Array.Sort<RaycastHit>(array, new Comparison<RaycastHit>(this.SortHitsByCenter));
			this._body.position -= GadgetBody.Translation;
			for (int i = 0; i < array.Length; i++)
			{
				Vector3 normal = array[i].normal;
				Vector3 point = array[i].point;
				normal.y = 0f;
				normal = -normal.normalized;
				this.OnCollisionEvent(eventBlock, array[i].collider, normal, point);
			}
			this.ProcessCollisionEvents();
		}

		private int SortHitsByCenter(RaycastHit x, RaycastHit y)
		{
			Vector3 position = this._body.position;
			float num = Vector3.SqrMagnitude(position - x.point);
			float value = Vector3.SqrMagnitude(position - y.point);
			return num.CompareTo(value);
		}

		private void OnTriggerStay(Collider other)
		{
			if (!this._onStayBlockAssigned)
			{
				return;
			}
			this.OnCollisionEvent(this._eventsBlocks.OnStayBlock, other, Vector3.zero, Vector3.zero);
		}

		private void OnTriggerEnter(Collider other)
		{
			if (!this._onEnterBlockAssigned)
			{
				return;
			}
			this.OnCollisionEvent(this._eventsBlocks.OnEnterBlock, other, Vector3.zero, Vector3.zero);
		}

		private void OnTriggerExit(Collider other)
		{
			if (!this._onExitBlockAssigned)
			{
				return;
			}
			this.OnCollisionEvent(this._eventsBlocks.OnExitBlock, other, Vector3.zero, Vector3.zero);
		}

		private void OnCollisionStay(Collision collisionInfo)
		{
			if (!this._onStayBlockAssigned)
			{
				return;
			}
			ContactPoint contactPoint = collisionInfo.contacts[0];
			this.OnCollisionEvent(this._eventsBlocks.OnStayBlock, collisionInfo.collider, contactPoint.normal, contactPoint.point);
		}

		private void OnCollisionEnter(Collision collisionInfo)
		{
			if (!this._onEnterBlockAssigned)
			{
				return;
			}
			ContactPoint contactPoint = collisionInfo.contacts[0];
			this.OnCollisionEvent(this._eventsBlocks.OnEnterBlock, collisionInfo.collider, contactPoint.normal, contactPoint.point);
		}

		private void OnCollisionExit(Collision collisionInfo)
		{
			if (!this._onExitBlockAssigned)
			{
				return;
			}
			ContactPoint contactPoint = collisionInfo.contacts[0];
			this.OnCollisionEvent(this._eventsBlocks.OnExitBlock, collisionInfo.collider, contactPoint.normal, contactPoint.point);
		}

		private void CheckDisplacement()
		{
			if (!this._isDisplacementEventBlockAssigned || !base.IsAlive)
			{
				return;
			}
			Vector3 position = this._movement.GetPosition(this._elapsedTime);
			Vector3 vector = position - this._lastPosition;
			this._lastPosition = position;
			float magnitude = vector.magnitude;
			float accumulatedDisplacement = this._accumulatedDisplacement;
			this._accumulatedDisplacement += magnitude;
			while (this._accumulatedDisplacement >= this._nextEventDisplacement)
			{
				float num = (this._nextEventDisplacement - accumulatedDisplacement) / magnitude;
				float basicEventParameters = Mathf.Lerp(this._lastTime, this._elapsedTime, num);
				this.SetBasicEventParameters(basicEventParameters);
				this._nextEventDisplacement += this._displacementInterval;
				this._context.TriggerEvent(GadgetEvent.GetInstance(this._eventsBlocks.OnDisplacementIntervalBlock.Id, this._context, this._parameters));
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(GadgetBody));

		private IHMMGadgetContext _context;

		private IGadgetBodyMovement _movement;

		private bool _eventTimeBlockAssigned;

		private bool _isDisplacementEventBlockAssigned;

		private bool _collisionEventTimeBlockAssigned;

		private bool _movementFinishedBlockAssigned;

		private bool _onEnterBlockAssigned;

		private bool _onStayBlockAssigned;

		private bool _onExitBlockAssigned;

		private int _creationTime;

		private float _lastTime;

		private float _elapsedTime;

		private float _eventInterval;

		private float _nextEventTime;

		private float _collisionEventInterval;

		private float _nextCollisionEventTime;

		private int _collisionTestsToPerform;

		private float _accumulatedDisplacement;

		private float _displacementInterval;

		private float _nextEventDisplacement;

		private Vector3 _lastPosition;

		private Transform _prefab;

		private LayerMask _nonCombatCollisionLayers;

		private Transform _transform;

		private Rigidbody _body;

		private Identifiable _identifiable;

		private GadgetBody.BodyEventsBlocks _eventsBlocks;

		private GadgetBody.BodyEventsParameters _eventsParameters;

		private List<BaseParameter> _parameters = new List<BaseParameter>(32);

		private List<ICombatFilter> _filters;

		private readonly Dictionary<ICombatObject, bool> _combatCollisionIsBarrier = new Dictionary<ICombatObject, bool>();

		private bool _hitOverBarrier;

		private HashSet<ICombatObject> _hitObjects = new HashSet<ICombatObject>();

		private GadgetBodyBoomerangMovement _gadgetBoomerangMovement;

		private readonly HashSet<Collider> _collidersCollided = new HashSet<Collider>();

		private readonly GadgetBody.GadgetBodyCollision[] _collisions = new GadgetBody.GadgetBodyCollision[128];

		private int _numOfCollision;

		private const float TranslationDistance = 100f;

		private static readonly Vector3 Translation = new Vector3(0f, 100f, 0f);

		[Serializable]
		public class BodyEventsBlocks
		{
			public BaseBlock OnEnterBlock;

			public BaseBlock OnStayBlock;

			public BaseBlock OnExitBlock;

			[Tooltip("Event called if is colliding the moment this body is created and on Collision Interval")]
			[FormerlySerializedAs("OnInstantCollisionBlock")]
			public BaseBlock OnCheckCollisionBlock;

			[Tooltip("Event called every Interval of Time")]
			public BaseBlock OnTimeIntervalBlock;

			[Tooltip("Event called when the Movement is finished")]
			public BaseBlock OnMovementFinishedBlock;

			[Tooltip("Event called every Interval of Movement")]
			public BaseBlock OnDisplacementIntervalBlock;

			[Tooltip("Called when the gadget body is detroyed.")]
			public BaseBlock OnDestroyed;
		}

		[Serializable]
		public class BodyEventsParameters
		{
			public void SetParameter<T>(object context, BaseParameter parameter, T value)
			{
				if (parameter != null)
				{
					parameter.SetValue<T>(context, value);
				}
			}

			public void AddParametersToList(List<BaseParameter> parameters)
			{
				if (this.Position != null)
				{
					parameters.Add(this.Position);
				}
				if (this.Direction != null)
				{
					parameters.Add(this.Direction);
				}
				if (this.Target != null)
				{
					parameters.Add(this.Target);
				}
				if (this.CollisionNormal != null)
				{
					parameters.Add(this.CollisionNormal);
				}
				if (this.CollisionPosition != null)
				{
					parameters.Add(this.CollisionPosition);
				}
				if (this.Body != null)
				{
					parameters.Add(this.Body);
				}
				if (this.TargetPosition != null)
				{
					parameters.Add(this.TargetPosition);
				}
				if (this.ElapsedTime != null)
				{
					parameters.Add(this.ElapsedTime);
				}
				if (this.IsBarrier != null)
				{
					parameters.Add(this.IsBarrier);
				}
			}

			[Restrict(false, new Type[]
			{
				typeof(Vector3)
			})]
			public BaseParameter Position;

			[Restrict(false, new Type[]
			{
				typeof(Vector3)
			})]
			public BaseParameter TargetPosition;

			[Restrict(false, new Type[]
			{
				typeof(Vector3)
			})]
			public BaseParameter Direction;

			[Restrict(false, new Type[]
			{
				typeof(bool)
			})]
			public BaseParameter IsBarrier;

			[Restrict(false, new Type[]
			{
				typeof(ICombatObject)
			})]
			public BaseParameter Target;

			[Restrict(false, new Type[]
			{
				typeof(float)
			})]
			public BaseParameter ElapsedTime;

			[Restrict(false, new Type[]
			{
				typeof(IGadgetBody)
			})]
			public BaseParameter Body;

			[Restrict(false, new Type[]
			{
				typeof(IGadgetBody)
			})]
			public BaseParameter TargetBody;

			[Restrict(false, new Type[]
			{
				typeof(bool)
			})]
			[Tooltip("First time colliding with a given Combat")]
			public BaseParameter UniqueCombatCollision;

			[Header("Only work with Non trigger collider")]
			[Restrict(false, new Type[]
			{
				typeof(Vector3)
			})]
			public BaseParameter CollisionNormal;

			[Restrict(false, new Type[]
			{
				typeof(Vector3)
			})]
			public BaseParameter CollisionPosition;
		}

		private struct GadgetBodyCollision
		{
			public Collider collider;

			public IBlock block;

			public Vector3 normal;

			public Vector3 position;

			public float time;

			public ICombatObject combat;

			public bool isCombat;

			public Vector3 targetPosition;
		}

		public struct InitializationParameters
		{
			public IGadgetContext GadgetContext;

			public GadgetBody.BodyEventsBlocks EventBlocks;

			public GadgetBody.BodyEventsParameters EventParameters;

			public float TimedEventInterval;

			public float CollisionCheckTimeInterval;

			public int CollisionCheckCount;

			public float DisplacementEventInterval;

			public List<ICombatFilter> Filters;

			public IEventContext EventContext;

			public bool HitOverBarrier;

			public Transform Prefab;

			public LayerMask NonCombatCollisionLayers;
		}
	}
}
