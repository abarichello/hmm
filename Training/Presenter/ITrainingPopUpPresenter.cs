using System;
using UniRx;

namespace HeavyMetalMachines.Training.Presenter
{
	public interface ITrainingPopUpPresenter
	{
		IObservable<Unit> Initialize();

		IObservable<Unit> ShowAndWaitForConclusion();
	}
}
