using System;
using System.Collections;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Render;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class FakeBombVisualController : GameHubBehaviour
	{
		private void Start()
		{
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				Object.Destroy(base.gameObject);
				return;
			}
			if (GameHubBehaviour.Hub.BombManager.ScoreBoard.CurrentState == BombScoreboardState.BombDelivery)
			{
				base.gameObject.SetActive(false);
			}
			GameHubBehaviour.Hub.BombManager.ListenToPhaseChange += this.OnPhaseChange;
			this.objectOverlay.SetColors(this.mainColor, this.outlineColor);
		}

		private void OnPhaseChange(BombScoreboardState state)
		{
			if (state != BombScoreboardState.BombDelivery && !GameHubBehaviour.Hub.BombManager.ActiveBomb.IsSpawned)
			{
				base.gameObject.SetActive(true);
			}
			else if (state == BombScoreboardState.BombDelivery)
			{
				this._timedCoroutine = base.StartCoroutine(this.SetActiveTimed(this.DelayToDesapear, false));
			}
		}

		private IEnumerator SetActiveTimed(float time, bool active)
		{
			yield return new WaitForSeconds(time);
			base.gameObject.SetActive(active);
			this._timedCoroutine = null;
			yield break;
		}

		private void OnDestroy()
		{
			if (this._timedCoroutine != null)
			{
				base.StopCoroutine(this._timedCoroutine);
			}
			GameHubBehaviour.Hub.BombManager.ListenToPhaseChange -= this.OnPhaseChange;
		}

		public float DelayToDesapear = 0.5f;

		private Coroutine _timedCoroutine;

		[Header("Effect Material Color")]
		[SerializeField]
		private ObjectOverlay objectOverlay;

		[SerializeField]
		private Color mainColor;

		[SerializeField]
		private Color outlineColor;
	}
}
