using System;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Battlepass.Injection
{
	[CreateAssetMenu(fileName = "BattlepassRewardComponentInstaller", menuName = "Installers/BattlepassRewardComponentInstaller")]
	public class BattlepassRewardComponentInstaller : ScriptableObjectInstaller<BattlepassRewardComponentInstaller>
	{
		public override void InstallBindings()
		{
			base.Container.BindInstance<IBattlepassRewardComponent>(this._battlepassRewardComponent);
		}

		[SerializeField]
		private BattlepassRewardComponent _battlepassRewardComponent;
	}
}
