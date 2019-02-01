using System;
using Pocketverse;

namespace HeavyMetalMachines.Frontend
{
	public class OnBuffTooltipEvent : GameHubBehaviour
	{
		private static GameGui GameGui
		{
			get
			{
				GameGui result;
				if ((result = OnBuffTooltipEvent._gameGui) == null)
				{
					result = (OnBuffTooltipEvent._gameGui = GameHubBehaviour.Hub.State.Current.GetStateGuiController<GameGui>());
				}
				return result;
			}
		}

		private void OnDestroy()
		{
			OnBuffTooltipEvent._gameGui = null;
		}

		private void OnTooltip(bool show)
		{
			if (!show)
			{
				this.DestroyTooltip();
			}
		}

		private void Update()
		{
		}

		public void DestroyTooltip()
		{
		}

		public string TooltipText = string.Empty;

		public bool Withlifetime;

		public long StartLifeTime;

		public long EndLifeTime;

		private static GameGui _gameGui;
	}
}
