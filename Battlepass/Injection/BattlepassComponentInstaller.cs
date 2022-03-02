using System;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Battlepass.Injection
{
	[CreateAssetMenu(fileName = "BattlepassComponentInstaller", menuName = "Installers/BattlepassComponentInstaller")]
	public class BattlepassComponentInstaller : ScriptableObjectInstaller<BattlepassComponentInstaller>
	{
		public override void InstallBindings()
		{
			base.Container.BindInstance<IBattlepassComponent>(this._battlepassComponent);
		}

		[SerializeField]
		private BattlepassComponent _battlepassComponent;
	}
}
