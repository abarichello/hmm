using System;
using UniRx;

namespace HeavyMetalMachines.Presenting.Navigation
{
	public class PresenterNode : IPresenterNode
	{
		public PresenterNode(string navigationName, IPresenterNode[] children, Func<IPresenter> presenterCreator)
		{
			this.NavigationName = navigationName;
			this.Children = children;
			this._presenterCreator = presenterCreator;
		}

		public string NavigationName { get; private set; }

		public IPresenterNode[] Children { get; private set; }

		public IObservable<Unit> Initialize()
		{
			this._currentPresenter = this._presenterCreator();
			return this._currentPresenter.Initialize();
		}

		public IObservable<Unit> Show()
		{
			return this._currentPresenter.Show();
		}

		public IObservable<Unit> Hide()
		{
			return this._currentPresenter.Hide();
		}

		public IObservable<Unit> Dispose()
		{
			return Observable.Do<Unit>(this._currentPresenter.Dispose(), delegate(Unit _)
			{
				this._currentPresenter = null;
			});
		}

		private readonly Func<IPresenter> _presenterCreator;

		private IPresenter _currentPresenter;
	}
}
