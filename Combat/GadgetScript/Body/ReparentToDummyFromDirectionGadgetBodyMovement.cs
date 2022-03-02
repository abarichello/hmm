using System;
using HeavyMetalMachines.Infra.Context;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Body
{
	internal class ReparentToDummyFromDirectionGadgetBodyMovement : MonoBehaviour, IGadgetBodyMovement
	{
		public Vector3 GetPosition(float elapsedTime)
		{
			return base.transform.position;
		}

		public Vector3 GetDirection()
		{
			return base.transform.forward;
		}

		public void Initialize(IGadgetBody body, IGadgetContext gadgetContext, IEventContext eventContext)
		{
			ICombatObject value = this._combatObjectParameter.GetValue(gadgetContext);
			this._dummyTransform = ((CombatObject)value).Dummy.GetDummy(this._dummyKind, this._customDummyName, null);
			this._originalParent = base.transform.parent;
			base.transform.parent = this._dummyTransform;
			IParameterTomate<Vector3> parameterTomate = (IParameterTomate<Vector3>)this._directionParameter.ParameterTomate;
			Vector3 value2 = parameterTomate.GetValue(gadgetContext);
			Debug.DrawLine(base.transform.position, base.transform.position + value2, Color.red, 10f);
			base.transform.LookAt(base.transform.position + value2);
			base.transform.localPosition = Vector3.zero;
			base.transform.localScale = Vector3.one;
		}

		public bool Finished
		{
			get
			{
				return false;
			}
		}

		public void Destroy()
		{
			this._dummyTransform = null;
			base.transform.parent = this._originalParent;
		}

		[SerializeField]
		private CombatObjectParameter _combatObjectParameter;

		[SerializeField]
		private BaseParameter _directionParameter;

		[SerializeField]
		private CDummy.DummyKind _dummyKind;

		[SerializeField]
		private string _customDummyName;

		private Transform _dummyTransform;

		private Transform _originalParent;
	}
}
