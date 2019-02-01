using System;

namespace HeavyMetalMachines.Utils
{
	public class LocklessQueue<T> where T : class, IDisposable, new()
	{
		public LocklessQueue(int capacity)
		{
			this._capacity = capacity;
			this._queue = new T[capacity];
			this._used = new bool[capacity];
			for (int i = 0; i < capacity; i++)
			{
				this._queue[i] = Activator.CreateInstance<T>();
				this._used[i] = false;
			}
		}

		public T StartRead()
		{
			if (!this._used[this._readIdx])
			{
				return (T)((object)null);
			}
			return this._queue[this._readIdx];
		}

		public void EndRead()
		{
			if (!this._used[this._readIdx])
			{
				return;
			}
			int readIdx = this._readIdx;
			this._readIdx = (this._readIdx + 1) % this._capacity;
			this._queue[readIdx].Dispose();
			this._used[readIdx] = false;
		}

		public T StartWrite()
		{
			if (this._used[this._writeIdx])
			{
				throw new IndexOutOfRangeException("Queue capacity overflown!");
			}
			return this._queue[this._writeIdx];
		}

		public void EndWrite()
		{
			int writeIdx = this._writeIdx;
			this._writeIdx = (this._writeIdx + 1) % this._capacity;
			this._used[writeIdx] = true;
		}

		public void DiscardWrite()
		{
			this._queue[this._writeIdx].Dispose();
			this._used[this._writeIdx] = false;
		}

		private int _capacity;

		private int _writeIdx;

		private int _readIdx;

		private T[] _queue;

		private bool[] _used;
	}
}
