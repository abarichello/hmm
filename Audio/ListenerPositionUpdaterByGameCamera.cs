using System;
using HeavyMetalMachines.GameCamera;
using Hoplon.Audio;
using UniRx;
using UnityEngine;

namespace HeavyMetalMachines.Audio
{
	public class ListenerPositionUpdaterByGameCamera : IListenerPositionUpdater
	{
		public ListenerPositionUpdaterByGameCamera(IGameCameraEngine gameCameraEngine, IUpdateListenerPosition updateListenerPosition)
		{
			this._gameCameraEngine = gameCameraEngine;
			this._updateListenerPosition = updateListenerPosition;
		}

		public void Start()
		{
			this._updateDisposable = ObservableExtensions.Subscribe<long>(Observable.Do<long>(Observable.Where<long>(Observable.EveryLateUpdate(), (long frameNumber) => frameNumber % 2L == 0L), delegate(long _)
			{
				this.UpdateListenerPosition();
			}));
		}

		public void UpdateListenerPosition()
		{
			Vector3 vector;
			if (null == this._gameCameraEngine.CurrentTargetTransform)
			{
				vector = this._gameCameraEngine.CameraTransform.position;
			}
			else
			{
				vector = this._gameCameraEngine.CurrentTargetPosition + this._listenerOffset;
			}
			this._updateListenerPosition.Update(vector.x, vector.y, vector.z);
		}

		public void Stop()
		{
			if (this._updateDisposable != null)
			{
				this._updateDisposable.Dispose();
				this._updateDisposable = null;
			}
			this._updateListenerPosition.Update(0f, 0f, 0f);
		}

		private readonly IGameCameraEngine _gameCameraEngine;

		private readonly IUpdateListenerPosition _updateListenerPosition;

		private readonly Vector3 _listenerOffset = new Vector3(0f, 3f, 0f);

		private IDisposable _updateDisposable;
	}
}
