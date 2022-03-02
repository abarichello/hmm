using System;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Match;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Body
{
	[RequireComponent(typeof(GadgetBody))]
	public class GadgetBodyProjectileLayerSetter : MonoBehaviour
	{
		private void Awake()
		{
			base.GetComponent<GadgetBody>().OnBodyInitialized += this.Initialize;
		}

		private void Initialize(IGadgetBody body)
		{
			GadgetBodyProjectileLayerSetter.TeamProjectileType teamToAffect = this._teamToAffect;
			if (teamToAffect != GadgetBodyProjectileLayerSetter.TeamProjectileType.SameTeam)
			{
				if (teamToAffect == GadgetBodyProjectileLayerSetter.TeamProjectileType.DifferentTeam)
				{
					base.gameObject.layer = this.GetLayerByTeam(body, LayerManager.Layer.PhysicsProjectileBlue, LayerManager.Layer.PhysicsProjectileRed);
				}
			}
			else
			{
				base.gameObject.layer = this.GetLayerByTeam(body, LayerManager.Layer.PhysicsProjectileRed, LayerManager.Layer.PhysicsProjectileBlue);
			}
		}

		private int GetLayerByTeam(IGadgetBody body, LayerManager.Layer blueTeamLayer, LayerManager.Layer redTeamLayer)
		{
			CombatObject combatObject = (CombatObject)this._targetParameter.GetValue(body.Context);
			TeamKind team = combatObject.Team;
			return (int)((team != TeamKind.Blue) ? redTeamLayer : blueTeamLayer);
		}

		[Header("Read")]
		[SerializeField]
		private CombatObjectParameter _targetParameter;

		[SerializeField]
		private GadgetBodyProjectileLayerSetter.TeamProjectileType _teamToAffect;

		private ICombatObject _target;

		private enum TeamProjectileType
		{
			SameTeam,
			DifferentTeam
		}
	}
}
