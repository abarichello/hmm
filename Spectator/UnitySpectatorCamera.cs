using System;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Utils;
using UnityEngine;

namespace HeavyMetalMachines.Spectator
{
	public class UnitySpectatorCamera : MonoBehaviour, ICameraAngle
	{
		public IHotKey HotKey
		{
			get
			{
				return this._hotKey;
			}
		}

		public Transform CameraTransform
		{
			get
			{
				return this._myTransform;
			}
		}

		public SpectatorCameraGroupType SpectatorCameraGroupType
		{
			get
			{
				return this._spectatorCameraGroupType;
			}
		}

		private void Awake()
		{
			this._myTransform = base.transform;
			if (this._cameraMan != null)
			{
				this._cameraMan.RegisterCameraAngle(this);
			}
		}

		private void OnDestroy()
		{
			if (this._cameraMan != null)
			{
				this._cameraMan.UnregisterCameraAngle(this);
			}
		}

		[SerializeField]
		private HotKeyData _hotKey;

		[SerializeField]
		private SpectatorCameraGroupType _spectatorCameraGroupType;

		private Transform _myTransform;

		[InjectOnClient]
		private ISpectatorCameraManager _cameraMan;
	}
}
