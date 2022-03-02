using System;
using Assets.ClientApiObjects;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.NGui;
using HeavyMetalMachines.VFX;
using JetBrains.Annotations;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class CharacterConfig : GameHubBehaviour
	{
		public IImage LockedInCompetitiveImage
		{
			get
			{
				return this._lockedInCompetitiveImage;
			}
		}

		public void OnCharacterIconHoverOver()
		{
			this.GlowSelectIconRef.gameObject.SetActive(true);
		}

		public void OnCharacterIconHoverOut()
		{
			this.GlowSelectIconRef.gameObject.SetActive(false);
		}

		[NonSerialized]
		public IItemType CharItemType;

		public UIEventTrigger UIeventTrigger;

		public BoxCollider Collider;

		public UIButton Button;

		public HMMUI2DDynamicSprite IconRef;

		public UI2DSprite GlowSelectIconRef;

		public GameObject SelectionAnimationGameObject;

		public UI2DSprite EmptyBorder;

		public UI2DSprite CharacterBorder;

		public UI2DSprite CarrierRing;

		public UI2DSprite TacklerRing;

		public UI2DSprite SupportRing;

		public UI2DSprite RecommendedIcon;

		[SerializeField]
		[UsedImplicitly]
		private NGuiImage _lockedInCompetitiveImage;

		public bool IsSelected;

		public bool IsEnabled;

		public GameObject RotationGroupGameObject;
	}
}
