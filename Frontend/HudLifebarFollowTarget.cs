using System;
using HeavyMetalMachines.Combat;
using Pocketverse;
using UnityEngine;
using UnityEngine.UI;

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
			this._gameCamera = ((!(GameHubBehaviour.Hub == null)) ? CarCamera.Singleton.Camera : Camera.main);
		}

		public void SetTarget(CombatObject combatObject, Transform targetTransform)
		{
			this.TargetTransform = combatObject.transform;
			if (combatObject.IsCreep)
			{
				this._offsetFromCenterX = (int)this.HudLifebarSettings.CreepOffset.x;
				this._offsetFromCenterY = (int)this.HudLifebarSettings.CreepOffset.y;
				return;
			}
			CharacterTarget character = combatObject.Player.Character.Character;
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
			Vector3 vector = this._gameCamera.WorldToViewportPoint(this.TargetTransform.position);
			Vector2 sizeDelta = this._mainCanvasRectTransform.sizeDelta;
			Vector3 vector2 = new Vector2(vector.x * sizeDelta.x, vector.y * sizeDelta.y);
			vector2.x = (float)Mathf.RoundToInt(vector2.x + (float)this._offsetFromCenterX);
			vector2.y = (float)Mathf.RoundToInt(vector2.y + (float)this._offsetFromCenterY);
			this.IsOutScreen = (vector2.x < -200f || vector2.x > sizeDelta.x + 200f || vector2.y < -200f || vector2.y > sizeDelta.y + 200f);
			if (!this.IsOutScreen)
			{
				if (this._localTransform.localPosition != vector2)
				{
					this._localTransform.localPosition = vector2;
					this._movementBlurImage.CrossFadeAlpha(1f, 0.5f, true);
				}
				else
				{
					this._movementBlurImage.CrossFadeAlpha(0f, 0.5f, true);
				}
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

		[SerializeField]
		private Image _movementBlurImage;

		private bool _isAlive = true;

		private Camera _gameCamera;

		private RectTransform _localTransform;

		private RectTransform _mainCanvasRectTransform;

		private int _offsetFromCenterX;

		private int _offsetFromCenterY;
	}
}
