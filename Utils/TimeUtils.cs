using System;
using Pocketverse;

namespace HeavyMetalMachines.Utils
{
	public static class TimeUtils
	{
		public static string FormatTime(TimeSpan timeSpan)
		{
			int key = (int)timeSpan.TotalSeconds;
			string text;
			return (!TimeUtils.TimeStringCache.TryGetValue(key, out text)) ? TimeUtils.TimeStringCache.GenerateValue(key) : text;
		}

		public static TimeSpan GetTimeSpan(this TimeUtils.Chronometer timer)
		{
			return new TimeSpan(0, 0, 0, 0, timer.GetTime());
		}

		private const int InitialTimeStringCacheSize = 7200;

		public static readonly TimeStringCache TimeStringCache = new TimeStringCache(7200, true);

		[Serializable]
		public class Chronometer : IBitStreamSerializable
		{
			public Chronometer(Func<int> getTimeMillis)
			{
				this._getTime = getTimeMillis;
			}

			public void Start()
			{
				if (this._running)
				{
					return;
				}
				this._running = true;
				this._zero = this._getTime();
			}

			public void Stop()
			{
				if (!this._running)
				{
					return;
				}
				this._acc = this.GetTime();
				this._running = false;
				this._zero = 0;
			}

			public void Reset()
			{
				this._running = false;
				this._acc = 0;
				this._zero = 0;
			}

			public int GetTime()
			{
				if (!this._running)
				{
					return this._acc;
				}
				return this._getTime() - this._zero + this._acc;
			}

			public float GetTimeSeconds()
			{
				return (float)this.GetTime() * 0.001f;
			}

			public void WriteToBitStream(BitStream bs)
			{
				bs.WriteCompressedInt(this._acc);
				bs.WriteBool(this._running);
				if (this._running)
				{
					bs.WriteCompressedInt(this._zero);
				}
			}

			public void ReadFromBitStream(BitStream bs)
			{
				this._acc = bs.ReadCompressedInt();
				this._running = bs.ReadBool();
				this._zero = ((!this._running) ? 0 : bs.ReadCompressedInt());
			}

			private Func<int> _getTime;

			private int _acc;

			private int _zero;

			private bool _running;
		}
	}
}
