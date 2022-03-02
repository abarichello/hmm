using System;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Body
{
	public class GadgetBodyRelativeStraightMovement : GadgetBodyStraightMovement
	{
		public override void Initialize(IGadgetBody body, IGadgetContext gadgetContext, IEventContext eventContext)
		{
			base.Initialize(body, gadgetContext, eventContext);
			this._context = (IHMMGadgetContext)gadgetContext;
			IParameterTomate<float> parameterTomate = GadgetBodyStraightMovement._rangeParameter.ParameterTomate as IParameterTomate<float>;
			this._range = parameterTomate.GetValue(gadgetContext);
			this._isInitialized = true;
		}

		private void LateUpdate()
		{
			if (this._isInitialized && this._context.IsClient)
			{
				base.transform.rotation = Quaternion.LookRotation(this._direction);
				Debug.DrawLine(base.transform.position, base.transform.position + this._direction * 10f, Color.red, 10f);
			}
		}

		public override Vector3 GetPosition(float elapsedTime)
		{
			this._direction = this._relativeDirection.GetValue<Vector3>(this._context);
			this._origin = this._relativeOrigin.GetValue<Vector3>(this._context);
			this._end = this._origin + this._direction * this._range;
			return base.GetPosition(elapsedTime);
		}

		public override void Destroy()
		{
			this._isInitialized = false;
			this.Finished = false;
		}

		[SerializeField]
		protected BaseParameter _relativeOrigin;

		[SerializeField]
		protected BaseParameter _relativeDirection;

		private IHMMGadgetContext _context;

		private float _range;

		private bool _isInitialized;
	}
}
