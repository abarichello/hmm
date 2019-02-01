using System;
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
			GameArenaInfo gameArenaInfo = GameHubBehaviour.Hub.ArenaConfig.Arenas[GameHubBehaviour.Hub.Match.ArenaIndex];
			this._invertedTeam = gameArenaInfo.ArenaFlipTeam;
			CarCamera.Singleton.CameraInversionIsInverted = this.IsInverted;
			if (this.IsInverted)
			{
				if (gameArenaInfo.ArenaFlipScale != 0)
				{
					Vector3 localScale = base.transform.localScale;
					localScale.x *= -1f;
					base.transform.localScale = localScale;
					for (int i = 0; i < this.DontFlipTransforms.Length; i++)
					{
						localScale = this.DontFlipTransforms[i].localScale;
						localScale.x *= (float)gameArenaInfo.ArenaFlipScale;
						this.DontFlipTransforms[i].localScale = localScale;
					}
				}
				if (gameArenaInfo.ArenaFlipRotation != 0)
				{
					this._shadowDirection.x = -this._shadowDirection.x;
					this._shadowDirection.z = -this._shadowDirection.z;
					base.transform.rotation = Quaternion.Euler(0f, (float)gameArenaInfo.ArenaFlipRotation, 0f);
				}
			}
			this._shadowDirection.Normalize();
			Shader.SetGlobalVector("_ShadowProjectDir", this._shadowDirection);
		}

		private Vector4 _shadowDirection = new Vector4(0.1572021f, -0.7891493f, -0.5937432f, 0f);

		private TeamKind _invertedTeam = TeamKind.Blue;

		public bool HorizontalFlipOnly;

		public Transform[] DontFlipTransforms;
	}
}
