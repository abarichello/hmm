using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class UIBuffTooltipController : GameHubBehaviour
	{
		private void Start()
		{
			this.mTrans = base.transform;
			this.mWidgets = base.GetComponentsInChildren<UIWidget>();
			this.mPos = this.mTrans.localPosition;
			this.mSize = this.mTrans.localScale;
			if (this.uiCamera == null)
			{
				this.uiCamera = NGUITools.FindCameraForLayer(base.gameObject.layer);
			}
			this.SetAlpha(0f);
		}

		private void Update()
		{
			if (this.mCurrent != this.mTarget)
			{
				this.mCurrent = Mathf.Lerp(this.mCurrent, this.mTarget, Time.deltaTime * this.appearSpeed);
				if (Mathf.Abs(this.mCurrent - this.mTarget) < 0.001f)
				{
					this.mCurrent = this.mTarget;
				}
				this.SetAlpha(this.mCurrent * this.mCurrent);
				if (this.scalingTransitions)
				{
					Vector3 vector = this.mSize * 0.25f;
					vector.y = -vector.y;
					Vector3 localScale = Vector3.one * (1.5f - this.mCurrent * 0.5f);
					Vector3 localPosition = Vector3.Lerp(this.mPos - vector, this.mPos, this.mCurrent);
					this.mTrans.localPosition = localPosition;
					this.mTrans.localScale = localScale;
				}
			}
			if (this.Withlifetime)
			{
				this.IntLifetime = (int)((double)(this.LongEndLifetime - (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime()) * 0.001);
				this.SetLifeTimeText(this.IntLifetime);
			}
		}

		private void SetLifeTimeText(int lifetime)
		{
			this.Lifetime.SetTextInt(lifetime);
		}

		private void SetAlpha(float val)
		{
			int i = 0;
			int num = this.mWidgets.Length;
			while (i < num)
			{
				UIWidget uiwidget = this.mWidgets[i];
				uiwidget.alpha = val;
				i++;
			}
		}

		private void SetText(string tooltipText, bool withlifetime, int lifetime)
		{
			if (this.text == null || string.IsNullOrEmpty(tooltipText))
			{
				this.mTarget = 0f;
				return;
			}
			this.mTarget = 1f;
			this.text.text = tooltipText;
			this.mPos = Input.mousePosition;
			if (withlifetime)
			{
				this.Lifetime.enabled = true;
				this.SetLifeTimeText(lifetime);
				this.LifetimeInfinity.enabled = false;
			}
			else
			{
				this.Lifetime.enabled = false;
				this.LifetimeInfinity.enabled = true;
			}
			if (this.uiCamera != null)
			{
				this.mPos.x = Mathf.Clamp01(this.mPos.x / (float)Screen.width);
				this.mPos.y = Mathf.Clamp01(this.mPos.y / (float)Screen.height);
				float num = this.uiCamera.orthographicSize / this.mTrans.parent.lossyScale.y;
				float num2 = (float)Screen.height * 0.5f / num;
				Vector2 vector;
				vector..ctor(num2 * this.mSize.x / (float)Screen.width, num2 * this.mSize.y / (float)Screen.height);
				this.mPos.x = Mathf.Min(this.mPos.x, 1f - vector.x);
				this.mPos.y = Mathf.Max(this.mPos.y, vector.y);
				this.mTrans.position = this.uiCamera.ViewportToWorldPoint(this.mPos);
				this.mPos = this.mTrans.localPosition;
				this.mPos.x = Mathf.Round(this.mPos.x);
				this.mPos.y = Mathf.Round(this.mPos.y);
				this.mTrans.localPosition = this.mPos;
			}
			else
			{
				if (this.mPos.x + this.mSize.x > (float)Screen.width)
				{
					this.mPos.x = (float)Screen.width - this.mSize.x;
				}
				if (this.mPos.y - this.mSize.y < 0f)
				{
					this.mPos.y = this.mSize.y;
				}
				this.mPos.x = this.mPos.x - (float)Screen.width * 0.5f;
				this.mPos.y = this.mPos.y - (float)Screen.height * 0.5f;
			}
		}

		public void ShowText(string tooltipText, bool withlifetime, long endlifetime)
		{
			this.TooltipText = tooltipText;
			this.Withlifetime = withlifetime;
			if (withlifetime)
			{
				this.LongEndLifetime = endlifetime;
				this.IntLifetime = (int)((double)(this.LongEndLifetime - (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime()) * 0.001);
			}
			this.SetText(this.TooltipText, withlifetime, this.IntLifetime);
		}

		public Camera uiCamera;

		public UILabel text;

		public UILabel Lifetime;

		public UISprite LifetimeInfinity;

		public UISprite background;

		public float appearSpeed = 10f;

		public bool scalingTransitions = true;

		private Transform mTrans;

		private float mTarget;

		private float mCurrent;

		private Vector3 mPos;

		private Vector3 mSize;

		public string TooltipText;

		public bool Withlifetime;

		public long LongEndLifetime;

		public int IntLifetime;

		private UIWidget[] mWidgets;
	}
}
