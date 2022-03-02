using System;
using HeavyMetalMachines.Crossplay;
using HeavyMetalMachines.HostingPlatforms;

namespace HeavyMetalMachines.Spectator
{
	public class GetSpectatorQueueFilter : IGetSpectatorQueueFilter
	{
		public GetSpectatorQueueFilter(SpectatorQueueFilterTable table, IGetHostPlatform getHostPlatform, IIsCrossplayEnabled isCrossplayEnabled)
		{
			this._table = table;
			this._getHostPlatform = getHostPlatform;
			this._isCrossplayEnabled = isCrossplayEnabled;
		}

		public CrossplayPlatformQueueSettings Get()
		{
			CrossplayPlatformQueueKey key = new CrossplayPlatformQueueKey(this._getHostPlatform.GetCurrent(), this._isCrossplayEnabled.Get());
			return this._table.Registry[key];
		}

		private readonly SpectatorQueueFilterTable _table;

		private readonly IGetHostPlatform _getHostPlatform;

		private readonly IIsCrossplayEnabled _isCrossplayEnabled;
	}
}
