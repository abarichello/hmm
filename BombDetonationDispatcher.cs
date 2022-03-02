using System;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Playback;
using Pocketverse;

namespace HeavyMetalMachines
{
	public class BombDetonationDispatcher : IBombDetonationDispatcher
	{
		public BombDetonationDispatcher(IServerPlaybackDispatcher dispatcher, IFrameProcessorFactory factory, IBombManager bombManager)
		{
			this._dispatcher = dispatcher;
			this._bombManager = bombManager;
			factory.GetProvider(OperationKind.Playback).Bind(FrameKind.BombDetonation, new ProcessFrame(this.Process));
			factory.GetProvider(OperationKind.ReplayPlayback).Bind(FrameKind.BombDetonation, new ProcessFrame(this.Process));
			factory.GetProvider(OperationKind.FastForward).Bind(FrameKind.BombDetonation, new ProcessFrame(this.Process));
		}

		public void Process(IFrame frame, IFrameProcessContext ctx)
		{
			BitStream readData = frame.GetReadData();
			TeamKind damagedTeam = (TeamKind)readData.ReadCompressedInt();
			int pickupInstanceId = readData.ReadCompressedInt();
			this._bombManager.DetonateBomb(damagedTeam, pickupInstanceId);
		}

		public void Send(TeamKind damagedTeam, int pickupId, int lastFrameId)
		{
			BitStream stream = this.GetStream();
			stream.WriteCompressedInt((int)damagedTeam);
			stream.WriteCompressedInt(pickupId);
			int nextFrameId = this._dispatcher.GetNextFrameId();
			this._dispatcher.SendFrame(FrameKind.BombDetonation, true, nextFrameId, lastFrameId, stream.ToArray());
		}

		protected BitStream GetStream()
		{
			if (this._myStream == null)
			{
				this._myStream = new BitStream(1024);
			}
			this._myStream.ResetBitsWritten();
			return this._myStream;
		}

		private const FrameKind Kind = FrameKind.BombDetonation;

		private IServerPlaybackDispatcher _dispatcher;

		private IBombManager _bombManager;

		private BitStream _myStream;
	}
}
