using System;
using System.Collections;
using HeavyMetalMachines.VFX;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class StoreConfirmationWindow : ModalGUIController
	{
		protected override void InitDialogTasks()
		{
		}

		protected override IEnumerator ResolveModalWindowTasks()
		{
			yield break;
		}

		protected override void Update()
		{
			if (base.gameObject.activeSelf && Input.GetKeyDown(KeyCode.Escape))
			{
				this.OnCancel();
			}
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
		}

		private void OnConfirm()
		{
			if (this.callback != null)
			{
				this.callback();
			}
			base.ResolveModalWindow();
		}

		private void OnCancel()
		{
			base.ResolveModalWindow();
		}

		public override bool IsStackableWithType(Type type)
		{
			return false;
		}

		public UILabel messageLabel;

		public GameObject buyGroup;

		public GameObject confirmationGroup;

		public UILabel confirmationYesButton;

		public UILabel confirmationCancelButton;

		public UILabel confirmationOkButton;

		public UILabel buyYesButton;

		public UILabel buyCancelButton;

		private Action callback;
	}
}
