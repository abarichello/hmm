using System;
using System.Collections.Generic;
using HeavyMetalMachines.Combat;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.BotAI
{
	internal class BotAIUtils
	{
		public static float GetDistanceSqr(Transform firstObject, Transform secondObject)
		{
			return (firstObject.transform.position - secondObject.transform.position).sqrMagnitude;
		}

		public static CombatObject SearchNearestCombatObject(IList<CombatObject> list, Vector3 position, out float distance, bool charsOnly = false)
		{
			if (list == null || list.Count == 0)
			{
				distance = float.MaxValue;
				return null;
			}
			CombatObject combatObject = list[0];
			distance = (position - combatObject.transform.position).sqrMagnitude;
			for (int i = 1; i < list.Count; i++)
			{
				CombatObject combatObject2 = list[i];
				if (!charsOnly || combatObject2.IsPlayer)
				{
					if (combatObject2.IsAlive())
					{
						if (!combatObject2.Attributes || !combatObject2.Attributes.IsInvulnerable)
						{
							float sqrMagnitude = (position - combatObject2.transform.position).sqrMagnitude;
							if (sqrMagnitude < distance)
							{
								combatObject = combatObject2;
								distance = sqrMagnitude;
							}
						}
					}
				}
			}
			return combatObject;
		}

		public static T GetDisabledComponent<T>(Identifiable identifiable) where T : MonoBehaviour
		{
			T[] componentsInChildren = identifiable.GetComponentsInChildren<T>(true);
			return Array.Find<T>(componentsInChildren, (T c) => c != null);
		}

		public static readonly BitLogger Log = new BitLogger(typeof(BotAIUtils));
	}
}
