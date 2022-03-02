using System;
using Pocketverse;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines
{
	[RequireComponent(typeof(Renderer))]
	public class ScreenDigits : GameHubBehaviour
	{
		private void Awake()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				this._mainTexPropertyId = Shader.PropertyToID("_MainTex");
				this._material = base.GetComponent<Renderer>().sharedMaterial;
			}
			else
			{
				base.enabled = false;
			}
		}

		private void LateUpdate()
		{
			switch (this.Kind)
			{
			case ScreenDigits.ScreenDigitsKind.TeamRedLeft:
				this.Digit = (float)(this._playerStatsFeature.BlueTeamDeaths / 10);
				break;
			case ScreenDigits.ScreenDigitsKind.TeamRedRight:
				this.Digit = (float)(this._playerStatsFeature.BlueTeamDeaths % 10);
				break;
			case ScreenDigits.ScreenDigitsKind.TeamBluLeft:
				this.Digit = (float)(this._playerStatsFeature.RedTeamDeaths / 10);
				break;
			case ScreenDigits.ScreenDigitsKind.TeamBluRight:
				this.Digit = (float)(this._playerStatsFeature.RedTeamDeaths % 10);
				break;
			}
			this._material.SetTextureOffset(this._mainTexPropertyId, new Vector2(this.Digit * 0.0625f, 0.1f));
		}

		[Inject]
		private IPlayerStatsFeature _playerStatsFeature;

		public float Digit = 1f;

		public ScreenDigits.ScreenDigitsKind Kind;

		private int _mainTexPropertyId;

		private Material _material;

		public enum ScreenDigitsKind
		{
			TeamRedLeft,
			TeamRedRight,
			TeamBluLeft,
			TeamBluRight
		}
	}
}
