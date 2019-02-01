using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	[ExecuteInEditMode]
	public class HMMDragItem : GameHubBehaviour
	{
		public Vector3 dragMovement
		{
			get
			{
				return this.scale;
			}
			set
			{
				this.scale = value;
			}
		}

		private void OnEnable()
		{
			if (this.scrollWheelFactor != 0f)
			{
				this.scrollMomentum = this.scale * this.scrollWheelFactor;
				this.scrollWheelFactor = 0f;
			}
			if (this.contentRect == null && this.target != null && Application.isPlaying)
			{
				UIWidget component = this.target.GetComponent<UIWidget>();
				if (component != null)
				{
					this.contentRect = component;
				}
			}
		}

		private void FindPanel()
		{
			this.mPanel = ((!(this.target != null)) ? null : UIPanel.Find(this.target.transform.parent));
			if (this.mPanel == null)
			{
				this.restrictWithinPanel = false;
			}
		}

		private void UpdateBounds()
		{
			if (this.contentRect)
			{
				Transform cachedTransform = this.mPanel.cachedTransform;
				Matrix4x4 worldToLocalMatrix = cachedTransform.worldToLocalMatrix;
				Vector3[] worldCorners = this.contentRect.worldCorners;
				for (int i = 0; i < 4; i++)
				{
					worldCorners[i] = worldToLocalMatrix.MultiplyPoint3x4(worldCorners[i]);
				}
				this.mBounds = new Bounds(worldCorners[0], Vector3.zero);
				for (int j = 1; j < 4; j++)
				{
					this.mBounds.Encapsulate(worldCorners[j]);
				}
			}
			else
			{
				this.mBounds = NGUIMath.CalculateRelativeWidgetBounds(this.mPanel.cachedTransform, this.target);
			}
		}

		private void OnPress(bool pressed)
		{
			if (base.enabled && NGUITools.GetActive(base.gameObject) && this.target != null)
			{
				if (pressed)
				{
					if (!this.mPressed)
					{
						this.mTouchID = UICamera.currentTouchID;
						this.mPressed = true;
						this.mStarted = false;
						this.CancelMovement();
						if (this.restrictWithinPanel && this.mPanel == null)
						{
							this.FindPanel();
						}
						if (this.restrictWithinPanel)
						{
							this.UpdateBounds();
						}
						this.CancelSpring();
						Transform transform = UICamera.currentCamera.transform;
						this.mPlane = new Plane(((!(this.mPanel != null)) ? transform.rotation : this.mPanel.cachedTransform.rotation) * Vector3.back, UICamera.lastHit.point);
					}
				}
				else if (this.mPressed && this.mTouchID == UICamera.currentTouchID)
				{
					this.mPressed = false;
					if (this.restrictWithinPanel && this.dragEffect == UIDragObject.DragEffect.MomentumAndSpring && this.mPanel.ConstrainTargetToBounds(this.target, ref this.mBounds, false))
					{
						this.CancelMovement();
					}
				}
			}
		}

		private void OnDisable()
		{
			if (this.draggingClone)
			{
				UnityEngine.Object.Destroy(this.draggingClone.gameObject);
			}
			this.draggingClone = null;
		}

		private void OnDrag(Vector2 delta)
		{
			if (this.mPressed && this.mTouchID == UICamera.currentTouchID && base.enabled && NGUITools.GetActive(base.gameObject) && this.target != null)
			{
				UICamera.currentTouch.clickNotification = UICamera.ClickNotification.BasedOnDelta;
				Ray ray = UICamera.currentCamera.ScreenPointToRay(UICamera.currentTouch.pos);
				float distance = 0f;
				if (this.mPlane.Raycast(ray, out distance))
				{
					if (this.draggingClone == null)
					{
						this.draggingClone = UnityEngine.Object.Instantiate<Transform>(this.target);
						this.draggingClone.transform.parent = this.dragParent;
						this.draggingClone.transform.localScale = Vector3.one;
						this.draggingClone.transform.position = this.target.position;
						this.draggingClone.gameObject.SetActive(true);
						if (this.draggingClone.GetComponent<Collider>())
						{
							UnityEngine.Object.Destroy(this.draggingClone.GetComponent<Collider>());
						}
						TweenColor.Begin(this.draggingClone.gameObject, 0.1f, new Color(1f, 1f, 1f, 1f));
						TweenColor.Begin(this.target.gameObject, 0.1f, new Color(1f, 1f, 1f, 0.2f));
						HMMDragItem.CurrentDrag = this.target.gameObject;
					}
					Vector3 point = ray.GetPoint(distance);
					Vector3 vector = point - this.mLastPos;
					this.mLastPos = point;
					if (!this.mStarted)
					{
						this.mStarted = true;
						vector = Vector3.zero;
					}
					if (vector.x != 0f || vector.y != 0f)
					{
						vector = this.draggingClone.InverseTransformDirection(vector);
						vector.Scale(this.scale);
						vector = this.draggingClone.TransformDirection(vector);
					}
					if (this.dragEffect != UIDragObject.DragEffect.None)
					{
						this.mMomentum = Vector3.Lerp(this.mMomentum, this.mMomentum + vector * (0.01f * this.momentumAmount), 0.67f);
					}
					Vector3 localPosition = this.draggingClone.localPosition;
					this.Move(vector);
					if (this.restrictWithinPanel)
					{
						this.mBounds.center = this.mBounds.center + (this.draggingClone.localPosition - localPosition);
						if (this.dragEffect != UIDragObject.DragEffect.MomentumAndSpring && this.mPanel.ConstrainTargetToBounds(this.draggingClone, ref this.mBounds, true))
						{
							this.CancelMovement();
						}
					}
				}
			}
		}

		private void Move(Vector3 worldDelta)
		{
			if (this.mPanel != null)
			{
				this.mTargetPos += worldDelta;
				this.draggingClone.position = this.mTargetPos;
				Vector3 localPosition = this.draggingClone.localPosition;
				localPosition.x = Mathf.Round(localPosition.x);
				localPosition.y = Mathf.Round(localPosition.y);
				this.draggingClone.localPosition = localPosition;
			}
			else
			{
				this.draggingClone.position += worldDelta;
			}
		}

		private void LateUpdate()
		{
			if (this.draggingClone == null)
			{
				return;
			}
			float deltaTime = RealTime.deltaTime;
			this.mMomentum -= this.mScroll;
			this.mScroll = NGUIMath.SpringLerp(this.mScroll, Vector3.zero, 20f, deltaTime);
			if (!this.mPressed)
			{
				this.mMomentum = this.target.position - this.draggingClone.transform.position;
				float magnitude = this.mMomentum.magnitude;
				if (100f * Time.deltaTime < magnitude)
				{
					this.mMomentum = this.mMomentum / magnitude * 100f * Time.deltaTime;
				}
				if (UICamera.hoveredObject != null)
				{
					HMMItemDropSurface component = UICamera.hoveredObject.GetComponent<HMMItemDropSurface>();
					if (component && component.ValidateDrop(this.target.gameObject))
					{
						TweenColor.Begin(this.target.gameObject, 0.1f, new Color(1f, 1f, 1f, 1f));
						UnityEngine.Object.Destroy(this.draggingClone.gameObject);
						HMMDragItem.CurrentDrag = null;
					}
				}
				if (this.mMomentum.magnitude < 0.01f)
				{
					TweenColor.Begin(this.target.gameObject, 0.1f, new Color(1f, 1f, 1f, 1f));
					UnityEngine.Object.Destroy(this.draggingClone.gameObject);
					HMMDragItem.CurrentDrag = null;
					return;
				}
				if (this.mPanel == null)
				{
					this.FindPanel();
				}
				this.Move(NGUIMath.SpringDampen(ref this.mMomentum, 9f, deltaTime));
				if (this.restrictWithinPanel && this.mPanel != null)
				{
					this.UpdateBounds();
					if (this.mPanel.ConstrainTargetToBounds(this.draggingClone, ref this.mBounds, this.dragEffect == UIDragObject.DragEffect.None))
					{
						this.CancelMovement();
					}
					else
					{
						this.CancelSpring();
					}
				}
			}
			else
			{
				this.mTargetPos = ((!(this.draggingClone != null)) ? Vector3.zero : this.draggingClone.position);
			}
			NGUIMath.SpringDampen(ref this.mMomentum, 9f, deltaTime);
		}

		public void CancelMovement()
		{
			this.mTargetPos = ((!(this.target != null)) ? Vector3.zero : this.target.position);
			this.mMomentum = Vector3.zero;
			this.mScroll = Vector3.zero;
		}

		public void CancelSpring()
		{
			SpringPosition component = this.target.GetComponent<SpringPosition>();
			if (component != null)
			{
				component.enabled = false;
			}
		}

		private void OnScroll(float delta)
		{
			if (base.enabled && NGUITools.GetActive(base.gameObject))
			{
				this.mScroll -= this.scrollMomentum * (delta * 0.05f);
			}
		}

		public Transform target;

		private Transform draggingClone;

		public Transform dragParent;

		public Vector3 scrollMomentum = Vector3.zero;

		public bool restrictWithinPanel;

		public UIRect contentRect;

		public UIDragObject.DragEffect dragEffect = UIDragObject.DragEffect.MomentumAndSpring;

		public float momentumAmount = 35f;

		[SerializeField]
		protected Vector3 scale = new Vector3(1f, 1f, 0f);

		[HideInInspector]
		[SerializeField]
		private float scrollWheelFactor;

		private Plane mPlane;

		private Vector3 mTargetPos;

		private Vector3 mLastPos;

		private UIPanel mPanel;

		private bool mPressed;

		private Vector3 mMomentum = Vector3.zero;

		private Vector3 mScroll = Vector3.zero;

		private Bounds mBounds;

		private int mTouchID;

		private bool mStarted;

		public static GameObject CurrentDrag;

		public enum DragEffect
		{
			None,
			Momentum,
			MomentumAndSpring
		}
	}
}
