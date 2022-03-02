using System;
using System.Diagnostics;
using HeavyMetalMachines.BI;
using Pocketverse;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Frontend
{
	[RequireComponent(typeof(Animator))]
	public class HudWindow : GameHubBehaviour, IHudWindow
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event HudWindow.OnVisibilityChangeListener OnVisibilityChange;

		public UIPanel Panel
		{
			get
			{
				if (this._panel == null)
				{
					this._panel = this.WindowGameObject.GetComponent<UIPanel>();
				}
				return this._panel;
			}
		}

		public virtual bool CanBeHiddenByEscKey()
		{
			return this.CanHideByEsc;
		}

		public void SetWindowVisibility(bool visible)
		{
			HudWindow.Log.DebugFormat("Set Window visibility to {0}. GameObjectName:{1}, name:{2}", new object[]
			{
				visible,
				base.gameObject.name,
				base.name
			});
			if (HudWindowManager.Instance == null)
			{
				HudWindow.Log.DebugFormat(string.Format("HudWindowManager instance is null while trying to Set \"{0}\" Window visibility", base.gameObject.name), new object[0]);
				return;
			}
			if (visible)
			{
				HudWindowManager.Instance.Push(this);
				return;
			}
			HudWindowManager.Instance.Remove(this);
		}

		public virtual void ChangeWindowVisibility(bool visible)
		{
			bool isVisible = this.IsVisible;
			this.IsVisible = visible;
			if (!this.HasEverBeenEnabled && visible)
			{
				this.HasEverBeenEnabled = true;
			}
			if (visible && !this.WindowGameObject.activeSelf)
			{
				this.SetWindowActive(true);
			}
			if (!visible && !this.WindowAnimator.GetBool("active"))
			{
				this.SetWindowActive(false);
			}
			this.WindowAnimator.SetBool("active", visible);
			if (isVisible != this.IsVisible && this.OnVisibilityChange != null)
			{
				this.OnVisibilityChange(this.IsVisible);
			}
		}

		public int GetDepth()
		{
			return (!(this.Panel == null)) ? this.Panel.depth : 0;
		}

		public virtual bool CanOpen()
		{
			return true;
		}

		public virtual bool IsStackableWithType(Type type)
		{
			return false;
		}

		public virtual void AnimationOnWindowExit()
		{
			if (!this.WindowAnimator.GetBool("active"))
			{
				this.SetWindowActive(false);
			}
		}

		private void SetWindowActive(bool bActive)
		{
			if (!this.EnableWindowActivation)
			{
				return;
			}
			this.WindowGameObject.SetActive(bActive);
		}

		public virtual bool IsWindowVisible()
		{
			return this.IsVisible;
		}

		public void ToggleVisibility()
		{
			this.SetWindowVisibility(!this.IsVisible);
		}

		public void HideFromUI()
		{
			this._buttonBILogger.LogButtonClick(ButtonName.EscMenuBack);
			this.SetWindowVisibility(false);
		}

		public void ShowFromUI()
		{
			this.SetWindowVisibility(true);
		}

		public virtual void OnDestroy()
		{
			if (!this._isQuitting)
			{
				this.SetWindowVisibility(false);
			}
		}

		public void OnApplicationQuit()
		{
			this._isQuitting = true;
		}

		[Inject]
		private IClientButtonBILogger _buttonBILogger;

		public bool IsModalWindow;

		private static readonly BitLogger Log = new BitLogger(typeof(HudWindow));

		protected const string WindowAnimatorActiveFieldName = "active";

		public const int InGameHudWindowGroup = 0;

		protected bool HasEverBeenEnabled;

		protected bool IsVisible;

		private UIPanel _panel;

		[SerializeField]
		protected Animator WindowAnimator;

		[Header("[Must be child from this gameObject]")]
		[SerializeField]
		protected GameObject WindowGameObject;

		[Header("[Auto window (de)activation on set visibility]")]
		[SerializeField]
		protected bool EnableWindowActivation = true;

		[Header("[Can be deactivated by pressing escape]")]
		[SerializeField]
		private bool CanHideByEsc;

		private bool _isQuitting;

		public delegate void OnVisibilityChangeListener(bool visible);
	}
}
