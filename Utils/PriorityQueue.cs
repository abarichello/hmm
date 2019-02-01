using System;
using System.Collections.Generic;

namespace HeavyMetalMachines.Utils
{
	public class PriorityQueue<TKey, TData> where TKey : IComparable
	{
		public PriorityQueue(int capacity = 128)
		{
			if (capacity <= 0)
			{
				throw new ArgumentException("Capacity must be greater than zero", "capacity");
			}
			this._elementArray = new PriorityQueue<TKey, TData>.Element[capacity];
			this._indexDict = new Dictionary<TData, uint>(capacity);
		}

		public bool IsEmpty
		{
			get
			{
				return this._heapSize == 0u;
			}
		}

		public bool IsFull
		{
			get
			{
				return (ulong)this._heapSize == (ulong)((long)this._elementArray.Length);
			}
		}

		public int Size
		{
			get
			{
				return (int)this._heapSize;
			}
		}

		public int Capacity
		{
			get
			{
				return this._elementArray.Length;
			}
		}

		~PriorityQueue()
		{
			this.Clear();
		}

		public void Push(TData data, TKey priority)
		{
			if (this.IsFull)
			{
				throw new InsufficientMemoryException("Queue is at full capacity");
			}
			if (this._indexDict.ContainsKey(data))
			{
				throw new ArgumentException("Queue does not support duplicated data");
			}
			uint num = this._heapSize++;
			this._elementArray[(int)((UIntPtr)num)].Key = priority;
			this._elementArray[(int)((UIntPtr)num)].Data = data;
			this._indexDict.Add(data, num);
			this.SiftUp(num);
		}

		public TData Pop()
		{
			if (this.IsEmpty)
			{
				throw new AccessViolationException("Queue is empty");
			}
			TData data = this._elementArray[0].Data;
			this._indexDict.Remove(data);
			this._elementArray[0] = this._elementArray[(int)((UIntPtr)(this._heapSize -= 1u))];
			this._elementArray[(int)((UIntPtr)this._heapSize)] = default(PriorityQueue<TKey, TData>.Element);
			this.SiftDown(0u);
			return data;
		}

		public TData Peek()
		{
			if (this.IsEmpty)
			{
				throw new AccessViolationException("Queue is empty");
			}
			return this._elementArray[0].Data;
		}

		public void SetPriority(TData data, TKey priority)
		{
			uint num;
			if (!this._indexDict.TryGetValue(data, out num))
			{
				throw new ArgumentException("Data not found in queue", "data");
			}
			int num2 = this._elementArray[(int)((UIntPtr)num)].Key.CompareTo(priority);
			this._elementArray[(int)((UIntPtr)num)].Key = priority;
			if (num2 > 0)
			{
				this.SiftUp(num);
			}
			else if (num2 < 0)
			{
				this.SiftDown(num);
			}
		}

		public TKey GetPriority(TData data)
		{
			uint num;
			if (!this._indexDict.TryGetValue(data, out num))
			{
				throw new ArgumentException("Data not found in queue", "data");
			}
			return this._elementArray[(int)((UIntPtr)num)].Key;
		}

		public bool Contains(TData data)
		{
			return this._indexDict.ContainsKey(data);
		}

		public void Clear()
		{
			for (uint num = 0u; num < this._heapSize; num += 1u)
			{
				this._elementArray[(int)((UIntPtr)num)] = default(PriorityQueue<TKey, TData>.Element);
			}
			this._heapSize = 0u;
			this._indexDict.Clear();
		}

		protected void SiftUp(uint index)
		{
			while (index != 0u)
			{
				uint num = index - 1u >> 1;
				if (this._elementArray[(int)((UIntPtr)num)].Key.CompareTo(this._elementArray[(int)((UIntPtr)index)].Key) <= 0)
				{
					break;
				}
				this.SwapElements(index, num);
				index = num;
			}
		}

		protected void SiftDown(uint index)
		{
			for (;;)
			{
				uint num = (index << 1) + 1u;
				uint num2 = num + 1u;
				uint num3 = index;
				if (num < this._heapSize && this._elementArray[(int)((UIntPtr)num)].Key.CompareTo(this._elementArray[(int)((UIntPtr)index)].Key) < 0)
				{
					num3 = num;
				}
				if (num2 < this._heapSize && this._elementArray[(int)((UIntPtr)num2)].Key.CompareTo(this._elementArray[(int)((UIntPtr)num3)].Key) < 0)
				{
					num3 = num2;
				}
				if (num3 == index)
				{
					break;
				}
				this.SwapElements(index, num3);
				index = num3;
			}
		}

		private void SwapElements(uint i, uint j)
		{
			this._indexDict[this._elementArray[(int)((UIntPtr)j)].Data] = i;
			this._indexDict[this._elementArray[(int)((UIntPtr)i)].Data] = j;
			PriorityQueue<TKey, TData>.Element element = this._elementArray[(int)((UIntPtr)i)];
			this._elementArray[(int)((UIntPtr)i)] = this._elementArray[(int)((UIntPtr)j)];
			this._elementArray[(int)((UIntPtr)j)] = element;
		}

		private readonly PriorityQueue<TKey, TData>.Element[] _elementArray;

		private readonly Dictionary<TData, uint> _indexDict;

		private uint _heapSize;

		private struct Element
		{
			public TKey Key;

			public TData Data;
		}
	}
}
