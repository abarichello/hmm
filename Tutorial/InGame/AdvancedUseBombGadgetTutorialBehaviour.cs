using System;
using HeavyMetalMachines.Combat;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Tutorial.InGame
{
	public class AdvancedUseBombGadgetTutorialBehaviour : FollowPathBehaviour
	{
		protected override void UpdateOnServer()
		{
			CombatObject combat = this._botStarter.BotAIController.Combat;
			if (GameHubBehaviour.Hub.BombManager.IsCarryingBomb(combat.Id.ObjId))
			{
				this._botStarter.BotAIController.SetGoal(this._throwbackTransform);
				combat.BombGadget.Pressed = (Vector3.SqrMagnitude(combat.transform.position - this._throwbackTransformAim.position) <= this._distanceToThrowback);
			}
			else
			{
				base.UpdateOnServer();
				combat.BombGadget.Pressed = (Vector3.SqrMagnitude(combat.transform.position - GameHubBehaviour.Hub.BombManager.BombMovement.transform.position) <= this._distanceToThrowback);
			}
		}

		[SerializeField]
		private Transform _throwbackTransform;

		[SerializeField]
		private Transform _throwbackTransformAim;

		[SerializeField]
		private float _distanceToThrowback = 9000f;

		[SerializeField]
		private float _distanceToGetBomb = 5000f;
	}
}
