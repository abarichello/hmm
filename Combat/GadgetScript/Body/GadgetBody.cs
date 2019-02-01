using System;
using System.Collections.Generic;
using HeavyMetalMachines.Combat.GadgetScript.Block;
using HeavyMetalMachines.Combat.GadgetScript.Body.Filter;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.VFX;
using Hoplon.GadgetScript;
using Pocketverse;
using UnityEngine;
using UnityEngine.Serialization;

namespace HeavyMetalMachines.Combat.GadgetScript.Body
{
	public class GadgetBody : MonoBehaviour, IGadgetBody
	{
		public int Id { get; private set; }

		public IEventContext Event { get; private set; }

		public int OwnerId
		{
			get
			{
				return this._context.OwnerId;
			}
		}

		public IHMMGadgetContext Context
		{
			get
			{
				return this._context;
			}
		}

		public Vector3 Position
		{
			get
			{
				return base.transform.position;
			}
		}

		public Quaternion Rotation
		{
			get
			{
				return base.transform.rotation;
			}
		}

		public string Tag
		{
			get
			{
				return base.gameObject.tag;
			}
		}

		public float ReamainingWhatever
		{
			get
			{
				return 0f;
			}
		}

		public void Initialize(IGadgetContext context, GadgetBody.BodyEventsBlocks eventBlocks, GadgetBody.BodyEventsParameters eventParameters, float eventTime, float collisionEventTime, int collisionTestsToPerform, List<ICombatFilter> filters, IEventContext eventContext, Transform prefab)
		{
			this._context = (IHMMGadgetContext)context;
			this._prefab = prefab;
			this._eventsBlocks = eventBlocks;
			this._eventsParameters = eventParameters;
			this._eventInterval = eventTime;
			this._nextEventTime = eventTime;
			this._eventTimeBlockAssigned = (this._eventsBlocks.OnTimeIntervalBlock != null);
			this._collisionEventTimeBlockAssigned = (this._eventsBlocks.OnCheckCollisionBlock != null);
			this._movementFinishedBlockAssigned = (this._eventsBlocks.OnMovementFinishedBlock != null && this._movement != null);
			this._collisionEventInterval = collisionEventTime;
			this._nextCollisionEventTime = collisionEventTime;
			this._collisionTestsToPerform = collisionTestsToPerform;
			this.Id = this._context.GetNewBodyId();
			this.Event = eventContext;
			this._attachedVFXs.Clear();
			this._destroyReason = BaseFX.EDestroyReason.Default;
			this._filters = filters;
			if (this._movement != null)
			{
				this._movement.Initialize(this, context, eventContext);
			}
			this.UpdatePositionAndDirection();
			this._parameters = new List<BaseParameter>();
			this._eventsParameters.AddParametersToList(this._parameters);
			this._isAlive = true;
			this.CheckCollisionAndDecrementCounter();
		}

		public void AttachVFX(MasterVFX vfx)
		{
			this._attachedVFXs.Add(vfx);
		}

		public bool IsAlive
		{
			get
			{
				return this._isAlive;
			}
		}

		public void Destroy()
		{
			this._isAlive = false;
			if (this._movement != null)
			{
				this._movement.Destroy();
			}
			this._elapsedTime = 0f;
			if (this._collisionTestsToPerform > 0 && this._context.IsServer)
			{
				GadgetBody.Log.WarnFormat("Body {0} destroyed before all collision tests were performed ({1} remaining).", new object[]
				{
					base.name,
					this._collisionTestsToPerform
				});
			}
			for (int i = 0; i < this._attachedVFXs.Count; i++)
			{
				this._attachedVFXs[i].Destroy(this._destroyReason);
			}
			this.TestCollision(this._eventsBlocks.OnExitBlock);
			ResourceLoader.Instance.ReturnToPrefabCache(this._prefab, this);
		}

		public float GetRemainingTime()
		{
			return this._nextEventTime - this._elapsedTime;
		}

		private void Awake()
		{
			this._transform = base.transform;
			this._body = base.GetComponent<Rigidbody>();
			this._movement = base.GetComponent<IGadgetBodyMovement>();
		}

		private void Update()
		{
			if (this._context.IsServer || this._movement == null)
			{
				return;
			}
			this._transform.position = this._movement.GetPosition(this._elapsedTime);
			this._elapsedTime = (float)(this._context.CurrentTime - this.Event.CreationTime) * 0.001f;
		}

