using System;
using HeavyMetalMachines.Extensions;
using Hoplon.Math;
using UniRx;
using UnityEngine;

namespace HeavyMetalMachines.Presenting.Tooltip
{
	public class NGuiNewTooltipTrigger : MonoBehaviour, ITooltipTrigger
	{
		private void OnHover(bool isOver)
		{
			this._isHovered = isOver;
		}

		private void FixedUpdate()
		{
			if (!this._isHovered)
			{
				if (this._mouseIsOver)
				{
					this._tooltipDismissed.OnNext(Unit.Default);
				}
				this._mouseIsOver = false;
				this._tooltipElapsed = this._tooltipTimeout;
				this._lastMousePos = Input.mousePosition;
				return;
			}
			if (this._mouseIsOver && this._lastMousePos == Input.mousePosition)
			{
				return;
			}
			if (!this._mouseIsOver && (double)this._tooltipElapsed <= 0.0)
			{
				this._mouseIsOver = true;
				this._tooltipTriggered.OnNext(Unit.Default);
			}
			this._tooltipElapsed -= Time.fixedDeltaTime;
			this._lastMousePos = Input.mousePosition;
		}

		public void OnDisable()
		{
			this._tooltipDismissed.OnNext(Unit.Default);
		}

		public bool IsMouseOver
		{
			get
			{
				return this._mouseIsOver;
			}
		}

		public IObservable<Unit> OnTooltipTriggered
		{
			get
			{
				return this._tooltipTriggered;
			}
		}

		public IObservable<Unit> OnTooltipDismissed
		{
			get
			{
				return this._tooltipDismissed;
			}
		}

		public TooltipPosition TooltipPosition
		{
			get
			{
				return this._tooltipPosition;
			}
		}

		public Vector2 Offset
		{
			get
			{
				return this._offset.ToHmmVector2();
			}
		}

		public ITooltipPivot Pivot { get; set; }

		private readonly Subject<Unit> _tooltipTriggered = new Subject<Unit>();

		private readonly Subject<Unit> _tooltipDismissed = new Subject<Unit>();

		[SerializeField]
		private float _tooltipTimeout;

		[SerializeField]
		private TooltipPosition _tooltipPosition;

		[SerializeField]
		private Vector2 _offset;

		private ITooltipPivot _pivot;

		private Vector3 _lastMousePos;

		private float _tooltipElapsed;

		private bool _mouseIsOver;

		private bool _isHovered;
	}
}
