using System;
using System.Collections.Generic;
using HeavyMetalMachines.Combat.GadgetScript.Body;
using HeavyMetalMachines.Combat.GadgetScript.Body.Filter;
using Hoplon.GadgetScript;
using Pocketverse;
using SharedUtils.Loading;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/Body/InstantiateBody")]
	public class InstantiateBodyBlock : BaseBlock, IGadgetBlockWithAsset
	{
		protected override void OnEnable()
		{
			base.OnEnable();
			if (InstantiateBodyBlock._finalPosition == null)
			{
				InstantiateBodyBlock._finalPosition = ScriptableObject.CreateInstance<Vector3Parameter>();
				InstantiateBodyBlock._finalDirection = ScriptableObject.CreateInstance<Vector3Parameter>();
			}
		}

		public void PrecacheAssets()
		{
			ResourceLoader.Instance.PreCachePrefab(this._bodyPrefabName, 1);
			if (this._collisionFilter == null)
			{
				return;
			}
			for (int i = 0; i < this._collisionFilter.Length; i++)
			{
				ICombatFilter combatFilter = (ICombatFilter)ScriptableObject.CreateInstance(this._collisionFilter[i].ClassName);
				JsonUtility.FromJsonOverwrite(this._collisionFilter[i].SerializedObject, combatFilter);
				this._combatFilter.Add(combatFilter);
			}
		}

		protected override bool CheckSanity(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			IHMMGadgetContext ihmmgadgetContext = (IHMMGadgetContext)gadgetContext;
			IHMMEventContext ihmmeventContext = (IHMMEventContext)eventContext;
			if (ihmmgadgetContext.IsClient && !ihmmeventContext.ShouldCreateBody)
			{
				return true;
			}
			if (string.IsNullOrEmpty(this._bodyPrefabName))
			{
				base.LogSanitycheckError("'Body Prefab Name' parameter cannot be null.");
				return false;
			}
			if (this._body == null)
			{
				base.LogSanitycheckError("'Body' parameter cannot be null.");
				return false;
			}
			return true;
		}

		protected override IBlock InnerExecute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			IHMMGadgetContext ihmmgadgetContext = (IHMMGadgetContext)gadgetContext;
			IHMMEventContext ihmmeventContext = (IHMMEventContext)eventContext;
			Vector3 value;
			Vector3 value2;
			if (ihmmgadgetContext.IsClient)
			{
				ihmmeventContext.LoadParameter(InstantiateBodyBlock._finalPosition);
				ihmmeventContext.LoadParameter(InstantiateBodyBlock._finalDirection);
				if (this._position != null)
				{
					ihmmeventContext.LoadParameter(this._position);
				}
				if (this._direction != null)
				{
					ihmmeventContext.LoadParameter(this._direction);
				}
				if (this._timeInterval != null)
				{
					ihmmeventContext.LoadParameter(this._timeInterval);
				}
				if (this._collisionCheckTimeInterval != null)
				{
					ihmmeventContext.LoadParameter(this._collisionCheckTimeInterval);
				}
				if (this._collisionCheckCount != null)
				{
					ihmmeventContext.LoadParameter(this._collisionCheckCount);
				}
				bool shouldCreateBody = ihmmeventContext.ShouldCreateBody;
				ihmmeventContext.TryConsumeFirstBody();
				if (!shouldCreateBody)
				{
					return this._nextBlock;
				}
				value = InstantiateBodyBlock._finalPosition.GetValue(gadgetContext);
				value2 = InstantiateBodyBlock._finalDirection.GetValue(gadgetContext);
			}
			else
			{
				Transform transform = ((Identifiable)ihmmgadgetContext.GetIdentifiable(ihmmgadgetContext.OwnerId)).transform;
				Transform dummy = ihmmgadgetContext.Owner.Dummy.GetDummy(this._dummyKind, this._customDummyName);
				this.GetPositionAndDirection(transform, dummy, gadgetContext, out value, out value2);
				value.y = 0f;
				value2.y = 0f;
				InstantiateBodyBlock._finalPosition.SetValue(gadgetContext, value);
				InstantiateBodyBlock._finalDirection.SetValue(gadgetContext, value2);
				ihmmeventContext.SaveParameter(InstantiateBodyBlock._finalPosition);
				ihmmeventContext.SaveParameter(InstantiateBodyBlock._finalDirection);
				if (this._position != null)
				{
					ihmmeventContext.SaveParameter(this._position);
				}
				if (this._direction != null)
				{
					ihmmeventContext.SaveParameter(this._direction);
				}
				if (this._timeInterval != null)
				{
					ihmmeventContext.SaveParameter(this._timeInterval);
				}
				if (this._collisionCheckTimeInterval != null)
				{
					ihmmeventContext.SaveParameter(this._collisionCheckTimeInterval);
				}
				if (this._collisionCheckCount != null)
				{
					ihmmeventContext.SaveParameter(this._collisionCheckCount);
				}
			}
			Transform prefab = (Transform)LoadingManager.ResourceContent.GetAsset(this._bodyPrefabName).Asset;
			float eventTime = (!(this._timeInterval != null)) ? -1f : this._timeInterval.GetValue(gadgetContext);
			float collisionEventTime = (!(this._collisionCheckTimeInterval != null)) ? 0f : this._collisionCheckTimeInterval.GetValue(gadgetContext);
			int collisionTestsToPerform = (!(this._collisionCheckCount != null)) ? 0 : this._collisionCheckCount.GetValue(gadgetContext);
			Quaternion rotation = (!(value2 == Vector3.zero)) ? Quaternion.LookRotation(value2, Vector3.up) : Quaternion.identity;
			Component component = ResourceLoader.Instance.PrefabCacheInstantiate(prefab, value, rotation);
			if (component == null)
			{
				InstantiateBodyBlock.Log.ErrorFormat("Could not get body GameObject from Cache. {0}", new object[]
				{
					this
				});
			}
			GadgetBody component2 = component.GetComponent<GadgetBody>();
			if (component2 == null)
			{
				InstantiateBodyBlock.Log.ErrorFormat("Could not get GadgetBody from Cache. {0}", new object[]
				{
					this
				});
			}
			this._body.SetValue(gadgetContext, component2);
			component2.Initialize(ihmmgadgetContext, this._bodyEvents, this._bodyParameters, eventTime, collisionEventTime, collisionTestsToPerform, this._combatFilter, eventContext, prefab);
			component2.transform.parent = ResourceLoader.Instance.Drawer.Effects;
			ihmmgadgetContext.Bodies.Add(component2.Id, component2);
			if (ihmmgadgetContext.IsServer)
			{
				ihmmeventContext.AddBody(component2.Id);
			}
			Identifiable component3 = component2.GetComponent<Identifiable>();
			bool flag = component3 != null;
			if (flag)
			{
				component3.Register(component2.Id);
			}
			if (this._forceSendToClient || flag)
			{
				ihmmeventContext.SendToClient();
			}
			return this._nextBlock;
		}

		public override bool UsesParameterWithId(int parameterId)
		{
			return base.CheckIsParameterWithId(this._position, parameterId) || base.CheckIsParameterWithId(this._direction, parameterId) || base.CheckIsParameterWithId(this._timeInterval, parameterId) || base.CheckIsParameterWithId(this._collisionCheckTimeInterval, parameterId) || base.CheckIsParameterWithId(this._collisionCheckCount, parameterId) || base.CheckIsParameterWithId(this._body, parameterId);
		}

		private void GetPositionAndDirection(Transform ownerTransform, Transform originTransform, IParameterContext context, out Vector3 position, out Vector3 direction)
		{
			direction = Vector3.zero;
			if (this._direction != null)
			{
				direction = this._direction.GetValue(context);
				if (this._useRelativeDirection)
				{
					direction = ownerTransform.TransformDirection(direction);
				}
			}
			if (this._position != null)
			{
				position = this._position.GetValue(context);
				if (this._usePositionAsOffset)
				{
					position += originTransform.position;
				}
				if (direction != Vector3.zero)
				{
					direction = (position - ownerTransform.position).normalized;
				}
			}
			else
			{
				position = originTransform.position;
			}
		}

		private static readonly BitLogger Log = new BitLogger(typeof(InstantiateBodyBlock));

		[SerializeField]
		private GadgetBody.BodyEventsBlocks _bodyEvents;

		[Header("Read")]
		[SerializeField]
		[Tooltip("Position where the Body will be created")]
		private Vector3Parameter _position;

		[SerializeField]
		[Tooltip("If marked, the Position will be added to the position of the Dummy")]
		private bool _usePositionAsOffset;

		[SerializeField]
		[Tooltip("Point where the Body will be created relative to the Owner. If not found, the center of the Owner is selected.")]
		private CDummy.DummyKind _dummyKind;

		[SerializeField]
		[Tooltip("Name of the Custom Dummy, if used.")]
		private string _customDummyName;

		[SerializeField]
		[Tooltip("Direction of the Body")]
		private Vector3Parameter _direction;

		[SerializeField]
		[Tooltip("If marked, the direction will be transformed to the local rotation of the owner")]
		private bool _useRelativeDirection;

		[SerializeField]
		[Tooltip("Interval of time between executions of OnTimeIntervalBlock")]
		private FloatParameter _timeInterval;

		[SerializeField]
		[Tooltip("The interval between collision checks (the first will be performed instantly.")]
		private FloatParameter _collisionCheckTimeInterval;

		[SerializeField]
		[Tooltip("How many collision checks must be performed, including the one performed instantly. If zero or empty, only the instant check will be performed.")]
		private IntParameter _collisionCheckCount;

		[SerializeField]
		[Tooltip("Filter of what is EXCLUDED from the collision. If any one of these filters is true, the collision event is not triggered.")]
		private BaseCombatFilter[] _collisionFilter;

		[SerializeField]
		[Tooltip("Name of the Prefab of the Body")]
		private string _bodyPrefabName;

		[SerializeField]
		[Tooltip("Creates this body on Client even if it has no VFX")]
		private bool _forceSendToClient;

		[Header("Write")]
		[SerializeField]
		private GadgetBodyParameter _body;

		[SerializeField]
		private GadgetBody.BodyEventsParameters _bodyParameters;

		private List<ICombatFilter> _combatFilter = new List<ICombatFilter>();

		protected static Vector3Parameter _finalPosition;

		protected static Vector3Parameter _finalDirection;
	}
}
