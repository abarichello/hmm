using System;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/Physics/Raycast")]
	public class RaycastBlock : BaseBlock
	{
		protected override bool CheckSanity(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			if (((IHMMGadgetContext)gadgetContext).IsClient)
			{
				return true;
			}
			if (this._origin == null)
			{
				base.LogSanitycheckError("'Origin' parameter cannot be null.");
				return false;
			}
			if (this._end == null)
			{
				base.LogSanitycheckError("'End' parameter cannot be null.");
				return false;
			}
			return true;
		}

		protected override IBlock InnerExecute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			if (((IHMMGadgetContext)gadgetContext).IsServer)
			{
				Vector3 value = this._origin.GetValue(gadgetContext);
				Vector3 value2 = this._end.GetValue(gadgetContext);
				float magnitude = (value2 - value).magnitude;
				RaycastBlock._isHit.SetValue(gadgetContext, Physics.Raycast(value, (value2 - value) / magnitude, magnitude, (int)this._layer));
				((IHMMEventContext)eventContext).SaveParameter(RaycastBlock._isHit);
			}
			else
			{
				((IHMMEventContext)eventContext).LoadParameter(RaycastBlock._isHit);
			}
			if (RaycastBlock._isHit.GetValue(gadgetContext))
			{
				return this._nextBlock;
			}
			return this._nothingBetweenPointsBlock;
		}

		public override bool UsesParameterWithId(int parameterId)
		{
			return base.CheckIsParameterWithId(this._origin, parameterId) || base.CheckIsParameterWithId(this._end, parameterId);
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			if (RaycastBlock._isHit == null)
			{
				RaycastBlock._isHit = ScriptableObject.CreateInstance<BoolParameter>();
			}
		}

		[SerializeField]
		private BaseBlock _nothingBetweenPointsBlock;

		[Header("Read")]
		[SerializeField]
		private Vector3Parameter _origin;

		[SerializeField]
		private Vector3Parameter _end;

		[SerializeField]
		private LayerManager.Mask _layer;

		private static BoolParameter _isHit;
	}
}
