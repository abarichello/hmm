using System;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.GameCamera;
using HeavyMetalMachines.Match;
using Hoplon.Unity.Loading;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Car
{
	public class CarIndicatorArrow : GameHubBehaviour
	{
		public void Initialize(Transform carTransform, TeamKind carTeam, Transform arrowsTransform, int playerObjId, IGameCameraEngine gameCameraEngine)
		{
			this._gameCameraEngine = gameCameraEngine;
			this._isIndicatorArrowActive = (!GameHubBehaviour.Hub.Options.Game.ShowObjectiveIndicator && !GameHubBehaviour.Hub.Match.LevelIsTutorial());
			GameHubBehaviour.Hub.Options.Game.ShowObjectiveIndicatorChanged += this.GameOnShowObjectiveIndicatorChanged;
			this._carIndicatorArrowTextures = new Texture[CarIndicatorArrow.CarIndicatorArrowTextureNames.Length];
			for (int i = 0; i < CarIndicatorArrow.CarIndicatorArrowTextureNames.Length; i++)
			{
				this._carIndicatorArrowTextures[i] = (Texture)Loading.Content.GetAsset(CarIndicatorArrow.CarIndicatorArrowTextureNames[i]).Asset;
			}
			this._carArrowsMeshRenderer = arrowsTransform.GetComponent<MeshRenderer>();
			Vector3 localScale = arrowsTransform.localScale;
			CarIndicatorArrowSettings carIndicatorArrowSettings = GameHubBehaviour.Hub.SharedConfigs.CarIndicatorArrowSettings;
			arrowsTransform.localScale = new Vector3(localScale.x * carIndicatorArrowSettings.ScaleArrowsModifierX, localScale.y * carIndicatorArrowSettings.ScaleArrowsModifierY, localScale.z * carIndicatorArrowSettings.ScaleArrowsModifierZ);
			this._arrowsTransform = arrowsTransform;
			this._playerTransform = carTransform;
			this._playerTeam = carTeam;
			this._timedUpdater = new TimedUpdater(500, true, true);
			this._isOtherPlayerCarryingBomb = false;
			this._otherPlayerTransform = null;
			this._tintColorPropertyId = Shader.PropertyToID("_TintColor");
			this._mainTexPropertyId = Shader.PropertyToID("_MainTex");
			if (!this._isIndicatorArrowActive)
			{
				this.DisableRenderArrow();
			}
			this.UpdateBombObjective();
		}

		private void GameOnShowObjectiveIndicatorChanged(bool isObjectiveIndicatorEnabled)
		{
			this._isIndicatorArrowActive = (!isObjectiveIndicatorEnabled && !GameHubBehaviour.Hub.Match.LevelIsTutorial());
			if (!this._isIndicatorArrowActive)
			{
				this.DisableRenderArrow();
			}
			this.UpdateBombObjective();
		}

		public void UpdateObjective(Vector3 newTargetPosition, bool isBombGame)
		{
			this._isBombGame = isBombGame;
			this._arrowTargetPosition = newTargetPosition;
			if (this._isIndicatorArrowActive)
			{
				this.EnableRenderArrow();
			}
		}

		public void UpdateBombObjective()
		{
			if (GameHubBehaviour.Hub.BombManager.IsCarryingBomb(this._playerTransform))
			{
				this.DisableRenderArrow();
			}
			else
			{
				Vector3 zero = Vector3.zero;
				GameHubBehaviour.Hub.BombManager.GetBombPosition(ref zero);
				this.UpdateObjective(zero, true);
			}
		}

		private void OnDestroy()
		{
			this._arrowsTransform = null;
			this._otherPlayerTransform = null;
			GameHubBehaviour.Hub.Options.Game.ShowObjectiveIndicatorChanged -= this.GameOnShowObjectiveIndicatorChanged;
		}

		private void EnableRenderArrow()
		{
			if (this._carArrowsMeshRenderer.enabled && this._renderArrow)
			{
				return;
			}
			this._renderArrow = true;
			this._carArrowsMeshRenderer.enabled = true;
		}

		private void DisableRenderArrow()
		{
			if (!this._carArrowsMeshRenderer.enabled && !this._renderArrow)
			{
				return;
			}
			this._renderArrow = false;
			this._arrowTargetPosition = Vector3.zero;
			this._carArrowsMeshRenderer.enabled = false;
			this._desiredColor = (this._currentColor = GameHubBehaviour.Hub.SharedConfigs.CarIndicatorArrowSettings.NeutralColor * this.ShaderColorOffset);
			this._carArrowsMeshRenderer.material.SetColor(this._tintColorPropertyId, this._currentColor);
		}

		private void LateUpdate()
		{
			if (this._arrowsTransform == null)
			{
				return;
			}
			if (this._isBombGame)
			{
				if (!GameHubBehaviour.Hub.BombManager.ActiveBomb.IsSpawned)
				{
					this.DisableRenderArrow();
					return;
				}
				this.UpdateBombObjective();
			}
			if (this._renderArrow)
			{
				Vector3 position = this._arrowsTransform.parent.position;
				Vector3 normalized = (this._arrowTargetPosition - position).normalized;
				Vector3 vector = position + normalized * (float)GameHubBehaviour.Hub.SharedConfigs.CarIndicatorArrowSettings.CarArrowDistance;
				float num = Vector3.Distance(vector, this._arrowTargetPosition);
				if (num <= 25f)
				{
					if (this._lastArrowPosition == Vector3.zero)
					{
						this._transitionDistanceCount = 0f;
						this._lastArrowPosition = vector;
					}
					Vector3 vector2;
					if (!this._isOtherPlayerCarryingBomb)
					{
						vector2 = this._arrowTargetPosition + Vector3.up * 10f;
					}
					else
					{
						vector2 = this._otherPlayerTransform.position + Vector3.up * 20f;
					}
					this._transitionDistanceCount += 70f * Time.deltaTime;
					if (this._transitionDistanceCount > 25f)
					{
						this._transitionDistanceCount = 25f;
						this._arrowsTransform.position = vector2;
					}
					else
					{
						this._arrowsTransform.position = Vector3.MoveTowards(this._lastArrowPosition, vector2, this._transitionDistanceCount);
					}
					this._arrowsTransform.LookAt(this._arrowTargetPosition);
					Vector3 vector3 = this._gameCameraEngine.CameraTransform.position - this._arrowsTransform.position;
					vector3.y = 0f;
					if (this._transitionDistanceCount >= 20f)
					{
						this._arrowsTransform.rotation = Quaternion.LookRotation(vector3, Vector3.up);
					}
					this._arrowWasUp = true;
				}
				else if (this._arrowWasUp)
				{
					if (!this._upAnimationAlreadyStarted)
					{
						this._transitionDistanceCount = 0f;
						this._lastArrowPosition = this._arrowTargetPosition + Vector3.up * 10f;
						this._upAnimationAlreadyStarted = true;
					}
					this._transitionDistanceCount += 70f * Time.deltaTime;
					if (this._transitionDistanceCount > 25f)
					{
						this._transitionDistanceCount = 25f;
						this._arrowsTransform.position = vector;
						this.ArrowTransitionResetData();
						this._arrowWasUp = false;
						this._upAnimationAlreadyStarted = false;
					}
					else
					{
						this._arrowsTransform.position = Vector3.MoveTowards(this._lastArrowPosition, vector, this._transitionDistanceCount);
					}
					this._arrowsTransform.LookAt(this._arrowTargetPosition);
				}
				else
				{
					this._arrowsTransform.position = vector;
					this._arrowsTransform.LookAt(this._arrowTargetPosition);
				}
			}
			if (this._desiredColor != this._currentColor)
			{
				this._colorTime += Time.unscaledDeltaTime;
				if (this._colorTime > 0.5f)
				{
					this._colorTime = 0.5f;
				}
				this._carArrowsMeshRenderer.material.SetColor(this._tintColorPropertyId, Color.Lerp(this._currentColor, this._desiredColor, this._colorTime / 0.5f));
				if (this._colorTime >= 0.5f)
				{
					this._currentColor = this._desiredColor;
				}
			}
			if (this._timedUpdater.ShouldHalt())
			{
				return;
			}
			this.UpdateMeshRenderer(this._arrowTargetPosition);
			this._isOtherPlayerCarryingBomb = false;
			this._otherPlayerTransform = null;
		}

		private void UpdateMeshRenderer(Vector3 arrowTargetPosition)
		{
			int distanceIndex = this.GetDistanceIndex(this._carIndicatorArrowTextures.Length, 200, arrowTargetPosition);
			Texture texture = this._carIndicatorArrowTextures[distanceIndex];
			this._carArrowsMeshRenderer.material.SetTexture(this._mainTexPropertyId, texture);
			BombInstance activeBomb = GameHubBehaviour.Hub.BombManager.ActiveBomb;
			CarIndicatorArrowSettings carIndicatorArrowSettings = GameHubBehaviour.Hub.SharedConfigs.CarIndicatorArrowSettings;
			Color color;
			if (activeBomb.TeamOwner != TeamKind.Zero)
			{
				color = ((activeBomb.TeamOwner != this._playerTeam) ? (carIndicatorArrowSettings.EnemyColor * this.ShaderColorOffset) : (carIndicatorArrowSettings.AllyColor * this.ShaderColorOffset));
			}
			else
			{
				color = carIndicatorArrowSettings.NeutralColor * this.ShaderColorOffset;
			}
			if (this._desiredColor != color)
			{
				this._currentColor = this._desiredColor;
				this._desiredColor = color;
				this._colorTime = 0f;
			}
		}

		private void ArrowTransitionResetData()
		{
			this._lastArrowPosition = Vector3.zero;
			this._transitionDistanceCount = 0f;
		}

		private int GetDistanceIndex(int maxIndex, int maxDistance, Vector3 targetPosition)
		{
			Vector3 position = this._playerTransform.position;
			float num = Vector3.Distance(position, targetPosition);
			if (num >= (float)maxDistance)
			{
				return maxIndex - 1;
			}
			int num2 = Mathf.RoundToInt((float)maxIndex * num / (float)maxDistance) - 1;
			return Mathf.Max(0, num2);
		}

		private bool _isIndicatorArrowActive;

		private const int UpdateFrequencyInMillis = 500;

		private const int ArrowMaxIndexDistance = 200;

		[SerializeField]
		private float PathArrowSlerpRotationModifier = 4f;

		private const int ArrowTransitionDistance = 25;

		private const int ArrowTransitionFixRotationDistance = 20;

		private const int ArrowTransitionSpeed = 70;

		private const int ArrowTransitionTargetBombOffset = 10;

		private const int ArrowTransitionTargetPlayerOffset = 20;

		private const float ColorTimeInSec = 0.5f;

		private readonly Color ShaderColorOffset = new Color(0.5f, 0.5f, 0.5f, 1f);

		public static readonly string[] CarIndicatorArrowTextureNames = new string[]
		{
			"car_indicator_arrow_1_texture",
			"car_indicator_arrow_2_texture",
			"car_indicator_arrow_3_texture",
			"car_indicator_arrow_4_texture"
		};

		private Texture[] _carIndicatorArrowTextures;

		private Transform _arrowsTransform;

		private Vector3 _arrowTargetPosition;

		private MeshRenderer _carArrowsMeshRenderer;

		private Transform _playerTransform;

		private TeamKind _playerTeam;

		private TimedUpdater _timedUpdater;

		private bool _renderArrow;

		private bool _isOtherPlayerCarryingBomb;

		private float _transitionDistanceCount;

		private Vector3 _lastArrowPosition;

		private Color _currentColor;

		private Color _desiredColor;

		private float _colorTime;

		private Transform _otherPlayerTransform;

		private int _tintColorPropertyId = -1;

		private int _mainTexPropertyId = -1;

		private IGameCameraEngine _gameCameraEngine;

		private bool _isBombGame;

		private bool _arrowWasUp;

		private bool _upAnimationAlreadyStarted;
	}
}
