using System;
using System.Collections.Generic;
using HeavyMetalMachines.News.Infra;
using UniRx;

namespace HeavyMetalMachines.News.Business
{
	public class NewsGetRecommendation : INewsGetRecommendation
	{
		public NewsGetRecommendation(INewsGetRecommendationService getRecommendationService)
		{
			this._getRecommendationService = getRecommendationService;
		}

		public IObservable<List<NewsRecommendationItem>> GetRecommendationItems()
		{
			return Observable.Select<NewsRecommendationItemJsonObject[], List<NewsRecommendationItem>>(this._getRecommendationService.GetRecommendationAsync(), delegate(NewsRecommendationItemJsonObject[] newsRecommendationItemJsonObjects)
			{
				List<NewsRecommendationItem> list = new List<NewsRecommendationItem>(newsRecommendationItemJsonObjects.Length);
				for (int i = 0; i < newsRecommendationItemJsonObjects.Length; i++)
				{
					NewsRecommendationItem item = new NewsRecommendationItem
					{
						ImageUrl = newsRecommendationItemJsonObjects[i].image,
						LinkUrl = newsRecommendationItemJsonObjects[i].link
					};
					list.Add(item);
				}
				return list;
			});
		}

		private readonly INewsGetRecommendationService _getRecommendationService;
	}
}
