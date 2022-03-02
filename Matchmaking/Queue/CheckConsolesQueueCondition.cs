using System;
using System.Linq;
using HeavyMetalMachines.Crossplay;
using HeavyMetalMachines.Crossplay.DataTransferObjects;
using HeavyMetalMachines.Social.Groups.Business;
using Hoplon.Logging;

namespace HeavyMetalMachines.Matchmaking.Queue
{
	public class CheckConsolesQueueCondition : ICheckConsolesQueueCondition
	{
		public CheckConsolesQueueCondition(IIsCrossplayEnabled isCrossplayEnabled, IGroupStorage groupeStorage, ILogger<ICheckConsolesQueueCondition> logger, IGroupCrossplayStorage groupCrossplayStorage)
		{
			this._isCrossplayEnabled = isCrossplayEnabled;
			this._groupeStorage = groupeStorage;
			this._logger = logger;
			this._groupCrossplayStorage = groupCrossplayStorage;
		}

		public bool Check()
		{
			if (this._groupeStorage.Group == null)
			{
				return this.IsConsolesPlayerWithCrossplayDisable();
			}
			return this.GroupHasSomeoneWhithCrossplayDisabled();
		}

		private bool GroupHasSomeoneWhithCrossplayDisabled()
		{
			bool flag = this._groupCrossplayStorage.GetMembers().Any((PlayerCrossplayData member) => !member.IsEnabled);
			this._logger.DebugFormat("Check for consoles queue condition in group result: {0}", new object[]
			{
				flag
			});
			return flag;
		}

		private bool IsConsolesPlayerWithCrossplayDisable()
		{
			bool flag = this._isCrossplayEnabled.Get();
			bool flag2 = !flag && Platform.Current.IsConsole();
			this._logger.DebugFormat("Check for consoles queue condition playerCrossplay: {0}, result: {1}", new object[]
			{
				flag,
				flag2
			});
			return flag2;
		}

		private readonly IIsCrossplayEnabled _isCrossplayEnabled;

		private readonly IGroupStorage _groupeStorage;

		private readonly ILogger<ICheckConsolesQueueCondition> _logger;

		private readonly IGroupCrossplayStorage _groupCrossplayStorage;
	}
}
