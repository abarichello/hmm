using System;
using Zenject;

namespace HeavyMetalMachines.AI.Steering
{
	public class SimpleSteeringContext : ISteeringContext, IAITask
	{
		[Inject]
		public ISteering Steering { get; private set; }

		public void SetBotContext(ISteeringBotContext botContext)
		{
			this._botContext = botContext;
		}

		public void Update(float deltaTime)
		{
			if (!this._botContext.IsBotControlled)
			{
				return;
			}
			if (this._botContext.DesiredDestination != null)
			{
				this.Steering.SteerToPosition(this._botContext, this._botContext.DesiredDestination.Value);
			}
			else
			{
				this.Steering.StopMoving(this._botContext);
			}
			this.Steering.Update(this._botContext);
		}

		public void Format(ISteeringContextFormater formater)
		{
		}

		private ISteeringBotContext _botContext;
	}
}
