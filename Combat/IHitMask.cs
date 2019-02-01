using System;

namespace HeavyMetalMachines.Combat
{
	public interface IHitMask
	{
		bool Self { get; }

		bool Enemies { get; }

		bool Friends { get; }

		bool Bomb { get; }

		bool Wards { get; }

		bool Turrets { get; }

		bool Buildings { get; }

		bool Creeps { get; }

		bool Players { get; }

		bool Boss { get; }

		bool Banished { get; }
	}
}
