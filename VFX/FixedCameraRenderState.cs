using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	internal class FixedCameraRenderState : GameHubBehaviour
	{
		private void OnPreRender()
		{
			this.originalAmbientLight = RenderSettings.ambientLight;
			this.originalFog = RenderSettings.fog;
			RenderSettings.ambientLight = this.AmbientLight;
			RenderSettings.fog = this.Fog;
		}

		private void OnPostRender()
		{
			RenderSettings.ambientLight = this.originalAmbientLight;
			RenderSettings.fog = this.originalFog;
		}

		public Color AmbientLight = Color.white;

		public bool Fog = true;

		private Color originalAmbientLight;

		private bool originalFog;
	}
}
