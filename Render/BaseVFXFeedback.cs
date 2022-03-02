using System;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.VFX;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Render
{
	public abstract class BaseVFXFeedback : GameHubBehaviour
	{
		private void Start()
		{
			if (GameHubBehaviour.Hub == null)
			{
				Debug.LogWarning("GameHub is not running, couldn't load data");
				base.enabled = false;
				return;
			}
			if (GameHubBehaviour.Hub.Net.IsServer() && !GameHubBehaviour.Hub.Net.IsTest())
			{
				base.enabled = false;
				return;
			}
			this.combatObject = base.GetComponentInParent<CombatObject>();
			if (this.combatObject == null || this.combatObject.Combat == null)
			{
				Debug.LogWarning("No CombatData available! Is this a Character/Car prefab?");
				base.enabled = false;
				return;
			}
			for (int i = 0; i < this.Feedbacks.Length; i++)
			{
				BaseVFXFeedback.VFXFeedbackItem vfxfeedbackItem = this.Feedbacks[i];
				GameHubBehaviour.Hub.Resources.PrefabPreCache(vfxfeedbackItem.VfxPrefab, 1);
			}
			this.OnStart();
		}

		private void OnDestroy()
		{
			this.OnFinished();
			for (int i = 0; i < this.Feedbacks.Length; i++)
			{
				BaseVFXFeedback.VFXFeedbackItem vfxfeedbackItem = this.Feedbacks[i];
				if (vfxfeedbackItem.VfxInstance)
				{
					vfxfeedbackItem.VfxInstance.Destroy(BaseFX.EDestroyReason.Default);
				}
			}
			this.combatObject = null;
		}

		private void OnDisable()
		{
			this.OnFinished();
			for (int i = 0; i < this.Feedbacks.Length; i++)
			{
				BaseVFXFeedback.VFXFeedbackItem vfxfeedbackItem = this.Feedbacks[i];
				if (vfxfeedbackItem.VfxInstance)
				{
					vfxfeedbackItem.VfxInstance.Destroy(BaseFX.EDestroyReason.Default);
				}
			}
		}

		private void LateUpdate()
		{
			this.OnUpdate();
			for (int i = 0; i < this.Feedbacks.Length; i++)
			{
				MasterVFX masterVFX = this.Feedbacks[i].VfxInstance;
				if (this.CompareValues(this.Feedbacks[i].Percent, this.percent))
				{
					if (!masterVFX)
					{
						masterVFX = (MasterVFX)GameHubBehaviour.Hub.Resources.PrefabCacheInstantiate(this.Feedbacks[i].VfxPrefab, base.transform.position, base.transform.rotation);
						if (masterVFX != null)
						{
							GameHubBehaviour.Hub.Drawer.AddEffect(masterVFX.transform);
							masterVFX.baseMasterVFX = this.Feedbacks[i].VfxPrefab;
							masterVFX.Activate(this.combatObject.Id, this.combatObject.Id, this.combatObject.transform);
						}
						else
						{
							Debug.LogError("[BaseVFXFeedback] Could not instantiate VFX Instance.", this);
						}
					}
				}
				else if (masterVFX)
				{
					masterVFX.Destroy(BaseFX.EDestroyReason.Default);
					masterVFX = null;
				}
				this.Feedbacks[i].VfxInstance = masterVFX;
			}
		}

		protected abstract void OnStart();

		protected abstract bool CompareValues(float target, float current);

		protected abstract void OnUpdate();

		protected abstract void OnFinished();

		[SerializeField]
		public BaseVFXFeedback.VFXFeedbackItem[] Feedbacks;

		protected CombatObject combatObject;

		protected float percent;

		[Serializable]
		public struct VFXFeedbackItem
		{
			[Range(0f, 1f)]
			public float Percent;

			public MasterVFX VfxPrefab;

			[HideInInspector]
			[NonSerialized]
			public MasterVFX VfxInstance;
		}
	}
}
