using System;

namespace HeavyMetalMachines.Playback.Snapshot
{
	public interface IBaseStreamSerialData<in T> where T : IBaseStreamSerialData<T>
	{
		void Apply(T other);
	}
}
