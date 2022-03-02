using System;
using System.Collections.Generic;

namespace HeavyMetalMachines.News.Business
{
	public class NewsHmmInvoker : INewsHmmInvoker
	{
		public NewsHmmInvoker()
		{
			this._newsHmmInvoker = new Dictionary<NewsHmmSelectionType, Action<Guid>>();
		}

		public void Add(NewsHmmSelectionType selectionType, Action<Guid> action)
		{
			this._newsHmmInvoker.Add(selectionType, action);
		}

		public void Run(NewsHmmSelectionType selectionType, Guid itemTypeId)
		{
			if (!this._newsHmmInvoker.ContainsKey(selectionType))
			{
				return;
			}
			this._newsHmmInvoker[selectionType](itemTypeId);
		}

		private readonly Dictionary<NewsHmmSelectionType, Action<Guid>> _newsHmmInvoker;
	}
}
