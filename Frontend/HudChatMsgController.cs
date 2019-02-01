using System;
using Pocketverse;

namespace HeavyMetalMachines.Frontend
{
	[Serializable]
	public class HudChatMsgController : GameHubBehaviour
	{
		public float Time;

		public UILabel UiLabel;

		public UI2DSprite Sprite;
	}
}
