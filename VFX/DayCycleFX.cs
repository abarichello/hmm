using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class DayCycleFX : GameHubBehaviour
	{
		private void Start()
		{
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				base.enabled = false;
				return;
			}
			base.gameObject.SetActive(this.DefaultActive);
			DayCycleManager.AddFX(this);
		}

		private void OnDestroy()
		{
			DayCycleManager.RemoveFX(this);
		}

		public void UpdateStates(DayCycleManager.DayCycleStageId newStageId)
		{
			for (int i = 0; i < this.States.Length; i++)
			{
				DayCycleFX.DayCycleFXState dayCycleFXState = this.States[i];
				if (dayCycleFXState.Stage == newStageId)
				{
					if (dayCycleFXState.Animation)
					{
						dayCycleFXState.Animation.Play();
					}
					base.gameObject.SetActive(dayCycleFXState.Active);
					return;
				}
			}
		}

		public bool DefaultActive;

		public DayCycleFX.DayCycleFXState[] States = new DayCycleFX.DayCycleFXState[]
		{
			new DayCycleFX.DayCycleFXState
			{
				Active = true,
				Animation = null,
				Name = DayCycleManager.DayCycleStageId.Day.ToString(),
				Stage = DayCycleManager.DayCycleStageId.Day
			},
			new DayCycleFX.DayCycleFXState
			{
				Active = true,
				Animation = null,
				Name = DayCycleManager.DayCycleStageId.Sunset1.ToString(),
				Stage = DayCycleManager.DayCycleStageId.Sunset1
			},
			new DayCycleFX.DayCycleFXState
			{
				Active = true,
				Animation = null,
				Name = DayCycleManager.DayCycleStageId.Sunset2.ToString(),
				Stage = DayCycleManager.DayCycleStageId.Sunset2
			},
			new DayCycleFX.DayCycleFXState
			{
				Active = true,
				Animation = null,
				Name = DayCycleManager.DayCycleStageId.Night.ToString(),
				Stage = DayCycleManager.DayCycleStageId.Night
			},
			new DayCycleFX.DayCycleFXState
			{
				Active = true,
				Animation = null,
				Name = DayCycleManager.DayCycleStageId.Sunrise1.ToString(),
				Stage = DayCycleManager.DayCycleStageId.Sunrise1
			},
			new DayCycleFX.DayCycleFXState
			{
				Active = true,
				Animation = null,
				Name = DayCycleManager.DayCycleStageId.Sunrise2.ToString(),
				Stage = DayCycleManager.DayCycleStageId.Sunrise2
			}
		};

		[Serializable]
		public class DayCycleFXState
		{
			public string Name;

			public DayCycleManager.DayCycleStageId Stage;

			public bool Active = true;

			public Animation Animation;
		}
	}
}
