using System;
using System.Text;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public static class ModifierDataExt
	{
		public static bool CanReturnDamage(this ModifierData mod)
		{
			return mod.Info.Effect.IsHPDamage() && mod.Info.LifeTime == 0f && mod.Info.TickDelta == 0f;
		}

		public static void SetLevel(this ModifierData[] modData, string upgradeName, int level)
		{
			for (int i = 0; i < modData.Length; i++)
			{
				modData[i].SetLevel(upgradeName, level);
			}
		}

		public static void SetDirection(this ModifierData[] modData, Vector3 direction)
		{
			for (int i = 0; i < modData.Length; i++)
			{
				modData[i].SetDirection(direction);
			}
		}

		public static void SetDirection(this ModifierData mod, Vector3 direction)
		{
			mod.DirectionSet = true;
			mod.Direction = direction;
		}

		public static void SetPosition(this ModifierData[] modData, Vector3 position)
		{
			for (int i = 0; i < modData.Length; i++)
			{
				modData[i].SetPosition(position);
			}
		}

		public static void SetPosition(this ModifierData mod, Vector3 position)
		{
			mod.PositionSet = true;
			mod.Position = position;
		}

		public static string ToReadableString(this ModifierData[] modData)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("ModDatas={");
			for (int i = 0; i < modData.Length; i++)
			{
				ModifierData value = modData[i];
				stringBuilder.Append(value);
				if (i < modData.Length - 1)
				{
					stringBuilder.Append(";");
				}
			}
			stringBuilder.Append("}");
			return stringBuilder.ToString();
		}
	}
}
