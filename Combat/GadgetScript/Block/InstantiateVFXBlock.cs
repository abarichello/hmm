using System;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Combat.GadgetScript.Body;
using HeavyMetalMachines.VFX;
using Hoplon.GadgetScript;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/Body/InstantiateVFX")]
	public class InstantiateVFXBlock : BaseBlock, IGadgetBlockWithAsset
	{
		public void PrecacheAssets()
		{
			for (int i = 0; i < this._vfxs.Length; i++)
			{
				ResourceLoader.Instance.PrefabPreCache(this._vfxs[i], 1);
			}
		}

		protected override bool CheckSanity(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			if (((IHMMGadgetContext)gadgetContext).IsServer)
			{
				return true;
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
			if (ihmmgadgetContext.IsServer)
			{
				ihmmeventContext.SaveParameter(this._body);
				ihmmeventContext.SendToClient();
				return this._nextBlock;
			}
			ihmmeventContext.LoadParameter(this._body);
			CombatObject combatObject = (CombatObject)ihmmgadgetContext.GetCombatObject(ihmmgadgetContext.OwnerId);
			if (combatObject == null)
			{
				return this._nextBlock;
			}
			GadgetBody value = this._body.GetValue(gadgetContext);
			Vector3 position = value.transform.position;
			InstantiateVFXBlock.DummyFX dummyFX = value.GetComponent<InstantiateVFXBlock.DummyFX>();
			if (dummyFX == null)
			{
				dummyFX = value.gameObject.AddComponent<InstantiateVFXBlock.DummyFX>();
			}
			dummyFX.Initialize(combatObject.GetGadget((GadgetSlot)ihmmgadgetContext.Id), combatObject, combatObject.Id, position, this._dummyKind);
			for (int i = 0; i < this._vfxs.Length; i++)
			{
				MasterVFX masterVFX = (MasterVFX)ResourceLoader.Instance.PrefabCacheInstantiate(this._vfxs[i], value.transform.position, value.transform.rotation);
				masterVFX.transform.parent = value.transform.parent;
				masterVFX.Origin = position;
				masterVFX.baseMasterVFX = this._vfxs[i];
				value.AttachVFX(masterVFX.Activate(dummyFX));
			}
			return this._nextBlock;
		}

		public override bool UsesParameterWithId(int parameterId)
		{
			return base.CheckIsParameterWithId(this._body, parameterId);
		}

		[Header("Read")]
		[SerializeField]
		private GadgetBodyParameter _body;

		[SerializeField]
		private CDummy.DummyKind _dummyKind;

		[SerializeField]
		private MasterVFX[] _vfxs;

		private class DummyFX : AbstractFX
		{
			public override int EventId { get; set; }

			public override Identifiable Target
			{
				get
				{
					return this._target;
				}
			}

			public override Identifiable Owner
			{
				get
				{
					return this._owner.Id;
				}
			}

			public override Vector3 TargetPosition
			{
				get
				{
					return this._position;
				}
			}

			public override byte CustomVar
			{
				get
				{
					return 0;
				}
			}

			public override CDummy.DummyKind GetDummyKind()
			{
				return this._dummyKind;
			}

			public override Transform GetDummy(CDummy.DummyKind kind)
			{
				return this._owner.GetDummy(this._dummyKind, string.Empty);
			}

			public override GadgetBehaviour GetGadget()
			{
				return this._gadget;
			}

			public override bool WasCreatedInFog()
			{
				return false;
			}

			public void Initialize(GadgetBehaviour gadget, CombatObject owner, Identifiable target, Vector3 position, CDummy.DummyKind dummyKind)
			{
				this._owner = owner;
				this._gadget = gadget;
				this._dummyKind = dummyKind;
				this._target = target;
				this._position = position;
			}

			private CombatObject _owner;

			private GadgetBehaviour _gadget;

			private CDummy.DummyKind _dummyKind;

			private Identifiable _target;

			private Vector3 _position;
		}
	}
}
