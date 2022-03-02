using System;
using ClientAPI.Service.Interfaces;
using HeavyMetalMachines.Publishing.SessionService;

namespace HeavyMetalMachines.Durango
{
	public class XboxLocalUser : IPublisherLocalUser
	{
		public XboxLocalUser(IUser userApi)
		{
			this._userApi = userApi;
		}

		public bool IsLogged
		{
			get
			{
				bool result;
				try
				{
					this._userApi.GetXboxUserLogged();
					result = true;
				}
				catch (Exception)
				{
					result = false;
				}
				return result;
			}
		}

		public int Id
		{
			get
			{
				return this._userApi.GetXboxUserLogged().Id;
			}
		}

		private IUser _userApi;
	}
}
