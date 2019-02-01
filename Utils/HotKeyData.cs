using System;
using UnityEngine;

namespace HeavyMetalMachines.Utils
{
	public class HotKeyData
	{
		public bool IsPressed()
		{
			return InputUtils.IsPressed(this.LCtrl, this.LAlt, this.LShift, this.LWin, this.Key);
		}

		public static bool operator ==(HotKeyData one, HotKeyData other)
		{
			if (one == null)
			{
				return other == null;
			}
			return other != null && (one.Key == other.Key && one.LCtrl == other.LCtrl && one.LShift == other.LShift && one.LAlt == other.LAlt) && one.LWin == other.LWin;
		}

		public static bool operator !=(HotKeyData one, HotKeyData other)
		{
			return !(one == other);
		}

		public override string ToString()
		{
			return string.Concat(new object[]
			{
				"(",
				(!this.LCtrl) ? string.Empty : "LCtrl+",
				(!this.LShift) ? string.Empty : "LShift+",
				(!this.LAlt) ? string.Empty : "LAlt+",
				(!this.LWin) ? string.Empty : "LWin+",
				this.Key,
				")"
			});
		}

		protected bool Equals(HotKeyData other)
		{
			return this.LCtrl.Equals(other.LCtrl) && this.LShift.Equals(other.LShift) && this.LAlt.Equals(other.LAlt) && this.LWin.Equals(other.LWin) && this.Key == other.Key;
		}

		public override bool Equals(object obj)
		{
			return !object.ReferenceEquals(null, obj) && (object.ReferenceEquals(this, obj) || (obj.GetType() == base.GetType() && this.Equals((HotKeyData)obj)));
		}

		public override int GetHashCode()
		{
			int num = this.LCtrl.GetHashCode();
			num = (num * 397 ^ this.LShift.GetHashCode());
			num = (num * 397 ^ this.LAlt.GetHashCode());
			num = (num * 397 ^ this.LWin.GetHashCode());
			return num * 397 ^ this.Key.GetHashCode();
		}

		public bool LCtrl;

		public bool LShift;

		public bool LAlt;

		public bool LWin;

		public KeyCode Key;
	}
}
