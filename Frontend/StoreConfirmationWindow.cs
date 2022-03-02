using System;
using System.Collections;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.VFX;
using Hoplon.Input.UiNavigation;
using Hoplon.ToggleableFeatures;
using UniRx;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class StoreConfirmationWindow : ModalGUIController
	{
		private UiNavigationGroupHolder UiNavigationGroupHolder
		{
			get
			{
				return this._uiNavigationGroupHolder;
			}
		}

		protected override void Start()
		{
			base.Start();
			this._inputCancelDownDisposable = ObservableExtensions.Subscribe<Unit>(this.UiNavigationGroupHolder.ObserveInputCancelDown(), delegate(Unit _)
			{
				this.OnCancel();
			});
		}

		protected override void InitDialogTasks()
		{
		}

		protected override IEnumerator ResolveModalWindowTasks()
		{
			yield break;
		}

		public void ShowConfirmationMessage(string message, string confirm, string cancel, Action confirmationCallbackm)
		{
			base.gameObject.SetActive(true);
			this.buyGroup.SetActive(false);
			this.confirmationGroup.SetActive(true);
			this.confirmationYesButton.transform.parent.gameObject.SetActive(true);
			this.confirmationCancelButton.transform.parent.gameObject.SetActive(true);
			this.confirmationOkButton.transform.parent.gameObject.SetActive(false);
			this.messageLabel.text = message;
			this.confirmationYesButton.text = confirm;
			this.confirmationCancelButton.text = cancel;
			this.callback = confirmationCallbackm;
			this.UiNavigationGroupHolder.AddHighPriorityGroup();
		}

		public void ShowBoughtMessage(string message, string confirm, string cancel, Action confirmationCallbackm)
		{
			base.gameObject.SetActive(true);
			this.buyGroup.SetActive(true);
			this.confirmationGroup.SetActive(false);
			this.messageLabel.text = message;
			this.buyYesButton.text = confirm;
			this.buyCancelButton.text = cancel;
			this.callback = confirmationCallbackm;
			this.UiNavigationGroupHolder.AddHighPriorityGroup();
		}

		public void ShowOkConfirmationMessage(string message, string ok, Action confirmationCallbackm)
		{
			base.gameObject.SetActive(true);
			this.buyGroup.SetActive(false);
			this.confirmationGroup.SetActive(true);
			this.confirmationYesButton.transform.parent.gameObject.SetActive(false);
			this.confirmationCancelButton.transform.parent.gameObject.SetActive(false);
			this.confirmationOkButton.transform.parent.gameObject.SetActive(true);
			this.messageLabel.text = message;
			this.confirmationOkButton.text = ok;
			this.callback = confirmationCallbackm;
			this.UiNavigationGroupHolder.AddHighPriorityGroup();
		}

		private void OnConfirm()
		{
			if (this.callback != null)
			{
				this.callback();
			}
			base.ResolveModalWindow();
			this.UiNavigationGroupHolder.RemoveHighPriorityGroup();
		}

		private void OnCancel()
		{
			base.ResolveModalWindow();
			this.UiNavigationGroupHolder.RemoveHighPriorityGroup();
		}

		public override bool IsStackableWithType(Type type)
		{
			return false;
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (this._inputCancelDownDisposable != null)
			{
				this._inputCancelDownDisposable.Dispose();
				this._inputCancelDownDisposable = null;
			}
		}

		public UILabel messageLabel;

		public GameObject buyGroup;

		public GameObject confirmationGroup;

		public UILabel confirmationYesButton;

		public UILabel confirmationCancelButton;

		public UILabel confirmationOkButton;

		public UILabel buyYesButton;

		public UILabel buyCancelButton;

		[Header("[Ui Navigation]")]
		[SerializeField]
		private UiNavigationGroupHolder _uiNavigationGroupHolder;

		private Action callback;

		[InjectOnClient]
		private IIsFeatureToggled _isFeatureToggled;

		private IDisposable _inputCancelDownDisposable;
	}
}
