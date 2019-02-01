using System;
using System.Collections;
using System.Collections.Generic;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class UpdateBackground : GameHubBehaviour
	{
		private void OnEnable()
		{
			if (!this._onEnableChangeBackgroundResolution)
			{
				return;
			}
			this._onEnableChangeBackgroundResolution = false;
			base.StartCoroutine(this.WaitOneFrameAndSetResolutionBackground());
		}

		private void Start()
		{
			UpdateBackground.UpdateBackGroundScrips.Add(this);
		}

		public void SetResolutionBackground()
		{
			if (!base.gameObject.activeInHierarchy)
			{
				this._onEnableChangeBackgroundResolution = true;
				return;
			}
			base.StartCoroutine(this.WaitOneFrameAndSetResolutionBackground());
		}

		public IEnumerator WaitOneFrameAndSetResolutionBackground()
		{
			yield return null;
			UITexture background = base.GetComponent<UITexture>();
			int widthScale = background.width;
			int heightScale = background.height;
			if (background.height < Screen.height)
			{
				int num = background.width / background.height;
				int num2 = Screen.height - background.height;
				widthScale += num2 * num;
				heightScale = Screen.height;
			}
			background.width = widthScale;
			background.height = heightScale;
			yield break;
		}

		public static void OnResolutionChanged()
		{
			for (int i = 0; i < UpdateBackground.UpdateBackGroundScrips.Count; i++)
			{
				UpdateBackground.UpdateBackGroundScrips[i].SetResolutionBackground();
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(UpdateBackground));

		private static readonly List<UpdateBackground> UpdateBackGroundScrips = new List<UpdateBackground>();

		private bool _onEnableChangeBackgroundResolution = true;
	}
}
