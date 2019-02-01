using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class DeliveryLineDropperController : GameHubBehaviour
	{
		private void OnEnable()
		{
			this._fixedDropperPiecesTotalSize = this._rightTip.localScale.x;
			GameHubBehaviour.Hub.BombManager.ListenToOvertimeStarted += this.OnOvertimeStarted;
			GameHubBehaviour.Hub.BombManager.ListenToPhaseChange += this.OnPhaseChanged;
			this.SetDropperEnabled(GameHubBehaviour.Hub.BombManager.ScoreBoard.IsInOvertime);
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				return;
			}
			bool flag = this._mimicTransform.GetComponent<BombTargetTrigger>().TeamOwner == GameHubBehaviour.Hub.Players.CurrentPlayerTeam;
			this.SetMaterialColor((!flag) ? this._enemyTeamColor : this._allyTeamColor);
			this._animator.SetTrigger((!flag) ? "init_enemy" : "init_ally");
		}

		private void OnDisable()
		{
			GameHubBehaviour.Hub.BombManager.ListenToOvertimeStarted -= this.OnOvertimeStarted;
			GameHubBehaviour.Hub.BombManager.ListenToPhaseChange -= this.OnPhaseChanged;
		}

		private void LateUpdate()
		{
			Vector3 vector = this._mimicTransform.position;
			vector.y += 0.1f;
			base.transform.position = vector;
			base.transform.rotation = this._mimicTransform.rotation;
			float x = this._mimicTransform.localScale.x;
			if (x == this._lastScale)
			{
				return;
			}
			this._lastScale = x;
			float x2 = Mathf.Max(x - this._fixedDropperPiecesTotalSize, 0f);
			vector = this._fill.localScale;
			vector.x = x2;
			this._fill.localScale = vector;
			float x3 = x * 0.5f;
			vector = this._rightTip.localPosition;
			vector.x = x3;
			this._rightTip.localPosition = vector;
			vector.x = -vector.x;
			this._leftTip.localPosition = vector;
		}

		private void SetDropperEnabled(bool isEnabled)
		{
			this._leftTip.gameObject.SetActive(isEnabled);
			this._fill.gameObject.SetActive(isEnabled);
			this._rightTip.gameObject.SetActive(isEnabled);
		}

		private void SetMaterialColor(Color color)
		{
			this._leftTip.GetComponent<Renderer>().material.SetColor("_TintColor", color);
			this._fill.GetComponent<Renderer>().material.SetColor("_TintColor", color);
			this._rightTip.GetComponent<Renderer>().material.SetColor("_TintColor", color);
		}

		private void OnOvertimeStarted()
		{
			this.SetDropperEnabled(true);
		}

		private void OnPhaseChanged(BombScoreBoard.State state)
		{
			if (state == BombScoreBoard.State.BombDelivery || state == BombScoreBoard.State.PreBomb || state == BombScoreBoard.State.Shop)
			{
				this.SetDropperEnabled(false);
			}
		}

		[SerializeField]
		[Tooltip("Color to be used in the allied delivery point")]
		private Color _allyTeamColor;

		[SerializeField]
		[Tooltip("Color to be used in the enemy delivery point")]
		private Color _enemyTeamColor;

		[SerializeField]
		private Transform _mimicTransform;

		[SerializeField]
		private Transform _leftTip;

		[SerializeField]
		private Transform _fill;

		[SerializeField]
		private Transform _rightTip;

		[SerializeField]
		private Animator _animator;

		private float _fixedDropperPiecesTotalSize;

		private float _lastScale;
	}
}
