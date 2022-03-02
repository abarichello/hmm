using System;
using HeavyMetalMachines.CharacterSelection.Client.Presenting.Infra;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.CharacterSelection.Client
{
	public class UnityArenaInfoView : MonoBehaviour, IArenaInfoView
	{
		public ILabel ModeLabel
		{
			get
			{
				return this._modeLabel;
			}
		}

		public ILabel NameLabel
		{
			get
			{
				return this._nameLabel;
			}
		}

		private void OnEnable()
		{
			this._viewProvider.Bind<IArenaInfoView>(this, null);
		}

		private void OnDisable()
		{
			this._viewProvider.Unbind<IArenaInfoView>(null);
		}

		[Inject]
		private IViewProvider _viewProvider;

		[SerializeField]
		private UnityLabel _modeLabel;

		[SerializeField]
		private UnityLabel _nameLabel;
	}
}
