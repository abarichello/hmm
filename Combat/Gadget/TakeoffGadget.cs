using System;
using Pocketverse;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class TakeoffGadget : BasicCannon
	{
		private TakeoffInfo MyInfo
		{
			get
			{
				return base.Info as TakeoffInfo;
			}
		}

		public override void SetInfo(GadgetInfo info)
		{
			base.SetInfo(info);
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				GameHubBehaviour.Hub.BombManager.GridController.ListenToGridGameFinished += this.GridControllerOnListenToGridGameFinished;
			}
		}

		protected override void OnDestroy()
		{
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				GameHubBehaviour.Hub.BombManager.GridController.ListenToGridGameFinished -= this.GridControllerOnListenToGridGameFinished;
			}
			base.OnDestroy();
		}

		private void GridControllerOnListenToGridGameFinished(byte address, float finalValue)
		{
			if (address != this.Combat.Player.PlayerAddress)
			{
				return;
			}
			if (this.Combat.IsBot)
			{
				return;
			}
			TakeoffInfo myInfo = this.MyInfo;
			if (finalValue < myInfo.YellowToGreenValue)
			{
				return;
			}
			this.FireGadget();
			this.FireExtraGadget();
		}
	}
}
