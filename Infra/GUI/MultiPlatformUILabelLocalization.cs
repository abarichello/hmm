using System;
using Standard_Assets.Scripts.HMM.Util;
using UnityEngine;

namespace HeavyMetalMachines.Infra.GUI
{
	public class MultiPlatformUILabelLocalization : AbstractUILabelLocalization
	{
		public override string Draft
		{
			get
			{
				return this._draft.CurrentPlatformDraft;
			}
		}

		[SerializeField]
		private MultiPlatformLocalizationDraft _draft;
	}
}
