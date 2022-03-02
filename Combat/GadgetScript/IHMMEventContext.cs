using System;
using System.Collections.Generic;
using Hoplon.GadgetScript;
using Pocketverse;

namespace HeavyMetalMachines.Combat.GadgetScript
{
	public interface IHMMEventContext : IEventContext
	{
		void LoadInitialParameters();

		void SaveParameter(BaseParameter parameter);

		void LoadParameter(BaseParameter outputParameter);

		void SetPreviousEventId(int eventId);

		void Undo();

		void SendToClient();

		void WriteToBitStream(BitStream bs);

		void ReadFromBitStream(BitStream bs);

		bool ConsumeBody();

		bool ShouldBeSent { get; }

		int PreviousEventId { get; }

		void GetBodies(out List<int> created, out List<int> removed);
	}
}
