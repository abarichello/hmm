using System;
using HeavyMetalMachines.Frontend;
using Pocketverse;

namespace HeavyMetalMachines.Tutorial.InGame
{
	public abstract class InGameTutorialBehaviourBase : GameHubBehaviour
	{
		public int Index { get; private set; }

		public InGameTutorialStep tutorialStep
		{
			get
			{
				InGameTutorialStep result;
				if ((result = this._tutorialStep) == null)
				{
					result = (this._tutorialStep = base.GetComponent<InGameTutorialStep>());
				}
				return result;
			}
		}

		public bool behaviourCompleted { get; protected set; }

		protected PlayerController playerController
		{
			get
			{
				if (InGameTutorialBehaviourBase._playerController == null && GameHubBehaviour.Hub.Players.Players != null && GameHubBehaviour.Hub.Players.Players.Count > 0)
				{
					InGameTutorialBehaviourBase._playerController = GameHubBehaviour.Hub.Players.Players[0].CharacterInstance.GetBitComponent<PlayerController>();
				}
				return InGameTutorialBehaviourBase._playerController;
			}
		}

		protected GameGui GameGui
		{
			get
			{
				GameGui result;
				if ((result = this._gameGui) == null)
				{
					result = (this._gameGui = GameHubBehaviour.Hub.State.Current.GetStateGuiController<GameGui>());
				}
				return result;
			}
		}

		public virtual void Setup(int tIndex)
		{
			this._gameGui = null;
			this.Index = tIndex;
		}

		public virtual void ResetBehaviour()
		{
			this.behaviourCompleted = false;
		}

		protected virtual void Destroy()
		{
			this._tutorialStep = null;
			this.behaviourCompleted = false;
			this.startedOnClient = false;
			this.startedOnServer = false;
			InGameTutorialBehaviourBase._playerController = null;
			this.onBehaviourCompleted = null;
		}

		public void SetPlayerInputsActive(bool active)
		{
			if (this.tutorialStep == null || this.tutorialStep.tutorialStepsController == null)
			{
				return;
			}
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				this.tutorialStep.tutorialStepsController.SetPlayerInputsActive(active);
			}
			else
			{
				this.tutorialStep.tutorialStepsController.SetClientPlayerInputsActive(active);
			}
		}

		public void StartBehaviour(Action<InGameTutorialBehaviourBase> pOnBehaviourFinished = null)
		{
			this.onBehaviourCompleted = pOnBehaviourFinished;
			if (GameHubBehaviour.Hub && GameHubBehaviour.Hub.Net.IsServer())
			{
				this.StartBehaviourOnServer();
			}
			else
			{
				this.StartBehaviourOnClient();
			}
		}

		public void OnStepCompleted()
		{
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				this.OnStepCompletedOnServer();
			}
			else
			{
				this.OnStepCompletedOnClient();
			}
		}

		protected virtual void StartBehaviourOnClient()
		{
			InGameTutorialBehaviourBase.Log.DebugFormat("StartBehaviourOnClient: {0} Index:{1} requiredToCompletedStep:{2}", new object[]
			{
				base.GetType().Name,
				this.Index,
				this.requiredToCompletedStep
			});
			this.startedOnClient = true;
		}

		protected virtual void StartBehaviourOnServer()
		{
			InGameTutorialBehaviourBase.Log.DebugFormat("StartBehaviourOnServer: {0} Index:{1} requiredToCompletedStep:{2}", new object[]
			{
				base.GetType().Name,
				this.Index,
				this.requiredToCompletedStep
			});
			this.startedOnServer = true;
		}

		protected virtual void OnStepCompletedOnClient()
		{
			InGameTutorialBehaviourBase.Log.DebugFormat("OnStepCompletedOnClient: {0}", new object[]
			{
				base.GetType().Name
			});
		}

		protected virtual void OnStepCompletedOnServer()
		{
			InGameTutorialBehaviourBase.Log.DebugFormat("OnStepCompletedOnServer: {0}", new object[]
			{
				base.GetType().Name
			});
		}

		public virtual void CompleteBehaviour()
		{
			this.behaviourCompleted = true;
			if (this.onBehaviourCompleted != null)
			{
				this.onBehaviourCompleted(this);
			}
			InGameTutorialBehaviourBase.Log.DebugFormat("CompleteBehaviour: {0}, {1} {2}", new object[]
			{
				base.GetType().Name,
				this.Index,
				(!GameHubBehaviour.Hub || !GameHubBehaviour.Hub.Net.IsServer()) ? "[CLIENT]" : "[SERVER]"
			});
		}

		public virtual void ForceBehaviourCompleted()
		{
			this.behaviourCompleted = true;
		}

		public virtual void CompleteBehaviourAndSync()
		{
			if (this.tutorialStep != null)
			{
				this.tutorialStep.SyncBehaviourCompleted(this);
			}
			this.CompleteBehaviour();
		}

		protected void Update()
		{
			if (GameHubBehaviour.Hub && GameHubBehaviour.Hub.Net.IsServer() && this.startedOnServer)
			{
				this.UpdateOnServer();
			}
			else if (this.startedOnClient)
			{
				this.UpdateOnClient();
			}
		}

		protected virtual void UpdateOnClient()
		{
		}

		protected virtual void UpdateOnServer()
		{
		}

		private static readonly BitLogger Log = new BitLogger(typeof(InGameTutorialBehaviourBase));

		public bool requiredToCompletedStep;

		private InGameTutorialStep _tutorialStep;

		protected bool startedOnClient;

		protected bool startedOnServer;

		private static PlayerController _playerController;

		private GameGui _gameGui;

		protected Action<InGameTutorialBehaviourBase> onBehaviourCompleted;
	}
}
