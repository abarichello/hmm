using System;
using UnityEngine;
using UnityEngine.Events;

namespace HeavyMetalMachines.Render
{
	public class BaseAnimatorGadgetFeedback : MonoBehaviour, IAnimatorGadgetFeedback
	{
		public void Activate()
		{
			this.animationTrigger.Invoke();
		}

		public string TriggerName
		{
			get
			{
				return this._triggerName;
			}
			set
			{
				this._triggerName = value;
			}
		}

		public bool TriggerBool
		{
			get
			{
				return this._triggerBool;
			}
			set
			{
				this._triggerBool = value;
			}
		}

		public int TriggerInteger
		{
			get
			{
				return this._triggerInteger;
			}
			set
			{
				this._triggerInteger = value;
			}
		}

		public AnimatorControllerParameterType TriggerType
		{
			get
			{
				return this.triggerType;
			}
		}

		[SerializeField]
		private string _triggerName;

		[SerializeField]
		private bool _triggerBool;

		[SerializeField]
		private int _triggerInteger;

		public AnimatorControllerParameterType triggerType;

		public UnityEvent animationTrigger = new UnityEvent();
	}
}
