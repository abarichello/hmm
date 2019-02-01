using System;
using System.Collections;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Match;
using UnityEngine;

namespace HeavyMetalMachines
{
	public static class LayerManager
	{
		public static LayerManager.Mask GetMask(int layer)
		{
			return (LayerManager.Mask)(1 << layer);
		}

		public static LayerManager.Mask GetMask(LayerManager.Layer layer)
		{
			return (LayerManager.Mask)(1 << (int)layer);
		}

		public static void SetLayerRecursively(Component obj, int layer)
		{
			LayerManager.SetLayerRecursively(obj, layer, -1);
		}

		public static void SetLayerRecursively(Component obj, LayerManager.Layer layer)
		{
			LayerManager.SetLayerRecursively(obj, (int)layer, -1);
		}

		public static void SetLayerRecursively(Component obj, int layer, int ignoreLayer)
		{
			if (obj.gameObject.layer != 23 && obj.gameObject.layer != ignoreLayer && obj.gameObject.layer != 28)
			{
				obj.gameObject.layer = layer;
			}
			IEnumerator enumerator = obj.transform.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj2 = enumerator.Current;
					Transform obj3 = (Transform)obj2;
					LayerManager.SetLayerRecursively(obj3, layer, ignoreLayer);
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = (enumerator as IDisposable)) != null)
				{
					disposable.Dispose();
				}
			}
		}

		public static int GetBombAndTeamRaycastLayer(BaseFX effect)
		{
			return LayerManager.GetBombAndTeamRaycastLayer(effect.CheckBombBlocking, effect.Team, effect.Attached);
		}

		public static int GetBombAndTeamSceneryMask(bool bombBlock, TeamKind team)
		{
			int num = 512;
			if (bombBlock)
			{
				num |= 524288;
			}
			if (team != TeamKind.Red)
			{
				if (team == TeamKind.Blue)
				{
					num |= 16777216;
				}
			}
			else
			{
				num |= 4194304;
			}
			return num;
		}

		public static int GetBombAndTeamRaycastLayer(bool checkBombBlocking, TeamKind team, CombatObject attached)
		{
			int num = 0;
			if (checkBombBlocking)
			{
				num |= 524288;
			}
			if (team != TeamKind.Red)
			{
				if (team == TeamKind.Blue)
				{
					num |= 16777216;
				}
			}
			else
			{
				num |= 4194304;
			}
			if (attached)
			{
				TeamKind team2 = attached.Team;
				if (team2 != TeamKind.Red)
				{
					if (team2 == TeamKind.Blue)
					{
						num |= 16777216;
					}
				}
				else
				{
					num |= 4194304;
				}
			}
			return num;
		}

		public static int GetWallMask(bool includeBombBlockers)
		{
			int num = 512;
			if (includeBombBlockers)
			{
				num |= 524288;
			}
			return num;
		}

		public static bool IsSceneryOrBombBlocker(BaseFX effect, Collider other)
		{
			bool flag = 9 == other.gameObject.layer;
			if (effect.CheckBombBlocking)
			{
				flag |= (19 == other.gameObject.layer);
			}
			TeamKind team = effect.Team;
			if (team != TeamKind.Red)
			{
				if (team == TeamKind.Blue)
				{
					flag |= (24 == other.gameObject.layer);
				}
			}
			else
			{
				flag |= (22 == other.gameObject.layer);
			}
			if (effect.Attached)
			{
				TeamKind team2 = effect.Attached.Team;
				if (team2 != TeamKind.Red)
				{
					if (team2 == TeamKind.Blue)
					{
						flag |= (24 == other.gameObject.layer);
					}
				}
				else
				{
					flag |= (22 == other.gameObject.layer);
				}
			}
			return flag;
		}

		public enum Layer
		{
			Default,
			UI = 5,
			PhysicsProjectileRed = 8,
			Scenery,
			PlayerRed,
			PlayerBlu,
			PlayerNeutral = 30,
			Creep = 12,
			Projectile,
			PlayerTrigger,
			RedBlocker = 22,
			BluBlocker = 24,
			Ward = 15,
			ProjectileTrigger,
			Bomb,
			PhysicsProjectileBlue,
			BombBlocker,
			Phasing,
			Flying,
			NoHit = 23,
			CombatLinkCorner = 25,
			Props,
			SfxHelper,
			IgnoreSurfaceFX,
			ModelViewer,
			UserInterface = 31
		}

		[Flags]
		public enum Mask
		{
			Default = 1,
			Scenery = 512,
			PlayerRed = 1024,
			PlayerBlu = 2048,
			PlayerNeutral = 1073741824,
			Player = 1073744896,
			RedBlocker = 4194304,
			BluBlocker = 16777216,
			Creep = 4096,
			Projectile = 8192,
			PhysicsProjectileRed = 256,
			PhysicsProjectileBlue = 262144,
			PlayerTrigger = 16384,
			Ward = 32768,
			Bomb = 131072,
			BombBlocker = 524288,
			Phasing = 1048576,
			Flying = 2097152,
			NoHit = 8388608,
			SfxHelper = 134217728,
			UI = 32,
			ModelViewer = 536870912,
			UserInterface = -2147483648,
			CombatLinkCorner = 33554432,
			CombatLayer = 1077058560,
			CombatRaycastLayer = 1085471744,
			OverlapLayer = 1182720
		}
	}
}
