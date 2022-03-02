using System;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Match;
using Pocketverse;
using UniRx;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public interface IBombManager
	{
		void UpdateBombInstance(BitStream stream);

		void DetonateBomb(TeamKind damagedTeam, int pickupInstanceId);

		void PhaseChanged();

		void MatchUpdated();

		void OvertimeStarted();

		IObservable<Unit> OnBombCarrierChanged();

		IObservable<BombScoreboardState> OnGamePhaseChanged();

		int GetLastCarrierObjId();

		Transform GetBombTransform();

		BombRulesInfo BombRules { get; }

		bool SlowMotionEnabled { get; }

		event Action<bool> OnSlowMotionToggled;
	}
}
