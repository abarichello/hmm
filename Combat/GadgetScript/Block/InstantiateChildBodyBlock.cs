using System;
using System.Collections.Generic;
using HeavyMetalMachines.Combat.GadgetScript.Body;
using HeavyMetalMachines.Infra.Context;
using Hoplon.GadgetScript;
using Plugins.Attributes.ReferenceByString;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	public class InstantiateChildBodyBlock : BaseBlock
	{
		protected override void OnEnable()
		{
			base.OnEnable();
			if (InstantiateChildBodyBlock._finalPositionParameter == null)
			{
				InstantiateChildBodyBlock._finalPositionParameter = ScriptableObject.CreateInstance<Vector3Parameter>();
				InstantiateChildBodyBlock._finalDirectionParameter = ScriptableObject.CreateInstance<Vector3Parameter>();
			}
		}

		protected override void InternalInitialize(ref IList<BaseBlock> referencedBlocks, IHMMContext context)
		{
			base.InternalInitialize(ref referencedBlocks, context);
			ResourceLoader.Instance.PreCachePrefab(this._bodyPrefabName, this._precacheNumber);
		}

		public override IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			IHMMGadgetContext ihmmgadgetContext = (IHMMGadgetContext)gadgetContext;
			IHMMEventContext ihmmeventContext = (IHMMEventContext)eventContext;
			Transform ownerTransform = CreateGadgetBody.GetOwnerTransform(ihmmgadgetContext);
			Transform parentTransform = CreateGadgetBody.GetParentTransform(ihmmgadgetContext, this._parentBody);
			GadgetBodyCreation creation = new GadgetBodyCreation
			{
				HmmEventContext = ihmmeventContext,
				HmmGadgetContext = ihmmgadgetContext,
				BodyParameter = this._bodyParameter,
				FinalPositionParameter = InstantiateChildBodyBlock._finalPositionParameter,
				FinalDirectionParameter = InstantiateChildBodyBlock._finalDirectionParameter,
				DummyKind = this._dummyKind,
				CustomDummyName = this._customDummyName,
				OwnerTransform = ownerTransform,
				DirectionParameter = this._direction,
				UseRelativeDirection = this._useRelativeDirection,
				PositionParameter = this._position,
				UsePositionAsOffset = this._usePositionAsOffset,
				ParentTransform = parentTransform
			};
			PositionDirection positionAndDirection = CreateGadgetBody.GetPositionAndDirection(creation);
			if (ihmmgadgetContext.IsClient && !ihmmeventContext.ConsumeBody())
			{
				this._bodyParameter.SetValue<GadgetBody>(gadgetContext, null);
				return this._nextBlock;
			}
			Transform prefab = CreateGadgetBody.GetPrefab(this._bodyPrefabName);
			Component component;
			if (!CreateGadgetBody.TryInstantiatePrefab(positionAndDirection, prefab, out component))
			{
				InstantiateChildBodyBlock.Log.ErrorFormat("Could not instantiate body. Block={0} Prefab={1}", new object[]
				{
					base.name,
					this._bodyPrefabName
				});
				return this._nextBlock;
			}
			GadgetChildBody component2 = component.GetComponent<GadgetChildBody>();
			if (component2 == null)
			{
				InstantiateChildBodyBlock.Log.ErrorFormat("Gadget child body prefab does not contains a GadgetChildBody component. Block={0} Prefab={1}", new object[]
				{
					base.name,
					this._bodyPrefabName
				});
				return this._nextBlock;
			}
			this._bodyParameter.SetValue<GadgetChildBody>(gadgetContext, component2);
			Transform transform = parentTransform ?? ownerTransform;
			component.transform.SetParent(transform, true);
			component.gameObject.layer = transform.gameObject.layer;
			component2.Initialize(ihmmgadgetContext, ihmmeventContext, prefab, this._eventBlocks);
			CreateGadgetBody.AddBodyToEventContext(ihmmgadgetContext, ihmmeventContext, component2, this._forceSendToClient);
			return this._nextBlock;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(InstantiateChildBodyBlock));

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

		[Tooltip("Name of the Prefab of the Body")]
		[SerializeField]
		[ReferenceByName(typeof(GadgetChildBody))]
		private string _bodyPrefabName;

		[SerializeField]
		private int _precacheNumber = 1;

		[Tooltip("Creates this body on Client even if it has no VFX")]
		[SerializeField]
		private bool _forceSendToClient;

		[Tooltip("Events executed on instantiated bodies.")]
		[SerializeField]
		private GadgetChildBody.ChildBodyEventsBlocks _eventBlocks;

		[Tooltip("Parent from which the instantiated body will be child to. Leave it empty to instantiate as a child of the owner of this gadget. Supported types: GadgetBodyParameter, GadgetChildBodyParameter")]
		[SerializeField]
		private BaseParameter _parentBody;

		[Header("Write")]
		[SerializeField]
		private BaseParameter _bodyParameter;

		private static Vector3Parameter _finalPositionParameter;

		private static Vector3Parameter _finalDirectionParameter;
	}
}
