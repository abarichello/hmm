using System;
using System.Collections;
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
				UnityEngine.Object.Destroy(base.gameObject);
				return;
			}
			if (GameHubBehaviour.Hub.BombManager.ScoreBoard.CurrentState == BombScoreBoard.State.BombDelivery)
			{
				base.gameObject.SetActive(false);
			}
			GameHubBehaviour.Hub.BombManager.ListenToPhaseChange += this.OnPhaseChange;
		}

		private void OnPhaseChange(BombScoreBoard.State state)
		{
			if (state != BombScoreBoard.State.BombDelivery && !GameHubBehaviour.Hub.BombManager.ActiveBomb.IsSpawned)
			{
				base.gameObject.SetActive(true);
			}
			else if (state == BombScoreBoard.State.BombDelivery)
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
	}
}
