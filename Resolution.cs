using System;

namespace HeavyMetalMachines
{
	public struct Resolution : IComparable<Resolution>
	{
		public Resolution(int width, int height)
		{
			this.Width = width;
			this.Height = height;
		}

		public int CompareTo(Resolution other)
		{
			int num = this.Width - other.Width;
			return (num != 0) ? num : (this.Height - other.Height);
		}

		public float AspectRatio()
		{
			return (float)this.Width / (float)this.Height;
		}

		public override string ToString()
		{
			return string.Format("{0}x{1}", this.Width, this.Height);
		}

		public int Width;

		public int Height;
	}
}
