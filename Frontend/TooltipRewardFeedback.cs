using System;
using System.Collections.Generic;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class TooltipRewardFeedback : GameHubBehaviour
	{
		public bool IsVisisble()
		{
			return this.Pivot.activeInHierarchy;
		}

		private void Configure(TooltipRewardFeedbackConfig config)
		{
			this.TitleLabel.text = config.Title;
			this.TitleBackgroundSprite.color = config.TitleBackgroundColor;
			for (int i = 0; i < config.Values.Count; i++)
			{
				TooltipValue tooltipValue = config.Values[i];
				ToolTipRewardFeedbackValues instantiatedTooltipLineFromCache = this.GetInstantiatedTooltipLineFromCache();
				instantiatedTooltipLineFromCache.gameObject.name = i.ToString();
				instantiatedTooltipLineFromCache.ItemNameLabel.text = tooltipValue.ItemName;
				instantiatedTooltipLineFromCache.ItemValueLabel.text = tooltipValue.ItemValue;
			}
			this.TotalValueLabel.text = config.TotalRewardsValue.ToString();
			this.ValuesGrid.repositionNow = true;
			this.TooltipBackgroundSprite.height = (int)((float)this.TitleBackgroundSprite.height + this.ValuesGrid.cellHeight * (float)(config.Values.Count + 1));
		}

		public void ShowWindow(TooltipRewardFeedbackConfig config)
		{
			this.Configure(config);
			this.Pivot.SetActive(true);
		}

		public void HideWindow()
		{
			this.Pivot.SetActive(false);
			while (this._enabledTooltipLines.Count > 0)
			{
				this.SendTooltipLineToCache(this._enabledTooltipLines.Pop());
			}
		}

		private ToolTipRewardFeedbackValues GetInstantiatedTooltipLineFromCache()
		{
			if (this._disabledTooltipLines.Count > 0)
			{
				ToolTipRewardFeedbackValues toolTipRewardFeedbackValues = this._disabledTooltipLines.Pop();
				toolTipRewardFeedbackValues.gameObject.SetActive(true);
				this._enabledTooltipLines.Push(toolTipRewardFeedbackValues);
				return toolTipRewardFeedbackValues;
			}
			GameObject gameObject = Object.Instantiate<GameObject>(this.TooltipValuesPrefab);
			gameObject.transform.parent = this.ValuesGrid.transform;
			gameObject.transform.position = Vector3.zero;
			gameObject.transform.localScale = Vector3.one;
			gameObject.SetActive(true);
			ToolTipRewardFeedbackValues component = gameObject.GetComponent<ToolTipRewardFeedbackValues>();
			this._enabledTooltipLines.Push(component);
			return component;
		}

		private void SendTooltipLineToCache(ToolTipRewardFeedbackValues line)
		{
			line.gameObject.SetActive(false);
			this._disabledTooltipLines.Push(line);
		}

		public GameObject Pivot;

		public UI2DSprite TooltipBackgroundSprite;

		public UILabel TitleLabel;

		public UI2DSprite TitleBackgroundSprite;

		public GameObject TooltipValuesPrefab;

		public UILabel TotalValueLabel;

		public UIGrid ValuesGrid;

		private readonly Stack<ToolTipRewardFeedbackValues> _disabledTooltipLines = new Stack<ToolTipRewardFeedbackValues>(8);

		private readonly Stack<ToolTipRewardFeedbackValues> _enabledTooltipLines = new Stack<ToolTipRewardFeedbackValues>(8);
	}
}
