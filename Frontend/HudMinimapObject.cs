using System;
using Pocketverse;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.Frontend
{
	public abstract class HudMinimapObject : GameHubBehaviour
	{
		public virtual void Setup()
		{
			this.Setup(true);
		}

		public virtual void Setup(bool compensateRotation)
		{
			if (compensateRotation)
			{
				Quaternion localRotation = base.transform.parent.localRotation;
				localRotation.z = -localRotation.z;
				base.transform.localRotation = localRotation;
			}
			this.CalculateMinimapProportions();
		}

		public void CalculateMinimapProportions()
		{
			GameArenaInfo gameArenaInfo = GameHubBehaviour.Hub.ArenaConfig.Arenas[GameHubBehaviour.Hub.Match.ArenaIndex];
			float preferredWidth = this._minimapBackgroundImage.preferredWidth;
			float preferredHeight = this._minimapBackgroundImage.preferredHeight;
			float num = (float)gameArenaInfo.MapSize;
			this._minimapProportion = new Vector2(preferredWidth / num, preferredHeight / num);
		}

		public abstract void OnUpdate();

		protected void UpdatePosition(Vector3 targetPosition)
		{
			this.UpdatePosition(base.transform, targetPosition);
		}

		protected void UpdatePosition(Transform localGuiTransform, Vector3 targetPosition)
		{
			Vector2 vector;
			vector.x = this._minimapProportion.x * targetPosition.x;
			vector.y = this._minimapProportion.y * targetPosition.z;
			localGuiTransform.localPosition = new Vector3(vector.x, vector.y, 0f);
		}

		protected void UpdateRotation(Transform localGuiTransform, Quaternion targetRotation)
		{
			Quaternion localRotation = localGuiTransform.localRotation;
			Vector3 eulerAngles = localRotation.eulerAngles;
			float new_z = -targetRotation.eulerAngles.y;
			eulerAngles.Set(0f, 0f, new_z);
			localRotation.eulerAngles = eulerAngles;
			localGuiTransform.localRotation = localRotation;
		}

		[NonSerialized]
		protected Transform TargetTransform;

		[SerializeField]
		private Image _minimapBackgroundImage;

		private Vector2 _minimapProportion = Vector2.one;
	}
}
