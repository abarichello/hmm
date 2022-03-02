using System;
using System.Collections.Generic;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/Time/ScheduleEvent")]
	public class ScheduleEventBlock : BaseBlock
	{
		public override IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			IHMMGadgetContext ihmmgadgetContext = (IHMMGadgetContext)gadgetContext;
			if (ihmmgadgetContext.IsServer)
			{
				float num = (!(null == this._scheduleTimeParameter)) ? this._scheduleTimeParameter.GetValue<float>(gadgetContext) : this._scheduleTime;
				GadgetEvent instance = GadgetEvent.GetInstance(this._onTimePassedBlock.Id, ihmmgadgetContext, this._parameterList);
				instance.CreationTime = (int)(num * 1000f) + ihmmgadgetContext.CurrentTime;
				gadgetContext.ScheduleEvent(instance);
				if (null != this._eventId)
				{
					IParameterTomate<float> parameterTomate = this._eventId.ParameterTomate as IParameterTomate<float>;
					parameterTomate.SetValue(gadgetContext, (float)instance.Id);
				}
			}
			return this._nextBlock;
		}

		[SerializeField]
		private BaseBlock _onTimePassedBlock;

		[Header("Read")]
		[Tooltip("Time to wait before triggering the event in seconds.")]
		[Restrict(false, new Type[]
		{
			typeof(float)
		})]
		[SerializeField]
		private BaseParameter _scheduleTimeParameter;

		[Tooltip("Not used if Parameter is set.")]
		[SerializeField]
		private float _scheduleTime;

		[SerializeField]
		private List<BaseParameter> _parameterList;

		[Header("Write")]
		[Tooltip("Id of the event that will be triggered. Used to cancel the schedule if needed.")]
		[Restrict(false, new Type[]
		{
			typeof(float)
		})]
		[SerializeField]
		private BaseParameter _eventId;
	}
}
