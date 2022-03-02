using System;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace HeavyMetalMachines.CompetitiveMode.View.Matches
{
	public class TemporaryComponentForCompetitiveMatchResultTransitionsTestScene : MonoBehaviour
	{
		private void OnEnable()
		{
			ObservableExtensions.Subscribe<Unit>(Observable.ContinueWith<Unit, Unit>(Observable.NextFrame(0), (Unit _) => Observable.ContinueWith<Unit, Unit>(this._presenter.Initialize(), (Unit __) => this._presenter.Show())));
		}

		private void Update()
		{
			if (Input.GetKeyDown(32))
			{
				SceneManager.LoadScene(SceneManager.GetActiveScene().name);
			}
			else if (Input.GetKeyDown(49))
			{
				FakeWaitAndGetMyPlayerCompetitiveStateProgress.SetProgressIndex(0);
			}
			else if (Input.GetKeyDown(50))
			{
				FakeWaitAndGetMyPlayerCompetitiveStateProgress.SetProgressIndex(1);
			}
			else if (Input.GetKeyDown(51))
			{
				if (Input.GetKey(304))
				{
					FakeWaitAndGetMyPlayerCompetitiveStateProgress.SetProgressIndex(7);
				}
				else
				{
					FakeWaitAndGetMyPlayerCompetitiveStateProgress.SetProgressIndex(2);
				}
			}
			else if (Input.GetKeyDown(52))
			{
				if (Input.GetKey(304))
				{
					FakeWaitAndGetMyPlayerCompetitiveStateProgress.SetProgressIndex(8);
				}
				else
				{
					FakeWaitAndGetMyPlayerCompetitiveStateProgress.SetProgressIndex(3);
				}
			}
			else if (Input.GetKeyDown(53))
			{
				if (Input.GetKey(304))
				{
					FakeWaitAndGetMyPlayerCompetitiveStateProgress.SetProgressIndex(9);
				}
				else
				{
					FakeWaitAndGetMyPlayerCompetitiveStateProgress.SetProgressIndex(4);
				}
			}
			else if (Input.GetKeyDown(54))
			{
				if (Input.GetKey(304))
				{
					FakeWaitAndGetMyPlayerCompetitiveStateProgress.SetProgressIndex(10);
				}
				else
				{
					FakeWaitAndGetMyPlayerCompetitiveStateProgress.SetProgressIndex(5);
				}
			}
			else if (Input.GetKeyDown(55))
			{
				if (Input.GetKey(304))
				{
					FakeWaitAndGetMyPlayerCompetitiveStateProgress.SetProgressIndex(11);
				}
				else
				{
					FakeWaitAndGetMyPlayerCompetitiveStateProgress.SetProgressIndex(6);
				}
			}
			else if (Input.GetKeyDown(56))
			{
				if (Input.GetKey(304))
				{
					FakeWaitAndGetMyPlayerCompetitiveStateProgress.SetProgressIndex(13);
				}
				else
				{
					FakeWaitAndGetMyPlayerCompetitiveStateProgress.SetProgressIndex(12);
				}
			}
		}

		[SerializeField]
		private NGuiCompetitiveMatchResultView _view;

		[Inject]
		private ICompetitiveMatchResultPresenter _presenter;
	}
}
