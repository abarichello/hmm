using System;
using System.Collections.Generic;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	[ExecuteInEditMode]
	public class GooSurfaceRender : GameHubBehaviour
	{
		public static T GetComponent<T>(Transform trans) where T : Component
		{
			T t = (T)((object)null);
			while (trans != null && t == null)
			{
				t = trans.GetComponent<T>();
				trans = trans.parent;
			}
			return t;
		}

		public void OnEnable()
		{
			List<MeshFilter> list = new List<MeshFilter>();
			foreach (MeshFilter item in base.GetComponentsInChildren<MeshFilter>(true))
			{
				list.Add(item);
			}
			this.InitializeFx(list, this.OverlapMaterial);
			this.lastTime = Time.realtimeSinceStartup;
		}

		private void OnDestroy()
		{
			if (this.overlapMaterialInstance)
			{
				Object.Destroy(this.overlapMaterialInstance);
			}
		}

		public void Start()
		{
			if (GameHubBehaviour.Hub != null && !GameHubBehaviour.Hub.Net.IsClient())
			{
				base.enabled = false;
				return;
			}
			if (this.overlapMaterialInstance == null && this.OverlapMaterial != null)
			{
				this.overlapMaterialInstance = Object.Instantiate<Material>(this.OverlapMaterial);
			}
		}

		public void InitializeFx(List<MeshFilter> meshFilters, Material overlayMaterial)
		{
			if (meshFilters == null)
			{
				return;
			}
			for (int i = 0; i < meshFilters.Count; i++)
			{
				GooSurfaceRender.RendererHolder item = default(GooSurfaceRender.RendererHolder);
				item.MeshFilter = meshFilters[i];
				item.Renderer = item.MeshFilter.GetComponent<Renderer>();
				this._rendererHolders.Add(item);
			}
			this._initialized = true;
		}

		public void InitializeFx(MeshFilter[] meshFilters, Material overlayMaterial)
		{
			if (meshFilters == null)
			{
				return;
			}
			for (int i = 0; i < meshFilters.Length; i++)
			{
				GooSurfaceRender.RendererHolder item = default(GooSurfaceRender.RendererHolder);
				item.MeshFilter = meshFilters[i];
				item.Renderer = item.MeshFilter.GetComponent<Renderer>();
				this._rendererHolders.Add(item);
			}
			this._initialized = true;
		}

		public void LateUpdate()
		{
			if (!this._initialized || this.overlapMaterialInstance == null)
			{
				return;
			}
			if (this.overlapMaterialInstance.HasProperty("_Percent"))
			{
				this.overlapMaterialInstance.SetFloat("_Percent", this._currentPercent * 0.8f + 0.4f);
			}
			if (this.fadeOut)
			{
				this.alpha -= Time.deltaTime * 4f;
			}
			else
			{
				this.alpha += Time.deltaTime * 4f;
			}
			this.alpha = Mathf.Clamp01(this.alpha);
			this.overlapMaterialInstance.color = new Color(1f, 1f, 1f, this.alpha);
			if (this.fadeOut && this.alpha == 0f)
			{
				Object.Destroy(this);
			}
			float num = Time.realtimeSinceStartup - this.lastTime;
			this.lastTime = Time.realtimeSinceStartup;
			this._currentPercent += num * (this.Percent - this._currentPercent);
			this._currentPercent = Mathf.Clamp(this._currentPercent, 0f, this.Percent);
			for (int i = 0; i < this._rendererHolders.Count; i++)
			{
				GooSurfaceRender.RendererHolder rendererHolder = this._rendererHolders[i];
				Renderer renderer = rendererHolder.Renderer;
				if (!(renderer == null))
				{
					if (renderer.enabled && renderer.gameObject.activeSelf)
					{
						if (renderer.gameObject.layer != 28)
						{
							if (!(rendererHolder.MeshFilter.sharedMesh == null))
							{
								for (int j = 0; j < rendererHolder.MeshFilter.sharedMesh.subMeshCount; j++)
								{
									Graphics.DrawMesh(rendererHolder.MeshFilter.sharedMesh, rendererHolder.MeshFilter.transform.localToWorldMatrix, this.overlapMaterialInstance, rendererHolder.Renderer.gameObject.layer);
								}
							}
						}
					}
				}
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(GooSurfaceRender));

		public Material OverlapMaterial;

		private Material overlapMaterialInstance;

		[Range(0f, 1f)]
		public float Percent;

		private float _currentPercent;

		private bool _initialized;

		private readonly List<GooSurfaceRender.RendererHolder> _rendererHolders = new List<GooSurfaceRender.RendererHolder>();

		private float lastTime;

		private float alpha = 1f;

		public bool fadeOut;

		private struct RendererHolder
		{
			public MeshFilter MeshFilter;

			public Renderer Renderer;
		}
	}
}
