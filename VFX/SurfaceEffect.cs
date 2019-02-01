using System;
using System.Collections.Generic;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class SurfaceEffect : GameHubBehaviour
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

		private void OnEnable()
		{
			if (this._combatObject != null && this._combatObject.IsLocalPlayer)
			{
				this._combatObject.OnDamageReceived += this.OnDamageReceived;
				this._combatObject.OnRepairReceived += this.OnRepairReceived;
				this._combatObject.OnDamageDealt += this.OnDamageDealt;
				this._combatObject.OnRepairDealt += this.OnRepairDealt;
			}
			if (this._shouldReenable == null)
			{
				return;
			}
			for (int i = 0; i < this._shouldReenable.Count; i++)
			{
				this._shouldReenable[i].renderer.enabled = true;
			}
		}

		private void Awake()
		{
			if (GameHubBehaviour.Hub && !GameHubBehaviour.Hub.Net.IsClient() && !GameHubBehaviour.Hub.Net.IsTest())
			{
				base.enabled = false;
				return;
			}
			this._colorPropertyId = Shader.PropertyToID("_Color");
			this._tintColorPropertyId = Shader.PropertyToID("_TintColor");
			this.propertyBlock = new MaterialPropertyBlock();
			this.propertyBlock.SetColor(this._colorPropertyId, Color.white);
			this._transform = base.transform;
			List<MeshFilter> list = new List<MeshFilter>();
			foreach (MeshFilter item in base.GetComponentsInChildren<MeshFilter>(true))
			{
				list.Add(item);
			}
			this.InitializeFX(list, this.overlapMaterial);
			this._combatObject = SurfaceEffect.GetComponent<CombatObject>(this._transform);
		}

		private void OnDestroy()
		{
			this.propertyBlock = null;
		}

		private void OnRepairDealt(float amount, int id)
		{
			PlayerData playerOrBotsByObjectId = GameHubBehaviour.Hub.Players.GetPlayerOrBotsByObjectId(id);
			if (playerOrBotsByObjectId == null)
			{
				return;
			}
			CarComponentHub component = playerOrBotsByObjectId.CharacterInstance.GetComponent<CarComponentHub>();
			component.surfaceEffect.Blink(GUIColorsInfo.Instance.HealColor);
		}

		private void OnDamageDealt(float amount, int id)
		{
			PlayerData playerOrBotsByObjectId = GameHubBehaviour.Hub.Players.GetPlayerOrBotsByObjectId(id);
			if (playerOrBotsByObjectId == null)
			{
				return;
			}
			CarComponentHub component = playerOrBotsByObjectId.CharacterInstance.GetComponent<CarComponentHub>();
			component.surfaceEffect.Blink(GUIColorsInfo.Instance.DamageColor);
		}

		private void OnRepairReceived(float amount, int id)
		{
			this.Blink(GUIColorsInfo.Instance.OwnDamageColor);
		}

		private void OnDamageReceived(float amount, int id)
		{
			this.Blink(GUIColorsInfo.Instance.OwnDamageColor);
		}

		private void Blink(Color color)
		{
			this.shouldBlink = true;
			this.blinkTime = 0f;
			this.propertyBlock.SetColor(this._colorPropertyId, color);
		}

		public void OnDisable()
		{
			if (this._combatObject != null && this._combatObject.IsLocalPlayer)
			{
				this._combatObject.OnDamageReceived -= this.OnDamageReceived;
				this._combatObject.OnRepairReceived -= this.OnRepairReceived;
				this._combatObject.OnDamageDealt -= this.OnDamageDealt;
				this._combatObject.OnRepairDealt -= this.OnRepairDealt;
			}
			for (int i = 0; i < this._shouldReenable.Count; i++)
			{
				this._shouldReenable[i].renderer.enabled = true;
			}
		}

		public void InitializeFX(List<MeshFilter> meshFilters, Material overlayMaterial)
		{
			if (meshFilters == null)
			{
				return;
			}
			for (int i = 0; i < meshFilters.Count; i++)
			{
				SurfaceEffect.RendererHolder item = default(SurfaceEffect.RendererHolder);
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
			this.overlapMaterial = overlayMaterial;
			this.initialized = true;
		}

		public void InitializeFX(MeshFilter[] meshFilters, Material overlayMaterial)
		{
			if (meshFilters == null)
			{
				return;
			}
			for (int i = 0; i < meshFilters.Length; i++)
			{
				SurfaceEffect.RendererHolder item = default(SurfaceEffect.RendererHolder);
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
			this.overlapMaterial = overlayMaterial;
			this.initialized = true;
		}

		private void LateUpdate()
		{
			if (!this.initialized)
			{
				return;
			}
			if (this.shouldBlink || this.blinkTime != 0f)
			{
				this.shouldBlink = false;
				this.blinkTime += Time.deltaTime * ((!this.isDoubleAlpha) ? this.blinkFrequency : (this.blinkFrequency + this.blinkFrequency));
				if (this.blinkTime > 3.14159274f)
				{
					this.blinkTime = 0f;
				}
				for (int i = 0; i < this._rendererHolders.Count; i++)
				{
					SurfaceEffect.RendererHolder rendererHolder = this._rendererHolders[i];
					Renderer renderer = rendererHolder.renderer;
					if (!(renderer == null))
					{
						if ((renderer.enabled && renderer.gameObject.activeInHierarchy) || this.shouldHideObject)
						{
							if (renderer.gameObject.layer != 28)
							{
								if (!(rendererHolder.meshFilter.sharedMesh == null))
								{
									float num = Mathf.Sin(this.blinkTime);
									if (this.isDoubleAlpha)
									{
										num *= 0.5f;
									}
									if (!this.overlapMaterial)
									{
										Debug.LogError("Missing material in gameobject: " + base.gameObject.name, base.gameObject);
									}
									if (this.overlapMaterial.HasProperty(this._colorPropertyId))
									{
										Color color = this.overlapMaterial.GetColor(this._colorPropertyId);
										color.a = num;
										this.overlapMaterial.SetColor(this._colorPropertyId, color);
									}
									else if (this.overlapMaterial.HasProperty(this._tintColorPropertyId))
									{
										Color color2 = this.overlapMaterial.GetColor(this._tintColorPropertyId);
										color2.a = num;
										this.overlapMaterial.SetColor(this._tintColorPropertyId, color2);
									}
									for (int j = 0; j < rendererHolder.meshFilter.sharedMesh.subMeshCount; j++)
									{
										Graphics.DrawMesh(rendererHolder.meshFilter.sharedMesh, rendererHolder.meshFilter.transform.localToWorldMatrix, this.overlapMaterial, base.gameObject.layer, CarCamera.Singleton.GetComponent<Camera>(), j, this.propertyBlock);
									}
								}
							}
						}
					}
				}
				return;
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(SurfaceEffect));

		public Material overlapMaterial;

		public float blinkFrequency = 4f;

		private bool initialized;

		public bool shouldHideObject;

		public bool isDoubleAlpha;

		public bool shouldBlink;

		private float blinkTime;

		private List<SurfaceEffect.RendererHolder> _rendererHolders = new List<SurfaceEffect.RendererHolder>();

		private List<SurfaceEffect.RendererHolder> _shouldReenable = new List<SurfaceEffect.RendererHolder>();

		private MaterialPropertyBlock propertyBlock;

		private CombatObject _combatObject;

		private Transform _transform;

		private float _lastHp;

		private int _colorPropertyId = -1;

		private int _tintColorPropertyId = -1;

		private struct RendererHolder
		{
			public MeshFilter meshFilter;

			public Renderer renderer;
		}
	}
}
