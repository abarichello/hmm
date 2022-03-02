using System;
using UniRx;

namespace HeavyMetalMachines.Players.Business
{
	public class LocalPlayerTotalLevelStorage : ILocalPlayerTotalLevelStorage
	{
		public LocalPlayerTotalLevelStorage(UserInfo userInfo)
		{
			this._userInfo = userInfo;
			this._onLevelUpdatedSubject = new Subject<int>();
		}

		public int Get()
		{
			return this._userInfo.GetTotalPlayerLevel();
		}

		public void RefreshTotalLevel()
		{
			int num = this.Get();
			this._onLevelUpdatedSubject.OnNext(num);
		}

		public IObservable<int> OnLevelUpdated
		{
			get
			{
				return this._onLevelUpdatedSubject;
			}
		}

		private readonly UserInfo _userInfo;

		private readonly Subject<int> _onLevelUpdatedSubject;
	}
}
