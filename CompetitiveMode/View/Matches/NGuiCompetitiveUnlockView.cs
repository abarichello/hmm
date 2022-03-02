using System;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.NGui;
using HeavyMetalMachines.Presenting.Unity;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.CompetitiveMode.View.Matches
{
	public class NGuiCompetitiveUnlockView : MonoBehaviour, ICompetitiveUnlockView
	{
		public IActivatable UnlockMatchesGroup
		{
			get
			{
				return this._unlockMatchesGroup;
			}
		}

		public ILabel MatchesPlayedLabel
		{
			get
			{
				return this._matchesPlayedLabel;
			}
		}

		public ILabel TotalMatchesNeededLabel
		{
			get
			{
				return this._totalMatchesNeededLabel;
			}
		}

		private void OnEnable()
		{
			this._viewProvider.Bind<ICompetitiveUnlockView>(this, null);
		}

		private void OnDisable()
		{
			this._viewProvider.Unbind<ICompetitiveUnlockView>(null);
		}

		[Inject]
		private IViewProvider _viewProvider;

		[SerializeField]
		private GameObjectActivatable _unlockMatchesGroup;

		[SerializeField]
		private NGuiLabel _matchesPlayedLabel;

		[SerializeField]
		private NGuiLabel _totalMatchesNeededLabel;
	}
}
