using System;

namespace HeavyMetalMachines.Social.Groups.Business
{
	public class GroupBusinessFactory : IGroupBusinessFactory
	{
		public GroupBusinessFactory(IGroupStorage groupStorage)
		{
			this._groupStorage = groupStorage;
		}

		public IObserveGroup CreateObserveGroup()
		{
			return new ObserveGroup(this._groupStorage);
		}

		private readonly IGroupStorage _groupStorage;
	}
}
