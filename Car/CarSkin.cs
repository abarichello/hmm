using System;
using HeavyMetalMachines.Render;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Car
{
	[HelpURL("https://confluence.hoplon.com/display/HMM/Car+Skin")]
	public class CarSkin : GameHubBehaviour
	{
		public string SkinName
		{
			get
			{
				return this._skinName;
			}
		}

		public void SetSkin(string skinName, bool isAlly)
		{
			this._skinName = skinName;
			this._renderers = base.GetComponentsInChildren<Renderer>();
			this._data = ((!isAlly) ? this._redTeamEmission : this._blueTeamEmission);
			this._glowPropertyId = Shader.PropertyToID("_Glow");
			this._glowColorPropertyId = Shader.PropertyToID("_GlowColor");
			this.SetProperties();
			if (base.gameObject.GetComponent<CarWheelsController>() && this._renderers.Length > 0)
			{
				base.gameObject.GetComponent<CarWheelsController>().UpdateWheelsMaterialTeam(this._data.Color, this._data.Intensity);
			}
		}

		private void OnEnable()
		{
			if (this._renderers != null)
			{
				this.SetProperties();
			}
		}

		private void SetProperties()
		{
			MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
			for (int i = 0; i < this._renderers.Length; i++)
			{
				Material sharedMaterial = this._renderers[i].sharedMaterial;
				if (sharedMaterial.HasProperty(this._glowPropertyId) && sharedMaterial.HasProperty(this._glowColorPropertyId))
				{
					this._renderers[i].GetPropertyBlock(materialPropertyBlock);
					materialPropertyBlock.SetFloat(this._glowPropertyId, this._data.Intensity);
					materialPropertyBlock.SetColor(this._glowColorPropertyId, this._data.Color);
					this._renderers[i].SetPropertyBlock(materialPropertyBlock);
				}
			}
		}

		[SerializeField]
		private CarSkin.TeamEmissionData _blueTeamEmission = new CarSkin.TeamEmissionData(Color.blue, 1f);

		[SerializeField]
		private CarSkin.TeamEmissionData _redTeamEmission = new CarSkin.TeamEmissionData(Color.red, 1f);

		private string _skinName = string.Empty;

		private Renderer[] _renderers;

		private int _glowPropertyId;

		private int _glowColorPropertyId;

		private CarSkin.TeamEmissionData _data;

		[Serializable]
		public struct TeamEmissionData
		{
			public TeamEmissionData(Color color, float intensity = 1f)
			{
				this.Color = color;
				this.Intensity = intensity;
			}

			public Color Color;

			public float Intensity;
		}
	}
}
