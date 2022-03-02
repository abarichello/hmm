using System;
using UnityEngine;

namespace HeavyMetalMachines.Presenting.NGui
{
	[Serializable]
	public class NGuiProgressBar : IProgressBar
	{
		public float FillPercent
		{
			get
			{
				return this._progressBar.value;
			}
			set
			{
				this._progressBar.value = value;
			}
		}

		[SerializeField]
		private UIProgressBar _progressBar;
	}
}
