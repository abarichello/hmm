using System;
using System.Collections.Generic;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class FeedbackText : GameHubScriptableObject
	{
		private void OnEnable()
		{
			for (int i = 0; i < this.ConfigList.Count; i++)
			{
				this.ConfigDic.Add(this.ConfigList[i].Name, this.ConfigList[i]);
			}
		}

		public List<FeedbackText.Config> ConfigList = new List<FeedbackText.Config>();

		public Dictionary<string, FeedbackText.Config> ConfigDic = new Dictionary<string, FeedbackText.Config>();

		[Serializable]
		public class Config
		{
			public string Name;

			public string ConfigText;

			public Color Color;
		}
	}
}
