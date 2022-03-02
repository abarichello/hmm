using System;

namespace HeavyMetalMachines.News.Business
{
	public interface INewsHmmInvoker
	{
		void Add(NewsHmmSelectionType selectionType, Action<Guid> action);

		void Run(NewsHmmSelectionType selectionType, Guid itemTypeId);
	}
}
