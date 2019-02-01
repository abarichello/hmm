using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Car
{
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
			Renderer[] componentsInChildren = base.GetComponentsInChildren<Renderer>();
			MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
			CarSkin.TeamEmissionData teamEmissionData = (!isAlly) ? this._redTeamEmission : this._blueTeamEmission;
			int nameID = Shader.PropertyToID("_Glow");
			int nameID2 = Shader.PropertyToID("_GlowColor");
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				Material sharedMaterial = componentsInChildren[i].sharedMaterial;
				if (sharedMaterial.HasProperty(nameID) && sharedMaterial.HasProperty(nameID2))
				{
					componentsInChildren[i].GetPropertyBlock(materialPropertyBlock);
					materialPropertyBlock.SetFloat(nameID, teamEmissionData.Intensity);
					materialPropertyBlock.SetColor(nameID2, teamEmissionData.Color);
					componentsInChildren[i].SetPropertyBlock(materialPropertyBlock);
				}
			}
		}

		[SerializeField]
		private CarSkin.TeamEmissionData _blueTeamEmission = new CarSkin.TeamEmissionData(Color.blue, 1f);

		[SerializeField]
		private CarSkin.TeamEmissionData _redTeamEmission = new CarSkin.TeamEmissionData(Color.red, 1f);

		private string _skinName = string.Empty;

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
