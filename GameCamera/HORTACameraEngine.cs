using System;
using System.Collections.Generic;
using HeavyMetalMachines.GameCamera.Behaviour;
using Pocketverse.MuralContext;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.GameCamera
{
	[RequireComponent(typeof(Camera))]
	public class HORTACameraEngine : MonoBehaviour, ICleanupListener, IGameCameraEngine
	{
		public Camera UnityCamera { get; private set; }

		public Transform CameraTransform { get; private set; }

		public void Enable()
		{
			base.enabled = true;
		}

		public void Disable()
		{
			base.enabled = false;
		}

		public bool IsEnabled()
		{
			return base.enabled;
		}

		public Transform CurrentTargetTransform
		{
			get
			{
				return this._behaviours[this._currentBehaviourIndex].CurrentTargetTransform;
			}
		}

		public Vector3 CurrentTargetPosition
		{
			get
			{
				return this._behaviours[this._currentBehaviourIndex].CurrentTargetPosition;
			}
		}

		private void Awake()
		{
			this.UnityCamera = base.GetComponent<Camera>();
			this.CameraTransform = base.transform;
			if (this._behaviours.Count > 0)
			{
				this._currentBehaviourIndex = this._behaviours.Count - 1;
				this._behaviours[this._currentBehaviourIndex].Enable(this._descriptorTarget);
			}
			this._descriptorTarget = new CameraState
			{
				Fov = this.UnityCamera.fieldOfView,
				NearPlane = this.UnityCamera.nearClipPlane,
				FarPlane = this.UnityCamera.farClipPlane,
				StartFog = RenderSettings.fogStartDistance,
				EndFog = RenderSettings.fogEndDistance,
				Position = this.CameraTransform.position,
				Rotation = this.CameraTransform.rotation
			};
		}

		private void LateUpdate()
		{
			this.UpdateFirstActiveBehaviour();
			this.ApplyCameraState(this._descriptorTarget);
		}

		private void UpdateFirstActiveBehaviour()
		{
			for (int i = 0; i < this._behaviours.Count; i++)
			{
				IGameCameraBehaviour gameCameraBehaviour = this._behaviours[i];
				if (gameCameraBehaviour.IsActive)
				{
					if (this._currentBehaviourIndex != i)
					{
						this._behaviours[this._currentBehaviourIndex].Disable();
						gameCameraBehaviour.Enable(this._descriptorTarget);
						this._currentBehaviourIndex = i;
					}
					this._descriptorTarget = gameCameraBehaviour.Update(this, this._descriptorTarget);
					break;
				}
			}
		}

		private void ApplyCameraState(CameraState state)
		{
			this.UnityCamera.fieldOfView = state.Fov;
			this.UnityCamera.nearClipPlane = state.NearPlane;
			this.UnityCamera.farClipPlane = state.FarPlane;
			RenderSettings.fogStartDistance = state.StartFog;
			RenderSettings.fogEndDistance = state.EndFog;
			this.CameraTransform.position = state.Position;
			this.CameraTransform.rotation = state.Rotation;
		}

		public void OnCleanup(CleanupMessage msg)
		{
			foreach (IGameCameraBehaviour gameCameraBehaviour in this._behaviours)
			{
				gameCameraBehaviour.Cleanup();
			}
			this._cameraPlayer.Cleanup();
		}

		[Inject]
		private IGameCameraPlayerSetup _cameraPlayer;

		[Inject]
		private List<IGameCameraBehaviour> _behaviours;

		private int _currentBehaviourIndex = -1;

		private CameraState _descriptorTarget;
	}
}
