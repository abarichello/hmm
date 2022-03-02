using System;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Combat.Gadget;
using UnityEngine;

namespace HeavyMetalMachines.Render
{
	public class GadgetFeedbackTransform : BaseActivatableGadgetFeedback
	{
		private void Awake()
		{
			this.combatObject = base.GetComponentInParent<CombatObject>();
			this.gadgetsPropertiesData = base.GetComponent<GadgetsPropertiesData>();
			if (!this.combatObject && !this.gadgetsPropertiesData)
			{
				base.enabled = false;
			}
		}

		protected override void OnActivate()
		{
		}

		protected override void OnDeactivate()
		{
		}

		private void Update()
		{
			if (!this.combatObject || !this.gadgetsPropertiesData)
			{
				return;
			}
			if (base.Slot == GadgetSlot.CustomGadget2)
			{
				this.percentage = this.combatObject.Data.EP / (float)this.combatObject.Data.EPMax;
			}
			else
			{
				this.percentage = this.gadgetsPropertiesData.GetCooldownPercentage(base.Slot);
			}
			if (float.IsNaN(this.percentage))
			{
				return;
			}
			this.UpdateTagertGOs();
		}

		private void UpdateTagertGOs()
		{
			float normalizedPercentage = this.GetNormalizedPercentage();
			for (int i = 0; i < this.targetGameObjects.Length; i++)
			{
				if (this.updatePositionLinear && !this.updatePositionStepped)
				{
					this.targetGameObjects[i].transform.localPosition = Vector3.LerpUnclamped(this.minLocalPosition, this.maxLocalPosition, normalizedPercentage);
				}
				if (this.updatePositionStepped)
				{
					this.targetGameObjects[i].transform.localPosition = this.GetSteppedVector3(this.positionSteppedConfig);
				}
				if (this.updateRotationLinear && !this.updateRotationStepped)
				{
					this.targetGameObjects[i].transform.localEulerAngles = Vector3.LerpUnclamped(this.minLocalRotation, this.maxLocalRotation, normalizedPercentage);
				}
				if (this.updateRotationStepped)
				{
					this.targetGameObjects[i].transform.localEulerAngles = this.GetSteppedVector3(this.rotationSteppedConfig);
				}
				if (this.updateScaleLinear && !this.updateScaleStepped)
				{
					this.targetGameObjects[i].transform.localScale = Vector3.LerpUnclamped(this.minLocalScale, this.maxLocalScale, normalizedPercentage);
				}
				if (this.updateScaleStepped)
				{
					this.targetGameObjects[i].transform.localScale = this.GetSteppedVector3(this.scaleSteppedConfig);
				}
			}
		}

		private float GetNormalizedPercentage()
		{
			if (this.percentage >= this.maxPercentage)
			{
				return 1f;
			}
			if (this.percentage <= this.minPercentage)
			{
				return 0f;
			}
			float num = this.maxPercentage - this.minPercentage;
			float num2 = this.percentage - this.minPercentage;
			return num2 / num;
		}

		private Vector3 GetSteppedVector3(GadgetFeedbackTransform.SteppedConfig[] steppedConfigList)
		{
			int num = 0;
			for (int i = 0; i < steppedConfigList.Length; i++)
			{
				if (this.percentage >= steppedConfigList[i].percentage)
				{
					num = i;
				}
			}
			return steppedConfigList[num].value;
		}

		[Header("Script references")]
		[SerializeField]
		private GadgetsPropertiesData gadgetsPropertiesData;

		[SerializeField]
		private CombatObject combatObject;

		[Header("Main config")]
		[SerializeField]
		private GameObject[] targetGameObjects;

		[Tooltip("Change transform according to cooldown % - Not used in stepped mode")]
		[SerializeField]
		private float minPercentage;

		[SerializeField]
		private float maxPercentage = 1f;

		[Header("Position config")]
		[SerializeField]
		private bool updatePositionLinear;

		[SerializeField]
		private Vector3 minLocalPosition;

		[SerializeField]
		private Vector3 maxLocalPosition;

		[SerializeField]
		private bool updatePositionStepped;

		[SerializeField]
		private GadgetFeedbackTransform.SteppedConfig[] positionSteppedConfig;

		[Header("Rotation config")]
		[SerializeField]
		private bool updateRotationLinear;

		[SerializeField]
		private Vector3 minLocalRotation;

		[SerializeField]
		private Vector3 maxLocalRotation;

		[SerializeField]
		private bool updateRotationStepped;

		[SerializeField]
		private GadgetFeedbackTransform.SteppedConfig[] rotationSteppedConfig;

		[Header("Scale config")]
		[SerializeField]
		private bool updateScaleLinear;

		[SerializeField]
		private Vector3 minLocalScale = Vector3.one;

		[SerializeField]
		private Vector3 maxLocalScale = Vector3.one;

		[SerializeField]
		private bool updateScaleStepped;

		[SerializeField]
		private GadgetFeedbackTransform.SteppedConfig[] scaleSteppedConfig;

		[Header("Debug only")]
		[SerializeField]
		[ReadOnly]
		private float percentage;

		[Serializable]
		public class SteppedConfig
		{
			public float percentage;

			public Vector3 value;
		}
	}
}
