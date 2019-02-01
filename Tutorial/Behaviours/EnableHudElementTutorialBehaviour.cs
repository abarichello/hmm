using System;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Tutorial.InGame;
using UnityEngine;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	public class EnableHudElementTutorialBehaviour : InGameTutorialBehaviourBase
	{
		protected override void StartBehaviourOnClient()
		{
			base.StartBehaviourOnClient();
			for (int i = 0; i < this._hudElements.Length; i++)
			{
				base.GameGui.SetHudVisibility(this._hudElements[i], this._elementsVisiblity);
			}
			this.CompleteBehaviourAndSync();
		}

		[SerializeField]
		private GameGui.HudElement[] _hudElements;

		[SerializeField]
		private bool _elementsVisiblity = true;
	}
}
