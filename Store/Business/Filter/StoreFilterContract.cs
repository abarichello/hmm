using System;
using System.Collections.Generic;

namespace HeavyMetalMachines.Store.Business.Filter
{
	public class StoreFilterContract : IStoreFilterContract
	{
		public StoreFilterContract(bool isOverflow, List<StoreFilterType> filterTypes)
		{
			this._filterTypes = filterTypes;
			this._isOverflow = isOverflow;
		}

		public bool IsOverflow
		{
			get
			{
				return this._isOverflow;
			}
		}

		public List<StoreFilterType> FilterTypes
		{
			get
			{
				return this._filterTypes;
			}
		}

		public int GetStoreFilterTypeIndex(StoreFilterType filterType)
		{
			return this._filterTypes.IndexOf(filterType);
		}

		private readonly bool _isOverflow;

		private readonly List<StoreFilterType> _filterTypes;
	}
}
