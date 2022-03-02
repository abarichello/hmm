using System;
using UniRx;

namespace HeavyMetalMachines.News.Infra
{
	public interface INewsGetRecommendationService
	{
		IObservable<NewsRecommendationItemJsonObject[]> GetRecommendationAsync();
	}
}
