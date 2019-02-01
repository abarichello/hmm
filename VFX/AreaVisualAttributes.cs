using System;
using System.Collections;
using System.Collections.Generic;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	[RequireComponent(typeof(BoxCollider))]
	internal class AreaVisualAttributes : GameHubBehaviour
	{
		private IEnumerator Interpolate()
		{
			this.coroutineRunning = true;
			Light light = UnityEngine.Object.FindObjectOfType<Light>();
			yield return null;
			while (AreaVisualAttributes.mustRefresh)
			{
				AreaVisualAttributes.mustRefresh = false;
				AreaVisualAttributes.AreaAttributes currentAttributes = AreaVisualAttributes.GetCurrentAttributes();
				AreaVisualAttributes.AreaAttributes nextAttributes = AreaVisualAttributes.areaAttributesStack.Peek();
				float interpol = 0f;
				while (interpol != 1f)
				{
					if (AreaVisualAttributes.mustRefresh)
					{
						break;
					}
					interpol += Time.deltaTime;
					interpol = Mathf.Min(1f, interpol);
					RenderSettings.fogDensity = Mathf.Lerp(currentAttributes.fogIntensity, nextAttributes.fogIntensity, interpol);
					RenderSettings.fogColor = Color.Lerp(currentAttributes.fogColor, nextAttributes.fogColor, interpol);
					RenderSettings.ambientLight = Color.Lerp(currentAttributes.ambientColor, nextAttributes.ambientColor, interpol);
					if (light != null)
					{
						light.intensity = Mathf.Lerp(currentAttributes.lightIntensity, nextAttributes.lightIntensity, interpol);
						light.color = Color.Lerp(currentAttributes.lightColor, nextAttributes.lightColor, interpol);
					}
					yield return null;
				}
			}
			this.coroutineRunning = false;
			yield break;
		}

		private void OnTriggerEnter()
		{
			if (AreaVisualAttributes.areaAttributesStack.Count == 0)
			{
				this.defaultAttributes = AreaVisualAttributes.GetCurrentAttributes();
				AreaVisualAttributes.areaAttributesStack.Push(this.defaultAttributes);
			}
			AreaVisualAttributes.areaAttributesStack.Push(this.areaAttributes);
			AreaVisualAttributes.mustRefresh = true;
			if (!this.coroutineRunning)
			{
				GameHubBehaviour.Hub.StartCoroutine(this.Interpolate());
			}
		}

		private static AreaVisualAttributes.AreaAttributes GetCurrentAttributes()
		{
			AreaVisualAttributes.AreaAttributes result = default(AreaVisualAttributes.AreaAttributes);
			result.fogIntensity = RenderSettings.fogDensity;
			result.fogColor = RenderSettings.fogColor;
			result.ambientColor = RenderSettings.ambientLight;
			Light light = UnityEngine.Object.FindObjectOfType<Light>();
			if (light != null)
			{
				result.lightIntensity = light.intensity;
				result.lightColor = light.color;
			}
			return result;
		}

		private void OnTriggerExit()
		{
			AreaVisualAttributes.areaAttributesStack.Pop();
			AreaVisualAttributes.mustRefresh = true;
			if (!this.coroutineRunning)
			{
				GameHubBehaviour.Hub.StartCoroutine(this.Interpolate());
			}
		}

		private static Stack<AreaVisualAttributes.AreaAttributes> areaAttributesStack = new Stack<AreaVisualAttributes.AreaAttributes>();

		private AreaVisualAttributes.AreaAttributes defaultAttributes;

		public AreaVisualAttributes.AreaAttributes areaAttributes = default(AreaVisualAttributes.AreaAttributes);

		private static bool mustRefresh;

		private bool coroutineRunning;

		[Serializable]
		public struct AreaAttributes
		{
			public float fogIntensity;

			public Color fogColor;

			public Color ambientColor;

			public float lightIntensity;

			public Color lightColor;
		}
	}
}
