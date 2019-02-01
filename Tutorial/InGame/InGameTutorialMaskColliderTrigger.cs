using System;
using System.Collections;
using HeavyMetalMachines.Tutorial.Behaviours;
using Holoville.HOTween;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Tutorial.InGame
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(Collider), typeof(UITexture))]
	public class InGameTutorialMaskColliderTrigger : MonoBehaviour
	{
		public bool isActive { get; private set; }

		private UIRoot Root
		{
			get
			{
				return InGameTutorialMaskController.Instance.UiRoot;
			}
		}

		private void Awake()
		{
			this.BindFields();
		}

		private void BindFields()
		{
			UICamera.onScreenResize += this.UpdateColliderSizes;
		}

		private IEnumerator Start()
		{
			yield return null;
			base.gameObject.layer = 0;
			base.GetComponent<Collider>().enabled = false;
			yield break;
		}

		private void OnDestroy()
		{
			UICamera.onScreenResize -= this.UpdateColliderSizes;
		}

		public void OnMouseEnter()
		{
			if (!this.isActive)
			{
				return;
			}
			this._mouseOver = true;
			InGameTutorialMaskController.Instance.DisableCollider();
		}

		public void OnMouseExit()
		{
			if (!this.isActive)
			{
				return;
			}
			this._mouseOver = false;
			InGameTutorialMaskController.Instance.EnableCollider();
		}

		public void ConfigureMaskBasedOnBehaviour(HudMaskTutorialBehaviour pHudMask)
		{
			Debug.Log("ConfigureMaskBasedOnBehaviour - step: " + pHudMask.Step);
			this._hudBehaviour = pHudMask;
			this.uiAnchor.enabled = true;
			if (Application.isEditor)
			{
				Debug.LogWarning("In editor mode the transform anchor type can not work well. (it�s based on transform remember!)");
			}
			if (this.AnchorTransform == null)
			{
				GameObject gameObject = null;
				if (this._hudBehaviour.RootObjectTargetAnchor != null)
				{
					gameObject = GameObject.Find(this._hudBehaviour.RootObjectTargetAnchor);
				}
				Transform transform = null;
				if (gameObject != null)
				{
					if (this._hudBehaviour.TargetAnchorPath != null)
					{
						transform = gameObject.transform.Find(this._hudBehaviour.TargetAnchorPath);
					}
					if (transform != null)
					{
						this.uiAnchor.side = UIAnchor.Side.Center;
						this.uiAnchor.relativeOffset = this._hudBehaviour.relativeOffset;
						this.uiAnchor.pixelOffset = this._hudBehaviour.pixelOffset;
						this.uiAnchor.container = transform.gameObject;
					}
					else
					{
						Debug.LogError("TargetAnchorPath not found. The name is incorrect or was not found on this scene. ");
					}
				}
				else
				{
					Debug.LogError("RootObjectTargetAnchor not found. The name is incorrect or was not found on this scene.", this._hudBehaviour);
				}
			}
			this.uiTexture.width = this._hudBehaviour.width;
			this.uiTexture.height = this._hudBehaviour.height;
			this.shapeType = this._hudBehaviour.shapeType;
			this.colliderSize = this._hudBehaviour.colliderSizeUpdate;
			this.step = this._hudBehaviour.Step;
			this.UpdateColliderSizes();
		}

		private void Update()
		{
			if (!this.isActive)
			{
				return;
			}
			if (this._animateMaskDelay > 0f)
			{
				this._animateMaskDelay -= Time.deltaTime;
				if (this._animateMaskDelay <= 0f)
				{
					this.StartPingPongScale();
				}
			}
			this._mousePos = Input.mousePosition;
			this._mousePos.z = 0f;
			if (UICamera.currentCamera == null)
			{
				return;
			}
			this._mousePos = UICamera.currentCamera.ScreenToWorldPoint(this._mousePos);
			this._mouseDistanceFromSphere = this._mousePos - this._rectCollider.center.ToVector3XY();
			this._mouseOverSphere = (this._mouseDistanceFromSphere.sqrMagnitude < this._sphereMagnitude);
			this._mouseOver = (this._rectCollider.Contains(this._mousePos) || this._mouseOverSphere);
			if (this._mouseOver)
			{
				this.OnMouseEnter();
			}
			else
			{
				this.OnMouseExit();
			}
			if (this._mouseOver && Input.GetMouseButtonUp(0) && !this._done)
			{
				this._done = true;
				this.Complete();
			}
		}

		private void Complete()
		{
			this._hudBehaviour.MaskTriggered();
			this.OnMouseExit();
		}

		public void UpdateColliderSizes()
		{
			float num = -1f;
			switch (this.colliderSize)
			{
			case InGameTutorialMaskColliderTrigger.ColliderSizeUpdate.SameMaskSize:
				num = 1f;
				break;
			case InGameTutorialMaskColliderTrigger.ColliderSizeUpdate.TwoThirdsMaskSize:
				num = 0.75f;
				break;
			case InGameTutorialMaskColliderTrigger.ColliderSizeUpdate.HalfMaskSize:
				num = 0.5f;
				break;
			case InGameTutorialMaskColliderTrigger.ColliderSizeUpdate.ThirdMaskSize:
				num = 0.3f;
				break;
			}
			if (this._hudBehaviour == null)
			{
				return;
			}
			SphereCollider sphereCollider = base.GetComponent<Collider>() as SphereCollider;
			BoxCollider boxCollider = base.GetComponent<Collider>() as BoxCollider;
			if (sphereCollider != null)
			{
				sphereCollider.radius = ((num >= 0f) ? ((float)this.uiTexture.width / 2f * num) : this._hudBehaviour.colliderSize.x);
				sphereCollider.center = this._hudBehaviour.colliderCenter;
				this._rectCollider.center = sphereCollider.transform.position;
				float num2 = sphereCollider.radius * this.Root.transform.localScale.x;
				this._sphereMagnitude = num2 * num2;
			}
			else if (boxCollider != null)
			{
				boxCollider.size = ((num >= 0f) ? new Vector3((float)this.uiTexture.width * num, (float)this.uiTexture.height * num) : this._hudBehaviour.colliderSize);
				boxCollider.center = this._hudBehaviour.colliderCenter;
				this._rectCollider.center = boxCollider.transform.position;
				this._rectCollider.size = new Vector2(boxCollider.size.x * this.Root.transform.localScale.x, boxCollider.size.y * this.Root.transform.localScale.y);
			}
			TutorialUIController.Instance.UpdateGlowFeedback(this._rectCollider.center);
		}

		public IEnumerator Show(float delay = 0f, float duration = 0.5f)
		{
			HOTween.Kill(this.uiTexture.transform);
			this.uiTexture.transform.localScale = Vector3.zero;
			Tweener tTween = HOTween.To(this.uiTexture.transform, duration, new TweenParms().Prop("localScale", new Vector3(1f, 1f, 1f)).Delay(delay));
			yield return base.StartCoroutine(tTween.WaitForCompletion());
			this.UpdateColliderSizes();
			this.isActive = true;
			this._animateMaskDelay = this._hudBehaviour.AnimateMaskDelay;
			yield break;
		}

		private void StartPingPongScale()
		{
			TweenScale tweenScale = TweenScale.Begin(this.uiTexture.gameObject, 1f, new Vector3(1.1f, 1.1f, 1.1f));
			tweenScale.from = Vector3.one;
			tweenScale.animationCurve = InGameTutorialMaskController.Instance.AnimationCurve;
			tweenScale.style = UITweener.Style.Loop;
		}

		public IEnumerator Hide(float duration = 0.5f)
		{
			this.isActive = false;
			if (this.uiTexture == null)
			{
				yield break;
			}
			HOTween.Kill(this.uiTexture.transform);
			Tweener tTween = HOTween.To(this.uiTexture.transform, duration, new TweenParms().Prop("localScale", new Vector3(0f, 0f, 0f)).OnComplete(delegate()
			{
				this.uiTexture.gameObject.SetActive(false);
			}));
			yield return base.StartCoroutine(tTween.WaitForCompletion());
			yield break;
		}

		public static readonly BitLogger Log = new BitLogger(typeof(InGameTutorialMaskColliderTrigger));

		public int step;

		public InGameTutorialMaskColliderTrigger.ShapeTypes shapeType;

		public InGameTutorialMaskColliderTrigger.ColliderSizeUpdate colliderSize = InGameTutorialMaskColliderTrigger.ColliderSizeUpdate.TwoThirdsMaskSize;

		public UIAnchor uiAnchor;

		public Transform AnchorTransform;

		public UITexture uiTexture;

		private bool _mouseOver;

		private HudMaskTutorialBehaviour _hudBehaviour;

		private float _animateMaskDelay;

		private Rect _rectCollider = default(Rect);

		private Vector3 _mousePos;

		private bool _mouseOverSphere;

		private float _sphereMagnitude;

		private Vector3 _sphereCenter;

		private Vector3 _mouseDistanceFromSphere;

		private bool _done;

		public enum ShapeTypes
		{
			Square,
			Rectangle,
			Cicle
		}

		public enum ColliderSizeUpdate
		{
			DontUpdateAutomatically,
			SameMaskSize,
			TwoThirdsMaskSize,
			HalfMaskSize,
			ThirdMaskSize
		}
	}
}
