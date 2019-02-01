using System;
using System.Collections;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Tutorial
{
	public class TutorialTriggerKreator : GameHubBehaviour
	{
		private void OnEnable()
		{
			if (this.NotFirstTwoSteps())
			{
				return;
			}
			this.CreateTutorialTrigger();
		}

		private bool NotFirstTwoSteps()
		{
			return string.IsNullOrEmpty(this.TutorialStep) || (!(this.TutorialStep == "InGameTuto0") && !(this.TutorialStep == "InGameTuto1"));
		}

		public void CreateTutorialTrigger()
		{
			Transform transform = null;
			if (!string.IsNullOrEmpty(this.ParentGameObjectName))
			{
				Transform[] componentsInChildren = base.GetComponentsInChildren<Transform>();
				if (componentsInChildren.Length > 0)
				{
					transform = Array.Find<Transform>(componentsInChildren, (Transform obj) => obj.name == this.ParentGameObjectName);
				}
			}
			else
			{
				transform = base.gameObject.transform;
			}
			if (transform == null)
			{
				return;
			}
			Transform[] componentsInChildren2 = transform.GetComponentsInChildren<Transform>();
			if (componentsInChildren2.Length < 0)
			{
				return;
			}
			Transform transform2 = Array.Find<Transform>(componentsInChildren2, (Transform obj) => obj.name == this.TargetGameObjectName);
			if (transform2 == null)
			{
				return;
			}
			this._tutorialTriggerCreated = transform2.gameObject.AddComponent<TutorialTrigger>();
			this._tutorialTriggerCreated.requiredTutorialName = this.RequiredTutorialStep;
			this._tutorialTriggerCreated.tutorialName = this.TutorialStep;
			this._tutorialTriggerCreated.nextTrigger = this.NextTutorialTrigger;
			this._tutorialTriggerCreated.LastStepToFinishThisObjective = this.LastStepToFinishThisObjective;
			if (this.FallbackTutorialTriggerKreator)
			{
				TutorialTriggerKreator[] components = this.FallbackTutorialTriggerKreator.GetComponents<TutorialTriggerKreator>();
				TutorialTriggerKreator tutorialTriggerKreator = Array.Find<TutorialTriggerKreator>(components, (TutorialTriggerKreator k) => k.TutorialStep == this.RequiredTutorialStep);
				tutorialTriggerKreator.SetNextTutorialTrigger(this._tutorialTriggerCreated);
			}
			if (this.FallbackTutorialTrigger)
			{
				TutorialTrigger[] components2 = this.FallbackTutorialTrigger.GetComponents<TutorialTrigger>();
				TutorialTrigger tutorialTrigger = Array.Find<TutorialTrigger>(components2, (TutorialTrigger t) => t.tutorialName == this.RequiredTutorialStep);
				tutorialTrigger.nextTrigger = this._tutorialTriggerCreated;
			}
			base.StartCoroutine(this.FocusOnTargetGameObject(transform.transform));
		}

		private IEnumerator FocusOnTargetGameObject(Transform targetTransform)
		{
			yield return this.waitHalfSecond;
			UICenterOnChild uiCenterOnChild = base.gameObject.GetComponent<UICenterOnChild>();
			if (uiCenterOnChild == null)
			{
				yield break;
			}
			Debug.Log(string.Format("Will center at:{0}", targetTransform.name));
			uiCenterOnChild.CenterOn(targetTransform);
			yield break;
		}

		public void SetNextTutorialTrigger(TutorialTrigger nextTutorialTrigger)
		{
			this._tutorialTriggerCreated.nextTrigger = nextTutorialTrigger;
		}

		[TutorialDataReference(false)]
		public string RequiredTutorialStep;

		[TutorialDataReference(false)]
		public string TutorialStep;

		[TutorialDataReference(false)]
		public string LastStepToFinishThisObjective;

		public string ParentGameObjectName;

		public string TargetGameObjectName;

		public TutorialTrigger FallbackTutorialTrigger;

		public TutorialTriggerKreator FallbackTutorialTriggerKreator;

		public TutorialTrigger NextTutorialTrigger;

		public TutorialTriggerKreator NextTutorialTriggerKreator;

		private TutorialTrigger _tutorialTriggerCreated;

		private WaitForSeconds waitHalfSecond = new WaitForSeconds(0.5f);
	}
}
