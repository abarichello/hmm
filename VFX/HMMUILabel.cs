using System;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	[ExecuteInEditMode]
	public class HMMUILabel : UILabel
	{
		public override string text
		{
			get
			{
				return base.text;
			}
			set
			{
				base.text = value;
				base.TryUpdateText();
				if (base.overflowMethod != UILabel.Overflow.ResizeHeight && base.overflowMethod != UILabel.Overflow.ShrinkContent)
				{
					return;
				}
				if (this._maxLineCountCache == -1)
				{
					this._maxLineCountCache = base.maxLineCount;
				}
				base.overflowMethod = UILabel.Overflow.ResizeHeight;
				base.maxLineCount = this._maxLineCountCache;
				bool flag = base.processedText.Length != value.Length;
				if (flag)
				{
					base.maxLineCount = 0;
					base.overflowMethod = UILabel.Overflow.ShrinkContent;
					base.text = value;
				}
			}
		}

		private int _maxLineCountCache = -1;
	}
}
