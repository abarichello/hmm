using System;
using HeavyMetalMachines.Arena;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Match;
using Pocketverse;

namespace HeavyMetalMachines.Announcer
{
	public class ServerAnnouncer : GameHubObject
	{
		public void Initialize()
		{
			GameHubObject.Hub.BombManager.ListenToBombCarrierChanged += this.OnListenToBombCarrierChanged;
		}

		private void OnListenToBombCarrierChanged(CombatObject carrier)
		{
			if (carrier != null && !carrier.IsBomb && GameHubObject.Hub.BombManager.IsCarryingBomb(carrier.Id.ObjId))
			{
				IGameArenaInfo currentArena = GameHubObject.Hub.ArenaConfig.GetCurrentArena();
				float num = (carrier.Team != TeamKind.Blue) ? GameHubObject.Hub.SensorController.DistanceToBlueGoal : GameHubObject.Hub.SensorController.DistanceToRedGoal;
				AnnouncerEvent content = new AnnouncerEvent
				{
					AnnouncerEventKind = ((num >= currentArena.NearGoalDistance) ? AnnouncerLog.AnnouncerEventKinds.BombPicked : AnnouncerLog.AnnouncerEventKinds.BombPickedNearGoal),
					Killer = carrier.Id.ObjId,
					KillerTeam = carrier.Team
				};
				GameHubObject.Hub.Events.TriggerEvent(content);
			}
		}
	}
}
