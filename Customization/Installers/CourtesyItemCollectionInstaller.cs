using System;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Customization.Installers
{
	[CreateAssetMenu(fileName = "CourtesyItemCollectionInstaller", menuName = "Installers/CourtesyItemCollectionInstaller")]
	public class CourtesyItemCollectionInstaller : ScriptableObjectInstaller<CourtesyItemCollectionInstaller>
	{
		public override void InstallBindings()
		{
			base.Container.Bind<ICourtesyItemCollection>().FromInstance(this._courtesyItemCollection);
		}

		[SerializeField]
		private CourtesyItemCollection _courtesyItemCollection;
	}
}
