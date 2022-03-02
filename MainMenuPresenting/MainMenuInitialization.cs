using System;

namespace HeavyMetalMachines.MainMenuPresenting
{
	public class MainMenuInitialization : IMainMenuInitialization
	{
		public MainMenuNode NodeToGo
		{
			get
			{
				return this._nodeToGo;
			}
			set
			{
				this._nodeToGo = value;
			}
		}

		public void ClearNodeToGo()
		{
			this._nodeToGo = MainMenuNode.None;
		}

		private MainMenuNode _nodeToGo;
	}
}
