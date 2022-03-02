using System;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Match;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Body
{
	[RequireComponent(typeof(GadgetBody))]
	public class GadgetBodyLayerSetter : MonoBehaviour
	{
		private void Awake()
		{
			base.GetComponent<GadgetBody>().OnBodyInitialized += this.Initialize;
		}

		private void Initialize(IGadgetBody body)
		{
			TeamKind team = ((CombatObject)this._targetParameter.GetValue(body.Context)).Team;
			base.gameObject.layer = ((team != TeamKind.Blue) ? 10 : 11);
		}

		[Header("Read")]
		[SerializeField]
		private CombatObjectParameter _targetParameter;

		private ICombatObject _target;
	}
}
