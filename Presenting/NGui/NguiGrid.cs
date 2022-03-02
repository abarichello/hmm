using System;
using UniRx;
using UnityEngine;

namespace HeavyMetalMachines.Presenting.NGui
{
	[Serializable]
	public class NguiGrid : IGrid
	{
		public void Reposition()
		{
			ObservableExtensions.Subscribe<Unit>(Observable.DoOnCompleted<Unit>(Observable.NextFrame(0), new Action(this._grid.Reposition)));
		}

		[SerializeField]
		private UIGrid _grid;
	}
}
