using System;
using ClientAPI;
using ClientAPI.Service.Interfaces;
using ClientAPI.Utils;
using HeavyMetalMachines.Swordfish;
using UniRx;

namespace HeavyMetalMachines.News.Infra
{
	public class NewsGetRecommendationService : INewsGetRecommendationService
	{
		public NewsGetRecommendationService(UserInfo userInfo, RegionController regionController, IRecommendation recommendation)
		{
			this._userInfo = userInfo;
			this._regionController = regionController;
			this._recommendation = recommendation;
		}

		public IObservable<NewsRecommendationItemJsonObject[]> GetRecommendationAsync()
		{
			return Observable.Select<string, NewsRecommendationItemJsonObject[]>(SwordfishObservable.FromSwordfishCall<string>(delegate(SwordfishClientApi.ParameterizedCallback<string> success, SwordfishClientApi.ErrorCallback error)
			{
				this._recommendation.GiveMeARecommendationFor(null, this._userInfo.UniversalId, Language.CurrentLanguage.ToString(), this._regionController.GetBestServerSaved().Region.RegionNameI18N, "mkt.url", success, error);
			}), delegate(string recommendationJsonResult)
			{
				if (string.IsNullOrEmpty(recommendationJsonResult))
				{
					throw new NullReferenceException("Null recommendation Json result");
				}
				return Json.ToObject<NewsRecommendationItemJsonObject[]>(recommendationJsonResult);
			});
		}

		private readonly UserInfo _userInfo;

		private readonly RegionController _regionController;

		private readonly IRecommendation _recommendation;
	}
}
