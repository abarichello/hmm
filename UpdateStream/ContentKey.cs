using System;

namespace HeavyMetalMachines.UpdateStream
{
	public struct ContentKey
	{
		public bool Equals(ContentKey other)
		{
			return this.ObjId == other.ObjId && this.ClassId == other.ClassId;
		}

		public override int GetHashCode()
		{
			return this.ObjId * 397 ^ this.ClassId.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			return !object.ReferenceEquals(null, obj) && obj is ContentKey && this.Equals((ContentKey)obj);
		}

		public int ObjId;

		public byte ClassId;
	}
}
