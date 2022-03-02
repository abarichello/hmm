using System;
using Pocketverse;
using UnityEngine;
using UnityEngine.Serialization;

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

		protected void Awake()
		{
			this._origMaterials = new Material[this._renderers.Length];
		}

		protected override void OnActivate()
		{
			HMMTeamMaterialVFX.replaceTypes replaceType = this._replaceType;
			if (replaceType != HMMTeamMaterialVFX.replaceTypes.material)
			{
				if (replaceType == HMMTeamMaterialVFX.replaceTypes.color)
				{
					this.ReplaceColor();
				}
			}
			else
			{
				this.ReplaceMaterial();
			}
		}

		protected override void WillDeactivate()
		{
		}

		protected override void OnDeactivate()
		{
		}

		private void OnDisable()
		{
			if (this._replaceType == HMMTeamMaterialVFX.replaceTypes.color)
			{
				return;
			}
			for (int i = 0; i < this._renderers.Length; i++)
			{
				if (this._origMaterials[i] != null)
				{
					this._renderers[i].sharedMaterial = this._origMaterials[i];
				}
			}
		}

		protected void OnValidate()
		{
			MeshRenderer[] componentsInChildren = base.GetComponentsInChildren<MeshRenderer>(true);
			SkinnedMeshRenderer[] componentsInChildren2 = base.GetComponentsInChildren<SkinnedMeshRenderer>(true);
			this._renderers = new Renderer[componentsInChildren.Length + componentsInChildren2.Length];
			Array.Copy(componentsInChildren, 0, this._renderers, 0, componentsInChildren.Length);
			Array.Copy(componentsInChildren2, 0, this._renderers, componentsInChildren.Length, componentsInChildren2.Length);
		}

		private void ReplaceColor()
		{
			bool flag;
			if (this.PrevizMode && this.CurrentTeam == VFXTeam.Enemy)
			{
				flag = false;
			}
			else
			{
				MatchPlayers players = GameHubBehaviour.Hub.Players;
				flag = (players.CurrentPlayerTeam == players.GetPlayerOrBotsByObjectId(this._targetFXInfo.Owner.ObjId).Team);
			}
			Color color = this._allyColor;
			float num = this._allyIntensity;
			if (!flag)
			{
				color = this._enemyColor;
				num = this._enemyIntensity;
			}
			this._glowPropertyId = Shader.PropertyToID("_Glow");
			this._glowColorPropertyId = Shader.PropertyToID("_GlowColor");
			MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
			for (int i = 0; i < this._renderers.Length; i++)
			{
				Material sharedMaterial = this._renderers[i].sharedMaterial;
				if (sharedMaterial.HasProperty(this._glowPropertyId) && sharedMaterial.HasProperty(this._glowColorPropertyId))
				{
					this._renderers[i].GetPropertyBlock(materialPropertyBlock);
					materialPropertyBlock.SetFloat(this._glowPropertyId, num);
					materialPropertyBlock.SetColor(this._glowColorPropertyId, color);
					this._renderers[i].SetPropertyBlock(materialPropertyBlock);
				}
			}
		}

		private void ReplaceMaterial()
		{
			if (this._teamMaterial == null || this._enemyMaterial == null)
			{
				return;
			}
			Material sharedMaterial;
			if (this.PrevizMode || this._targetFXInfo.Owner == null)
			{
				if (this.CurrentTeam == VFXTeam.Ally)
				{
					sharedMaterial = this._teamMaterial;
				}
				else
				{
					sharedMaterial = this._enemyMaterial;
				}
			}
			else
			{
				MatchPlayers players = GameHubBehaviour.Hub.Players;
				sharedMaterial = ((players.CurrentPlayerTeam != players.GetPlayerOrBotsByObjectId(this._targetFXInfo.Owner.ObjId).Team) ? this._enemyMaterial : this._teamMaterial);
			}
			for (int i = 0; i < this._renderers.Length; i++)
			{
				this._origMaterials[i] = this._renderers[i].sharedMaterial;
				this._renderers[i].sharedMaterial = sharedMaterial;
			}
		}

		[Tooltip("The material of the friendly team (blue team)")]
		[SerializeField]
		[FormerlySerializedAs("TeamMaterial")]
		private Material _teamMaterial;

		[Tooltip("The material of the enemy team (red team)")]
		[SerializeField]
		[FormerlySerializedAs("EnemyMaterial")]
		private Material _enemyMaterial;

		[SerializeField]
		[ReadOnly]
		private Renderer[] _renderers;

		[NonSerialized]
		private Material[] _origMaterials;

		public HMMTeamMaterialVFX.replaceTypes _replaceType;

		[SerializeField]
		private Color _allyColor = Color.blue;

		[SerializeField]
		private float _allyIntensity = 1f;

		[SerializeField]
		private Color _enemyColor = Color.red;

		[SerializeField]
		private float _enemyIntensity = 1f;

		private int _glowPropertyId;

		private int _glowColorPropertyId;

		public enum replaceTypes
		{
			material,
			color
		}
	}
}
