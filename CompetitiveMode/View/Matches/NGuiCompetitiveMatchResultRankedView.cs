using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Fmod;
using HeavyMetalMachines.Presenting.NGui;
using HeavyMetalMachines.Presenting.Unity;
using Hoplon.Input.UiNavigation;
using JetBrains.Annotations;
using UniRx;
using UnityEngine;

namespace HeavyMetalMachines.CompetitiveMode.View.Matches
{
	public class NGuiCompetitiveMatchResultRankedView : MonoBehaviour, ICompetitiveMatchResultRankedView
	{
		public IActivatable Group
		{
			get
			{
				return this._group;
			}
		}

		public IButton ContinueButton
		{
			get
			{
				return this._continueButton;
			}
		}

		public IAnimation ShowAnimation
		{
			get
			{
				return this._showAnimation;
			}
		}

		public IAnimation HideAnimation
		{
			get
			{
				return this._hideAnimation;
			}
		}

		public IAnimation TransitionAnimation
		{
			get
			{
				return this._transitionAnimation;
			}
		}

		public ILabel DivisionNameLabel
		{
			get
			{
				return this._divisionNameLabel;
			}
		}

		public ILabel RankChangeDescriptionLabel
		{
			get
			{
				return this._rankChangeDescriptionLabel;
			}
		}

		public IObservable<Unit> DivisionOpenTrigger
		{
			get
			{
				return this._divisionOpenTriggerSubject;
			}
		}

		public IActivatable ScoreProgressGroup
		{
			get
			{
				return this._scoreProgressGroup;
			}
		}

		public ILabel CurrentScoreLabel
		{
			get
			{
				return this._currentScoreLabel;
			}
		}

		public ILabel NextScoreLabel
		{
			get
			{
				return this._nextScoreLabel;
			}
		}

		public IActivatable ProgresslessScoreGroup
		{
			get
			{
				return this._progresslessScoreGroup;
			}
		}

		public ILabel ProgresslessCurrentScoreLabel
		{
			get
			{
				return this._progresslessCurrentScoreLabel;
			}
		}

		public ILabel ProgresslessChangedScoreLabel
		{
			get
			{
				return this._progresslessChangedScoreLabel;
			}
		}

		public IActivatable ProgresslessPositiveScoreChangeActivatable
		{
			get
			{
				return this._progresslessPositiveScoreChangeActivatable;
			}
		}

		public IActivatable ProgresslessNegativeScoreChangeActivatable
		{
			get
			{
				return this._progresslessNegativeScoreChangeActivatable;
			}
		}

		public ILabel ChangedScoreLabel
		{
			get
			{
				return this._changedScoreLabel;
			}
		}

		public IActivatable ScoreChangeGroup
		{
			get
			{
				return this._scoreChangeGroup;
			}
		}

		public IActivatable PositiveScoreChangeActivatable
		{
			get
			{
				return this._positiveScoreChangeActivatable;
			}
		}

		public IActivatable NegativeScoreChangeActivatable
		{
			get
			{
				return this._negativeScoreChangeActivatable;
			}
		}

		public IProgressBar ScoreProgressBar
		{
			get
			{
				return this._scoreProgressBar;
			}
		}

		public float ScoreFillTimeSeconds
		{
			get
			{
				return this._scoreFillTimeSeconds;
			}
		}

		public AnimationCurve ScoreFillAnimationCurve
		{
			get
			{
				return this._scoreFillAnimationCurve;
			}
		}

		public IAudio ScoreFillUpAudio
		{
			get
			{
				return this._scoreFillUpAudio;
			}
		}

		public IAudio ScoreFillDownAudio
		{
			get
			{
				return this._scoreFillDownAudio;
			}
		}

		public IImage DivisionGlowImage
		{
			get
			{
				return this._divisionGlowImage;
			}
		}

