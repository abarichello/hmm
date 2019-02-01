using System;
using Pocketverse;

namespace HeavyMetalMachines
{
	public interface IKeyFrameParser
	{
		KeyFrameType Type { get; }

		void Process(BitStream stream);

		bool RewindProcess(IFrame frame);
	}
}
