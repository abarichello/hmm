using System;
using HeavyMetalMachines.Combat.Gadget;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Render
{
	public class GadgetsPropertiesData : MonoBehaviour
	{
		public void SetCooldown(int cooldown, int cooldownStart, GadgetSlot gadgetSlot)
		{
			switch (gadgetSlot)
			{
			case GadgetSlot.CustomGadget0:
				this.gadget0Cooldown = cooldown;
				this.gadget0CooldownStart = cooldownStart;
				break;
			case GadgetSlot.CustomGadget1:
				this.gadget1Cooldown = cooldown;
				this.gadget1CooldownStart = cooldownStart;
				break;
			case GadgetSlot.CustomGadget2:
				this.gadget2Cooldown = cooldown;
				this.gadget2CooldownStart = cooldownStart;
				break;
			case GadgetSlot.BoostGadget:
				this.boostCooldown = cooldown;
				this.boostCooldownStart = cooldownStart;
				break;
			}
		}

		public float GetCooldownPercentage(GadgetSlot gadgetSlot)
		{
			float num = 0f;
			switch (gadgetSlot)
			{
			case GadgetSlot.CustomGadget0:
				num = (float)(this.currentTime - this.gadget0CooldownStart) / (float)this.gadget0Cooldown;
				break;
			case GadgetSlot.CustomGadget1:
				num = (float)(this.currentTime - this.gadget1CooldownStart) / (float)this.gadget1Cooldown;
				break;
			case GadgetSlot.CustomGadget2:
				num = (float)(this.currentTime - this.gadget2CooldownStart) / (float)this.gadget2Cooldown;
				break;
			case GadgetSlot.BoostGadget:
				num = (float)(this.currentTime - this.boostCooldownStart) / (float)this.boostCooldown;
				break;
			}
			if (num > 1f)
			{
				num = 1f;
			}
			return num;
		}

		private void Awake()
		{
			this.hub = GameHubBehaviour.Hub;
			if (!this.carGenerator)
			{
				this.carGenerator = base.GetComponent<CarGenerator>();
			}
		}

		private void Update()
		{
			this.currentTime = this.hub.GameTime.GetPlaybackTime();
			this.UpdateCheckAnimationConfig();
		}

		private void UpdateCheckAnimationConfig()
		{
			if (this.gadget0AnimationTriggerConfig.Length > 0)
			{
				this.CheckAnimationConfig(ref this.gadget0AnimationTriggerConfig, this.GetCooldownPercentage(GadgetSlot.CustomGadget0));
			}
			if (this.gadget1AnimationTriggerConfig.Length > 0)
			{
				this.CheckAnimationConfig(ref this.gadget1AnimationTriggerConfig, this.GetCooldownPercentage(GadgetSlot.CustomGadget1));
			}
			if (this.gadget2AnimationTriggerConfig.Length > 0)
			{
				this.CheckAnimationConfig(ref this.gadget2AnimationTriggerConfig, this.GetCooldownPercentage(GadgetSlot.CustomGadget2));
			}
			if (this.boostAnimationTriggerConfig.Length > 0)
			{
				this.CheckAnimationConfig(ref this.boostAnimationTriggerConfig, this.GetCooldownPercentage(GadgetSlot.BoostGadget));
			}
		}

		private void CheckAnimationConfig(ref GadgetsPropertiesData.AnimationTriggerConfig[] animationTriggerConfigList, float percentage)
		{
			if (animationTriggerConfigList == null || !this.carGenerator)
			{
				return;
			}
			int num = 0;
			for (int i = 0; i < animationTriggerConfigList.Length; i++)
			{
				if (percentage >= animationTriggerConfigList[i].percentage)
				{
					num = i;
				}
			}
			if (animationTriggerConfigList[num].activated || !this.carGenerator.CarAnimator)
			{
				return;
			}
			this.ResetActivatedAnimations(ref animationTriggerConfigList);
			animationTriggerConfigList[num].activated = true;
			this.carGenerator.CarAnimator.SetTrigger(animationTriggerConfigList[num].triggerName);
		}

		private void ResetActivatedAnimations(ref GadgetsPropertiesData.AnimationTriggerConfig[] animationTriggerConfigList)
		{
			for (int i = 0; i < animationTriggerConfigList.Length; i++)
			{
				animationTriggerConfigList[i].activated = false;
			}
		}

		[SerializeField]
		[ReadOnly]
		private int gadget0Cooldown;

		[SerializeField]
		[ReadOnly]
		private int gadget0CooldownStart;

		[SerializeField]
		[ReadOnly]
		private int gadget1Cooldown;

		[SerializeField]
		[ReadOnly]
		private int gadget1CooldownStart;

		[SerializeField]
		[ReadOnly]
		private int gadget2Cooldown;

		[SerializeField]
		[ReadOnly]
		private int gadget2CooldownStart;

		[SerializeField]
		[ReadOnly]
		private int boostCooldown;

		[SerializeField]
		[ReadOnly]
		private int boostCooldownStart;

		[SerializeField]
		[ReadOnly]
		private int currentTime;

		[SerializeField]
		private CarGenerator carGenerator;

		[SerializeField]
		private GadgetsPropertiesData.AnimationTriggerConfig[] gadget0AnimationTriggerConfig;

		[SerializeField]
		private GadgetsPropertiesData.AnimationTriggerConfig[] gadget1AnimationTriggerConfig;

		[SerializeField]
		private GadgetsPropertiesData.AnimationTriggerConfig[] gadget2AnimationTriggerConfig;

		[SerializeField]
		private GadgetsPropertiesData.AnimationTriggerConfig[] boostAnimationTriggerConfig;

		private bool checkAnimationConfig;

		private HMMHub hub;

		[Serializable]
		public class AnimationTriggerConfig
		{
			public float percentage;

			public string triggerName;

			[HideInInspector]
			public bool activated;
		}
	}
}
