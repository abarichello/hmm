using System;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public struct CollisionCallback : Mural.IMuralMessage
	{
		public CollisionCallback(CombatObject combat, CombatObject other, bool isScenery, GadgetBehaviour gadget, EffectEvent effectData, Vector3 collisionNormal)
		{
			this.Combat = combat;
			this.Gadget = gadget;
			this.EffectData = effectData;
			this.Other = other;
			this.IsScenery = isScenery;
			this.CollisionNormal = collisionNormal;
		}

		public string Message
		{
			get
			{
				return "OnCollisionCallback";
			}
		}

		public CombatObject Combat;

		public CombatObject Other;

		public bool IsScenery;

		public GadgetBehaviour Gadget;

		public EffectEvent EffectData;

		public Vector3 CollisionNormal;

		public const string Msg = "OnCollisionCallback";

		public interface ICollisionCallbackListener
		{
			void OnCollisionCallback(CollisionCallback evt);
		}
	}
}
