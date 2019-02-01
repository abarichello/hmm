using System;

namespace HeavyMetalMachines.Scene
{
	public interface IActivatable
	{
		void Activate(bool enable, int causer);
	}
}
