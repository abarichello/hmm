using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class DeliveryLineController : GameHubBehaviour
	{
		private void OnEnable()
		{
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				base.gameObject.SetActive(false);
				return;
			}
			float x = this._center.localScale.x;
			float x2 = this._rightTip.localScale.x;
			float num = x2 * 0.3f;
			this._halfCenterPieceSize = x * 0.5f;
			this._halfTipSize = x2 * 0.5f;
			this._fixedPiecesTotalSize = x + num * 2f;
			this.CacheAllRenderers();
			this._leftFillMaterial = this._leftFill.GetComponent<Renderer>().material;
			this._rightFillMaterial = this._rightFill.GetComponent<Renderer>().material;
			if (this._mimicTransform.GetComponent<BombTargetTrigger>().TeamOwner == GameHubBehaviour.Hub.Players.CurrentPlayerTeam)
			{
				this.SetMaterialColor(this._allyTeamColor);
			}
			else
			{
				this.SetMaterialColor(this._enemyTeamColor);
			}
		}

		private void OnDisable()
		{
			this._allRenderers = null;
		}

		private void CacheAllRenderers()
		{
			this._allRenderers = new Renderer[5];
			this._allRenderers[0] = this._leftTip.GetComponent<Renderer>();
			this._allRenderers[1] = this._leftFill.GetComponent<Renderer>();
			this._allRenderers[2] = this._center.GetComponent<Renderer>();
			this._allRenderers[3] = this._rightFill.GetComponent<Renderer>();
			this._allRenderers[4] = this._rightTip.GetComponent<Renderer>();
		}

		private void SetMaterialColor(Color color)
		{
			for (int i = 0; i < this._allRenderers.Length; i++)
			{
				this._allRenderers[i].material.SetColor("_TintColor", color);
			}
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
			float num = Mathf.Max((x - this._fixedPiecesTotalSize) * 0.5f, 0f);
			float num2 = num * 0.5f;
			vector = this._rightFill.localScale;
			vector.x = num;
			this._rightFill.localScale = vector;
			vector.x = -vector.x;
			this._leftFill.localScale = vector;
			float x2 = num / vector.y;
			vector = this._rightFill.localPosition;
			vector.x = this._halfCenterPieceSize + num2;
			this._rightFill.localPosition = vector;
			vector.x = -vector.x;
			this._leftFill.localPosition = vector;
			vector = this._rightTip.localPosition;
			vector.x = this._halfCenterPieceSize + num + this._halfTipSize;
			this._rightTip.localPosition = vector;
			vector.x = -vector.x;
			this._leftTip.localPosition = vector;
			this._leftFillMaterial.SetTextureScale("_MainTex", new Vector2(x2, 1f));
			this._rightFillMaterial.SetTextureScale("_MainTex", new Vector2(x2, 1f));
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
		private Transform _leftFill;

		[SerializeField]
		private Transform _center;

		[SerializeField]
		private Transform _rightFill;

		[SerializeField]
		private Transform _rightTip;

		private Renderer[] _allRenderers;

		private Material _leftFillMaterial;

		private Material _rightFillMaterial;

		private float _lastScale;

		private float _halfTipSize;

		private float _halfCenterPieceSize;

		private float _fixedPiecesTotalSize;
	}
}
