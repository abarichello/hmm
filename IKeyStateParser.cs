using System;
using Pocketverse;

namespace HeavyMetalMachines
{
	public interface IKeyStateParser
	{
		StateType Type { get; }

		void Update(BitStream data);
	}
}
