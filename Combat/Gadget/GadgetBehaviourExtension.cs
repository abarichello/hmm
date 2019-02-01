using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	public static class GadgetBehaviourExtension
	{
		public static bool FireSphereCastHit(this GadgetBehaviour gadgetBehaviour, float radius, float range, CombatCheckHitData hitMask, out CombatObject combat, out float distance)
		{
			Vector3 position = gadgetBehaviour.Combat.Transform.position;
			Vector3 direction = gadgetBehaviour.Target - position;
			Ray ray = new Ray(position, direction);
			RaycastHit[] array = Physics.SphereCastAll(ray, radius, range, 1085471744);
			Array.Sort<RaycastHit>(array, (RaycastHit x, RaycastHit y) => x.distance.CompareTo(y.distance));
			foreach (RaycastHit raycastHit in array)
			{
				combat = CombatRef.GetCombat(raycastHit.collider);
				if (combat && hitMask.CheckHit(gadgetBehaviour.Combat, combat))
				{
					distance = raycastHit.distance;
					return true;
				}
			}
			combat = null;
			distance = 0f;
			return false;
		}

		public static void GetCombatsInArea(this GadgetBehaviour gadget, Vector3 position, float range, int combatLayer, ref List<CombatObject> cpoFoundObjects)
		{
			cpoFoundObjects.Clear();
			Collider[] array = Physics.OverlapSphere(position, range, combatLayer);
			if (array == null)
			{
				return;
			}
			foreach (Collider comp in array)
			{
				CombatObject combat = CombatRef.GetCombat(comp);
				if (combat && !cpoFoundObjects.Contains(combat))
				{
					cpoFoundObjects.Add(combat);
				}
			}
		}

		public static void GetHittingCombatsInArea(this GadgetBehaviour gadget, Vector3 position, float range, int combatLayer, IHitMask fxInfo, ref List<CombatObject> cpoFoundObjects)
		{
			cpoFoundObjects.Clear();
			Collider[] array = Physics.OverlapSphere(position, range, combatLayer);
			if (array == null)
			{
				return;
			}
			foreach (Collider comp in array)
			{
				CombatObject combat = CombatRef.GetCombat(comp);
				if (combat && BaseFX.CheckHit(gadget.Combat, combat, fxInfo) && !cpoFoundObjects.Contains(combat))
				{
					cpoFoundObjects.Add(combat);
				}
			}
		}

		public static void GetHittingCombatsInLine(this GadgetBehaviour gadget, Vector3 position, float lineWidth, Vector3 direction, float range, int combatLayer, FXInfo fxInfo, ref List<CombatObject> cpoFoundObjects)
		{
			cpoFoundObjects.Clear();
			RaycastHit[] array;
			if (lineWidth == 0f)
			{
				array = Physics.RaycastAll(position, direction, range, combatLayer);
			}
			else
			{
				array = Physics.SphereCastAll(position, lineWidth, direction, range, combatLayer);
			}
			if (array == null)
			{
				return;
			}
			foreach (RaycastHit raycastHit in array)
			{
				CombatObject combat = CombatRef.GetCombat(raycastHit.collider);
				if (combat && BaseFX.CheckHit(gadget.Combat, combat, fxInfo) && !cpoFoundObjects.Contains(combat))
				{
					cpoFoundObjects.Add(combat);
				}
			}
		}
	}
}
