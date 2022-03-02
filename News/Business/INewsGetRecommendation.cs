using System;
using System.Collections.Generic;
using UniRx;

namespace HeavyMetalMachines.News.Business
{
	public interface INewsGetRecommendation
	{
		IObservable<List<NewsRecommendationItem>> GetRecommendationItems();
	}
}