		public Color[] DivisionsGlowColors
		{
			get
			{
				IEnumerable<Color> divisionsGlowColors = this._divisionsGlowColors;
				if (NGuiCompetitiveMatchResultRankedView.<>f__mg$cache0 == null)
				{
					NGuiCompetitiveMatchResultRankedView.<>f__mg$cache0 = new Func<Color, Color>(ColorExtensions.ToHmmColor);
				}
				return divisionsGlowColors.Select(NGuiCompetitiveMatchResultRankedView.<>f__mg$cache0).ToArray<Color>();
			}
		}

		public IAnimation ShowButtonAnimation
		{
			get
			{
				return this._showButtonAnimation;
			}
		}

		public IAudio[] DivisionsIdleAudios
		{
			get
			{
				return this._divisionsIdleAudios;
			}
		}

		public IEnumerable<ICompetitiveMatchResultRankedDivisionView> DivisionViews
		{
			get
			{
				return this._divisionViews;
			}
		}

		public ICompetitiveMatchResultRankedDivisionView TopPlacementDivisionView
		{
			get
			{
				return this._topPlacementDivisionView;
			}
		}

		public IUiNavigationGroupHolder UiNavigationGroupHolder
		{
			get
			{
				return this._uiNavigationGroupHolder;
			}
		}

		[UsedImplicitly]
		public void OnDivisionOpen()
		{
			this._divisionOpenTriggerSubject.OnNext(Unit.Default);
		}

		[SerializeField]
		private GameObjectActivatable _group;

		[SerializeField]
		private NGuiButton _continueButton;

		[SerializeField]
		private UiNavigationGroupHolder _uiNavigationGroupHolder;

		[SerializeField]
		private UnityAnimation _showAnimation;

		[SerializeField]
		private UnityAnimation _hideAnimation;

		[SerializeField]
		private UnityAnimation _transitionAnimation;

		[SerializeField]
		private NGuiLabel _divisionNameLabel;

		[SerializeField]
		private NGuiLabel _rankChangeDescriptionLabel;

		[SerializeField]
		private GameObjectActivatable _scoreProgressGroup;

		[SerializeField]
		private NGuiLabel _currentScoreLabel;

		[SerializeField]
		private NGuiLabel _nextScoreLabel;

		[SerializeField]
		private GameObjectActivatable _progresslessScoreGroup;

		[SerializeField]
		private NGuiLabel _progresslessCurrentScoreLabel;

		[SerializeField]
		private NGuiLabel _progresslessChangedScoreLabel;

		[SerializeField]
		private GameObjectActivatable _progresslessPositiveScoreChangeActivatable;

		[SerializeField]
		private GameObjectActivatable _progresslessNegativeScoreChangeActivatable;

		[SerializeField]
		private NGuiLabel _changedScoreLabel;

		[SerializeField]
		private GameObjectActivatable _scoreChangeGroup;

		[SerializeField]
		private GameObjectActivatable _positiveScoreChangeActivatable;

		[SerializeField]
		private GameObjectActivatable _negativeScoreChangeActivatable;

		[SerializeField]
		private NGuiTextureProgressBar _scoreProgressBar;

		[SerializeField]
		private float _scoreFillTimeSeconds;

		[SerializeField]
		private AnimationCurve _scoreFillAnimationCurve;

		[SerializeField]
		private FmodAudio _scoreFillUpAudio;

		[SerializeField]
		private FmodAudio _scoreFillDownAudio;

		[SerializeField]
		private NGuiImage _divisionGlowImage;

		[SerializeField]
		private Color[] _divisionsGlowColors;

		[SerializeField]
		private NGuiCompetitiveMatchResultRankedDivisionView[] _divisionViews;

		[SerializeField]
		private NGuiCompetitiveMatchResultRankedDivisionView _topPlacementDivisionView;

		[SerializeField]
		private UnityAnimation _showButtonAnimation;

		[SerializeField]
		private FmodAudio[] _divisionsIdleAudios;

		private readonly Subject<Unit> _divisionOpenTriggerSubject = new Subject<Unit>();

		[CompilerGenerated]
		private static Func<Color, Color> <>f__mg$cache0;
	}
}
