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

		public override IBlock Execute(IGadgetContext context, IEventContext eventContext)
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
				IParameterTomate<float> parameterTomate = this._currentCooldownTime.ParameterTomate as IParameterTomate<float>;
				int num = (int)parameterTomate.GetValue(context);
				flag = (num > creationTime);
				CheckCooldownBlock._resultParameter.SetValue(context, flag);
				ihmmeventContext.SaveParameter(CheckCooldownBlock._resultParameter);
			}
			if (flag)
			{
				return this._inCooldownNextBlock;
			}
			return this._nextBlock;
		}

		[Header("Read")]
		[SerializeField]
		private BaseBlock _inCooldownNextBlock;

		[SerializeField]
		private BaseParameter _currentCooldownTime;

		private static BoolParameter _resultParameter;
	}
}
