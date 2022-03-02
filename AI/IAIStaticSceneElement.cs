using System;
using HeavyMetalMachines.Match;
using UnityEngine;

namespace HeavyMetalMachines.AI
{
	public interface IAIStaticSceneElement
	{
		Vector2 BoundsMin { get; }

		Vector2 BoundsMax { get; }

		TeamKind AffectedTeam { get; }

		IAIElementKind Kind { get; }

		Collider ElementCollider { get; }
	}
}
