using System;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using UnityEngine;

namespace HeavymetalMachines.ReportSystem
{
	public class UnityReportSystemToggleView : MonoBehaviour, IReportSystemToggleView
	{
		public IToggle ReportToggle
		{
			get
			{
				return this._reportToggle;
			}
		}

		public ILabel ReportToggleLabel
		{
			get
			{
				return this._reportToggleLabel;
			}
		}

		[SerializeField]
		private UnityToggle _reportToggle;

		[SerializeField]
		private UnityLabel _reportToggleLabel;
	}
}
