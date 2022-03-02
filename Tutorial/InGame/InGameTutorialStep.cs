using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Tutorial.InGame
{
	public class InGameTutorialStep : GameHubBehaviour
	{
		public TutorialStepsController tutorialStepsController
		{
			get
			{
				TutorialStepsController result;
				if ((result = this._tutorialStepsController) == null)
				{
					result = (this._tutorialStepsController = base.GetComponentInParent<TutorialStepsController>());
				}
				return result;
			}
		}

		public int stepIndex { get; private set; }

		public List<InGameTutorialBehaviourBase> TutorialBehaviours
		{
			get
			{
				if (this._tutorialBehaviours == null)
				{
					InGameTutorialBehaviourBase[] components = base.GetComponents<InGameTutorialBehaviourBase>();
					this._tutorialBehaviours = new List<InGameTutorialBehaviourBase>(components);
				}
				return this._tutorialBehaviours;
			}
		}

		private void Awake()
		{
			this._defaultGameObjectName = base.gameObject.name;
		}

		private void OnEnable()
		{
			this._destroyed = false;
			if (this.SetupDone)
			{
				return;
			}
			this.SetupDone = true;
			for (int i = 0; i < this.TutorialBehaviours.Count; i++)
			{
				InGameTutorialBehaviourBase inGameTutorialBehaviourBase = this.TutorialBehaviours[i];
				inGameTutorialBehaviourBase.Setup(i);
			}
		}

		private void OnDestroy()
		{
			this._tutorialStepsController = null;
			this._tutorialBehaviours.Clear();
			this._tutorialBehaviours = null;
			this.SetupDone = false;
			this._destroyed = true;
		}

		public void Setup(int pStepIndex)
		{
			this.stepIndex = pStepIndex;
			base.gameObject.SetActive(false);
		}

		public void ResetBehaviours()
		{
			for (int i = 0; i < this.TutorialBehaviours.Count; i++)
			{
				InGameTutorialBehaviourBase inGameTutorialBehaviourBase = this.TutorialBehaviours[i];
				inGameTutorialBehaviourBase.ResetBehaviour();
			}
		}

		public void ForceBehaviourCompletition()
		{
			for (int i = 0; i < this.TutorialBehaviours.Count; i++)
			{
				InGameTutorialBehaviourBase inGameTutorialBehaviourBase = this.TutorialBehaviours[i];
				inGameTutorialBehaviourBase.ForceBehaviourCompleted();
			}
		}

		public void StartBehaviours(Action<InGameTutorialStep> onStepCompleted = null)
		{
			if (GameHubBehaviour.Hub && GameHubBehaviour.Hub.Net.IsClient())
			{
				string msg = string.Format("Step={0} Name={1}", this.stepIndex, this._defaultGameObjectName);
				GameHubBehaviour.Hub.Swordfish.Log.BILogClientMsg(41, msg, false);
			}
			if (GameHubBehaviour.Hub && GameHubBehaviour.Hub.Net.IsServer())
			{
				string text = string.Format("Step={0} Name={1} UserID={2}", this.stepIndex, this._defaultGameObjectName, GameHubBehaviour.Hub.Players.Players[0].UserSF.UniversalID);
				InGameTutorialStep.Log.Debug("ServerBITags.GameServerTutorialStepStart - " + text);
				GameHubBehaviour.Hub.Swordfish.Log.BILogServerMsg(7, text, false);
			}
			InGameTutorialStep.Log.DebugFormat("Step started: {0} {1}", new object[]
			{
				this.stepIndex,
				this._defaultGameObjectName
			});
			this._onStepCompletedCallback = onStepCompleted;
			base.gameObject.SetActive(true);
			base.gameObject.name = string.Format("{0} [Step Started]", this._defaultGameObjectName);
			this._requiredTutorialBehaviours = new List<InGameTutorialBehaviourBase>();
			List<InGameTutorialBehaviourBase> list = (from tb in this.TutorialBehaviours
			where tb.requiredToCompletedStep
			select tb).ToList<InGameTutorialBehaviourBase>();
			for (int i = 0; i < list.Count; i++)
			{
				InGameTutorialBehaviourBase item = list[i];
				this._requiredTutorialBehaviours.Add(item);
			}
			for (int j = 0; j < this.TutorialBehaviours.Count; j++)
			{
				InGameTutorialBehaviourBase inGameTutorialBehaviourBase = this.TutorialBehaviours[j];
				if (inGameTutorialBehaviourBase.requiredToCompletedStep)
				{
					inGameTutorialBehaviourBase.StartBehaviour(new Action<InGameTutorialBehaviourBase>(this.OnBehaviourFinished));
				}
				else
				{
					inGameTutorialBehaviourBase.StartBehaviour(null);
				}
			}
		}

		private void OnBehaviourFinished(InGameTutorialBehaviourBase pInGameTutorialBehaviourBase)
		{
			this._requiredTutorialBehaviours.Remove(pInGameTutorialBehaviourBase);
			if (this._requiredTutorialBehaviours.Count != 0)
			{
				return;
			}
			if (!this._destroyed && base.gameObject.activeInHierarchy)
			{
				base.StartCoroutine(this.OnStepCompleted());
			}
			this._destroyed = true;
		}

		private IEnumerator OnStepCompleted()
		{
			if (GameHubBehaviour.Hub.Net.IsServer() && base.gameObject.activeInHierarchy && this.DelayBeforeComplete > 0f)
			{
				yield return new WaitForSeconds(this.DelayBeforeComplete);
			}
			this.InnerOnStepCompleted();
			if (GameHubBehaviour.Hub.Net.IsClient() && this.DelayBeforeComplete > 0f)
			{
				yield return new WaitForSeconds(this.DelayBeforeComplete);
			}
			if (this._onStepCompletedCallback != null)
			{
				this._onStepCompletedCallback(this);
			}
			base.gameObject.SetActive(false);
			if ((GameHubBehaviour.Hub.Net.isTest && GameHubBehaviour.Hub.Net.IsClient()) || (GameHubBehaviour.Hub.Net.IsClient() && this.IsOfflineStep))
			{
				InGameTutorialStep.Log.Warn("will force next tutorial step");
				TutorialStepsController.Instance.ForceNextStepClient();
			}
			yield break;
		}

		private void InnerOnStepCompleted()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				GameHubBehaviour.Hub.Swordfish.Log.BILogClientMsg(42, string.Format("Step={0} Name={1}", this.stepIndex, this._defaultGameObjectName), false);
			}
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				string text = string.Format("Step={0} Name={1} UserID={2}", this.stepIndex, this._defaultGameObjectName, GameHubBehaviour.Hub.Players.Players[0].UserSF.UniversalID);
				InGameTutorialStep.Log.Debug("ServerBITags.GameServerTutorialStepEnd - " + text);
				GameHubBehaviour.Hub.Swordfish.Log.BILogServerMsg(8, text, false);
				if (this.stepIndex == this.tutorialStepsController.inGameTutorialSteps.Count - 1)
				{
					InGameTutorialStep.Log.Debug("ServerBITags.GameServerTutorialEnd");
					string msg = string.Format("Tutorial={0} UserID={1}", "GameServer Tutorial END", GameHubBehaviour.Hub.Players.Players[0].UserSF.UniversalID);
					GameHubBehaviour.Hub.Swordfish.Log.BILogServerMsg(6, msg, false);
				}
			}
			base.gameObject.name = string.Format("{0} [Step Completed]", this._defaultGameObjectName);
			for (int i = 0; i < this.TutorialBehaviours.Count; i++)
			{
				InGameTutorialBehaviourBase inGameTutorialBehaviourBase = this.TutorialBehaviours[i];
				if (!(inGameTutorialBehaviourBase == null))
				{
					inGameTutorialBehaviourBase.OnStepCompleted();
				}
			}
		}

		public void SyncBehaviourCompleted(InGameTutorialBehaviourBase inGameTutorialBehaviourBase)
		{
			if (GameHubBehaviour.Hub && GameHubBehaviour.Hub.Net.IsServer())
			{
				InGameTutorialStep.Log.DebugFormat("Will SyncBehaviourCompleted SERVER Step:{0} Index:{1}", new object[]
				{
					this.stepIndex,
					inGameTutorialBehaviourBase.Index
				});
				this.tutorialStepsController.BehaviourCompletedOnServer(this, inGameTutorialBehaviourBase.Index);
			}
			else
			{
				InGameTutorialStep.Log.DebugFormat("Will SyncBehaviourCompleted CLIENT Step:{0} Index:{1}", new object[]
				{
					this.stepIndex,
					inGameTutorialBehaviourBase.Index
				});
				this.tutorialStepsController.BehaviourCompletedOnClient(this, inGameTutorialBehaviourBase.Index);
			}
		}

		public void CompleteBehaviour(int pBehaviourIndex)
		{
			InGameTutorialBehaviourBase inGameTutorialBehaviourBase = this.TutorialBehaviours[pBehaviourIndex];
			inGameTutorialBehaviourBase.CompleteBehaviour();
		}

		protected static readonly BitLogger Log = new BitLogger(typeof(InGameTutorialStep));

		private TutorialStepsController _tutorialStepsController;

		private List<InGameTutorialBehaviourBase> _tutorialBehaviours;

		public float DelayBeforeComplete = -1f;

		public bool IsOfflineStep;

		private List<InGameTutorialBehaviourBase> _requiredTutorialBehaviours;

		private Action<InGameTutorialStep> _onStepCompletedCallback;

		private string _defaultGameObjectName;

		public bool SetupDone;

		private bool _destroyed;
	}
}
