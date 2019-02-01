using System;
using System.Collections.Generic;
using HeavyMetalMachines.Combat;
using UnityEngine;

namespace HeavyMetalMachines
{
	public class TimeLineRepositionTest : MonoBehaviour
	{
		public void Update()
		{
			if (this.TimeLineHack)
			{
				this.TimeLineHack = false;
				if (!this.ProgressionController.pivot.activeInHierarchy)
				{
					this.ProgressionController.pivot.SetActive(true);
				}
				if (!this.MatchInfoController.gameObject.activeInHierarchy)
				{
					this.MatchInfoController.gameObject.SetActive(true);
				}
				if (this.MatchInfoController.MyWidgetAlpha.alpha < 1f)
				{
					this.MatchInfoController.MyWidgetAlpha.alpha = 1f;
				}
				this.MatchInfoController.CreateOrRestBombDeliverIconList(this.MatchInfoController.TimelinePlayers);
				if (this.MatchInfoController.TimelinePlayers.Count == 0)
				{
					this.MatchInfoController.TimelinePlayerPrefab.SetActive(true);
					for (int i = 0; i < this.TimeLineRoundsTest.Count; i++)
					{
						GameObject gameObject = this.MatchInfoController.TimelineGrid.gameObject.AddChild(this.MatchInfoController.TimelinePlayerPrefab);
						this.MatchInfoController.TimelinePlayers.Add(gameObject);
						gameObject.name = i + "_round";
					}
					this.MatchInfoController.TimelinePlayerPrefab.SetActive(false);
				}
				UIProgressionMatchInfoController.RepositionBombDeliverIcons(this.MatchInfoController.TimelineBaseLine.GetComponent<UI2DSprite>(), this.MatchInfoController.TimelinePlayers, this.MatchInfoController.TimelineCellWidth, this.TimeLineRoundsTest);
			}
		}

		public UIProgressionController ProgressionController;

		public UIProgressionMatchInfoController MatchInfoController;

		public bool TimeLineHack;

		public List<RoundStats> TimeLineRoundsTest;
	}
}
