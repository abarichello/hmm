using System;
using System.Collections.Generic;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines
{
	internal class OccluderSolver : GameHubBehaviour
	{
		public bool IsOccluding
		{
			get
			{
				return this._isOccluding;
			}
			set
			{
				if (this._isOccluding != value)
				{
					this._isOccluding = value;
					this.SwapOccludingState();
					return;
				}
				this._isOccluding = value;
			}
		}

		public override void OnBeforeSerialize()
		{
			base.OnBeforeSerialize();
			this.IsOccluding = false;
		}

		private void Update()
		{
			if (this._isOccluding && !OccluderSolver.ForceRemoveOcclusion)
			{
				this.fade += Time.deltaTime * 5f;
				if (this.fade > 1f)
				{
					this.fade = 1f;
				}
			}
			else
			{
				this.fade -= Time.deltaTime;
				if (this.fade <= 0f)
				{
					this.fade = 0f;
					for (int i = 0; i < this.materialData.Length; i++)
					{
						OccluderSolver.MaterialData materialData = this.materialData[i];
						materialData.material.shader = materialData.originalMaterialShader;
						materialData.renderer.receiveShadows = true;
					}
				}
			}
			for (int j = 0; j < this.materialData.Length; j++)
			{
				OccluderSolver.MaterialData materialData2 = this.materialData[j];
				materialData2.material.SetFloat("_OcclusionAmmount", 1f - this.fade);
			}
		}

		private void OnEnable()
		{
			if (this.target != null)
			{
				this.renderers = this.target.GetComponentsInChildren<Renderer>();
			}
			else
			{
				this.renderers = base.gameObject.GetComponentsInChildren<Renderer>();
			}
			List<OccluderSolver.MaterialData> list = new List<OccluderSolver.MaterialData>(this.renderers.Length);
			for (int i = 0; i < this.renderers.Length; i++)
			{
				Renderer renderer = this.renderers[i];
				Material[] materials = renderer.materials;
				for (int j = 0; j < renderer.materials.Length; j++)
				{
					Material material = materials[j];
					Shader shader = Shader.Find(material.shader.name + " Occluded");
					if (shader)
					{
						list.Add(new OccluderSolver.MaterialData
						{
							material = material,
							materialShader = shader,
							renderer = renderer,
							originalMaterialShader = material.shader
						});
					}
				}
			}
			this.materialData = list.ToArray();
		}

		private void SwapOccludingState()
		{
			if (this._isOccluding)
			{
				for (int i = 0; i < this.materialData.Length; i++)
				{
					OccluderSolver.MaterialData materialData = this.materialData[i];
					materialData.material.shader = materialData.materialShader;
					materialData.renderer.receiveShadows = false;
				}
			}
		}

		private Renderer[] renderers;

		private OccluderSolver.MaterialData[] materialData;

		private float fade;

		public GameObject target;

		public static bool ForceRemoveOcclusion;

		private bool _isOccluding;

		private struct MaterialData
		{
			public Material material;

			public Renderer renderer;

			public Shader originalMaterialShader;

			public Shader materialShader;
		}
	}
}
