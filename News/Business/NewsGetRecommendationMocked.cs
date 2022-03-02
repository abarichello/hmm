using System;
using System.Collections.Generic;
using UniRx;

namespace HeavyMetalMachines.News.Business
{
	public class NewsGetRecommendationMocked : INewsGetRecommendation
	{
		public IObservable<List<NewsRecommendationItem>> GetRecommendationItems()
		{
			List<NewsRecommendationItem> list = new List<NewsRecommendationItem>
			{
				new NewsRecommendationItem
				{
					ImageUrl = "https://cdn2.heavymetalmachines.com/play/web/assets/play/pilots/t_blacklotus.png",
					LinkUrl = "https://heavymetalmachines.com/"
				},
				new NewsRecommendationItem
				{
					ImageUrl = "https://www.heavymetalmachines.com/blog/wp-content/uploads/2017/02/photon3-768x432.jpg",
					LinkUrl = "https://www.youtube.com/heavymetalmachines"
				}
			};
			return Observable.Return<List<NewsRecommendationItem>>(list);
		}
	}
}
