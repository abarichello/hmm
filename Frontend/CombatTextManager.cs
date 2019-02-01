using System;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Match;
using Pocketverse;
using Pocketverse.MuralContext;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class CombatTextManager : GameHubBehaviour, PlayerBuildComplete.IPlayerBuildCompleteListener, ICleanupListener
	{
		public void Start()
		{
			this.CombatTextObjectReference.gameObject.SetActive(false);
			this._combatTextObjects = new CombatTextObject[10];
			this._combatTextObjects[0] = this.CombatTextObjectReference;
			for (int i = 1; i < this._combatTextObjects.Length; i++)
			{
				CombatTextObject combatTextObject = UnityEngine.Object.Instantiate<CombatTextObject>(this.CombatTextObjectReference, Vector3.zero, Quaternion.identity);
				combatTextObject.transform.SetParent(base.transform);
				combatTextObject.transform.localScale = Vector3.one;
				combatTextObject.gameObject.SetActive(false);
				this._combatTextObjects[i] = combatTextObject;
			}
		}

		public void OnDestroy()
		{
			for (int i = 0; i < this._combatTextObjects.Length; i++)
			{
				this._combatTextObjects[i] = null;
			}
			this._combatTextObjects = null;
		}

		public void ShowCombatText(CombatTextType type, float value, Transform target)
		{
			if (GameHubBehaviour.Hub.GuiScripts.AfkControllerGui.IsWindowVisible())
			{
				return;
			}
			CombatTextSettings.TypeSettings settings = this.GetSettings(type);
			if (settings.Threshold < value)
			{
				CombatTextObject combatTextObject = null;
				if (this.TryToGetStackCombatTextObject(type, target, ref combatTextObject))
				{
					combatTextObject.SetToPauseOnTopAnimationCurve(value);
				}
				else if (this.TryToGetFreeCombatTextObject(ref combatTextObject))
				{
					combatTextObject.ShowCombatText(type, value, target, this.GetAnimationClip(type, value), settings.Color, this.CombatTextSettings.StackTimeInSec);
				}
			}
		}

		private CombatTextSettings.TypeSettings GetSettings(CombatTextType type)
		{
			for (int i = 0; i < this.CombatTextSettings.Types.Length; i++)
			{
				CombatTextSettings.TypeSettings result = this.CombatTextSettings.Types[i];
				if (result.Type == type)
				{
					return result;
				}
			}
			throw new ArgumentException(string.Format("Invalid CombatTextType: {0}. Could not find it on CombatTextSettings.", type), "type");
		}

		private AnimationClip GetAnimationClip(CombatTextType type, float value)
		{
			CombatTextAnimation[] array;
			if (type == CombatTextType.DamageSelf || type == CombatTextType.DamageEnemy)
			{
				array = this.CombatTextSettings.DamageAnimations;
			}
			else
			{
				array = this.CombatTextSettings.HealAnimations;
			}
			foreach (CombatTextAnimation combatTextAnimation in array)
			{
				if (value >= (float)combatTextAnimation.MinValue && value <= (float)combatTextAnimation.MaxValue)
				{
					return combatTextAnimation.AnimationClip;
				}
			}
			return (array.Length <= 0) ? null : array[0].AnimationClip;
		}

		private bool TryToGetFreeCombatTextObject(ref CombatTextObject combatTextObject)
		{
			for (int i = 0; i < this._combatTextObjects.Length; i++)
			{
				CombatTextObject combatTextObject2 = this._combatTextObjects[i];
				if (!combatTextObject2.gameObject.activeSelf)
				{
					combatTextObject = combatTextObject2;
					return true;
				}
			}
			return false;
		}

		private bool TryToGetStackCombatTextObject(CombatTextType combatTextType, Transform target, ref CombatTextObject combatTextObject)
		{
			for (int i = 0; i < this._combatTextObjects.Length; i++)
			{
				CombatTextObject combatTextObject2 = this._combatTextObjects[i];
				if (combatTextObject2.gameObject.activeSelf)
				{
					if (combatTextObject2.GetCombatTextType() == combatTextType && !combatTextObject2.PassedTopAnimationCurve() && combatTextObject2.FollowTarget.IsTargetTransform(target.GetInstanceID()))
					{
						combatTextObject = combatTextObject2;
						return true;
					}
				}
			}
			return false;
		}

		public void OnPlayerBuildComplete(PlayerBuildComplete evt)
		{
			PlayerData playerOrBotsByObjectId = GameHubBehaviour.Hub.Players.GetPlayerOrBotsByObjectId(evt.Id);
			if (!playerOrBotsByObjectId.IsCurrentPlayer)
			{
				return;
			}
			this._combatObject = playerOrBotsByObjectId.CharacterInstance.GetBitComponent<CombatObject>();
			this._combatObject.OnDamageReceived += this.OnDamageReceived;
			this._combatObject.OnRepairReceived += this.OnRepairReceived;
			this._combatObject.OnDamageDealt += this.OnDamageDealt;
			this._combatObject.OnRepairDealt += this.OnRepairDealt;
			this._combatObject.OnTempHPReceived += this.OnTempHPReceived;
		}

		private void OnDamageReceived(float damage, int id)
		{
			this.ShowCombatText(CombatTextType.DamageSelf, damage, this._combatObject.transform);
		}

		private void OnRepairReceived(float damage, int id)
		{
			this.ShowCombatText(CombatTextType.RepairSelf, damage, this._combatObject.transform);
		}

		private void OnDamageDealt(float damage, int id)
		{
			PlayerData playerOrBotsByObjectId = GameHubBehaviour.Hub.Players.GetPlayerOrBotsByObjectId(id);
			if (playerOrBotsByObjectId == null)
			{
				return;
			}
			if (!playerOrBotsByObjectId.IsCurrentPlayer)
			{
				this.ShowCombatText(CombatTextType.DamageEnemy, damage, playerOrBotsByObjectId.CharacterInstance.transform);
			}
		}

		private void OnRepairDealt(float damage, int id)
		{
			PlayerData playerOrBotsByObjectId = GameHubBehaviour.Hub.Players.GetPlayerOrBotsByObjectId(id);
			if (!playerOrBotsByObjectId.IsCurrentPlayer)
			{
				this.ShowCombatText(CombatTextType.RepairAlly, damage, playerOrBotsByObjectId.CharacterInstance.transform);
			}
		}

		private void OnTempHPReceived(float amount, int id)
		{
			PlayerData playerOrBotsByObjectId = GameHubBehaviour.Hub.Players.GetPlayerOrBotsByObjectId(id);
			if (playerOrBotsByObjectId != null)
			{
				this.ShowCombatText(CombatTextType.TempHP, amount, playerOrBotsByObjectId.CharacterInstance.transform);
			}
		}

		public void OnCleanup(CleanupMessage msg)
		{
			if (this._combatObject != null)
			{
				this._combatObject.OnDamageReceived -= this.OnDamageReceived;
				this._combatObject.OnRepairReceived -= this.OnRepairReceived;
				this._combatObject.OnDamageDealt -= this.OnDamageDealt;
				this._combatObject.OnRepairDealt -= this.OnRepairDealt;
				this._combatObject.OnTempHPReceived -= this.OnTempHPReceived;
			}
		}

		private const int CombatTextMaxPool = 10;

		public CombatTextSettings CombatTextSettings;

		public CombatTextObject CombatTextObjectReference;

		private CombatTextObject[] _combatTextObjects;

		private CombatObject _combatObject;
	}
}
