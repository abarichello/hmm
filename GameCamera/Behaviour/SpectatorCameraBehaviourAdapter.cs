using System;
using Assets.Standard_Assets.Scripts.HMM.PlotKids;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using UnityEngine;

namespace HeavyMetalMachines.GameCamera.Behaviour
{
	public class SpectatorCameraBehaviourAdapter : ISpectatorCameraBehaviour
	{
		public Transform CurrentTargetTransform
		{
			get
			{
				return this._cameraEngine.CurrentTargetTransform;
			}
		}

		public bool IsOrbitalMode
		{
			get
			{
				return false;
			}
		}

		public bool IsSkyViewMode
		{
			get
			{
				return this._camera.CameraMode == CarCameraMode.SkyView;
			}
		}

		public bool IsFixedCamMode
		{
			get
			{
				return this._camera.CameraMode == CarCameraMode.StageCamera;
			}
		}

		public void ToggleOrbitalOrSkyViewMode()
		{
		}

		public void SetTarget(Transform target, bool snap)
		{
			this.SetTarget(target, snap, CarCameraMode.SkyView);
		}

		private void SetTarget(Transform target, bool snap, CarCameraMode mode)
		{
			if (target != SingletonMonoBehaviour<SpectatorController>.Instance.Dummy)
			{
				SpectatorCameraBehaviourAdapter.SpectatorCameraTarget currentTarget = this._currentTarget;
				this._currentTarget = new SpectatorCameraBehaviourAdapter.SpectatorCameraTarget
				{
					TargetTransform = target,
					Mode = mode,
					Snap = snap,
					Follow = true,
					SmoothTeleport = false
				};
				this._camera.SetTarget("SpectatorFollow", this._currentTarget);
				if (currentTarget != null)
				{
					currentTarget.Kill();
				}
			}
			else
			{
				if (this._currentTarget != null)
				{
					this._currentTarget.Kill();
					this._currentTarget = null;
				}
				IGameCamera camera = this._camera;
				string context = "Spectator";
				BaseCameraTarget baseCameraTarget = default(BaseCameraTarget);
				baseCameraTarget.TargetTransform = SingletonMonoBehaviour<SpectatorController>.Instance.Dummy;
				baseCameraTarget.Condition = (() => true);
				baseCameraTarget.Mode = CarCameraMode.SkyView;
				baseCameraTarget.Snap = false;
				baseCameraTarget.Follow = true;
				baseCameraTarget.SmoothTeleport = false;
				camera.SetTarget(context, baseCameraTarget);
			}
		}

		public void SetFixedCamera(Transform target)
		{
			this.SetTarget(target, true, CarCameraMode.StageCamera);
		}

		[InjectOnClient]
		private IGameCamera _camera;

		[InjectOnClient]
		private IGameCameraEngine _cameraEngine;

		private SpectatorCameraBehaviourAdapter.SpectatorCameraTarget _currentTarget;

		private class SpectatorCameraTarget : ICameraTarget
		{
			public SpectatorCameraTarget()
			{
				this._alive = true;
			}

			public CarCameraMode Mode { get; set; }

			public Transform TargetTransform { get; set; }

			public bool Snap { get; set; }

			public bool Follow { get; set; }

			public bool SmoothTeleport { get; set; }

			public Func<bool> Condition
			{
				get
				{
					return new Func<bool>(this.StayCondition);
				}
			}

			private bool StayCondition()
			{
				return this._alive;
			}

			public void Kill()
			{
				this._alive = false;
			}

			public override string ToString()
			{
				return string.Format("[Spectator Mode={0} Target={1} SFS={2}]", this.Mode, (!(this.TargetTransform == null)) ? this.TargetTransform.name : "null", ((!this.Snap) ? 0 : 100) + ((!this.Follow) ? 0 : 10) + ((!this.SmoothTeleport) ? 0 : 1));
			}

			private bool _alive;
		}
	}
}
