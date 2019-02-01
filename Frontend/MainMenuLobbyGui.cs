using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class MainMenuLobbyGui : GameHubBehaviour
	{
		public void ButtonsAnimatorUpdate(MainMenuLobbyGui.MainMenuLobbyAnimatorAction animatorAction)
		{
			string allButtonsInAnimatorPropertyName;
			bool value;
			if (animatorAction != MainMenuLobbyGui.MainMenuLobbyAnimatorAction.AllButtonsIn)
			{
				if (animatorAction != MainMenuLobbyGui.MainMenuLobbyAnimatorAction.AllButtonsOut)
				{
					MainMenuLobbyGui.Log.ErrorFormat("Invalid Animator Action:[{0}]", new object[]
					{
						animatorAction
					});
					return;
				}
				allButtonsInAnimatorPropertyName = this._allButtonsInAnimatorPropertyName;
				value = false;
			}
			else
			{
				allButtonsInAnimatorPropertyName = this._allButtonsInAnimatorPropertyName;
				value = true;
			}
			this._buttonsAnimator.SetBool(allButtonsInAnimatorPropertyName, value);
		}

		public void SetPlayButtonLabelText(string labelText)
		{
			this._playButtonLabel.text = labelText;
		}

		public void EnablePlayButtonLabel(bool isActive)
		{
			this._playButtonLabel.gameObject.SetActive(isActive);
		}

		public void EnablePlayButton(bool isEnabled)
		{
			foreach (UIButton uibutton in this._playButtonGameObject.GetComponents<UIButton>())
			{
				uibutton.GetComponent<BoxCollider>().enabled = isEnabled;
				uibutton.SetState(isEnabled ? UIButtonColor.State.Normal : UIButtonColor.State.Disabled, true);
			}
		}

		public void SetWaitingButtonLabelText(string labelText)
		{
			this._waitingButtonLabel.text = labelText;
		}

		public void EnableWaitingButtonLabel(bool isActive)
		{
			this._waitingButtonLabel.gameObject.SetActive(isActive);
		}

		public void EnableRegionTooltip(bool isActive)
		{
			this._regionTooltipGameObject.SetActive(isActive);
		}

		public void EnableTimerSprite(bool isActive)
		{
			this._timerSpriteGameObject.SetActive(isActive);
		}

		public void EnableGroupNotReadyTooltip(bool isActive)
		{
			this._groupNotReadyTooltipGameObject.SetActive(isActive);
		}

		protected void OnEnable()
		{
			this._versionLabel.text = "2.07.972";
		}

		private static readonly BitLogger Log = new BitLogger(typeof(HudLifebarController));

		[SerializeField]
		private UILabel _versionLabel;

		[SerializeField]
		private GameObject _playButtonGameObject;

		[SerializeField]
		private UILabel _playButtonLabel;

		[SerializeField]
		private UILabel _waitingButtonLabel;

		[SerializeField]
		private GameObject _timerSpriteGameObject;

		[SerializeField]
		private GameObject _regionTooltipGameObject;

		[SerializeField]
		private GameObject _groupNotReadyTooltipGameObject;

		[SerializeField]
		private Animator _buttonsAnimator;

		[SerializeField]
		private string _allButtonsInAnimatorPropertyName;

		public enum MainMenuLobbyAnimatorAction
		{
			AllButtonsIn,
			AllButtonsOut
		}
	}
}
