using System;
using Pocketverse;

namespace HeavyMetalMachines.UpdateStream
{
	public interface IStreamContent
	{
		Identifiable Id { get; }

		int GetStreamData(ref byte[] buffer, bool boForceSerialization);

		void ApplyStreamData(byte[] data);

		short Version { get; set; }
	}
}
