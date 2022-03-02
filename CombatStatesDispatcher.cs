using System;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Playback;
using Pocketverse;

namespace HeavyMetalMachines
{
	public class CombatStatesDispatcher : StateDataParser, ICombatStatesFeature
	{
		public CombatStatesDispatcher(IFrameProcessorFactory factory)
		{
			factory.GetProvider(OperationKind.Playback).Bind(FrameKind.CombatStates, new ProcessFrame(this.PlaybackProcess));
			factory.GetProvider(OperationKind.ReplayPlayback).Bind(FrameKind.CombatStates, new ProcessFrame(this.PlaybackProcess));
			factory.GetProvider(OperationKind.FastForward).Bind(FrameKind.CombatStates, new ProcessFrame(this.PlaybackProcess));
			factory.GetProvider(OperationKind.ReplayExecutionQueue).Bind(FrameKind.CombatStates, new ProcessFrame(this.PlaybackProcess));
			factory.GetProvider(OperationKind.RewindExecutionQueue).Bind(FrameKind.CombatStates, new ProcessFrame(this.PlaybackProcess));
		}

		private void PlaybackProcess(IFrame frame, IFrameProcessContext ctx)
		{
			this.Process(frame.GetReadData());
		}

		public ICombatAttributesSerialData GetCombatAttributes(int id)
		{
			return GameHubObject.Hub.Stream.CombatAttStream.GetObject(id);
		}

		public ICombatDataSerialData GetCombatData(int id)
		{
			return GameHubObject.Hub.Stream.CombatDataStream.GetObject(id);
		}

		public IGadgetDataSerialData GetGadgetData(int id)
		{
			return GameHubObject.Hub.Stream.GadgetDataStream.GetObject(id);
		}

		public BombGridController.IGridGamePlayerSerialData GetGridGamePlayerData(int id)
		{
			return (BombGridController.IGridGamePlayerSerialData)GameHubObject.Hub.Stream.StateStream.GetObject(2, id);
		}

		public ISpawnControllerSerialData GetSpawnControllerData(int id)
		{
			return GameHubObject.Hub.Stream.SpawnControllerStream.GetObject(id);
		}

		private const FrameKind Kind = FrameKind.CombatStates;
	}
}
