using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Audio
{
	public class HMMAudioListener : GameHubBehaviour
	{
		private void Awake()
		{
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		}

		private void OnEnable()
		{
			GameHubBehaviour.Hub.State.ListenToStateChanged += this.StateOnListenToStateChanged;
		}

		private void OnDisable()
		{
			GameHubBehaviour.Hub.State.ListenToStateChanged -= this.StateOnListenToStateChanged;
		}

		private void StateOnListenToStateChanged(GameState changedState)
		{
			if (GameHubBehaviour.Hub.State.Current.StateKind != GameState.GameStateKind.Game)
			{
				this._audioListener.transform.position = Vector3.zero;
			}
		}

		private void LateUpdate()
		{
			if (GameHubBehaviour.Hub.State.Current.StateKind != GameState.GameStateKind.Game)
			{
				return;
			}
			if (null == this._carCamera.CameraTargetTransform)
			{
				this._audioListener.transform.position = this._carCamera.transform.position;
			}
			else
			{
				this._audioListener.transform.position = this._carCamera.CurrentTargetPosition + this._listenerOffset;
			}
		}

		[SerializeField]
		private FMOD_Listener _audioListener;

		[SerializeField]
		private Vector3 _listenerOffset;

		[SerializeField]
		private CarCamera _carCamera;
	}
}
