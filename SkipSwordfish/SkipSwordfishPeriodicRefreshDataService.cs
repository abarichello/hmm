using System;
using System.Collections.Generic;
using HeavyMetalMachines.PeriodicRefresh;
using HeavyMetalMachines.PeriodicRefresh.Infra;
using HeavyMetalMachines.ReportSystem.DataTransferObjects;
using HeavyMetalMachines.ReportSystem.Infra;
using HeavyMetalMachines.Social.Teams.Models;
using HeavyMetalMachines.Tournaments;
using Hoplon.Serialization;
using UniRx;

namespace HeavyMetalMachines.SkipSwordfish
{
	public class SkipSwordfishPeriodicRefreshDataService : IPeriodicRefreshDataService
	{
		public SkipSwordfishPeriodicRefreshDataService()
		{
			this._fakeFeedbackHistory = new Dictionary<int, List<IPlayerFeedbackInfo>>();
			this._fakeRestrictionHistory = new Dictionary<DateTime, List<IRestriction>>();
			this.FillFakeFeedbacks();
			this.FillFakeRestrictions();
		}

		public IObservable<PeriodicRefreshData> Get()
		{
			return Observable.Select<Unit, PeriodicRefreshData>(Observable.ReturnUnit(), delegate(Unit _)
			{
				this._requestCount++;
				List<IPlayerFeedbackInfo> list;
				this._fakeFeedbackHistory.TryGetValue(this._requestCount, out list);
				if (list == null)
				{
					list = new List<IPlayerFeedbackInfo>();
				}
				List<IRestriction> list2 = null;
				foreach (KeyValuePair<DateTime, List<IRestriction>> keyValuePair in this._fakeRestrictionHistory)
				{
					if (!(DateTime.UtcNow > keyValuePair.Key))
					{
						list2 = keyValuePair.Value;
						break;
					}
				}
				if (list2 == null)
				{
					list2 = new List<IRestriction>();
				}
				return new PeriodicRefreshData
				{
					Team = new Team
					{
						Tag = "SKIP",
						Id = Guid.NewGuid(),
						IconName = "team_image_meme_02",
						Name = "SKIPPERS"
					},
					TournamentTeamStatus = new List<TournamentTeamStatus>(),
					Feedbacks = list,
					Restrictions = list2
				};
			});
		}

		private IPlayerFeedbackInfo FakeFeedback(long id, PlayerFeedbackKind kind, ReportMotive motive, float deltaStart, float deltaEnd = 0f)
		{
			SerializableFeedback serializableFeedback = new SerializableFeedback();
			serializableFeedback.Id = id;
			serializableFeedback.FeedbackType = PlayerFeedbackKindEx.ToInfraType(kind);
			switch (kind)
			{
			case 1:
				serializableFeedback.Bag = new WarningFeedbackBag
				{
					Motives = motive,
					CreateTime = (SerializedDateTime)(DateTime.UtcNow + TimeSpan.FromSeconds((double)deltaStart))
				}.Serialize();
				break;
			case 2:
				serializableFeedback.Bag = new RankedResetFeedbackBag
				{
					Motives = motive,
					CreateTime = (SerializedDateTime)(DateTime.UtcNow + TimeSpan.FromSeconds((double)deltaStart))
				}.Serialize();
				break;
			case 4:
			case 5:
			case 6:
				serializableFeedback.Bag = new RestrictionFeedbackBag
				{
					Motives = motive,
					Kind = PlayerFeedbackKindEx.ToRestriction(kind),
					RestrictionTimeStart = (SerializedDateTime)(DateTime.UtcNow + TimeSpan.FromSeconds((double)deltaStart)),
					RestrictionTimeEnd = (SerializedDateTime)(DateTime.UtcNow + TimeSpan.FromSeconds((double)deltaEnd))
				}.Serialize();
				break;
			}
			return new PlayerFeedbackInfo(serializableFeedback);
		}

		private void FillFakeFeedbacks()
		{
			this._fakeFeedbackHistory[1] = new List<IPlayerFeedbackInfo>
			{
				this.FakeFeedback(0L, 1, 16, 0f, 0f)
			};
		}

		private void FillFakeRestrictions()
		{
		}

		private Dictionary<int, List<IPlayerFeedbackInfo>> _fakeFeedbackHistory;

		private Dictionary<DateTime, List<IRestriction>> _fakeRestrictionHistory;

		private int _requestCount;
	}
}
