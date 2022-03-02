using System;
using HeavyMetalMachines.Options.Presenting;
using UniRx;
using UnityEngine;

namespace HeavyMetalMachines.Options
{
	public class FakeOptionsPresenter : IOptionsPresenter
	{
		public FakeOptionsPresenter()
		{
			this._visibilityChangedSubject = new Subject<bool>();
		}

		public bool Visible { get; private set; }

		public IObservable<bool> VisibilityChanged()
		{
			return this._visibilityChangedSubject;
		}

		public void Show()
		{
			if (!this.Visible)
			{
				this._visibilityChangedSubject.OnNext(true);
			}
			this.Visible = true;
			Debug.LogWarning("FakeOptionsPresenter.Show");
		}

		public void SetCharacterSelectionActive()
		{
			Debug.LogWarning("FakeOptionsPresenter.SetCharacterSelectionActive");
		}

		public void SetCharacterSelectionInactive()
		{
			Debug.LogWarning("FakeOptionsPresenter.SetCharacterSelectionInactive");
		}

		private readonly Subject<bool> _visibilityChangedSubject;
	}
}
