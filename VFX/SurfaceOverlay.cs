using System;
using System.Collections.Generic;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class SurfaceOverlay : GameHubBehaviour
	{
		public virtual void OnEnable()
		{
			if (this._shouldReenable == null)
			{
				return;
			}
			for (int i = 0; i < this._shouldReenable.Count; i++)
			{
				this._shouldReenable[i].renderer.enabled = true;
			}
		}

		public virtual void OnDisable()
		{
			for (int i = 0; i < this._shouldReenable.Count; i++)
			{
				this._shouldReenable[i].renderer.enabled = true;
			}
		}

		public void SetProperty(string propertyName, float value)
		{
			if (this.propertyBlock == null)
			{
				this.propertyBlock = new MaterialPropertyBlock();
			}
			int num;
			if (!this.nameIDS.TryGetValue(propertyName, out num))
			{
				num = Shader.PropertyToID(propertyName);
				this.nameIDS[propertyName] = num;
				this.propertyBlock.SetFloat(num, value);
			}
			else
			{
				this.propertyBlock.SetFloat(num, value);
			}
		}

		public void SetProperty(string propertyName, Vector3 value)
		{
			if (this.propertyBlock == null)
			{
				this.propertyBlock = new MaterialPropertyBlock();
			}
			int num;
			if (!this.nameIDS.TryGetValue(propertyName, out num))
			{
				num = Shader.PropertyToID(propertyName);
				this.nameIDS[propertyName] = num;
				this.propertyBlock.SetVector(num, value);
			}
			else
			{
				this.propertyBlock.SetVector(num, value);
			}
		}

		protected void SetTarget(Transform target)
		{
			if (target == null)
			{
				return;
			}
			List<MeshFilter> list = new List<MeshFilter>();
			foreach (MeshFilter item in target.GetComponentsInChildren<MeshFilter>(true))
			{
				list.Add(item);
			}
			this.InitializeFX(list);
		}

		public void InitializeFX(List<MeshFilter> meshFilters)
		{
			if (meshFilters == null)
			{
				return;
			}
			for (int i = 0; i < meshFilters.Count; i++)
			{
				SurfaceOverlay.RendererHolder item = default(SurfaceOverlay.RendererHolder);
				item.meshFilter = meshFilters[i];
				item.renderer = item.meshFilter.GetComponent<Renderer>();
				this._rendererHolders.Add(item);
				if (this.shouldHideObject)
				{
					if (item.renderer.enabled)
					{
						this._shouldReenable.Add(item);
					}
					item.renderer.enabled = false;
				}
			}
			this.initialized = true;
		}

		public void InitializeFX(MeshFilter[] meshFilters)
		{
			if (meshFilters == null)
			{
				return;
			}
			for (int i = 0; i < meshFilters.Length; i++)
			{
				SurfaceOverlay.RendererHolder item = default(SurfaceOverlay.RendererHolder);
				item.meshFilter = meshFilters[i];
				item.renderer = item.meshFilter.GetComponent<Renderer>();
				this._rendererHolders.Add(item);
				if (this.shouldHideObject)
				{
					if (item.renderer.enabled)
					{
						this._shouldReenable.Add(item);
					}
					item.renderer.enabled = false;
				}
			}
			this.initialized = true;
		}

		protected virtual void LateUpdate()
		{
			if (!this.initialized)
			{
				return;
			}
			for (int i = 0; i < this._rendererHolders.Count; i++)
			{
				SurfaceOverlay.RendererHolder rendererHolder = this._rendererHolders[i];
				Renderer renderer = rendererHolder.renderer;
				if (!(renderer == null))
				{
					if ((renderer.enabled && renderer.gameObject.activeInHierarchy) || this.shouldHideObject)
					{
						if (renderer.gameObject.layer != 28)
						{
							if (!(rendererHolder.meshFilter.sharedMesh == null))
							{
								if (!this.overlapMaterial)
								{
									Debug.LogError("Missing material in gameobject: " + base.gameObject.name, base.gameObject);
								}
								for (int j = 0; j < rendererHolder.meshFilter.sharedMesh.subMeshCount; j++)
								{
									Graphics.DrawMesh(rendererHolder.meshFilter.sharedMesh, rendererHolder.meshFilter.transform.localToWorldMatrix, this.overlapMaterial, base.gameObject.layer, null, j, this.propertyBlock);
								}
							}
						}
					}
				}
			}
		}

		public Material overlapMaterial;

		private bool initialized;

		public bool shouldHideObject;

		private List<SurfaceOverlay.RendererHolder> _rendererHolders = new List<SurfaceOverlay.RendererHolder>();

		private List<SurfaceOverlay.RendererHolder> _shouldReenable = new List<SurfaceOverlay.RendererHolder>();

		private Dictionary<string, int> nameIDS = new Dictionary<string, int>();

		private MaterialPropertyBlock propertyBlock;

		private struct RendererHolder
		{
			public MeshFilter meshFilter;

			public Renderer renderer;
		}
	}
}
