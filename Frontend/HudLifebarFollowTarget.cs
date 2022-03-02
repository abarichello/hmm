using System;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.GameCamera;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class HudLifebarFollowTarget : GameHubBehaviour
	{
		public bool IsAlive
		{
			get
			{
				return this._isAlive;
			}
			set
			{
				if (!this._isAlive && value)
				{
					this._isAlive = true;
					this.LateUpdate();
				}
				else
				{
					this._isAlive = value;
				}
			}
		}

		public float OffsetFromCenterY
		{
			get
			{
				return (float)this._offsetFromCenterY;
			}
			set
			{
				this._offsetFromCenterY = (int)value;
			}
		}

		public void Start()
		{
			Canvas[] componentsInParent = base.GetComponentsInParent<Canvas>();
			this._mainCanvasRectTransform = componentsInParent[componentsInParent.Length - 1].GetComponent<RectTransform>();
			this._localTransform = base.GetComponent<RectTransform>();
			this._camera = ((!(GameHubBehaviour.Hub == null)) ? this._gameCameraEngine.UnityCamera : Camera.main);
		}

		public void SetTarget(CombatObject combatObject, Transform targetTransform)
		{
			this.TargetTransform = combatObject.transform;
			CharacterTarget character = combatObject.Player.GetCharacter();
			Vector2 characterOffset = this.HudLifebarSettings.GetCharacterOffset(character);
			this._offsetFromCenterX = (int)characterOffset.x;
			this._offsetFromCenterY = (int)characterOffset.y;
		}

		public void LateUpdate()
		{
			if (!this._isAlive)
			{
				return;
			}
			Vector3 vector = this._camera.WorldToViewportPoint(this.TargetTransform.position);
			Vector2 sizeDelta = this._mainCanvasRectTransform.sizeDelta;
			Vector3 vector2 = new Vector2(vector.x * sizeDelta.x, vector.y * sizeDelta.y);
			vector2.x = (float)Mathf.RoundToInt(vector2.x + (float)this._offsetFromCenterX);
			vector2.y = (float)Mathf.RoundToInt(vector2.y + (float)this._offsetFromCenterY);
			this.IsOutScreen = (vector2.x < -200f || vector2.x > sizeDelta.x + 200f || vector2.y < -200f || vector2.y > sizeDelta.y + 200f);
			if (!this.IsOutScreen && this._localTransform.localPosition != vector2)
			{
				this._localTransform.localPosition = vector2;
			}
		}

		public void Dispose()
		{
			this.TargetTransform = null;
			this.IsAlive = false;
		}

		public HudLifebarSettings HudLifebarSettings;

		public Transform TargetTransform;

		public bool IsOutScreen;

		[InjectOnClient]
		private IGameCameraEngine _gameCameraEngine;

		private bool _isAlive = true;

		private Camera _camera;

		private RectTransform _localTransform;

		private RectTransform _mainCanvasRectTransform;

		private int _offsetFromCenterX;

		private int _offsetFromCenterY;
	}
}
