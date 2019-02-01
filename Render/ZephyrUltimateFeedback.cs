using System;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Combat.Gadget;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Render
{
	public class ZephyrUltimateFeedback : GameHubBehaviour
	{
		protected void Awake()
		{
			if (!GameHubBehaviour.Hub || (GameHubBehaviour.Hub.Net.IsServer() && !GameHubBehaviour.Hub.Net.IsTest()))
			{
				UnityEngine.Object.Destroy(this);
				return;
			}
		}

		protected void Start()
		{
			this._combat = base.GetComponentInParent<CombatObject>();
			if (this._combat == null || this._combat.Combat == null)
			{
				Debug.LogWarning("No CombatData available! Is this a Character/Car prefab?");
				base.enabled = false;
			}
			else if (this._combat.Team == GameHubBehaviour.Hub.Players.CurrentPlayerTeam)
			{
				this._teamClearMaterial = this._allyClearMaterial;
				this._teamUltimateActiveMaterial = this._allyUltimateActiveMaterial;
				this._teamUltimateReadyMaterial = this._allyUltimateReadyMaterial;
			}
			else
			{
				this._teamClearMaterial = this._enemyClearMaterial;
				this._teamUltimateActiveMaterial = this._enemyUltimateActiveMaterial;
				this._teamUltimateReadyMaterial = this._enemyUltimateReadyMaterial;
			}
		}

		protected void LateUpdate()
		{
			Material material;
			if (this._combat.GadgetStates.GetGadgetState(GadgetSlot.CustomGadget2).EffectState == EffectState.Running)
			{
				material = this._teamUltimateActiveMaterial;
			}
			else if (this._combat.GadgetStates.GetGadgetState(GadgetSlot.CustomGadget2).GadgetState == GadgetState.Ready)
			{
				material = this._teamUltimateReadyMaterial;
			}
			else
			{
				material = this._teamClearMaterial;
			}
			if (material == this._prevMaterial)
			{
				return;
			}
			this._prevMaterial = material;
			for (int i = 0; i < this._renderers.Length; i++)
			{
				this._renderers[i].material = material;
			}
		}

		protected void OnDestroy()
		{
			this._combat = null;
			this._prevMaterial = null;
			this._teamClearMaterial = null;
			this._teamUltimateActiveMaterial = null;
			this._teamUltimateReadyMaterial = null;
		}

		[SerializeField]
		private MeshRenderer[] _renderers;

		[SerializeField]
		private Material _allyClearMaterial;

		[SerializeField]
		private Material _allyUltimateReadyMaterial;

		[SerializeField]
		private Material _allyUltimateActiveMaterial;

		[SerializeField]
		private Material _enemyClearMaterial;

		[SerializeField]
		private Material _enemyUltimateReadyMaterial;

		[SerializeField]
		private Material _enemyUltimateActiveMaterial;

		private Material _teamClearMaterial;

		private Material _teamUltimateReadyMaterial;

		private Material _teamUltimateActiveMaterial;

		private CombatObject _combat;

		private Material _prevMaterial;
	}
}