		private void FixedUpdate()
		{
			if (this._context.IsClient || !this._isAlive)
			{
				return;
			}
			if (this._movement != null)
			{
				this.UpdateVelocity(this._movement.GetPosition(this._elapsedTime));
			}
			this._elapsedTime = (this._elapsedTime = (float)(this._context.CurrentTime - this.Event.CreationTime) * 0.001f);
			if (!this._isAlive)
			{
				return;
			}
			this.TestCollisionEventTime();
			this.TestEventTime();
			this.TestMovementFinished();
		}

		private void TestMovementFinished()
		{
			if (!this._movementFinishedBlockAssigned || !this._movement.Finished)
			{
				return;
			}
			this.UpdatePositionAndDirection();
			this._destroyReason = BaseFX.EDestroyReason.Lifetime;
			this._eventsParameters.SetParameter<GadgetBody>(this._context, this._eventsParameters.Body, this);
			this._eventsParameters.SetParameter<float>(this._context, this._eventsParameters.ElapsedTime, this._elapsedTime);
			this._context.TriggerEvent(new GadgetEvent(this._eventsBlocks.OnMovementFinishedBlock.Id, this._context, this._parameters));
		}

		private void TestEventTime()
		{
			if (!this._eventTimeBlockAssigned || this._nextEventTime - this._elapsedTime > Mathf.Epsilon)
			{
				return;
			}
			this.UpdatePositionAndDirection();
			this._destroyReason = BaseFX.EDestroyReason.Lifetime;
			this._eventsParameters.SetParameter<GadgetBody>(this._context, this._eventsParameters.Body, this);
			this._eventsParameters.SetParameter<float>(this._context, this._eventsParameters.ElapsedTime, this._elapsedTime);
			this._context.TriggerEvent(new GadgetEvent(this._eventsBlocks.OnTimeIntervalBlock.Id, this._context, this._parameters));
			this._nextEventTime += this._eventInterval;
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
			if (Time.deltaTime > 0f)
			{
				this._body.velocity = (newPos - this._body.position) / Time.deltaTime;
			}
			else
			{
				this._body.velocity = Vector3.zero;
			}
		}

		private void UpdatePositionAndDirection()
		{
			if (this._movement != null)
			{
				this._eventsParameters.SetParameter<Vector3>(this._context, this._eventsParameters.Direction, this._movement.GetDirection());
			}
			this._eventsParameters.SetParameter<Vector3>(this._context, this._eventsParameters.Position, this._transform.position);
		}

		private void OnCollisionEvent(IBlock block, Collider other)
		{
			if (block == null || this._context.IsClient)
			{
				return;
			}
			CombatObject combatObject = this._context.GetCombatObject(other) as CombatObject;
			if (BarrierUtils.IsBarrier(other))
			{
				return;
			}
			bool flag = this._filters.Count == 0;
			for (int i = 0; i < this._filters.Count; i++)
			{
				flag |= this._filters[i].Match(combatObject, this._context.GetCombatObject(this.OwnerId), other);
			}
			if (!flag)
			{
				return;
			}
			int id = block.Id;
			this.UpdatePositionAndDirection();
			this._eventsParameters.SetParameter<GadgetBody>(this._context, this._eventsParameters.Body, this);
			this._eventsParameters.SetParameter<Vector3>(this._context, this._eventsParameters.TargetPosition, other.transform.position);
			this._eventsParameters.SetParameter<float>(this._context, this._eventsParameters.ElapsedTime, this._elapsedTime);
			if (combatObject != null)
			{
				this._destroyReason = BaseFX.EDestroyReason.HitIdentifiable;
				this._eventsParameters.SetParameter<ICombatObject>(this._context, this._eventsParameters.Target, combatObject);
				this._context.TriggerEvent(new GadgetEvent(id, this._context, this._parameters));
			}
			else if (other.gameObject.layer == 9)
			{
				this._destroyReason = BaseFX.EDestroyReason.HitScenery;
				this._eventsParameters.SetParameter<ICombatObject>(this._context, this._eventsParameters.Target, null);
				this._context.TriggerEvent(new GadgetEvent(id, this._context, this._parameters));
			}
		}

		private void TestCollision(IBlock eventBlock)
		{
			if (eventBlock == null)
			{
				return;
			}
			this._body.position += GadgetBody.Translation;
			RaycastHit[] array = this._body.SweepTestAll(Vector3.down, 100f, QueryTriggerInteraction.Collide);
			Array.Sort<RaycastHit>(array, new Comparison<RaycastHit>(this.SortHitsByCenter));
			for (int i = 0; i < array.Length; i++)
			{
				if (this._eventsParameters.CollisionNormal != null)
				{
					Vector3 normal = array[i].normal;
					normal.y = 0f;
					this._eventsParameters.SetParameter<Vector3>(this._context, this._eventsParameters.CollisionNormal, -normal.normalized);
				}
				this.OnCollisionEvent(eventBlock, array[i].collider);
			}
			this._body.position -= GadgetBody.Translation;
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
			if (!this._isAlive)
			{
				return;
			}
			this._eventsParameters.SetParameter<Vector3>(this._context, this._eventsParameters.CollisionNormal, (other.transform.position - this._body.position).normalized);
			this.OnCollisionEvent(this._eventsBlocks.OnStayBlock, other);
		}

