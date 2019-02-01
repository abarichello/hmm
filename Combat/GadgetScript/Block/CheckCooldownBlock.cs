using System;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/Cooldown/CheckCooldown")]
	public class CheckCooldownBlock : BaseBlock
	{
		protected override void OnEnable()
		{
			base.OnEnable();
			if (CheckCooldownBlock._resultParameter == null)
			{
				CheckCooldownBlock._resultParameter = ScriptableObject.CreateInstance<BoolParameter>();
			}
		}

		protected override bool CheckSanity(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			IHMMGadgetContext ihmmgadgetContext = (IHMMGadgetContext)gadgetContext;
			if (ihmmgadgetContext.IsClient)
			{
				return true;
			}
			if (this._currentCooldownTime == null)
			{
				base.LogSanitycheckError("'Current Cooldown Time' parameter cannot be null.");
				return false;
			}
			return true;
		}

		protected override IBlock InnerExecute(IGadgetContext context, IEventContext eventContext)
		{
			IHMMGadgetContext ihmmgadgetContext = (IHMMGadgetContext)context;
			IHMMEventContext ihmmeventContext = (IHMMEventContext)eventContext;
			bool flag;
			if (ihmmgadgetContext.IsClient)
			{
				ihmmeventContext.LoadParameter(CheckCooldownBlock._resultParameter);
				flag = CheckCooldownBlock._resultParameter.GetValue(context);
			}
			else
			{
				int creationTime = eventContext.CreationTime;
				int value = this._currentCooldownTime.GetValue(context);
				flag = (value > creationTime);
				CheckCooldownBlock._resultParameter.SetValue(context, flag);
				ihmmeventContext.SaveParameter(CheckCooldownBlock._resultParameter);
			}
			if (flag)
			{
				return this._inCooldownNextBlock;
			}
			return this._nextBlock;
		}

		public override bool UsesParameterWithId(int parameterId)
		{
			return base.CheckIsParameterWithId(this._currentCooldownTime, parameterId);
		}

		[SerializeField]
		private BaseBlock _inCooldownNextBlock;

		[SerializeField]
		private IntParameter _currentCooldownTime;

		private static BoolParameter _resultParameter;
	}
}
