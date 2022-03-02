using System;
using Assets.ClientApiObjects;
using HeavyMetalMachines.VFX;
using Pocketverse;

namespace HeavyMetalMachines.Frontend
{
	public class SkinConfig : GameHubBehaviour
	{
		public void OnCharacterIconHoverOver()
		{
			if (UICamera.currentScheme != UICamera.ControlScheme.Controller || this.OnHoverOverAction == null)
			{
				return;
			}
			this.OnHoverOverAction(this.Eventlistener.IntParameter);
		}

		[NonSerialized]
		public ItemTypeScriptableObject Item;

		[NonSerialized]
		public string CharacterName;

		public HMMUI2DDynamicSprite SkinSprite;

		public UILabel CardSkinName;

		public UIButton CardButton;

		public GUIEventListener Eventlistener;

		public UIPanel Panel;

		public TweenScale TweenScale;

		public TweenPosition TweenPosition;

		public bool IsDefault;

		public Action<int> OnHoverOverAction;
	}
}
