using System;
using Pocketverse;

namespace HeavyMetalMachines.Frontend
{
	public class ChatWindowReference : GameHubBehaviour
	{
		public UIScrollView ScrollView
		{
			get
			{
				if (this._scrollView == null)
				{
					this._scrollView = this.Window.GetComponent<UIScrollView>();
				}
				return this._scrollView;
			}
		}

		public UIPanel Window;

		public UIInput Input;

		public UIScrollBar ScrollBar;

		private UIScrollView _scrollView;
	}
}
