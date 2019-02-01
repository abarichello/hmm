﻿using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class ShopScreen : GameHubBehaviour
	{
		public int CurrentPage
		{
			get
			{
				return this._currentPage;
			}
			set
			{
				this._currentPage = value;
			}
		}

		public int TotalNumberofPages
		{
			get
			{
				return this._totalNumberofPages;
			}
			set
			{
				this._totalNumberofPages = value;
			}
		}

		protected void SetupPageToggleControllers(int pTotalNumberOfPages)
		{
			if (this.PageToggleControllers != null && this.PageToggleControllers.Length > 0)
			{
				for (int i = 0; i < this.PageToggleControllers.Length; i++)
				{
					if (this.PageToggleControllers[i].Visible)
					{
						this.PageControllersGridPivot.RemoveChild(this.PageToggleControllers[i].transform);
						NGUITools.Destroy(this.PageToggleControllers[i].transform);
					}
				}
			}
			this.PageControllersGridPivot.Reposition();
			this.TotalNumberofPages = pTotalNumberOfPages;
			this.PageToggleControllers = new ShopDriverPageToggle[this.TotalNumberofPages];
			for (int j = 0; j < this.TotalNumberofPages; j++)
			{
				ShopDriverPageToggle component = UnityEngine.Object.Instantiate<GameObject>(this.PageTogglePrefab).GetComponent<ShopDriverPageToggle>();
				component.Visible = true;
				component.PageNumber.text = j + 1 + string.Empty;
				component.transform.parent = this.PageControllersGridPivot.transform;
				component.transform.localScale = this.PageControllersGridPivot.transform.localScale;
				component.Eventlistener.TheParameterKind = GUIEventListener.ParameterKind.Integer;
				component.Eventlistener.MethodName = "ShowPage";
				component.Eventlistener.IntParameter = j;
				this.PageToggleControllers[j] = component;
				component.gameObject.name = "page_" + j.ToString("000");
				component.gameObject.SetActive(true);
				this.PageControllersGridPivot.AddChild(component.transform);
			}
			this.NextButton.gameObject.SetActive(this.TotalNumberofPages > 1);
			this.PreviousButton.gameObject.SetActive(this.TotalNumberofPages > 1);
			this.PageControllersGridPivot.Reposition();
		}

		protected void UpdateTotalNumberofPages(int pTotalNumberOfPages)
		{
			this.TotalNumberofPages = pTotalNumberOfPages;
			for (int i = 0; i < this.PageToggleControllers.Length; i++)
			{
				ShopDriverPageToggle shopDriverPageToggle = this.PageToggleControllers[i];
				shopDriverPageToggle.Toggle.value = false;
				shopDriverPageToggle.Toggle.Set(false, true);
				shopDriverPageToggle.gameObject.SetActive(false);
			}
			for (int j = 0; j < this.TotalNumberofPages; j++)
			{
				this.PageToggleControllers[j].gameObject.SetActive(true);
				this.PageToggleControllers[j].GetComponent<Collider>().enabled = true;
			}
			this.PageToggleControllers[this.CurrentPage].Toggle.value = true;
			this.PageToggleControllers[this.CurrentPage].GetComponent<Collider>().enabled = false;
		}

		public void UpdatePageButtonControllers()
		{
			if (this.TotalNumberofPages == 0)
			{
				this.PreviousButton.isEnabled = false;
				this.NextButton.isEnabled = false;
				return;
			}
			if (this._currentPage == 0)
			{
				this.PreviousButton.isEnabled = false;
			}
			else
			{
				this.PreviousButton.isEnabled = true;
			}
			if (this._currentPage == this._totalNumberofPages - 1)
			{
				this.NextButton.isEnabled = false;
			}
			else
			{
				this.NextButton.isEnabled = true;
			}
		}

		public virtual void Setup()
		{
			base.gameObject.SetActive(true);
		}

		public virtual void CleanUp()
		{
		}

		public virtual void Hide()
		{
			this.ShopAnimator.SetBool("show", false);
		}

		public virtual void Show()
		{
			this.Setup();
			this.ShopAnimator.SetBool("show", true);
		}

		public virtual bool IsVisible()
		{
			return this.ShopAnimator.GetBool("show");
		}

		public virtual void AnimationAlphaZero()
		{
		}

		[Header("EXTERNAL")]
		public ShopGUI Shop;

		public UIButton PreviousButton;

		public UIButton NextButton;

		public Animator ShopAnimator;

		public GameObject PageTogglePrefab;

		public UIGrid PageControllersGridPivot;

		public ShopDriverPageToggle[] PageToggleControllers;

		private int _totalNumberofPages;

		private int _currentPage;
	}
}
