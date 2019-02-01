using System;
using HeavyMetalMachines.Tutorial.InGame;
using UnityEngine;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	[Obsolete]
	[ExecuteInEditMode]
	public class HudMaskTutorialBehaviour : InGameTutorialBehaviourBase
	{
		private void Awake()
		{
			TutorialPathNode component = base.gameObject.GetComponent<TutorialPathNode>();
			this._finishOnBehaviourComplete = (component != null);
			if (this.maskTrigger == null)
			{
				this.maskTrigger = InGameTutorialMaskController.Instance.FindTriggerForBehaviour(this);
			}
		}

		protected override void StartBehaviourOnClient()
		{
			base.StartBehaviourOnClient();
			TutorialUIController.screenCollider.enabled = false;
			if (this.maskTrigger == null)
			{
				this.maskTrigger = InGameTutorialMaskController.Instance.FindTriggerForBehaviour(this);
			}
			this.maskTrigger.ConfigureMaskBasedOnBehaviour(this);
			InGameTutorialMaskController.Instance.StartStep(this);
		}

		public void MaskTriggered()
		{
			Debug.Log("MaskTriggered");
			this.CompleteBehaviourAndSync();
		}

		protected override void OnStepCompletedOnClient()
		{
			base.OnStepCompletedOnClient();
			if (!this._finishOnBehaviourComplete)
			{
				TutorialUIController.screenCollider.enabled = true;
				InGameTutorialMaskController.Instance.StepCompleted();
			}
		}

		public override void CompleteBehaviour()
		{
			base.CompleteBehaviour();
			if (this._finishOnBehaviourComplete)
			{
				TutorialUIController.screenCollider.enabled = true;
				InGameTutorialMaskController.Instance.StepCompleted();
			}
		}

		public int Step;

		[SerializeField]
		public InGameTutorialMaskColliderTrigger maskTrigger;

		public float Delay;

		public float AnimateMaskDelay;

		public float Duration = 1f;

		public string RootObjectTargetAnchor;

		public string TargetAnchorPath;

		public Vector2 relativeOffset;

		public Vector2 pixelOffset = new Vector2(200f, 200f);

		public int width = 40;

		public int height = 40;

		public InGameTutorialMaskColliderTrigger.ShapeTypes shapeType;

		public InGameTutorialMaskColliderTrigger.ColliderSizeUpdate colliderSizeUpdate = InGameTutorialMaskColliderTrigger.ColliderSizeUpdate.TwoThirdsMaskSize;

		public Vector3 colliderCenter;

		[HideInInspector]
		public Vector3 colliderSize;

		public string GlowFeedbackName;

		private bool _finishOnBehaviourComplete;
	}
}
