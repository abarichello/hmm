using System;
using HeavyMetalMachines.Infra.GUI;
using UnityEngine;
using UnityEngine.Serialization;

namespace HeavyMetalMachines.VFX
{
	public class UILabelLocalization : AbstractUILabelLocalization
	{
		public void SetDraft(string newDraft)
		{
			this._draft = newDraft;
		}

		public override string Draft
		{
			get
			{
				return this._draft;
			}
		}

		[SerializeField]
		[FormerlySerializedAs("draft")]
		private string _draft;
	}
}
