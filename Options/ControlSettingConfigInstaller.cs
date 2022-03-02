using System;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Options
{
	[CreateAssetMenu(fileName = "ControlSettingConfigInstaller", menuName = "Installers/ControlSettingConfigInstaller")]
	public class ControlSettingConfigInstaller : ScriptableObjectInstaller<ControlSettingConfigInstaller>
	{
		public override void InstallBindings()
		{
			base.Container.BindInstance<IControlSetting>(this._controlSetting);
		}

		[SerializeField]
		private ControlSetting _controlSetting;
	}
}
