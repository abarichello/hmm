using System;
using HeavyMetalMachines.BI;
using HeavyMetalMachines.Utils;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class OpenLink : GameHubBehaviour
	{
		public void Open()
		{
			if (string.IsNullOrEmpty(this._url))
			{
				return;
			}
			OpenUrlUtils.OpenUrl(this._url);
			if (!this._logBi || GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish))
			{
				return;
			}
			GameHubBehaviour.Hub.Swordfish.Log.BILogClient(this._biTag, true);
		}

		[SerializeField]
		protected string _url;

		[SerializeField]
		protected bool _logBi;

		[SerializeField]
		protected ClientBITags _biTag;
	}
}
