using System;
using System.Collections;
using System.Collections.Generic;
using HeavyMetalMachines.Tutorial.Behaviours;
using Holoville.HOTween;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Tutorial.InGame
{
	[ExecuteInEditMode]
	[Obsolete]
	public class InGameTutorialMaskController : GameHubBehaviour
	{
		public static InGameTutorialMaskController Instance
		{
			get
			{
				if (InGameTutorialMaskController._instance == null)
				{
					InGameTutorialMaskController._instance = Object.FindObjectOfType<InGameTutorialMaskController>();
				}
				return InGameTutorialMaskController._instance;
			}
		}

		private void Start()
		{
			this._uiPanel = base.GetComponent<UIPanel>();
			if (!Application.isPlaying || (GameHubBehaviour.Hub && GameHubBehaviour.Hub.Net.IsServer()))
			{
				return;
			}
			base.StartCoroutine(this.HideOverlay(0f));
			this.CheckMasks();
			this.HideAllMasks();
			this.DisableCollider();
			this.maskFill.SetActive(true);
			this.SetupOverlay();
		}

		private void CheckMasks()
		{
			this._masks = new List<InGameTutorialMaskColliderTrigger>(base.GetComponentsInChildren<InGameTutorialMaskColliderTrigger>(true));
		}

		public void PreviewStep(HudMaskTutorialBehaviour hudMaskRef)
		{
			this.CheckMasks();
			for (int i = 0; i < this._masks.Count; i++)
			{
				InGameTutorialMaskColliderTrigger inGameTutorialMaskColliderTrigger = this._masks[i];
				if (!(inGameTutorialMaskColliderTrigger == null))
				{
					if (inGameTutorialMaskColliderTrigger == hudMaskRef.maskTrigger)
					{
						Debug.Log("PreviewStep set active: " + hudMaskRef.maskTrigger.gameObject.name);
						inGameTutorialMaskColliderTrigger.gameObject.SetActive(true);
					}
					else
					{
						inGameTutorialMaskColliderTrigger.gameObject.SetActive(false);
						InGameTutorialMaskController.Log.DebugFormat("disabling:{0}", new object[]
						{
							inGameTutorialMaskColliderTrigger.name
						});
					}
				}
			}
		}

		public InGameTutorialMaskColliderTrigger FindTriggerForBehaviour(HudMaskTutorialBehaviour pHudMaskTutorialBehaviour)
		{
			this.CheckMasks();
			for (int i = 0; i < this._masks.Count; i++)
			{
				InGameTutorialMaskColliderTrigger inGameTutorialMaskColliderTrigger = this._masks[i];
				if (inGameTutorialMaskColliderTrigger.name.Contains(pHudMaskTutorialBehaviour.name))
				{
					pHudMaskTutorialBehaviour.maskTrigger = inGameTutorialMaskColliderTrigger;
					return inGameTutorialMaskColliderTrigger;
				}
			}
			return null;
		}

		public void DisableCollider()
		{
			this.alphaCleanLayer.GetComponent<Collider>().enabled = false;
		}

		public void EnableCollider()
		{
			this.alphaCleanLayer.GetComponent<Collider>().enabled = true;
		}

		public void StartStep(HudMaskTutorialBehaviour pHudMaskRef)
		{
			InGameTutorialMaskController.Log.Debug("StartStep" + pHudMaskRef.gameObject.name);
			this.currentHudMaskRef = pHudMaskRef;
			this.PreviewStep(pHudMaskRef);
			base.StartCoroutine(this.ShowStepCoroutine());
		}

		public void StepCompleted()
		{
			base.StartCoroutine(this.CompleteCurrentStepCoroutine());
		}

		private IEnumerator CompleteCurrentStepCoroutine()
		{
			this.HideCurrentMasks();
			yield return base.StartCoroutine(this.HideOverlay(0.5f));
			this.DisableCollider();
			yield break;
		}

		private IEnumerator ShowStepCoroutine()
		{
			this.ShowCurrentMasks();
			yield break;
		}

		private void ShowCurrentMasks()
		{
			for (int i = 0; i < this._masks.Count; i++)
			{
				InGameTutorialMaskColliderTrigger inGameTutorialMaskColliderTrigger = this._masks[i];
				if (inGameTutorialMaskColliderTrigger == this.currentHudMaskRef.maskTrigger)
				{
					this._currentMask = inGameTutorialMaskColliderTrigger;
					InGameTutorialMaskController.Log.DebugFormat("ShowCurrentMasks" + this._currentMask.name, new object[0]);
					base.StartCoroutine(this.ShowOverlay(this.currentHudMaskRef.Delay, this.currentHudMaskRef.Duration));
					base.StartCoroutine(this._currentMask.Show(this.currentHudMaskRef.Delay, this.currentHudMaskRef.Duration));
					return;
				}
			}
		}

		private void HideCurrentMasks()
		{
			InGameTutorialMaskController.Log.DebugFormat("HideCurrentMasks!!! {0}", new object[]
			{
				this._currentMask
			});
			if (this._currentMask != null)
			{
				base.StartCoroutine(this._currentMask.Hide(0.5f));
			}
		}

		private void HideAllMasks()
		{
			for (int i = 0; i < this._masks.Count; i++)
			{
				InGameTutorialMaskColliderTrigger inGameTutorialMaskColliderTrigger = this._masks[i];
				base.StartCoroutine(inGameTutorialMaskColliderTrigger.Hide(0f));
			}
		}

		private float OriginalAlphaOverlay
		{
			get
			{
				return TutorialUIController.Instance.OriginalAlphaOverlay;
			}
		}

		public void SetupOverlay()
		{
			this.alphaCleanLayer.alpha = this.OriginalAlphaOverlay;
			for (int i = 0; i < this._masks.Count; i++)
			{
				InGameTutorialMaskColliderTrigger inGameTutorialMaskColliderTrigger = this._masks[i];
				inGameTutorialMaskColliderTrigger.uiTexture.alpha = this.OriginalAlphaOverlay;
			}
		}

		public IEnumerator ShowOverlay(float delay = 0f, float duration = 0.5f)
		{
			Debug.Log("Show overlay");
			this.EnableCollider();
			HOTween.Kill(this._uiPanel);
			if (this.currentHudMaskRef != null && !string.IsNullOrEmpty(this.currentHudMaskRef.GlowFeedbackName))
			{
				TutorialUIController.Instance.EnableGlowFeedback(this.currentHudMaskRef.GlowFeedbackName, this.currentHudMaskRef.maskTrigger.GetComponent<Collider>().transform.position, delay, duration);
			}
			return HOTween.To(this._uiPanel, duration, new TweenParms().Prop("alpha", 1f).Delay(delay)).WaitForCompletion();
		}

		public IEnumerator HideOverlay(float duration = 0.5f)
		{
			HOTween.Kill(this._uiPanel);
			if (this.currentHudMaskRef != null && !string.IsNullOrEmpty(this.currentHudMaskRef.GlowFeedbackName))
			{
				TutorialUIController.Instance.DisableGlowFeedback(duration);
			}
			return HOTween.To(this._uiPanel, duration, new TweenParms().Prop("alpha", 0.01f)).WaitForCompletion();
		}

		public static readonly BitLogger Log = new BitLogger(typeof(InGameTutorialMaskController));

		private static InGameTutorialMaskController _instance;

		private const string _stepsHolderName = "1.steps";

		public AnimationCurve AnimationCurve;

		public UITexture alphaCleanLayer;

		public GameObject maskFill;

		public HudMaskTutorialBehaviour currentHudMaskRef;

		public Texture2D circleTextureMask;

		public Texture2D squareTextureMask;

		public InGameTutorialMaskColliderTrigger hudMaskTriggerSource;

		[SerializeField]
		private Transform _stepsHolder;

		private UIPanel _uiPanel;

		private List<InGameTutorialMaskColliderTrigger> _masks;

		private InGameTutorialMaskColliderTrigger _currentMask;

		public UIRoot UiRoot;

		public Camera Camera;
	}
}
