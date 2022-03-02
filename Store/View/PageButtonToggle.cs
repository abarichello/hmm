using System;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.NGui;
using UnityEngine;

namespace HeavyMetalMachines.Store.View
{
	public class PageButtonToggle : MonoBehaviour, IPageButtonToggle
	{
		public IButton PageButton
		{
			get
			{
				return this._button;
			}
		}

		public int PageIndex { get; set; }

		public ILabel PageLabel
		{
			get
			{
				return this._pageNumber;
			}
		}

		public IToggle Toggle
		{
			get
			{
				return this._toggle;
			}
		}

		public void SetActive(bool active)
		{
			base.gameObject.SetActive(active);
		}

		[SerializeField]
		private NGuiLabel _pageNumber;

		[SerializeField]
		private NguiToggle _toggle;

		[SerializeField]
		private NGuiButton _button;
	}
}
