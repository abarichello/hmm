using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Combat.GadgetScript.Body;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.VFX;
using Hoplon.GadgetScript;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/Body/InstantiateVFX")]
	public class InstantiateVFXBlock : BaseBlock
	{
		protected override void InternalInitialize(ref IList<BaseBlock> referencedBlocks, IHMMContext context)
		{
			base.InternalInitialize(ref referencedBlocks, context);
			if (context.IsClient)
			{
				for (int i = 0; i < this._vfxs.Length; i++)
				{
					ResourceLoader.Instance.PrefabPreCache(this._vfxs[i], this._precacheNumber);
				}
			}
		}

		public override IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			IHMMGadgetContext ihmmgadgetContext = (IHMMGadgetContext)gadgetContext;
			IHMMEventContext ihmmeventContext = (IHMMEventContext)eventContext;
			IGadgetBody gadgetBody = this._body.ParameterTomate.GetBoxedValue(gadgetContext) as IGadgetBody;
			if (ihmmgadgetContext.IsServer && !ihmmgadgetContext.IsTest && gadgetBody != null)
			{
				ihmmeventContext.SendToClient();
				return this._nextBlock;
			}
			CombatObject combatObject = (CombatObject)ihmmgadgetContext.Owner;
			if (combatObject == null)
			{
				return this._nextBlock;
			}
			if (gadgetBody == null)
			{
				InstantiateVFXBlock.Log.InfoFormat("TOMATE2 body is null. Block={0} Parameter={1}", new object[]
				{
					base.name,
					this._body.name
				});
				return this._nextBlock;
			}
			Vector3 position = gadgetBody.Transform.position;
			InstantiateVFXBlock.DummyFX dummyFX = gadgetBody.Transform.GetComponent<InstantiateVFXBlock.DummyFX>();
			if (dummyFX == null)
			{
				dummyFX = gadgetBody.Transform.gameObject.AddComponent<InstantiateVFXBlock.DummyFX>();
			}
			dummyFX.Initialize(combatObject.GetGadget((GadgetSlot)ihmmgadgetContext.Id), combatObject, combatObject.Id, position, this._dummyKind);
			for (int i = 0; i < this._vfxs.Length; i++)
			{
				MasterVFX vfxInstance = (MasterVFX)ResourceLoader.Instance.PrefabCacheInstantiate(this._vfxs[i], gadgetBody.Transform.position, gadgetBody.Transform.rotation);
				this.InstantiateVfx(this._vfxs[i], vfxInstance, gadgetBody, position, dummyFX);
			}
			if (this._vfxParameters == null)
			{
				Debug.LogError(this, this);
				return this._nextBlock;
			}
			for (int j = 0; j < this._vfxParameters.Length; j++)
			{
				MasterVFX value = this._vfxParameters[j].GetValue(gadgetContext);
				Component component = ResourceLoader.Instance.PrefabCacheInstantiate(value, gadgetBody.Transform.position, gadgetBody.Transform.rotation);
				if (component is Transform)
				{
					Transform transform = (Transform)component;
					this.InstantiateVfx(value, transform.GetComponent<MasterVFX>(), gadgetBody, position, dummyFX);
				}
				else if (component is MasterVFX)
				{
					this.InstantiateVfx(value, (MasterVFX)component, gadgetBody, position, dummyFX);
				}
				else
				{
					Debug.LogError("unkown type " + component.GetType());
				}
			}
			return this._nextBlock;
		}

		[Conditional("AllowHacks")]
		private void WarnBodyNotAlive(IGadgetBody body)
		{
			if (!body.IsAlive)
			{
				IEnumerable<string> source = from vfx in this._vfxs
				select vfx.name;
				string text = string.Join(", ", source.ToArray<string>());
				InstantiateVFXBlock.Log.WarnFormat("Gadget block '{0}' is trying to instantiate a VFX ({1}) on a body that is not alive ({2}).", new object[]
				{
					base.name,
					text,
					body.Name
				});
			}
		}

		private void InstantiateVfx(MasterVFX vfxPrefab, MasterVFX vfxInstance, IGadgetBody body, Vector3 position, InstantiateVFXBlock.DummyFX targetFX)
		{
			ResourceLoader.Instance.Drawer.AddEffect(vfxInstance.transform);
			vfxInstance.Origin = position;
			vfxInstance.baseMasterVFX = vfxPrefab;
			body.AttachVfx(vfxInstance.Activate(targetFX));
		}

		[Header("Read")]
		[Restrict(true, new Type[]
		{
			typeof(GadgetBody)
		})]
		[SerializeField]
		private BaseParameter _body;

		[SerializeField]
		private CDummy.DummyKind _dummyKind;

		[SerializeField]
		private MasterVFX[] _vfxs;

		[SerializeField]
		private VFXParameter[] _vfxParameters;

		[SerializeField]
		private int _precacheNumber = 1;

		private static readonly BitLogger Log = new BitLogger(typeof(InstantiateVFXBlock));

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
