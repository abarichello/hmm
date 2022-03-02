using System;
using System.Collections.Generic;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public static class BarrierUtils
	{
		public static bool IsBarrier(Collider col)
		{
			return col.gameObject.name.Contains("Barrier");
		}

		public static void OverlapSphereRaycastFromCenter(Vector3 center, float radius, int layer, List<BarrierUtils.CombatHit> hitsResult)
		{
			BarrierUtils.OverlapSphereThenFilter(center, radius, false, center, layer, hitsResult);
		}

		public static void OverlapSphereRaycastFromPoint(Vector3 center, float radius, Vector3 rayOrigin, int layer, List<BarrierUtils.CombatHit> hitsResult)
		{
			BarrierUtils.OverlapSphereThenFilter(center, radius, false, rayOrigin, layer, hitsResult);
		}

		public static void OverlapSphereBarrierPriority(Vector3 center, float radius, int layer, List<BarrierUtils.CombatHit> hitsResult)
		{
			BarrierUtils.OverlapSphereThenFilter(center, radius, true, center, layer, hitsResult);
		}

		public static void OverlapSphereThenFilter(Vector3 center, float radius, bool barrierPriority, Vector3 rayOrigin, int layer, List<BarrierUtils.CombatHit> hitsResult)
		{
			int num = Physics.OverlapSphereNonAlloc(center, radius, BarrierUtils.ColliderBuffer, layer);
			for (int i = 0; i < num; i++)
			{
				Collider collider = BarrierUtils.ColliderBuffer[i];
				CombatObject combat = CombatRef.GetCombat(collider);
				if (!(combat == null))
				{
					bool barrier = BarrierUtils.IsBarrier(collider);
					hitsResult.Add(new BarrierUtils.CombatHit
					{
						Combat = combat,
						Col = collider,
						Barrier = barrier
					});
				}
			}
			if (barrierPriority)
			{
				BarrierUtils.FilterByBarrierPriority(hitsResult);
			}
			else
			{
				BarrierUtils.FilterByRaycastFromPoint(rayOrigin, hitsResult);
			}
		}

		public static void FilterByBarrierPriority(List<BarrierUtils.CombatHit> hitsResult)
		{
			for (int i = 0; i < hitsResult.Count - 1; i++)
			{
				BarrierUtils.CombatHit hit = hitsResult[i];
				BarrierUtils._sameCombatChecker.Init(hit);
				int num = hitsResult.FindIndex(i + 1, new Predicate<BarrierUtils.CombatHit>(BarrierUtils._sameCombatChecker.Check));
				if (num > i)
				{
					if (hitsResult[num].Barrier == hit.Barrier || hit.Barrier)
					{
						hitsResult.RemoveAt(num);
					}
					else
					{
						hitsResult.RemoveAt(i);
						i--;
					}
				}
			}
			BarrierUtils._sameCombatChecker.Clear();
		}

		public static void FilterByRaycastFromPoint(Vector3 point, List<BarrierUtils.CombatHit> hitsResult)
		{
			for (int i = 0; i < hitsResult.Count - 1; i++)
			{
				BarrierUtils.CombatHit hit = hitsResult[i];
				BarrierUtils._sameCombatChecker.Init(hit);
				int num = hitsResult.FindIndex(i + 1, BarrierUtils._sameCombatCheckerCheck);
				if (num > i)
				{
					BarrierUtils.CombatHit combatHit = hitsResult[num];
					if (combatHit.Barrier == hit.Barrier)
					{
						hitsResult.RemoveAt(num);
						i--;
					}
					else
					{
						Vector3 position = hit.Combat.Transform.position;
						Vector3 vector = position - point;
						float magnitude = vector.magnitude;
						vector /= magnitude;
						Ray ray;
						ray..ctor(point, vector);
						RaycastHit raycastHit;
						bool flag = hit.Col.Raycast(ray, ref raycastHit, magnitude);
						float num2 = (!flag) ? float.MaxValue : raycastHit.distance;
						bool flag2 = false;
						if (!flag && BarrierUtils.CheckInsideCollider2D(hit.Col, point, out raycastHit))
						{
							flag2 = true;
							num2 = raycastHit.distance;
						}
						bool flag3 = combatHit.Col.Raycast(ray, ref raycastHit, magnitude);
						float num3 = (!flag3) ? float.MaxValue : raycastHit.distance;
						bool flag4 = false;
						if (!flag3 && BarrierUtils.CheckInsideCollider2D(combatHit.Col, point, out raycastHit))
						{
							flag4 = true;
							num3 = raycastHit.distance;
						}
						bool flag5 = false;
						if (flag2 && flag4)
						{
							flag5 = (combatHit.Barrier && !hit.Barrier);
						}
						else if (flag4)
						{
							flag5 = true;
						}
						else if (!flag2)
						{
							flag5 = ((flag3 && !flag) || (flag3 && num2 > num3));
						}
						if (flag5)
						{
							hitsResult.RemoveAt(i);
							i--;
						}
						else
						{
							hitsResult.RemoveAt(num);
							i--;
						}
					}
				}
			}
			BarrierUtils._sameCombatChecker.Clear();
		}

		private static bool CheckInsideCollider2D(Collider col, Vector3 point, out RaycastHit hitInfo)
		{
			Ray ray;
			ray..ctor(point + Vector3.up * 100f, Vector3.down);
			return col.Raycast(ray, ref hitInfo, 100f);
		}

		private static readonly BitLogger Log = new BitLogger(typeof(BarrierUtils));

		private const int BufferSize = 64;

		private static Collider[] ColliderBuffer = new Collider[64];

		private static BarrierUtils.SameCombatChecker _sameCombatChecker = new BarrierUtils.SameCombatChecker();

		private static Predicate<BarrierUtils.CombatHit> _sameCombatCheckerCheck = new Predicate<BarrierUtils.CombatHit>(BarrierUtils._sameCombatChecker.Check);

		private const float SkyCast = 100f;

		public struct CombatHit
		{
			public CombatObject Combat;

			public Collider Col;

			public bool Barrier;
		}

		private class SameCombatChecker
		{
			public void Init(BarrierUtils.CombatHit hit)
			{
				this._hit = hit;
			}

			public void Clear()
			{
				this._hit = default(BarrierUtils.CombatHit);
			}

			public bool Check(BarrierUtils.CombatHit other)
			{
				return other.Combat == this._hit.Combat;
			}

			private BarrierUtils.CombatHit _hit;
		}
	}
}
