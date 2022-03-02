using System;
using Assets.Standard_Assets.Scripts.HMM.PlotKids;
using HeavyMetalMachines.Utils;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class SpectatorTopController : GameHubBehaviour
	{
		public void Start()
		{
			if (!GameHubBehaviour.Hub.Net.IsClient() || !SpectatorController.IsSpectating)
			{
				Object.Destroy(base.gameObject);
				return;
			}
			this._widget = base.GetComponent<UIWidget>();
			this._widget.alpha = 0f;
			GameHubBehaviour.Hub.State.ListenToStateChanged += this.ListenToStateChanged;
		}

		private void ListenToStateChanged(GameState pChangedstate)
		{
			bool flag = pChangedstate is PickModeSetup || pChangedstate is Game;
			if (flag && Mathf.Approximately(this._widget.alpha, 0f))
			{
				this._widget.alpha = 1f;
			}
			this.PlayAnimation(!flag);
		}

		private void PlayAnimation(bool reverse)
		{
			GUIUtils.PlayAnimation(this.SpectatorTopAnimation, reverse, 1f, string.Empty);
		}

		public void OnDestroy()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				GameHubBehaviour.Hub.State.ListenToStateChanged -= this.ListenToStateChanged;
			}
		}

		[SerializeField]
		private Animation SpectatorTopAnimation;

		private UIWidget _widget;
	}
}
