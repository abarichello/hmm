using System;
using HeavyMetalMachines.Combat;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Scene
{
	internal class ScrapAnimatorTrigger : GameHubBehaviour, IObjectSpawnListener
	{
		public void OnObjectUnspawned(UnspawnEvent evt)
		{
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				return;
			}
			if (evt.Causer == -1)
			{
				return;
			}
			Identifiable @object = GameHubBehaviour.Hub.ObjectCollection.GetObject(evt.Causer);
			if (!@object)
			{
				return;
			}
			CombatObject component = base.GetComponent<CombatObject>();
			CombatObject component2 = @object.GetComponent<CombatObject>();
			Vector3 position;
			Quaternion rotation;
			if (!component)
			{
				position = base.transform.position;
				rotation = base.transform.rotation;
			}
			else
			{
				position = component.transform.position;
				rotation = component.transform.rotation;
			}
			Vector3 position2;
			Quaternion rotation2;
			if (!component2)
			{
				position2 = @object.transform.position;
				rotation2 = @object.transform.rotation;
			}
			else
			{
				position2 = component2.transform.position;
				rotation2 = component2.transform.rotation;
			}
			ScrapAnimator.AddScrap(position, rotation, base.transform.lossyScale, position2, rotation2, this.scrapType, @object.transform.gameObject.layer);
		}

		public void OnObjectSpawned(SpawnEvent evt)
		{
		}

		public ScrapAnimator.ScrapType scrapType;
	}
}
