using System;

namespace HeavyMetalMachines.News.Business
{
	public interface INewsLinkSelector
	{
		void Execute(string link, NewsCardBiPosition biPosition);
	}
}
