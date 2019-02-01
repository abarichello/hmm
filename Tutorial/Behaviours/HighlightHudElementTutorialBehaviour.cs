using System;
using HeavyMetalMachines.Tutorial.InGame;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	public class HighlightHudElementTutorialBehaviour : InGameTutorialBehaviourBase
	{
		protected override void StartBehaviourOnClient()
		{
			base.StartBehaviourOnClient();
			GameObject gameObject = GameObject.Find(this._objectPath);
			if (gameObject)
			{
				TutorialUIController.Instance.EnableGlowFeedback(this._glowFeedbackName, gameObject.transform.position, 0f, 0.5f);
			}
			else
			{
				HighlightHudElementTutorialBehaviour.Log.ErrorFormat("Could not find valid object to highlight!!! DesidedObjectPath:{0}", new object[]
				{
					this._objectPath
				});
			}
		}

		protected override void OnStepCompletedOnClient()
		{
			base.OnStepCompletedOnClient();
			TutorialUIController.Instance.DisableGlowFeedback(0.5f);
		}

		private static readonly BitLogger Log = new BitLogger(typeof(HighlightHudElementTutorialBehaviour));

		[SerializeField]
		private string _objectPath;

		[SerializeField]
		private string _glowFeedbackName;
	}
}
