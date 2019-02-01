using System;
using UnityEngine;

namespace HeavyMetalMachines.Battlepass
{
	public class UnityUiBattlepassSeasonSelector : MonoBehaviour
	{
		protected void LateUpdate()
		{
			if (!this._imediate && this._targetInstanceId != -1)
			{
				base.transform.position = Vector3.SmoothDamp(base.transform.position, this._target.position, ref this._velocity, this._dampTimeInSec);
			}
		}

		public void ResetTarget()
		{
			this.SetTarget(null, false, false);
		}

		public void SetTarget(Transform targetTransform, bool isVisible = false, bool imediate = false)
		{
			bool flag = this._targetInstanceId == -1;
			this._imediate = (imediate || flag);
			this._target = targetTransform;
			this._targetInstanceId = ((!(targetTransform == null)) ? targetTransform.GetInstanceID() : -1);
			this._canvasGroup.alpha = ((this._targetInstanceId == -1 || !isVisible) ? 0f : 1f);
			if (this._imediate && this._targetInstanceId != -1)
			{
				base.transform.position = this._target.position;
			}
		}

		protected void OnDestroy()
		{
			this._imediate = false;
			this._target = null;
			this._targetInstanceId = -1;
		}

		[SerializeField]
		private CanvasGroup _canvasGroup;

		[SerializeField]
		private float _dampTimeInSec = 0.5f;

		[SerializeField]
		private Transform _target;

		private Vector3 _velocity = Vector3.zero;

		private bool _imediate;

		private int _targetInstanceId = -1;
	}
}
