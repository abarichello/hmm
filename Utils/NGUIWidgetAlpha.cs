using System;
using System.Collections.Generic;
using HeavyMetalMachines.Presenting;
using UnityEngine;

namespace HeavyMetalMachines.Utils
{
	[ExecuteInEditMode]
	public class NGUIWidgetAlpha : MonoBehaviour, IAlpha
	{
		private void OnDidApplyAnimationProperties()
		{
			this.UpdateAlphaValues();
		}

		private void UpdateAlphaValues()
		{
			if (this.widget != null)
			{
				this.widget.alpha = this.alpha;
			}
			for (int i = 0; i < this._extraUIRects.Count; i++)
			{
				UIRect uirect = this._extraUIRects[i];
				if (!(uirect == null))
				{
					if (uirect is UIPanel)
					{
						UIPanel uipanel = (UIPanel)uirect;
						uipanel.alpha = this.alpha;
						uipanel.UpdateSelf();
					}
					else
					{
						uirect.alpha = this.alpha;
					}
				}
			}
		}

		public void OnEnable()
		{
			if (this._animateChildPanels)
			{
				if (this._childPanels == null)
				{
					this._childPanels = base.GetComponentsInChildren<UIPanel>();
				}
				for (int i = 0; i < this._childPanels.Length; i++)
				{
					UIPanel uipanel = this._childPanels[i];
					uipanel.alpha = this.alpha;
					if (!this._extraUIRects.Contains(uipanel))
					{
						this._extraUIRects.Add(uipanel);
					}
				}
			}
			this.UpdateAlphaValues();
		}

		public float Alpha
		{
			get
			{
				return this.alpha;
			}
			set
			{
				this.alpha = value;
				this.UpdateAlphaValues();
			}
		}

		public float alpha = 1f;

		public UIWidget widget;

		[SerializeField]
		private bool _animateChildPanels;

		[Header("Useful for animating extra UIRects, such as child panels")]
		[SerializeField]
		private List<UIRect> _extraUIRects;

		private UIPanel[] _childPanels;
	}
}
