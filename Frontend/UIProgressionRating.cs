using System;
using Assets.Standard_Assets.Scripts.HMM.PlotKids;
using HeavyMetalMachines.Utils;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class UIProgressionRating : GameHubBehaviour
	{
		public void Start()
		{
			if (SpectatorController.IsSpectating)
			{
				base.gameObject.SetActive(false);
				return;
			}
			this._questionId = 0;
			this._selection = -1;
			for (int i = 0; i < this._selectionToggles.Length; i++)
			{
				UIToggle uitoggle = this._selectionToggles[i];
				uitoggle.Start();
				EventDelegate eventDelegate = new EventDelegate(this, "GuiButtonSelection");
				eventDelegate.parameters[0] = new EventDelegate.Parameter(i);
				uitoggle.onChange.Add(eventDelegate);
			}
		}

		protected void GuiButtonSelection(int index)
		{
			bool value = this._selectionToggles[index].value;
			int selection = this._selection;
			if (value || selection != index)
			{
				for (int i = 0; i < this._selectionToggles.Length; i++)
				{
					this._selectionToggles[i].Set(i <= index, false);
					this._selectionToggles[i].GetComponent<BoxCollider>().enabled = false;
				}
				this._selection = index;
				this._selectionToggles[index].GetComponent<Animation>().Play();
			}
			else
			{
				for (int j = 0; j < this._selectionToggles.Length; j++)
				{
					if (j != index)
					{
						this._selectionToggles[j].Set(false, false);
					}
				}
				this._selection = -1;
			}
			if (GameHubBehaviour.Hub)
			{
			}
		}

		public void CommitSelection()
		{
			if (SpectatorController.IsSpectating)
			{
				return;
			}
			int num = this._selection + 1;
			string msg = string.Format("SteamID={0} QuestionID={1} Answer={2}", GameHubBehaviour.Hub.User.UniversalId, this._questionId, num);
			GameHubBehaviour.Hub.Swordfish.Log.BILogClientMatchMsg(ClientBITags.MatchReview, msg, false);
		}

		public void OnQuestionButtonClick()
		{
			Guid guid = Guid.Empty;
			if (!GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish))
			{
				guid = GameHubBehaviour.Hub.Swordfish.Msg.ClientMatchId;
			}
			OpenUrlUtils.OpenSteamUrl(GameHubBehaviour.Hub, string.Format("{0}?lang={1}&steamid={2}&matchid={3}", new object[]
			{
				GameHubBehaviour.Hub.Config.GetValue(ConfigAccess.SFQuizUrl),
				Language.CurrentLanguage(),
				GameHubBehaviour.Hub.User.UniversalId,
				guid
			}));
		}

		public void AnimateRating()
		{
			this._ratingAnimator.SetBool("show", true);
		}

		private static readonly BitLogger Log = new BitLogger(typeof(UIProgressionRating));

		[SerializeField]
		private UIToggle[] _selectionToggles;

		[SerializeField]
		private Animator _ratingAnimator;

		private int _questionId;

		private int _selection;
	}
}