		private void OnTriggerEnter(Collider other)
		{
			if (!this._isAlive)
			{
				return;
			}
			this._eventsParameters.SetParameter<Vector3>(this._context, this._eventsParameters.CollisionNormal, (other.transform.position - this._body.position).normalized);
			this.OnCollisionEvent(this._eventsBlocks.OnEnterBlock, other);
		}

		private void OnTriggerExit(Collider other)
		{
			if (!this._isAlive)
			{
				return;
			}
			this._eventsParameters.SetParameter<Vector3>(this._context, this._eventsParameters.CollisionNormal, Vector3.zero);
			this.OnCollisionEvent(this._eventsBlocks.OnExitBlock, other);
		}

		private void OnCollisionStay(Collision collisionInfo)
		{
			if (!this._isAlive)
			{
				return;
			}
			this._eventsParameters.SetParameter<Vector3>(this._context, this._eventsParameters.CollisionNormal, -collisionInfo.contacts[0].normal);
			this.OnCollisionEvent(this._eventsBlocks.OnStayBlock, collisionInfo.collider);
		}

		private void OnCollisionEnter(Collision collisionInfo)
		{
			if (!this._isAlive)
			{
				return;
			}
			this._eventsParameters.SetParameter<Vector3>(this._context, this._eventsParameters.CollisionNormal, -collisionInfo.contacts[0].normal);
			this.OnCollisionEvent(this._eventsBlocks.OnEnterBlock, collisionInfo.collider);
		}

		private void OnCollisionExit(Collision collisionInfo)
		{
			if (!this._isAlive)
			{
				return;
			}
			this._eventsParameters.SetParameter<Vector3>(this._context, this._eventsParameters.CollisionNormal, Vector3.zero);
			this.OnCollisionEvent(this._eventsBlocks.OnExitBlock, collisionInfo.collider);
		}

		public static readonly BitLogger Log = new BitLogger(typeof(GadgetBody));

		private IHMMGadgetContext _context;

		private IGadgetBodyMovement _movement;

		private bool _eventTimeBlockAssigned;

		private bool _collisionEventTimeBlockAssigned;

		private bool _movementFinishedBlockAssigned;

		private float _elapsedTime;

		private float _eventInterval;

		private float _nextEventTime;

		private float _collisionEventInterval;

		private float _nextCollisionEventTime;

		private int _collisionTestsToPerform;

		private Transform _prefab;

		private List<MasterVFX> _attachedVFXs = new List<MasterVFX>();

		private Transform _transform;

		private Rigidbody _body;

		private GadgetBody.BodyEventsBlocks _eventsBlocks;

		private GadgetBody.BodyEventsParameters _eventsParameters;

		private List<BaseParameter> _parameters;

		private bool _isAlive;

		private List<ICombatFilter> _filters;

		[Obsolete]
		private BaseFX.EDestroyReason _destroyReason;

		private const float TranslationDistance = 100f;

		private static readonly Vector3 Translation = new Vector3(0f, 100f, 0f);

		[Serializable]
		public class BodyEventsBlocks
		{
			public BaseBlock OnEnterBlock;

			public BaseBlock OnStayBlock;

			public BaseBlock OnExitBlock;

			[FormerlySerializedAs("OnInstantCollisionBlock")]
			[Tooltip("Event called if is colliding the moment this body is created and on Collision Interval")]
			public BaseBlock OnCheckCollisionBlock;

			[Tooltip("Event called every Interval of Time")]
			public BaseBlock OnTimeIntervalBlock;

			[Tooltip("Event called when the Movement is finished")]
			public BaseBlock OnMovementFinishedBlock;
		}

		[Serializable]
		public class BodyEventsParameters
		{
			public void SetParameter<T>(IParameterContext context, IParameter<T> parameter, T value)
			{
				if (parameter != null)
				{
					parameter.SetValue(context, value);
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
			}

			public Vector3Parameter Position;

			public Vector3Parameter TargetPosition;

			public Vector3Parameter Direction;

			public CombatObjectParameter Target;

			public Vector3Parameter CollisionNormal;

			public FloatParameter ElapsedTime;

			public GadgetBodyParameter Body;

			public GadgetBodyParameter TargetBody;
		}
	}
}
