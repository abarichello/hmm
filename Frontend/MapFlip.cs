using System;
using HeavyMetalMachines.Arena;
using HeavyMetalMachines.GameCamera;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	internal class MapFlip : GameHubBehaviour
	{
		public bool IsInverted
		{
			get
			{
				return GameHubBehaviour.Hub.Players.CurrentPlayerData != null && GameHubBehaviour.Hub.Players.CurrentPlayerData.Team == this._invertedTeam;
			}
		}

		private void Awake()
		{
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				return;
			}
			IGameArenaInfo currentArena = GameHubBehaviour.Hub.ArenaConfig.GetCurrentArena();
			this._invertedTeam = currentArena.ArenaFlipTeam;
			this._gameCameraInversion.CameraInverted = this.IsInverted;
			if (this.IsInverted)
			{
				if (currentArena.ArenaFlipScale != 0)
				{
					Vector3 localScale = base.transform.localScale;
					localScale.x *= -1f;
					base.transform.localScale = localScale;
					for (int i = 0; i < this.DontFlipTransforms.Length; i++)
					{
						localScale = this.DontFlipTransforms[i].localScale;
						localScale.x *= (float)currentArena.ArenaFlipScale;
						this.DontFlipTransforms[i].localScale = localScale;
					}
				}
				if (currentArena.ArenaFlipRotation != 0)
				{
					this._shadowDirection.x = -this._shadowDirection.x;
					this._shadowDirection.z = -this._shadowDirection.z;
					base.transform.rotation = Quaternion.Euler(0f, (float)currentArena.ArenaFlipRotation, 0f);
				}
			}
			this._shadowDirection.Normalize();
			Shader.SetGlobalVector("_ShadowProjectDir", this._shadowDirection);
		}

		[InjectOnClient]
		private IGameCameraInversion _gameCameraInversion;

		private Vector4 _shadowDirection = new Vector4(0.1572021f, -0.7891493f, -0.5937432f, 0f);

		private TeamKind _invertedTeam = TeamKind.Blue;

		public bool HorizontalFlipOnly;

		public Transform[] DontFlipTransforms;
	}
}
