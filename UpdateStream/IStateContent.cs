using System;

namespace HeavyMetalMachines.UpdateStream
{
	public interface IStateContent
	{
		int ObjId { get; }

		byte ClassId { get; }

		short Version { get; set; }

		bool IsCached();

		byte[] GetStreamData();

		void ApplyStreamData(byte[] data);
	}
}
