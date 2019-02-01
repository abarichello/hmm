using System;
using HeavyMetalMachines.Character;
using HeavyMetalMachines.VFX;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class CharacterConfig : GameHubBehaviour
	{
		public void OnCharacterIconHoverOver()
		{
			this.GlowSelectIconRef.gameObject.SetActive(true);
		}

		public void OnCharacterIconHoverOut()
		{
			this.GlowSelectIconRef.gameObject.SetActive(false);
		}

		[NonSerialized]
		public HeavyMetalMachines.Character.CharacterInfo CharInfo;

		[NonSerialized]
		public PickModeGUI PickModeGUI;

		public UIEventTrigger UIeventTrigger;

		public BoxCollider Collider;

		public UIButton Button;

		public HMMUI2DDynamicSprite IconRef;

		public UI2DSprite GlowSelectIconRef;

		public HMM2DSpriteAnimation Animation;

		public UI2DSprite EmptyBorder;

		public UI2DSprite CharacterBorder;

		public UI2DSprite CarrierRing;

		public UI2DSprite TacklerRing;

		public UI2DSprite SupportRing;

		public UI2DSprite RecommendedIcon;

		public bool IsSelected;

		public bool IsEnabled;

		public GameObject RotationGroupGameObject;
	}
}
