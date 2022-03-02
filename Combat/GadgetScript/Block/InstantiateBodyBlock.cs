using System;
using System.Collections.Generic;
using HeavyMetalMachines.Combat.GadgetScript.Body;
using HeavyMetalMachines.Combat.GadgetScript.Body.Filter;
using HeavyMetalMachines.Infra.Context;
using Hoplon.GadgetScript;
using Plugins.Attributes.ReferenceByString;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/Body/InstantiateBody")]
	public class InstantiateBodyBlock : BaseBlock
	{
		private static LayerMask GetDefaultNonCombatCollisionLayers()
		{
			return 20972032;
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			if (InstantiateBodyBlock._finalPosition == null)
			{
				InstantiateBodyBlock._finalPosition = ScriptableObject.CreateInstance<Vector3Parameter>();
				InstantiateBodyBlock._finalDirection = ScriptableObject.CreateInstance<Vector3Parameter>();
			}
		}

		protected override void InternalInitialize(ref IList<BaseBlock> referencedBlocks, IHMMContext context)
		{
			base.InternalInitialize(ref referencedBlocks, context);
			ResourceLoader.Instance.PreCachePrefab(this._bodyPrefabName, this._precacheNumber);
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
			referencedBlocks.Add(this._bodyEvents.OnCheckCollisionBlock);
			referencedBlocks.Add(this._bodyEvents.OnDisplacementIntervalBlock);
			referencedBlocks.Add(this._bodyEvents.OnMovementFinishedBlock);
			referencedBlocks.Add(this._bodyEvents.OnTimeIntervalBlock);
			referencedBlocks.Add(this._bodyEvents.OnEnterBlock);
			referencedBlocks.Add(this._bodyEvents.OnExitBlock);
			referencedBlocks.Add(this._bodyEvents.OnStayBlock);
		}

		public override IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			IHMMGadgetContext ihmmgadgetContext = (IHMMGadgetContext)gadgetContext;
			IHMMEventContext ihmmeventContext = (IHMMEventContext)eventContext;
			Transform ownerTransform = CreateGadgetBody.GetOwnerTransform(ihmmgadgetContext);
			GadgetBodyCreation creation = new GadgetBodyCreation
			{
				HmmEventContext = ihmmeventContext,
				HmmGadgetContext = ihmmgadgetContext,
				BodyParameter = this._body,
				FinalPositionParameter = InstantiateBodyBlock._finalPosition,
				FinalDirectionParameter = InstantiateBodyBlock._finalDirection,
				DummyKind = this._dummyKind,
				CustomDummyName = this._customDummyName,
				OwnerTransform = ownerTransform,
				DirectionParameter = this._direction,
				UseRelativeDirection = this._useRelativeDirection,
				PositionParameter = this._position,
				UsePositionAsOffset = this._usePositionAsOffset
			};
			PositionDirection positionAndDirection = CreateGadgetBody.GetPositionAndDirection(creation);
			if (ihmmgadgetContext.IsClient)
			{
				ihmmeventContext.LoadParameterIfExisting(this._position);
				ihmmeventContext.LoadParameterIfExisting(this._direction);
				ihmmeventContext.LoadParameterIfExisting(this._timeInterval);
				ihmmeventContext.LoadParameterIfExisting(this._displacementInterval);
				ihmmeventContext.LoadParameterIfExisting(this._collisionCheckTimeInterval);
				ihmmeventContext.LoadParameterIfExisting(this._collisionCheckCount);
				if (!ihmmeventContext.ConsumeBody())
				{
					this._body.SetValue<GadgetBody>(gadgetContext, null);
					return this._nextBlock;
				}
			}
			else
			{
				ihmmeventContext.SaveParameterIfExisting(this._position);
				ihmmeventContext.SaveParameterIfExisting(this._direction);
				ihmmeventContext.SaveParameterIfExisting(this._timeInterval);
				ihmmeventContext.SaveParameterIfExisting(this._displacementInterval);
				ihmmeventContext.SaveParameterIfExisting(this._collisionCheckTimeInterval);
				ihmmeventContext.SaveParameterIfExisting(this._collisionCheckCount);
			}
			float floatParameter = InstantiateBodyBlock.GetFloatParameter(gadgetContext, this._timeInterval, -1f);
			float floatParameter2 = InstantiateBodyBlock.GetFloatParameter(gadgetContext, this._displacementInterval, -1f);
			float floatParameter3 = InstantiateBodyBlock.GetFloatParameter(gadgetContext, this._collisionCheckTimeInterval, 0f);
			float floatParameter4 = InstantiateBodyBlock.GetFloatParameter(gadgetContext, this._collisionCheckCount, 0f);
			Transform prefab = CreateGadgetBody.GetPrefab(this._bodyPrefabName);
			GadgetBody.InitializationParameters parameters = new GadgetBody.InitializationParameters
			{
				GadgetContext = ihmmgadgetContext,
				EventContext = ihmmeventContext,
				EventBlocks = this._bodyEvents,
				EventParameters = this._bodyParameters,
				TimedEventInterval = floatParameter,
				CollisionCheckTimeInterval = floatParameter3,
				CollisionCheckCount = (int)floatParameter4,
				DisplacementEventInterval = floatParameter2,
				Filters = this._combatFilter,
				HitOverBarrier = this._hitOverBarrier,
				Prefab = prefab,
				NonCombatCollisionLayers = this._nonCombatCollisionLayers
			};
			Component component;
			if (!CreateGadgetBody.TryInstantiatePrefab(positionAndDirection, prefab, out component))
			{
				InstantiateBodyBlock.Log.ErrorFormat("Could not instantiate body. Block={0} Prefab={1}", new object[]
				{
					base.name,
					this._bodyPrefabName
				});
				return this._nextBlock;
			}
			GadgetBody component2 = component.GetComponent<GadgetBody>();
			if (component2 == null)
			{
				InstantiateBodyBlock.Log.ErrorFormat("Gadget body prefab does not contains a GadgetBody component. Block={0} Prefab={1}", new object[]
				{
					base.name,
					this._bodyPrefabName
				});
				return this._nextBlock;
			}
			this._body.SetValue<GadgetBody>(gadgetContext, component2);
			ResourceLoader.Instance.Drawer.AddEffect(component2.transform);
			component2.Initialize(parameters);
			CreateGadgetBody.AddBodyToEventContext(ihmmgadgetContext, ihmmeventContext, component2, this._forceSendToClient);
			return this._nextBlock;
		}

		private static float GetFloatParameter(IGadgetContext gadgetContext, BaseParameter parameter, float fallbackValue)
		{
			if (null != parameter)
			{
				IParameterTomate<float> parameterTomate = parameter.ParameterTomate as IParameterTomate<float>;
				return parameterTomate.GetValue(gadgetContext);
			}
			return fallbackValue;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(InstantiateBodyBlock));

		[SerializeField]
		private GadgetBody.BodyEventsBlocks _bodyEvents;

		[Header("Read")]
		[Tooltip("Position where the Body will be created")]
		[Restrict(false, new Type[]
		{
			typeof(Vector3)
		})]
		[SerializeField]
		private BaseParameter _position;

		[Tooltip("If marked, the Position will be added to the position of the Dummy")]
		[SerializeField]
		private bool _usePositionAsOffset;

		[Tooltip("Point where the Body will be created relative to the Owner. If not found, the center of the Owner is selected.")]
		[SerializeField]
		private CDummy.DummyKind _dummyKind;

		[Tooltip("Name of the Custom Dummy, if used.")]
		[SerializeField]
		private string _customDummyName;

		[Tooltip("Direction of the Body")]
		[Restrict(false, new Type[]
		{
			typeof(Vector3)
		})]
		[SerializeField]
		private BaseParameter _direction;

		[Tooltip("If marked, the direction will be transformed to the local rotation of the owner")]
		[SerializeField]
		private bool _useRelativeDirection;

		[Tooltip("Interval of time between executions of OnTimeIntervalBlock")]
		[Restrict(false, new Type[]
		{
			typeof(float)
		})]
		[SerializeField]
		private BaseParameter _timeInterval;

		[Tooltip("Interval of time between executions of OnDisplacementIntervalBlock")]
		[Restrict(false, new Type[]
		{
			typeof(float)
		})]
		[SerializeField]
		private BaseParameter _displacementInterval;

		[Tooltip("The interval between collision checks (the first will be performed instantly.")]
		[Restrict(false, new Type[]
		{
			typeof(float)
		})]
		[SerializeField]
		private BaseParameter _collisionCheckTimeInterval;

		[Tooltip("How many collision checks must be performed, including the one performed instantly. If zero or empty, only the instant check will be performed.")]
		[Restrict(false, new Type[]
		{
			typeof(float)
		})]
		[SerializeField]
		private BaseParameter _collisionCheckCount;

		[Tooltip("Filter of what is EXCLUDED from the collision. If any one of these filters is true, the collision event is not triggered.")]
		[Obsolete("Obsolete! Use FilterBlock")]
		[SerializeField]
		private BaseCombatFilter[] _collisionFilter;

		[Tooltip("When colliding with a Combat and a Barrier at same time, ignores the Barrier")]
		[SerializeField]
		private bool _hitOverBarrier;

		[Tooltip("Name of the Prefab of the Body")]
		[SerializeField]
		[ReferenceByName(typeof(GameObject))]
		[Restrict(true, new Type[]
		{

		})]
		private string _bodyPrefabName;

		[SerializeField]
		private int _precacheNumber = 1;

		[Tooltip("Creates this body on Client even if it has no VFX")]
		[SerializeField]
		private bool _forceSendToClient;

		[Tooltip("Specifices the layers of the non-combat bodies (e.g. the scenery) which should trigger collision events with the created body.")]
		[SerializeField]
		private LayerMask _nonCombatCollisionLayers = InstantiateBodyBlock.GetDefaultNonCombatCollisionLayers();

		[Header("Write")]
		[Restrict(true, new Type[]
		{
			typeof(IGadgetBody)
		})]
		[SerializeField]
		private BaseParameter _body;

		[SerializeField]
		private GadgetBody.BodyEventsParameters _bodyParameters;

		private List<ICombatFilter> _combatFilter = new List<ICombatFilter>();

		protected static Vector3Parameter _finalPosition;

		protected static Vector3Parameter _finalDirection;
	}
}
