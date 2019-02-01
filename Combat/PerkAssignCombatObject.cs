using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PerkAssignCombatObject : BasePerk
	{
		public override void PerkInitialized()
		{
			base.PerkInitialized();
			this._refsSet = false;
			Identifiable identifiable;
			switch (this._sourceCombat)
			{
			case BasePerk.PerkTarget.Owner:
				identifiable = this.Effect.Owner;
				break;
			case BasePerk.PerkTarget.Target:
				identifiable = this.Effect.Target;
				break;
			case BasePerk.PerkTarget.Effect:
				identifiable = this.Effect.Id;
				break;
			default:
				PerkAssignCombatObject.Log.ErrorFormat("PerkTarget not implemented: {0}.", new object[]
				{
					this._sourceCombat
				});
				return;
			}
			if (!identifiable)
			{
				PerkAssignCombatObject.Log.ErrorFormat("Identifiable not found in {0} ({1})", new object[]
				{
					this._sourceCombat,
					this.Effect.gameObject.name
				});
				return;
			}
			CombatObject bitComponent = identifiable.GetBitComponent<CombatObject>();
			if (!bitComponent)
			{
				PerkAssignCombatObject.Log.ErrorFormat("CombatObject not found in {0} ({1})", new object[]
				{
					this._sourceCombat,
					this.Effect.gameObject.name
				});
				return;
			}
			this.SetCombatToRefs(bitComponent);
			this._refsSet = true;
		}

		public override void PerkDestroyed(DestroyEffect destroyEffect)
		{
			base.PerkDestroyed(destroyEffect);
			if (this._refsSet)
			{
				this.SetCombatToRefs(null);
			}
		}

		private void SetCombatToRefs(CombatObject combatObject)
		{
			if (this._combatRefs == null)
			{
				PerkAssignCombatObject.Log.ErrorFormat("Combat Refs array is null ({0})", new object[]
				{
					base.gameObject.name
				});
				return;
			}
			for (int i = 0; i < this._combatRefs.Length; i++)
			{
				CombatRef combatRef = this._combatRefs[i];
				if (combatRef)
				{
					combatRef.Combat = combatObject;
				}
				else
				{
					PerkAssignCombatObject.Log.WarnFormat("Null element in Combat Refs array: Index {0} ({1})", new object[]
					{
						i,
						base.gameObject.name
					});
				}
			}
		}

		private static readonly BitLogger Log = new BitLogger(typeof(PerkAssignCombatObject));

		[SerializeField]
		private BasePerk.PerkTarget _sourceCombat;

		[SerializeField]
		private CombatRef[] _combatRefs;

		private bool _refsSet;
	}
}
