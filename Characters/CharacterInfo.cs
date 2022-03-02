using System;
using Assets.ClientApiObjects;
using HeavyMetalMachines.Car;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Combat.GadgetScript;
using HeavyMetalMachines.Utils;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Characters
{
	public class CharacterInfo : GameHubScriptableObject, IContent, ISerializationCallbackReceiver
	{
		public int ContentId
		{
			get
			{
				return this.CharacterId;
			}
			set
			{
				this.CharacterId = value;
			}
		}

		public override string ToString()
		{
			return string.Format("CharacterId={0}", this.CharacterId);
		}

		public Guid CharacterItemTypeGuid
		{
			get
			{
				return this._characterItemTypeGuid;
			}
		}

		public void OnBeforeSerialize()
		{
		}

		public void OnAfterDeserialize()
		{
			if (!string.IsNullOrEmpty(this._characterItemType))
			{
				this._characterItemTypeGuid = new Guid(this._characterItemType);
			}
		}

		[ScriptId]
		public int CharacterId;

		public CarInfo Car;

		public TurretMovement.Configuration TurretMovementConfiguration;

		public CarCollider Collider;

		public CarCollider[] ExtraColliders;

		public CombatInfo Combat;

		public CarIndicator.CustomConfig IndicatorConfig;

		[ItemType(typeof(ItemTypeScriptableObject))]
		[SerializeField]
		private string _characterItemType;

		protected Guid _characterItemTypeGuid;

		public GadgetInfo CustomGadget0;

		public GadgetInfo CustomGadget1;

		public GadgetInfo CustomGadget2;

		public GadgetInfo GenericGadget;

		public GadgetInfo BoostGadget;

		public GadgetInfo PassiveGadget;

		public GadgetInfo TrailGadget;

		public GadgetInfo OutOfCombatGadget;

		public GadgetInfo DmgUpgrade;

		public GadgetInfo HPUpgrade;

		public GadgetInfo EPUpgrade;

		public GadgetInfo BombGadget;

		public GadgetInfo RespawnGadget;

		public GadgetInfo[] TakeoffGadgets;

		public GadgetInfo KillGadget;

		public GadgetInfo BombExplosionGadget;

		public GadgetInfo SprayGadget;

		public GadgetInfo GridHighlightGadget;

		public CombatGadget[] CustomGadgets;

		public bool ApplyOnGadget0UsedModifiers = true;

		public bool ApplyOnGadget1UsedModifiers;

		public bool ApplyOnGadget2UsedModifiers;

		public ModifierInfo[] OnGadgetUsedModifiers = new ModifierInfo[]
		{
			new ModifierInfo
			{
				Amount = -0.34f,
				Attribute = AttributeBuffKind.ObsoleteSpeedMax,
				FriendlyFire = true,
				IsPercent = true,
				LifeTime = 1.5f
			}
		};
	}
}
