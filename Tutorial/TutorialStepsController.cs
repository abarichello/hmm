using System;
using System.Collections.Generic;
using HeavyMetalMachines.Options;
using HeavyMetalMachines.Tutorial.InGame;
using Pocketverse;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Tutorial
{
	[RemoteClass]
	public class TutorialStepsController : GameHubBehaviour, IBitComponent
	{
		public static TutorialStepsController Instance
		{
			get
			{
				if (TutorialStepsController._instance == null)
				{
					TutorialStepsController._instance = Object.FindObjectOfType<TutorialStepsController>();
				}
				return TutorialStepsController._instance;
			}
		}

		public int CurrentStep
		{
			get
			{
				return this._currentStep;
			}
			private set
			{
				this._currentStep = value;
				if (GameHubBehaviour.Hub.Net.IsServer())
				{
					InGameTutorialStep inGameTutorialStep = null;
					if (value >= 0 && value < this.inGameTutorialSteps.Count)
					{
						inGameTutorialStep = this.inGameTutorialSteps[value];
					}
					this.DispatchReliable(GameHubBehaviour.Hub.SendAll).StepChangedOnServer(value);
					if (inGameTutorialStep != null)
					{
						this.GenerateBiLogIfBusinessStep(inGameTutorialStep);
						inGameTutorialStep.StartBehaviours(new Action<InGameTutorialStep>(this.OnStepCompletedOnServer));
					}
				}
			}
		}

		private void GenerateBiLogIfBusinessStep(InGameTutorialStep step)
		{
			if (this.businessStepBiLogger == null)
			{
				return;
			}
			this.businessStepBiLogger.LogStep(step, this.VersionHash);
		}

		public void ForceStepAndSync(int stepIndex)
		{
		}

		[RemoteMethod]
		public void ForceStep(int stepIndex)
		{
			for (int i = 0; i < this.inGameTutorialSteps.Count; i++)
			{
				if (i >= stepIndex)
				{
					this.inGameTutorialSteps[stepIndex].ResetBehaviours();
				}
				else
				{
					this.inGameTutorialSteps[stepIndex].ForceBehaviourCompletition();
				}
				this.inGameTutorialSteps[i].gameObject.SetActive(false);
			}
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				this.CurrentStep = stepIndex;
			}
			else
			{
				this.StepChangedOnServer(stepIndex);
			}
		}

		public void ForceNextStepClient()
		{
			this.StepChangedOnServer(this._currentStep + 1);
		}

		public void CompleteStepHack()
		{
		}

		private void OnStepCompletedOnServer(InGameTutorialStep pInGameTutorialStep)
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			TutorialStepsController.Log.DebugFormat("OnStepCompletedOnServer currentStep:{0}", new object[]
			{
				this.CurrentStep
			});
			if (this.CurrentStep < this.inGameTutorialSteps.Count)
			{
				this.CurrentStep++;
			}
		}

		private void Awake()
		{
			TutorialStepsController._instance = this;
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				this._container.InstantiatePrefab(this._tutorialUiController, Vector3.zero, Quaternion.identity, GameHubBehaviour.Hub.State.CurrentSceneStateData.StateGuiController.transform);
			}
			ControlOptions.UnlockAllControlActions();
		}

		private void Start()
		{
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				TutorialStepsController.Log.Debug("ServerBITags.GameServerTutorialStart - " + string.Format("Tutorial={0} UserID={1}", "GameServer Tutorial START", GameHubBehaviour.Hub.Players.Players[0].UserSF.UniversalID));
				GameHubBehaviour.Hub.Swordfish.Log.BILogServerMsg(5, string.Format("Tutorial={0} UserID={1}", "GameServer Tutorial START", GameHubBehaviour.Hub.Players.Players[0].UserSF.UniversalID), false);
			}
			for (int i = 0; i < this.inGameTutorialSteps.Count; i++)
			{
				InGameTutorialStep inGameTutorialStep = this.inGameTutorialSteps[i];
				inGameTutorialStep.Setup(i);
			}
		}

		[RemoteMethod]
		private void StepChangedOnServer(int step)
		{
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				return;
			}
			this.CurrentStep = step;
			if (this.CurrentStep >= this.inGameTutorialSteps.Count)
			{
				this.InGameTutorialFinishedOnClient();
				return;
			}
			InGameTutorialStep inGameTutorialStep = this.inGameTutorialSteps[this.CurrentStep];
			if (inGameTutorialStep != null)
			{
				this.SetPlayerInputsActive(true);
				inGameTutorialStep.StartBehaviours(null);
			}
		}

		private void InGameTutorialFinishedOnClient()
		{
			TutorialStepsController.Log.Debug("InGameTutorialFinishedOnClient");
		}

		public void BehaviourCompletedOnClient(InGameTutorialStep pTutorialBehaviours, int pBehaviourIndex)
		{
			int num = this.inGameTutorialSteps.IndexOf(pTutorialBehaviours);
			TutorialStepsController.Log.DebugFormat("BehaviourCompletedOnClient: step={0}, behaviour={1}", new object[]
			{
				num,
				pBehaviourIndex
			});
			if (GameHubBehaviour.Hub != null && !GameHubBehaviour.Hub.Net.isTest && !pTutorialBehaviours.IsOfflineStep)
			{
				this.DispatchReliable(new byte[0]).SyncBehaviourCompletedOnClient(num, pBehaviourIndex);
			}
		}

		[RemoteMethod]
		private void SyncBehaviourCompletedOnClient(int pStep, int pBehaviourIndex)
		{
			TutorialStepsController.Log.DebugFormat("SyncBehaviourCompletedOnClient: step={0}, behaviour={1}", new object[]
			{
				pStep,
				pBehaviourIndex
			});
			InGameTutorialStep inGameTutorialStep = this.inGameTutorialSteps[pStep];
			inGameTutorialStep.CompleteBehaviour(pBehaviourIndex);
		}

		public void BehaviourCompletedOnServer(InGameTutorialStep pTutorialBehaviours, int pBehaviourIndex)
		{
			int num = this.inGameTutorialSteps.IndexOf(pTutorialBehaviours);
			TutorialStepsController.Log.DebugFormat("BehaviourCompletedOnServer: step={0}, behaviour={1}", new object[]
			{
				num,
				pBehaviourIndex
			});
			if (GameHubBehaviour.Hub != null)
			{
				this.DispatchReliable(GameHubBehaviour.Hub.SendAll).SyncBehaviourCompletedOnServer(num, pBehaviourIndex);
			}
		}

		[RemoteMethod]
		private void SyncBehaviourCompletedOnServer(int pStep, int pBehaviourIndex)
		{
			TutorialStepsController.Log.DebugFormat("SyncBehaviourCompletedOnServer: step={0}, behaviour={1}", new object[]
			{
				pStep,
				pBehaviourIndex
			});
			InGameTutorialStep inGameTutorialStep = this.inGameTutorialSteps[pStep];
			if (pStep != this.CurrentStep)
			{
				TutorialStepsController.Log.WarnFormat("Behaviour of step {0} completed after step {1} started. Tutorial might fail.", new object[]
				{
					pStep,
					this.CurrentStep
				});
			}
			inGameTutorialStep.CompleteBehaviour(pBehaviourIndex);
		}

		[RemoteMethod]
		public void SetPlayerInputsActive(bool activeInput)
		{
			if (activeInput)
			{
				ControlOptions.LockAllInputs(false);
			}
			else
			{
				ControlOptions.LockAllInputs(true);
			}
		}

		private void Update()
		{
			if (GameHubBehaviour.Hub == null)
			{
				return;
			}
			if (GameHubBehaviour.Hub.Players.Players.Count <= 0 || !GameHubBehaviour.Hub.Players.Players[0].CharacterInstance)
			{
				return;
			}
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			if (!this._waitAddPlayerToHub)
			{
				return;
			}
			if (GameHubBehaviour.Hub.SendAll.Length <= 0)
			{
				return;
			}
			this._waitAddPlayerToHub = false;
			this.CurrentStep = 0;
		}

		public void SetClientPlayerInputsActive(bool activeInput)
		{
			this.DispatchReliable(GameHubBehaviour.Hub.SendAll).SetPlayerInputsActive(activeInput);
		}

		private int OID
		{
			get
			{
				if (!this._identifiable)
				{
					this._identifiable = base.GetComponent<Identifiable>();
				}
				return this._identifiable.ObjId;
			}
		}

		public byte Sender { get; set; }

		public ITutorialStepsControllerAsync Async()
		{
			return this.Async(0);
		}

		public ITutorialStepsControllerAsync Async(byte to)
		{
			if (this._async == null)
			{
				this._async = new TutorialStepsControllerAsync(this.OID);
			}
			return (to != 0) ? this._async.To(to) : this._async.To();
		}

		public ITutorialStepsControllerDispatch DispatchReliable(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new TutorialStepsControllerDispatch(this.OID);
			}
			this._dispatch.Reliable(true);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		public ITutorialStepsControllerDispatch Dispatch(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new TutorialStepsControllerDispatch(this.OID);
			}
			this._dispatch.Reliable(false);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		protected IFuture Delayed
		{
			get
			{
				return this._delayed;
			}
		}

		protected void Delay(IFuture future)
		{
			this._delayed = future;
		}

		public object Invoke(int classId, short methodId, object[] args, BitStream bitstream = null)
		{
			this._delayed = null;
			switch (methodId)
			{
			case 9:
				this.StepChangedOnServer((int)args[0]);
				return null;
			default:
				if (methodId != 3)
				{
					throw new ScriptMethodNotFoundException(classId, (int)methodId);
				}
				this.ForceStep((int)args[0]);
				return null;
			case 12:
				this.SyncBehaviourCompletedOnClient((int)args[0], (int)args[1]);
				return null;
			case 14:
				this.SyncBehaviourCompletedOnServer((int)args[0], (int)args[1]);
				return null;
			case 15:
				this.SetPlayerInputsActive((bool)args[0]);
				return null;
			}
		}

		private static TutorialStepsController _instance;

		public static readonly BitLogger Log = new BitLogger(typeof(TutorialStepsController));

		private bool _waitAddPlayerToHub = true;

		[HideInInspector]
		[SerializeField]
		public List<InGameTutorialStep> inGameTutorialSteps;

		[SerializeField]
		private TutorialBusinessStepBiLogger businessStepBiLogger;

		[SerializeField]
		public string VersionHash;

		[SerializeField]
		private TutorialUIController _tutorialUiController;

		[Inject]
		private DiContainer _container;

		private int _currentStep = -1;

		public const int StaticClassId = 1010;

		private Identifiable _identifiable;

		[ThreadStatic]
		private TutorialStepsControllerAsync _async;

		[ThreadStatic]
		private TutorialStepsControllerDispatch _dispatch;

		private IFuture _delayed;
	}
}
