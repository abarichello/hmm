using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class UIPlayerStatsController : GameHubBehaviour
	{
		private void Start()
		{
			UICamera.onScreenResize += this.OnScreenResize;
		}

		private void OnDestroy()
		{
			UICamera.onScreenResize -= this.OnScreenResize;
		}

		private void Update()
		{
		}

		private void HidePanel()
		{
			this.Pivot.localPosition = new Vector3(this.Pivot.localPosition.x, 10000f, this.Pivot.localPosition.z);
		}

		private void ShowPanel()
		{
			this.Pivot.localPosition = new Vector3(this.Pivot.localPosition.x, 0f, this.Pivot.localPosition.z);
		}

		private void OnScreenResize()
		{
			UIGrid component = this.TGrid.GetComponent<UIGrid>();
			component.Reposition();
		}

		public Transform Pivot;

		public Transform TGrid;
	}
}
