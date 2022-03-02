using System;
using System.Diagnostics;
using EnhancedUI.EnhancedScroller;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Players.Presenting;
using HeavyMetalMachines.Publishing.Presenting;
using HeavyMetalMachines.Social;
using HeavyMetalMachines.Utils;
using Hoplon.Logging;
using Hoplon.Time;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace HeavyMetalMachines.Storyteller.Presenting
{
	public class StorytellerGameserverCellView : EnhancedScrollerCellView
	{
		static StorytellerGameserverCellView()
		{
			for (int j = 0; j < 2; j++)
			{
				StorytellerGameserverCellView.MaxStorytellerCache.GenerateValue(j);
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private event Action<int> OnButtonClickedEvent = delegate(int A_0)
		{
		};

		private void Start()
		{
			ObservableExtensions.Subscribe<Unit>(UnityUIComponentExtensions.OnClickAsObservable(this._watchButton), delegate(Unit _)
			{
				this.OnButtonClick();
			});
		}

		private void Update()
		{
			TimeSpan timeSpan = this._currentTime.NowServerUtc() - this._startTime;
			int seconds = timeSpan.Seconds;
			if (seconds == this._lastSeconds)
			{
				return;
			}
			this._lastSeconds = seconds;
			this._durationText.text = TimeUtils.FormatTime(timeSpan);
		}

		private void OnButtonClick()
		{
			this.OnButtonClickedEvent(this._index);
		}

		public void Initialize(IStorytellerMatchInfo matchInfo, int index, Action<int> onButtonClick)
		{
			this._matchInfo = matchInfo;
			this._index = index;
			this.OnButtonClickedEvent += onButtonClick;
			this._serverPhaseText.text = matchInfo.TranslatedPhase;
			this._startTime = matchInfo.ServerBag.GetDate();
			this._lastSeconds = -1;
			this._storytellerCountText.text = StringCaches.NonPaddedIntegers.Get(matchInfo.ServerBag.StorytellerCount);
			string text;
			if (!StorytellerGameserverCellView.MaxStorytellerCache.TryGetValue(matchInfo.MaxStorytellers, out text))
			{
				text = StorytellerGameserverCellView.MaxStorytellerCache.GenerateValue(matchInfo.MaxStorytellers);
			}
			this._storytellerMaxCountText.text = text;
			this._storytellerCountFill.fillAmount = (float)matchInfo.ServerBag.StorytellerCount / (float)matchInfo.MaxStorytellers;
			this.SetTeamMembers(this._redTeamSlots, matchInfo.RedMembers);
			this.SetTeamMembers(this._blueTeamSlots, matchInfo.BlueMembers);
			this.SetTeamImage(this._redTeamImage, matchInfo.ServerBag.RedTeamIconURL);
			this.SetTeamImage(this._bluTeamImage, matchInfo.ServerBag.BluTeamIconURL);
			bool canConnect = this._matchInfo.CanConnect;
			bool flag = !canConnect;
			this._watchButton.interactable = canConnect;
			this._inactiveSelectionFeedback.interactable = flag;
			this._inactiveSelectionFeedback.targetGraphic.raycastTarget = flag;
		}

		private void SetTeamMembers(StorytellerTeamMemberSlot[] memberSlots, StorytellerMatchMember[] members)
		{
			if (members == null)
			{
				return;
			}
			for (int i = 0; i < members.Length; i++)
			{
				StorytellerTeamMemberSlot storytellerTeamMemberSlot = memberSlots[i];
				storytellerTeamMemberSlot.NameLabel.Text = members[i].PlayerName;
				this.UpdatePublisherUserName(storytellerTeamMemberSlot, members[i]);
			}
		}

		private void UpdatePublisherUserName(StorytellerTeamMemberSlot memberSlot, StorytellerMatchMember member)
		{
			memberSlot.PsnIdIconActivatable.SetActive(false);
			if (member.IsBot)
			{
				memberSlot.PsnIdLabel.IsActive = false;
				return;
			}
			if (member.ShowPublisherUsername)
			{
				memberSlot.PsnIdLabel.Text = member.PublisherUsername;
			}
			memberSlot.PsnIdLabel.IsActive = member.ShowPublisherUsername;
		}

		private void SetTeamImage(HmmUiRawImage image, string teamIconURL)
		{
			if (string.IsNullOrEmpty(teamIconURL))
			{
				image.texture = this._defaultTeamTexture;
				return;
			}
			image.TryToLoadAsset(teamIconURL);
		}

		public float GetSize()
		{
			return this._rectTransform.sizeDelta.x;
		}

		private void OnValidate()
		{
			this._rectTransform = base.GetComponent<RectTransform>();
		}

		[SerializeField]
		private RectTransform _rectTransform;

		[SerializeField]
		private Text _serverPhaseText;

		[SerializeField]
		private Text _durationText;

		[SerializeField]
		private Image _storytellerCountFill;

		[SerializeField]
		private Text _storytellerCountText;

		[SerializeField]
		private Text _storytellerMaxCountText;

		[SerializeField]
		private Button _watchButton;

		[SerializeField]
		private StorytellerTeamMemberSlot[] _redTeamSlots;

		[SerializeField]
		private StorytellerTeamMemberSlot[] _blueTeamSlots;

		[SerializeField]
		private HmmUiRawImage _redTeamImage;

		[SerializeField]
		private HmmUiRawImage _bluTeamImage;

		[SerializeField]
		private Texture _defaultTeamTexture;

		[SerializeField]
		private Selectable _inactiveSelectionFeedback;

		private IStorytellerMatchInfo _matchInfo;

		private int _index;

		private DateTime _startTime;

		private int _lastSeconds;

		[Inject]
		private IGetDisplayableNickName _getDisplayableNickName;

		[Inject]
		private ICurrentTime _currentTime;

		[Inject]
		private IBadNameCensor _badNameCensor;

		[Inject]
		private IGetPublisherPresentingData _getPublisherPresentingData;

		[Inject]
		private ILogger<StorytellerGameserverCellView> _logger;

		private static StringCache<int> MaxStorytellerCache = new StringCache<int>((int i) => string.Format("/{0}", i), 2);
	}
}
