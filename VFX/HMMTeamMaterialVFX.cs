using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class HMMTeamMaterialVFX : BaseVFX
	{
		public override int Priority
		{
			get
			{
				return 2000;
			}
		}

		private void Awake()
		{
			Renderer[] componentsInChildren = base.GetComponentsInChildren<Renderer>(true);
			if (componentsInChildren != null && componentsInChildren.Length > 0)
			{
				this._materials = new Material[componentsInChildren.Length];
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					this._materials[i] = componentsInChildren[i].material;
				}
			}
		}

		private void OnDestroy()
		{
			if (this._materials != null)
			{
				for (int i = 0; i < this._materials.Length; i++)
				{
					UnityEngine.Object.Destroy(this._materials[i]);
				}
			}
			this._materials = null;
		}

		protected override void OnActivate()
		{
			if (this._materials != null)
			{
				MatchPlayers players = GameHubBehaviour.Hub.Players;
				Material mat = (players.CurrentPlayerTeam != players.GetPlayerOrBotsByObjectId(this._targetFXInfo.Owner.ObjId).Team) ? this.EnemyMaterial : this.TeamMaterial;
				for (int i = 0; i < this._materials.Length; i++)
				{
					this._materials[i].CopyPropertiesFromMaterial(mat);
				}
			}
		}

		protected override void WillDeactivate()
		{
		}

		protected override void OnDeactivate()
		{
		}

		[Tooltip("The material of the friendly team (blue team)")]
		public Material TeamMaterial;

		[Tooltip("The material of the enemy team (red team)")]
		public Material EnemyMaterial;

		private Material[] _materials;
	}
}
