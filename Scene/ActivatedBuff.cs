using System;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Match;
using Pocketverse;

namespace HeavyMetalMachines.Scene
{
	public class ActivatedBuff : GameHubBehaviour, IActivatable
	{
		public void Activate(bool enable, int causer)
		{
			if (!enable)
			{
				return;
			}
			Identifiable @object = GameHubBehaviour.Hub.ObjectCollection.GetObject(causer);
			if (@object == null)
			{
				return;
			}
			CombatObject component = @object.GetComponent<CombatObject>();
			if (component == null)
			{
				return;
			}
			if (this.TeamBuff)
			{
				this.BuffTeam(component);
				return;
			}
			ModifierData[] datas = ModifierData.CreateData(this.Modifiers);
			component.Controller.AddModifiers(datas, component, -1, false);
		}

		private void BuffTeam(CombatObject causer)
		{
			TeamKind team = causer.Team;
			ModifierData[] datas = ModifierData.CreateData(this.Modifiers);
			for (int i = 0; i < GameHubBehaviour.Hub.Players.PlayersAndBots.Count; i++)
			{
				PlayerData playerData = GameHubBehaviour.Hub.Players.PlayersAndBots[i];
				if (playerData.Team == team)
				{
					Identifiable characterInstance = playerData.CharacterInstance;
					if (!(characterInstance == null))
					{
						CombatObject component = characterInstance.GetComponent<CombatObject>();
						if (!(component == null))
						{
							component.Controller.AddModifiers(datas, component, -1, false);
						}
					}
				}
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(ActivatedBuff));

		public bool TeamBuff;

		public ModifierInfo[] Modifiers;
	}
}
