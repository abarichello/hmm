using System;
using System.Collections.Generic;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Tutorial.InGame;
using UnityEngine;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	[RequireComponent(typeof(Collider))]
	public class AreaTriggerTutorialBehaviour : InGameTutorialBehaviourBase
	{
		private void OnTriggerEnter(Collider coll)
		{
			CombatObject combat = CombatRef.GetCombat(coll);
			if (base.playerController.Combat == combat)
			{
				this.EnableAndDisableObjs();
				this.CompleteBehaviourAndSync();
			}
		}

		private void EnableAndDisableObjs()
		{
			if (this._objsToDisable != null)
			{
				for (int i = 0; i < this._objsToDisable.Count; i++)
				{
					GameObject gameObject = this._objsToDisable[i];
					if (gameObject)
					{
						gameObject.SetActive(false);
					}
				}
			}
			if (this._objsToEnable != null)
			{
				for (int j = 0; j < this._objsToEnable.Count; j++)
				{
					GameObject gameObject2 = this._objsToEnable[j];
					if (gameObject2)
					{
						gameObject2.SetActive(true);
					}
				}
			}
		}

		[SerializeField]
		private List<GameObject> _objsToEnable;

		[SerializeField]
		private List<GameObject> _objsToDisable;
	}
}
