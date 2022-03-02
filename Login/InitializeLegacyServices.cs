using System;
using System.Collections.Generic;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.CustomMatch;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Infra;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Social;
using HeavyMetalMachines.Swordfish;
using HeavyMetalMachines.VFX.PlotKids;

namespace HeavyMetalMachines.Login
{
	public class InitializeLegacyServices : IInitializeLegacyServices
	{
		public InitializeLegacyServices(SwordfishLog swordfishLog, List<IInitializeOnSwordfishConnected> swordfishInitializations, SwordfishMatchmaking swordfishMatchmaking)
		{
			this._swordfishLog = swordfishLog;
			this._swordfishInitializations = swordfishInitializations;
			this._swordfishMatchmaking = swordfishMatchmaking;
		}

		public void Initialize()
		{
			ManagerController.Get<GroupManager>().InitializeSwordfishConnected();
			ManagerController.Get<ChatManager>().InitializeSwordfishConnected();
			ManagerController.Get<MatchManager>().InitializeSwordfishConnected();
			SingletonMonoBehaviour<CustomMatchController>.Instance.InitializeSwordfishConnected();
			this._swordfishLog.InitializeSwordfishConnected();
			this._swordfishMatchmaking.Initialize();
			foreach (IInitializeOnSwordfishConnected initializeOnSwordfishConnected in this._swordfishInitializations)
			{
				initializeOnSwordfishConnected.Initialize(null);
			}
		}

		public void Dispose()
		{
			ManagerController.Get<GroupManager>().DisposeFromSwordfishConnection();
			ManagerController.Get<ChatManager>().DisposeFromSwordfishConnection();
			ManagerController.Get<MatchManager>().DisposeFromSwordfishConnection();
			SingletonMonoBehaviour<CustomMatchController>.Instance.DisposeFromSwordfishConnection();
			foreach (IInitializeOnSwordfishConnected initializeOnSwordfishConnected in this._swordfishInitializations)
			{
				initializeOnSwordfishConnected.Dispose();
			}
			this._swordfishMatchmaking.Dispose();
		}

		private readonly SwordfishLog _swordfishLog;

		private readonly SwordfishMatchmaking _swordfishMatchmaking;

		private readonly List<IInitializeOnSwordfishConnected> _swordfishInitializations;
	}
}
