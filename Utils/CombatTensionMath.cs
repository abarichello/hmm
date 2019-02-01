using System;
using HeavyMetalMachines.Combat;
using UnityEngine;

namespace HeavyMetalMachines.Utils
{
	[Serializable]
	public class CombatTensionMath
	{
		public void Setup(CombatObject srcCombat, CombatObject dstCombat, float realRange)
		{
			this._srcMods = ModifierData.CreateData(this.SrcModifiers);
			this._dstMods = ModifierData.CreateData(this.DstModifiers);
			this._srcCombat = srcCombat;
			this._dstCombat = dstCombat;
			this._srqRealRange = realRange * realRange;
			this._sqrRange = realRange * realRange * this.StartTensionRangeMultiplier * this.StartTensionRangeMultiplier;
		}

		public void ExecuteTension()
		{
			Vector3 position = this._srcCombat.transform.position;
			Vector3 position2 = this._dstCombat.transform.position;
			float num = Vector3.SqrMagnitude(position2 - position);
			if (num < this._sqrRange)
			{
				return;
			}
			Vector3 normalized = this._srcCombat.GetComponent<Rigidbody>().velocity.normalized;
			Vector3 normalized2 = this._dstCombat.GetComponent<Rigidbody>().velocity.normalized;
			Vector3 b = (position + position2) * 0.5f;
			Vector3 normalized3 = (position - b).normalized;
			Vector3 normalized4 = (position2 - b).normalized;
			bool flag = Vector3.Dot(normalized, normalized3) > 0f;
			bool flag2 = Vector3.Dot(normalized2, normalized4) > 0f;
			if (!flag && this.SrcApplyOnTensionOnly && !flag2 && this.DstApplyOnTensionOnly)
			{
				return;
			}
			Vector3 vector = position2 - position;
			Vector3 normalized5 = vector.normalized;
			float num2 = vector.sqrMagnitude / this._srqRealRange;
			if (!this.QuadraticTension)
			{
				num2 = Mathf.Sqrt(num2);
			}
			if (!this.SrcApplyOnTensionOnly || flag)
			{
				ModifierData[] array = ModifierData.CreateConvoluted(this._srcMods, num2);
				array.SetPosition(position);
				array.SetDirection(normalized5);
				this._srcCombat.Controller.AddModifiers(array, this._srcCombat, -1, false);
			}
			if (!this.DstApplyOnTensionOnly || flag2)
			{
				ModifierData[] array = ModifierData.CreateConvoluted(this._dstMods, num2);
				array.SetPosition(position2);
				array.SetDirection(-normalized5);
				this._dstCombat.Controller.AddModifiers(array, this._srcCombat, -1, false);
			}
		}

		public bool QuadraticTension;

		public bool SrcApplyOnTensionOnly;

		public bool DstApplyOnTensionOnly;

		public float StartTensionRangeMultiplier = 0.95f;

		public ModifierInfo[] SrcModifiers = new ModifierInfo[]
		{
			new ModifierInfo
			{
				Attribute = AttributeBuffKind.Drag,
				HitOwner = true,
				IsPercent = true,
				Amount = 1f,
				LifeTime = 0.3f
			},
			new ModifierInfo
			{
				Effect = EffectKind.Impulse,
				HitOwner = true,
				Amount = 1f
			}
		};

		public ModifierInfo[] DstModifiers = new ModifierInfo[]
		{
			new ModifierInfo
			{
				Attribute = AttributeBuffKind.Drag,
				HitOwner = true,
				IsPercent = true,
				Amount = 1f,
				LifeTime = 0.3f
			},
			new ModifierInfo
			{
				Effect = EffectKind.Impulse,
				HitOwner = true,
				Amount = 1f
			}
		};

		private float _srqRealRange;

		private float _sqrRange;

		private CombatObject _srcCombat;

		private CombatObject _dstCombat;

		private ModifierData[] _srcMods;

		private ModifierData[] _dstMods;
	}
}
