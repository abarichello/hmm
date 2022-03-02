using System;
using System.Collections.Generic;
using HeavyMetalMachines.Presenting;
using Hoplon.Input.UiNavigation;
using UniRx;
using UnityEngine;

namespace HeavyMetalMachines.CompetitiveMode.View.Matches
{
	public interface ICompetitiveMatchResultRankedView
	{
		IActivatable Group { get; }

		IButton ContinueButton { get; }

		IAnimation ShowAnimation { get; }

		IAnimation HideAnimation { get; }

		IAnimation TransitionAnimation { get; }

		ILabel DivisionNameLabel { get; }

		ILabel RankChangeDescriptionLabel { get; }

		IObservable<Unit> DivisionOpenTrigger { get; }

		IActivatable ScoreProgressGroup { get; }

		ILabel CurrentScoreLabel { get; }

		ILabel NextScoreLabel { get; }

		ILabel ChangedScoreLabel { get; }

		IActivatable ScoreChangeGroup { get; }

		IActivatable PositiveScoreChangeActivatable { get; }

		IActivatable NegativeScoreChangeActivatable { get; }

		IProgressBar ScoreProgressBar { get; }

		IActivatable ProgresslessScoreGroup { get; }

		ILabel ProgresslessCurrentScoreLabel { get; }

		ILabel ProgresslessChangedScoreLabel { get; }

		IActivatable ProgresslessPositiveScoreChangeActivatable { get; }

		IActivatable ProgresslessNegativeScoreChangeActivatable { get; }

		float ScoreFillTimeSeconds { get; }

		AnimationCurve ScoreFillAnimationCurve { get; }

		IAudio ScoreFillUpAudio { get; }

		IAudio ScoreFillDownAudio { get; }

		IImage DivisionGlowImage { get; }

		Color[] DivisionsGlowColors { get; }

		IAnimation ShowButtonAnimation { get; }

		IAudio[] DivisionsIdleAudios { get; }

		IEnumerable<ICompetitiveMatchResultRankedDivisionView> DivisionViews { get; }

		ICompetitiveMatchResultRankedDivisionView TopPlacementDivisionView { get; }

		IUiNavigationGroupHolder UiNavigationGroupHolder { get; }
	}
}
