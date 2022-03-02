using System;
using System.Collections;
using HeavyMetalMachines.Localization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HeavyMetalMachines.Frontend
{
	[RequireComponent(typeof(Selectable))]
	public class HMMUnityUiTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, IEventSystemHandler
	{
		private void Reset()
		{
			this._selectable = base.GetComponent<Selectable>();
		}

		private void OnDisable()
		{
			this.StopScheduledShowIfNecessary();
			UITooltip.Hide();
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			this.ShowDelayed();
		}

		public void OnSelect(BaseEventData eventData)
		{
			this.ShowDelayed();
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			this.StopScheduledShowIfNecessary();
			UITooltip.Hide();
		}

		public void OnDeselect(BaseEventData eventData)
		{
			this.StopScheduledShowIfNecessary();
			UITooltip.Hide();
		}

		private void StopScheduledShowIfNecessary()
		{
			if (this._scheduledCoroutine != null)
			{
				base.StopCoroutine(this._scheduledCoroutine);
			}
		}

		private void ShowDelayed()
		{
			if (!this.ShouldShow())
			{
				return;
			}
			this.StopScheduledShowIfNecessary();
			this._scheduledCoroutine = base.StartCoroutine(this.Coroutine_ShowDelayed());
		}

		private IEnumerator Coroutine_ShowDelayed()
		{
			yield return new WaitForSeconds(this.DelayInSec);
			this.Show();
			yield break;
		}

		private void Show()
		{
			Vector3 targetPosition = (!(this.TargetPosition != null)) ? Vector3.zero : this.TargetPosition.position;
			string text = (!this.TranslateText) ? this.TooltipText : Language.Get(this.TooltipText, this.Sheet);
			UITooltip.Show(text, this.FollowMouse, this.ScaleTextInOverflow, targetPosition);
		}

		private bool ShouldShow()
		{
			bool flag = this._selectable.IsInteractable();
			switch (this.IteractableStateToShow)
			{
			case HMMUnityUiTooltipTrigger.ShowOnInteractableState.Enabled:
				return flag.Equals(true);
			case HMMUnityUiTooltipTrigger.ShowOnInteractableState.Disabled:
				return flag.Equals(false);
			case HMMUnityUiTooltipTrigger.ShowOnInteractableState.Any:
				return true;
			default:
				throw new InvalidOperationException(string.Format("UnityGUITooltip ShowInteractableState enum value \"{0}\" unhandled.", this.IteractableStateToShow));
			}
		}

		[Header("Dependencies")]
		[SerializeField]
		private Selectable _selectable;

		[Header("Settings")]
		public string TooltipText;

		public TranslationSheets Sheet;

		public bool TranslateText;

		public float DelayInSec;

		public bool ScaleTextInOverflow;

		public HMMUnityUiTooltipTrigger.ShowOnInteractableState IteractableStateToShow;

		public bool FollowMouse;

		public Transform TargetPosition;

		private Coroutine _scheduledCoroutine;

		public enum ShowOnInteractableState
		{
			Enabled,
			Disabled,
			Any
		}
	}
}
