using System;
using HeavyMetalMachines.Frontend;
using UnityEngine;

namespace HeavyMetalMachines.Presenting.Unity
{
	[Serializable]
	public class UnityTooltipLabel : ILabel, IValueHolder
	{
		public bool HasValue
		{
			get
			{
				return this._tooltipTrigger != null;
			}
		}

		public bool IsActive
		{
			get
			{
				return this._tooltipTrigger.enabled;
			}
			set
			{
				this._tooltipTrigger.enabled = value;
			}
		}

		public string Text
		{
			get
			{
				return this._tooltipTrigger.TooltipText;
			}
			set
			{
				this._tooltipTrigger.TooltipText = value;
			}
		}

		public Color Color
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public float Size
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		[SerializeField]
		private HMMUnityUiTooltipTrigger _tooltipTrigger;
	}
}
