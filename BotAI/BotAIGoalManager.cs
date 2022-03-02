using System;
using System.Collections.Generic;
using Assets.Standard_Assets.Scripts.HMM.Bot;
using HeavyMetalMachines.AI;
using HeavyMetalMachines.Characters;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Combat.GadgetScript;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.BotAI
{
	public class BotAIGoalManager : GameHubBehaviour, IObjectSpawnListener, IBotAIDirectives
	{
		public BotAIGoalManager.TeamSharedData Aggros
		{
			get
			{
				this.CheckAggroBoard();
				return this._aggros;
			}
		}

		private void CheckAggroBoard()
		{
			if (this._aggros != null)
			{
				return;
			}
			for (int i = 0; i < this._friendCarsList.Count; i++)
			{
				CombatObject combatObject = this._friendCarsList[i];
				BotAIGoalManager bitComponent = combatObject.Id.GetBitComponent<BotAIGoalManager>();
				if (bitComponent._aggros != null)
				{
					this._aggros = bitComponent._aggros;
					return;
				}
			}
			this._aggros = new BotAIGoalManager.TeamSharedData();
		}

		public CombatObject Combat { get; set; }

		public CarComponentHub CarHub { get; set; }

		private BombObjectives BombObjectives
		{
			get
			{
				BombObjectives result;
				if ((result = this._bombObjectives) == null)
				{
					result = (this._bombObjectives = Object.FindObjectOfType<BombObjectives>());
				}
				return result;
			}
		}

		private Transform BombObjectiveDeliver
		{
			get
			{
				Transform result;
				if ((result = this._bombObjectiveDeliver) == null)
				{
					result = (this._bombObjectiveDeliver = this.BombObjectives.GetObjective("Delivery", this.Combat.Team));
				}
				return result;
			}
		}

		public BotAIGoalManager.Action CurrentAction
		{
			get
			{
				return this._currentAction;
			}
			set
			{
				this._currentAction = value;
			}
		}

		public BotAIGoalManager.SubState CurrentSubState
		{
			get
			{
				return this._currentSubState;
			}
			set
			{
				this._currentSubState = value;
			}
		}

		public BotAIGoalManager.State CurrentState
		{
			get
			{
				return this._currentState;
			}
			set
			{
				this._currentState = value;
			}
		}

		public BotAIGoalManager.TeamState CurrentTeamState
		{
			get
			{
				return this._currentTeamState;
			}
			set
			{
				this._currentTeamState = value;
			}
		}

		private void Start()
		{
			if (!GameHubBehaviour.Hub || GameHubBehaviour.Hub.Net.IsClient())
			{
				base.enabled = false;
				return;
			}
			this.Initialize();
		}

		public void ReThinkState()
		{
			this.CheckState();
		}

		public void Initialize()
		{
			if (this.initialized)
			{
				return;
			}
			this.initialized = true;
			this.MyData = GameHubBehaviour.Hub.Players.PlayersAndBots.Find((PlayerData b) => b.PlayerCarId == this.Combat.Id.ObjId);
			this.Enemyteam = ((this.MyData.Team != TeamKind.Red) ? TeamKind.Red : TeamKind.Blue);
			this._myRole = this.MyData.GetCharacterRole();
			for (int i = 0; i < GameHubBehaviour.Hub.Players.PlayersAndBots.Count; i++)
			{
				PlayerData playerData = GameHubBehaviour.Hub.Players.PlayersAndBots[i];
				if (playerData.PlayerAddress != this.Combat.Player.PlayerAddress)
				{
					CombatObject bitComponent = playerData.CharacterInstance.GetBitComponent<CombatObject>();
					if (playerData.Team == this.Enemyteam)
					{
						this._enemyCarsList.Add(bitComponent);
					}
					else
					{
						this._friendCarsList.Add(bitComponent);
					}
				}
			}
			List<CombatObject> list = new List<CombatObject>(this._enemyCarsList.Count);
			list.AddRange(this._enemyCarsList);
			for (int j = 0; j < list.Count; j++)
			{
				this._enemyCarsList[(j + this.Combat.Player.TeamSlot) % list.Count] = list[j];
			}
			this._gadgets.Add(this.CreateGadgetAIState(this.Combat.CustomGadget2, this.Goals.Gadget2, this.Combat.GadgetStates.G2StateObject));
			this._gadgets.Add(this.CreateGadgetAIState(this.Combat.CustomGadget1, this.Goals.Gadget1, this.Combat.GadgetStates.G1StateObject));
			this._gadgets.Add(this.CreateGadgetAIState(this.Combat.CustomGadget0, this.Goals.Gadget0, this.Combat.GadgetStates.G0StateObject));
			this._gadgets.Add(this.CreateGadgetAIState(this.Combat.BoostGadget, this.Goals.BoostGadget, this.Combat.GadgetStates.GBoostStateObject));
			this._bombGadget = this.CreateGadgetAIState(this.Combat.BombGadget, this.Goals.BombGadget, this.Combat.GadgetStates.BombStateObject);
			this.UpdateBotOnlyTeamGoalCap();
			GameHubBehaviour.Hub.BombManager.ServerOnBombCarrierIdentifiableChanged += this.OnBombCarrierChange;
			if (BotAIAmbushSystem.instance)
			{
				BotAIAmbushSystem.instance.OnTriggerAmbushPoint += this.OnTriggerAmbushPoint;
			}
			this.Combat.ListenToPosDamageTaken += this.OnDamage;
		}

		private GadgetAIState CreateGadgetAIState(GadgetBehaviour gadget, BotAIGoal.GadgetUseInfo gadgetUseInfo, GadgetData.GadgetStateObject gadgetState)
		{
			IsGadgetDisarmedByCombatAttributes isGadgetDisarmed = new IsGadgetDisarmedByCombatAttributes(gadget.Slot, gadget.Nature);
			return new GadgetAIState(isGadgetDisarmed)
			{
				Gadget = gadget,
				UseInfo = gadgetUseInfo,
				GadgetState = gadgetState
			};
		}

		public GadgetAIState GetGadgetState(GadgetSlot slot)
		{
			if (slot == GadgetSlot.BombGadget)
			{
				return this._bombGadget;
			}
			for (int i = 0; i < this._gadgets.Count; i++)
			{
				if (this._gadgets[i].Gadget.Slot == slot)
				{
					return this._gadgets[i];
				}
			}
			return null;
		}

		public void UpdateBotOnlyTeamGoalCap()
		{
			this.Goals.IsOnBotOnlyTeam = true;
			for (int i = 0; i < GameHubBehaviour.Hub.Players.Players.Count; i++)
			{
				PlayerData playerData = GameHubBehaviour.Hub.Players.Players[i];
				if (playerData.Team == this.Combat.Team && !playerData.IsBotControlled)
				{
					this.Goals.IsOnBotOnlyTeam = false;
					break;
				}
			}
		}

		public void UpdateAllBotOnlyTeamGoalCap()
		{
			for (int i = 0; i < GameHubBehaviour.Hub.Players.PlayersAndBots.Count; i++)
			{
				PlayerData playerData = GameHubBehaviour.Hub.Players.PlayersAndBots[i];
				if (!playerData.IsNarrator && playerData.Team == this.Combat.Team && playerData.CharacterInstance.ObjId != this.Combat.Player.CharacterInstance.ObjId)
				{
					CarComponentHub componentHub = playerData.CharacterInstance.GetComponentHub<CarComponentHub>();
					if (componentHub.AIAgent.GoalManager.initialized)
					{
						componentHub.AIAgent.GoalManager.Goals.IsOnBotOnlyTeam = this.Goals.IsOnBotOnlyTeam;
					}
				}
			}
		}

		private void OnDamage(CombatObject combatObject, CombatObject taker, ModifierData mod, float amount, int eventId)
		{
		}

		private void OnDestroy()
		{
			Object.Destroy(this.Goals);
			GameHubBehaviour.Hub.BombManager.ServerOnBombCarrierIdentifiableChanged -= this.OnBombCarrierChange;
			if (BotAIAmbushSystem.instance)
			{
				BotAIAmbushSystem.instance.OnTriggerAmbushPoint -= this.OnTriggerAmbushPoint;
			}
			this.Combat.ListenToPosDamageTaken -= this.OnDamage;
		}

		private void OnTriggerAmbushPoint(BotAIAmbushPoint ambush)
		{
			if (this.CurrentState != BotAIGoalManager.State.KillCarrier || this.CurrentSubState != BotAIGoalManager.SubState.HealDefendAttack)
			{
				return;
			}
			if (this.CurrentAction == BotAIGoalManager.Action.Ambush || Random.Range(0f, 1f) < this.Goals.Ambush)
			{
				bool flag = Random.Range(0f, 1f) < 0.5f;
				if (flag && ambush.optionalPoint != null)
				{
					this.SetTarget(ambush.optionalPoint.transform, BotAIGoalManager.Action.Ambush);
					return;
				}
				if (ambush.nextWaypoint == null)
				{
					return;
				}
				this.CurrentAction = BotAIGoalManager.Action.Ambush;
				this.SetTarget(ambush.nextWaypoint.transform, BotAIGoalManager.Action.Ambush);
			}
		}

		private void OnBombCarrierChange(Identifiable carrier)
		{
			this.CheckState();
		}

		private void CheckState()
		{
			TeamKind teamOwner = GameHubBehaviour.Hub.BombManager.ActiveBomb.TeamOwner;
			if (teamOwner != TeamKind.Zero)
			{
				this.CurrentTeamState = ((GameHubBehaviour.Hub.BombManager.ActiveBomb.TeamOwner != this.Enemyteam) ? BotAIGoalManager.TeamState.BombOurs : BotAIGoalManager.TeamState.BombTheirs);
			}
			else
			{
				this.CurrentTeamState = BotAIGoalManager.TeamState.BombGround;
			}
			BotAIGoalManager.State state = BotAIGoalManager.State.None;
			BotAIGoalManager.TeamState currentTeamState = this.CurrentTeamState;
			if (currentTeamState != BotAIGoalManager.TeamState.BombGround)
			{
				if (currentTeamState != BotAIGoalManager.TeamState.BombTheirs)
				{
					if (currentTeamState == BotAIGoalManager.TeamState.BombOurs)
					{
						state = ((!GameHubBehaviour.Hub.BombManager.ActiveBomb.BombCarriersIds.Contains(this.Combat.Id.ObjId)) ? BotAIGoalManager.State.ProtectCarrier : BotAIGoalManager.State.DeliverBomb);
					}
				}
				else
				{
					state = BotAIGoalManager.State.KillCarrier;
				}
			}
			else
			{
				state = BotAIGoalManager.State.GetBomb;
			}
			if (this.CurrentState == state)
			{
				return;
			}
			this.CurrentState = state;
			CombatObject bombCarrier = this._bombCarrier;
			if (GameHubBehaviour.Hub.BombManager.ActiveBomb.BombCarriersIds.Count > 0)
			{
				Identifiable @object = GameHubBehaviour.Hub.ObjectCollection.GetObject(GameHubBehaviour.Hub.BombManager.ActiveBomb.BombCarriersIds[0]);
				this._bombCarrier = ((!(@object == null)) ? @object.GetBitComponent<CombatObject>() : null);
			}
			else
			{
				this._bombCarrier = null;
			}
			if (bombCarrier != this._bombCarrier)
			{
				if (bombCarrier != null)
				{
					bombCarrier.ListenToPosDamageTaken -= this.OnCarrierDamaged;
				}
				if (this._bombCarrier != null)
				{
					this._bombCarrier.ListenToPosDamageTaken += this.OnCarrierDamaged;
				}
			}
			this.Aggros.SetCarrier(this._bombCarrier);
			this.nextGoalTime = -1f;
			this.CurrentAction = BotAIGoalManager.Action.Idle;
		}

		private void CheckSubState()
		{
			BotAIGoalManager.SubState subState = BotAIGoalManager.SubState.None;
			bool flag = this.Combat.Data.HP < (float)this.Combat.Data.HPMax * this.Goals.DropBomb;
			switch (this.CurrentState)
			{
			case BotAIGoalManager.State.GetBomb:
				if (this._exitWaitingToPassAllowedTime > Time.time)
				{
					subState = BotAIGoalManager.SubState.HealDefendAttack;
				}
				else if (flag)
				{
					subState = BotAIGoalManager.SubState.SelfRecover;
				}
				else
				{
					DriverRoleKind myRole = this._myRole;
					if (myRole != 1)
					{
						subState = ((!this.CheckCloseToBomb(true, null)) ? BotAIGoalManager.SubState.GetBomb : BotAIGoalManager.SubState.HealDefendAttack);
					}
					else
					{
						subState = BotAIGoalManager.SubState.GetBomb;
					}
				}
				break;
			case BotAIGoalManager.State.KillCarrier:
				subState = ((!flag) ? BotAIGoalManager.SubState.HealDefendAttack : BotAIGoalManager.SubState.SelfRecover);
				break;
			case BotAIGoalManager.State.DeliverBomb:
				subState = ((!flag) ? BotAIGoalManager.SubState.Deliver : BotAIGoalManager.SubState.Pass);
				if (subState != BotAIGoalManager.SubState.Pass && (this.Combat.IsBot || this.CarHub.Player.IsBotControlled))
				{
					List<PlayerData> players = GameHubBehaviour.Hub.Players.Players;
					for (int i = 0; i < players.Count; i++)
					{
						PlayerData playerData = players[i];
						if (playerData.PlayerAddress != this.Combat.Player.PlayerAddress && !playerData.IsBotControlled && playerData.Team == this.Combat.Team)
						{
							CombatObject bitComponent = playerData.CharacterInstance.GetBitComponent<CombatObject>();
							if (this.ShouldPassBomb(bitComponent))
							{
								subState = BotAIGoalManager.SubState.Pass;
								this.CurrentAction = BotAIGoalManager.Action.PassBomb;
								break;
							}
						}
					}
				}
				break;
			case BotAIGoalManager.State.ProtectCarrier:
				if (flag)
				{
					subState = BotAIGoalManager.SubState.SelfRecover;
				}
				else
				{
					DriverRoleKind myRole2 = this._myRole;
					if (myRole2 != 1)
					{
						subState = BotAIGoalManager.SubState.HealDefendAttack;
					}
					else
					{
						subState = BotAIGoalManager.SubState.GetPass;
					}
				}
				break;
			}
			if (subState == this.CurrentSubState)
			{
				return;
			}
			this._bombGadget.ForceStop();
			this.CurrentSubState = subState;
			this.CurrentAction = BotAIGoalManager.Action.Idle;
			this.nextGoalTime = this.Goals.DecisionInterval;
			switch (this.CurrentSubState)
			{
			case BotAIGoalManager.SubState.GetBomb:
			case BotAIGoalManager.SubState.HealDefendAttack:
			case BotAIGoalManager.SubState.GetPass:
				this.SetTarget(GameHubBehaviour.Hub.BombManager.BombMovement.transform, BotAIGoalManager.Action.Idle);
				break;
			case BotAIGoalManager.SubState.Deliver:
			case BotAIGoalManager.SubState.Pass:
				this.SetTarget(this.BombObjectiveDeliver, BotAIGoalManager.Action.Idle);
				break;
			case BotAIGoalManager.SubState.SelfRecover:
				this.SetTarget(this.GetClosestRepairPoint(), BotAIGoalManager.Action.Idle);
				break;
			}
		}

		private Transform GetClosestRepairPoint()
		{
			Transform transform = null;
			float num = float.MaxValue;
			Vector3 position = this.Combat.Transform.position;
			foreach (Transform transform2 in GameHubBehaviour.Hub.BotAIHub.RepairPoints)
			{
				float num2 = Vector3.SqrMagnitude(transform2.position - position);
				if (num2 <= num)
				{
					num = num2;
					transform = transform2;
				}
			}
			if (transform)
			{
				return transform;
			}
			return (this.Combat.Team != TeamKind.Red) ? GameHubBehaviour.Hub.BotAIHub.GarageControllerBlu.transform : GameHubBehaviour.Hub.BotAIHub.GarageControllerRed.transform;
		}

		private bool ShouldPassBomb(CombatObject otherCombat)
		{
			return otherCombat.BombGadget.Pressed && !otherCombat.Attributes.IsGadgetDisarmed(GadgetSlot.BombGadget, (GadgetNatureKind)0) && (float)this.Goals.SqrdMinPassBombDistance > Vector3.SqrMagnitude(this.Combat.transform.position - otherCombat.transform.position);
		}

		private bool UpdateSubState()
		{
			switch (this.CurrentSubState)
			{
			case BotAIGoalManager.SubState.GetBomb:
				return this.UpdateGetBombState();
			case BotAIGoalManager.SubState.HealDefendAttack:
				return this.UpdateHealDefendAttackState();
			case BotAIGoalManager.SubState.Deliver:
				return this.UpdateDeliverState();
			case BotAIGoalManager.SubState.Pass:
				return this.UpdatePassState();
			case BotAIGoalManager.SubState.GetPass:
				return this.UpdateGetPassState();
			case BotAIGoalManager.SubState.SelfRecover:
				return this.UpdateSelfRecoverState();
			default:
				return false;
			}
		}

		private bool UpdateGetBombState()
		{
			if (this.Combat.Attributes.IsGadgetDisarmed(GadgetSlot.BombGadget, (GadgetNatureKind)0))
			{
				this._bombGadget.Press(false);
			}
			else
			{
				float num = this.Combat.BombGadget.BombGadgetInfo.JammedRange * this.Combat.BombGadget.BombGadgetInfo.JammedRange;
				bool value = num >= Vector3.SqrMagnitude(GameHubBehaviour.Hub.BombManager.BombMovement.Combat.Transform.position - this.Combat.Transform.position);
				this._bombGadget.Press(value);
			}
			return false;
		}

		private bool UpdateDeliverState()
		{
			BotAIGoalManager.Action currentAction = this.CurrentAction;
			if (currentAction != BotAIGoalManager.Action.Idle)
			{
				if (currentAction == BotAIGoalManager.Action.DeliverBomb)
				{
					this.ExecuteDeliverBombAction();
				}
			}
			else
			{
				this.ExecuteIdleAction();
			}
			return false;
		}

		private void ExecuteIdleAction()
		{
			bool flag = this.CheckLineOfSightToDelivery();
			if (flag)
			{
				this._bombGadget.Press(true);
				this._holdTime = Time.time + this._bombGadget.UseInfo.AttackSpeed;
				this.SetTarget(this.BombObjectiveDeliver, BotAIGoalManager.Action.DeliverBomb);
			}
		}

		private void ExecuteDeliverBombAction()
		{
			Vector3 normalized = (this.BombObjectiveDeliver.position - this.Combat.Transform.position).normalized;
			float num = Vector3.Dot(this.Combat.Transform.forward, normalized);
			bool flag = Time.time > this._holdTime && num > this._bombGadget.UseInfo.GetDotLimit;
			if (flag)
			{
				this._bombGadget.Press(false);
				this.SetExitWaitingToPassBomb();
			}
			else
			{
				this._bombGadget.Press(true);
			}
		}

		private bool CheckLineOfSightToDelivery()
		{
			Vector3 position = this.BombObjectiveDeliver.position;
			Vector3 position2 = GameHubBehaviour.Hub.BombManager.BombMovement.transform.position;
			Vector3 vector = position - position2;
			float num = GameHubBehaviour.Hub.BotAIMatchRules.CloseToDeliveryDistance * GameHubBehaviour.Hub.BotAIMatchRules.CloseToDeliveryDistance;
			float num2 = Vector3.SqrMagnitude(vector);
			if (num2 >= num)
			{
				return false;
			}
			float num3 = Mathf.Sqrt(num2);
			RaycastHit[] array = Physics.RaycastAll(position2, vector / num3, num3, LayerManager.GetWallMask(true));
			bool flag = false;
			for (int i = 0; i < array.Length; i++)
			{
				Collider collider = array[i].collider;
				if (!collider.GetComponent<BombTargetTrigger>())
				{
					flag = true;
					break;
				}
			}
			return !flag;
		}

		private bool UpdatePassState()
		{
			BotAIGoalManager.Action currentAction = this.CurrentAction;
			if (currentAction != BotAIGoalManager.Action.Idle)
			{
				if (currentAction != BotAIGoalManager.Action.PassBomb)
				{
					if (currentAction == BotAIGoalManager.Action.DeliverBomb)
					{
						this.ExecuteDeliverBombAction();
					}
				}
				else if (!this.Combat.BombGadget.Pressed)
				{
					this._bombGadget.Press(true);
					this.SetExitWaitingToPassBomb();
				}
				else
				{
					this._bombGadget.Press(false);
				}
			}
			else if (this.CarHub.Player.IsBotControlled || this.CarHub.Player.HasCounselor)
			{
				bool flag = this.CheckLineOfSightToDelivery();
				if (flag)
				{
					this._bombGadget.Press(true);
					this._holdTime = Time.time + this._bombGadget.UseInfo.AttackSpeed;
					this.SetTarget(this.BombObjectiveDeliver, BotAIGoalManager.Action.DeliverBomb);
				}
				else
				{
					float num = Vector3.SqrMagnitude(this.BombObjectiveDeliver.position - this.Combat.Transform.position);
					float num2 = GameHubBehaviour.Hub.BotAIMatchRules.CloseToDeliveryDistance * GameHubBehaviour.Hub.BotAIMatchRules.CloseToDeliveryDistance;
					bool flag2 = num > num2 && this.CheckCloseToBomb(true, null);
					this._bombGadget.Press(flag2);
					if (flag2)
					{
						this.CurrentAction = BotAIGoalManager.Action.PassBomb;
					}
				}
			}
			return false;
		}

		private void SetExitWaitingToPassBomb()
		{
			this._exitWaitingToPassAllowedTime = Time.time + this.Goals.WaitForPassStateSeconds;
		}

		private bool UpdateHealDefendAttackState()
		{
			switch (this.CurrentAction)
			{
			case BotAIGoalManager.Action.Idle:
			{
				this._pursuitTime = -1f;
				DriverRoleKind myRole = this._myRole;
				if (myRole != null)
				{
					this.CurrentAction = BotAIGoalManager.Action.Kill;
				}
				else
				{
					this.CurrentAction = BotAIGoalManager.Action.Heal;
				}
				break;
			}
			case BotAIGoalManager.Action.Heal:
				if ((this.CurrentState == BotAIGoalManager.State.ProtectCarrier && this._bombCarrier.Data.HP < (float)this._bombCarrier.Data.HPMax) || !this._closeToBomb)
				{
					this.SetTarget(this._bombCarrier, BotAIGoalManager.Action.Heal);
					this._bombGadget.Press(false);
					return true;
				}
				if (!(this._currentTargetCombat != null) || !this._currentTargetCombat.IsAlive() || this._currentTargetCombat.Data.HP >= (float)this._currentTargetCombat.Data.HPMax || !this.CheckTargetCloseToBomb(this._currentTargetCombat))
				{
					this._closer.Clear();
					bool flag = this.CheckCloseToBomb(true, this._closer);
					this._closer.Sort(new Comparison<CombatObject>(this.SortByHealth));
					if (!flag)
					{
						this.SetTarget(GameHubBehaviour.Hub.BombManager.BombMovement.transform, BotAIGoalManager.Action.Heal);
						return false;
					}
					for (int i = 0; i < this._closer.Count; i++)
					{
						CombatObject combatObject = this._closer[i];
						if (!this.Aggros.IsAggroed(combatObject.Id.ObjId) && combatObject.IsAlive() && combatObject.Data.HP < (float)combatObject.Data.HPMax)
						{
							this.SetTarget(combatObject, BotAIGoalManager.Action.Heal);
							return true;
						}
					}
				}
				break;
			case BotAIGoalManager.Action.Kill:
				if (this._closeToBomb && this._bombCarrier != null && this._bombCarrier.Team != this.Combat.Team && this.Aggros.CarrierAggroCount < GameHubBehaviour.Hub.BotAIMatchRules.MaxCarrierAggroCount)
				{
					this._pursuitTime = GameHubBehaviour.Hub.BotAIMatchRules.PursuitTime;
					this.SetTarget(this._bombCarrier, BotAIGoalManager.Action.Kill);
					return true;
				}
				if (this._pursuitTime > 0f && this._currentTargetCombat.IsAlive())
				{
					if (!this.CheckTargetCloseToBomb(this._currentTargetCombat))
					{
						this._pursuitTime -= Time.deltaTime;
					}
				}
				else
				{
					this._closer.Clear();
					bool flag2 = this.CheckCloseToBomb(false, this._closer);
					this._closer.Sort(new Comparison<CombatObject>(this.SortByDistance));
					if (!flag2 || !this._closeToBomb)
					{
						this._pursuitTime = -1f;
						this.SetTarget(GameHubBehaviour.Hub.BombManager.BombMovement.transform, BotAIGoalManager.Action.Kill);
						return false;
					}
					for (int j = 0; j < this._closer.Count; j++)
					{
						CombatObject combatObject2 = this._closer[j];
						if (!this.Aggros.IsAggroed(combatObject2.Id.ObjId) && combatObject2.IsAlive())
						{
							this._pursuitTime = GameHubBehaviour.Hub.BotAIMatchRules.PursuitTime;
							this.SetTarget(combatObject2, BotAIGoalManager.Action.Kill);
							return true;
						}
					}
				}
				break;
			case BotAIGoalManager.Action.Ambush:
			{
				float num = GameHubBehaviour.Hub.BotAIMatchRules.AmbushTargetDistance * GameHubBehaviour.Hub.BotAIMatchRules.AmbushTargetDistance;
				bool flag3 = Vector3.SqrMagnitude(base.transform.position - this._currentTarget.position) < num;
				if (flag3)
				{
					BotAIAmbushPoint component = this._currentTarget.GetComponent<BotAIAmbushPoint>();
					if (component.IsMidPoint && component.nextWaypoint != null)
					{
						this.SetTarget(component.nextWaypoint.transform, BotAIGoalManager.Action.Ambush);
						break;
					}
				}
				if (this._closeToBomb || flag3)
				{
					this.SetTarget(GameHubBehaviour.Hub.BombManager.BombMovement.transform, BotAIGoalManager.Action.Idle);
				}
				break;
			}
			}
			return false;
		}

		private bool UpdateGetPassState()
		{
			return false;
		}

		private bool UpdateSelfRecoverState()
		{
			return false;
		}

		private bool CheckTargetCloseToBomb(CombatObject target)
		{
			if (target == null || !target.IsAlive())
			{
				return false;
			}
			if (target == this._bombCarrier)
			{
				return true;
			}
			Vector3 vector = (!(this._bombCarrier == null)) ? this._bombCarrier.Transform.position : GameHubBehaviour.Hub.BombManager.BombMovement.transform.position;
			float num = GameHubBehaviour.Hub.BotAIMatchRules.MaxRangeOfAction * GameHubBehaviour.Hub.BotAIMatchRules.MaxRangeOfAction;
			float num2 = Vector3.SqrMagnitude(vector - target.Transform.position);
			return num2 < num;
		}

		private float CheckDistanceToBomb()
		{
			if (this.Combat == this._bombCarrier)
			{
				return 0f;
			}
			Vector3 vector = (!(this._bombCarrier == null)) ? this._bombCarrier.Transform.position : GameHubBehaviour.Hub.BombManager.BombMovement.transform.position;
			return Vector3.Distance(vector, this.Combat.Transform.position);
		}

		private bool CheckCloseToBomb(bool friends, List<CombatObject> close)
		{
			Vector3 vector = (!(this._bombCarrier == null)) ? this._bombCarrier.Transform.position : GameHubBehaviour.Hub.BombManager.BombMovement.transform.position;
			float num = GameHubBehaviour.Hub.BotAIMatchRules.MaxRangeOfAction * GameHubBehaviour.Hub.BotAIMatchRules.MaxRangeOfAction;
			List<CombatObject> list = (!friends) ? this._enemyCarsList : this._friendCarsList;
			bool flag = true;
			bool flag2 = false;
			DriverRoleKind myRole = this._myRole;
			if (myRole != 1)
			{
				if (myRole == 2)
				{
					flag = false;
				}
			}
			else
			{
				flag2 = true;
			}
			for (int i = 0; i < list.Count; i++)
			{
				CombatObject combatObject = list[i];
				if (combatObject.IsAlive())
				{
					bool flag3 = false;
					if (this.Combat.IsBot && combatObject.Team == this.Combat.Team && combatObject.IsPlayer && combatObject.BombGadget.Pressed)
					{
						flag3 = true;
					}
					else
					{
						DriverRoleKind characterRole = combatObject.Player.GetCharacterRole();
						if (characterRole != 2)
						{
							if (characterRole != 1)
							{
								if (characterRole == null)
								{
									flag3 = flag2;
								}
							}
							else
							{
								flag3 = true;
							}
						}
						else
						{
							flag3 = flag;
						}
					}
					if (flag3)
					{
						float num2 = Vector3.SqrMagnitude(vector - combatObject.Transform.position);
						if (num2 < num)
						{
							if (close == null)
							{
								return true;
							}
							close.Add(combatObject);
						}
					}
				}
			}
			return close != null && close.Count > 0;
		}

		private void GoToClosestFriend()
		{
			this._friendCarsList.Sort(new Comparison<CombatObject>(this.SortByDistance));
			for (int i = 0; i < this._friendCarsList.Count; i++)
			{
				CombatObject combatObject = this._friendCarsList[i];
				if (combatObject.IsAlive())
				{
					this.CheckState();
					this.SetTarget(combatObject, BotAIGoalManager.Action.Idle);
				}
			}
		}

		private void OnCarrierDamaged(CombatObject combatObject, CombatObject taker, ModifierData mod, float amount, int eventId)
		{
		}

		private void SetTarget(Transform target, BotAIGoalManager.Action action)
		{
			this._currentTargetCombat = null;
			this._currentTarget = target;
			this.BotAIController.SetGoal(target);
			this.CurrentAction = action;
			this.Aggros.ClearFor(base.Id.ObjId);
		}

		private void SetTarget(CombatObject combatObject, BotAIGoalManager.Action action)
		{
			if (combatObject == null)
			{
				this._currentTargetCombat = null;
				this._currentTarget = null;
				this.Aggros.ClearFor(base.Id.ObjId);
				return;
			}
			this._currentTargetCombat = combatObject;
			this._currentTarget = combatObject.transform;
			this.BotAIController.SetGoal(combatObject);
			this.CurrentAction = action;
			this.Aggros.SetAggro(combatObject.Id.ObjId, base.Id.ObjId);
		}

		private void Update()
		{
			if (!this.CarHub.Player.HasCounselor && !this.CarHub.Player.IsBotControlled)
			{
				return;
			}
			if (GameHubBehaviour.Hub.BombManager.ScoreBoard.CurrentState != BombScoreboardState.BombDelivery && !GameHubBehaviour.Hub.Match.LevelIsTutorial())
			{
				for (int i = 0; i < this._gadgets.Count; i++)
				{
					this._gadgets[i].ForceStop();
				}
				this._bombGadget.ForceStop();
				return;
			}
			this._closeToBomb = this.CheckTargetCloseToBomb(this.Combat);
			this.UpdateSubState();
			this.UseGadgets(this._currentTargetCombat);
			bool flag = false;
			bool flag2 = this._bombGadget.Update(Time.deltaTime, false, this.Combat);
			flag = (flag || flag2);
			if (this.CarHub.Player.IsBotControlled)
			{
				this._bombGadget.Gadget.Pressed = this._bombGadget.Using;
			}
			for (int j = 0; j < this._gadgets.Count; j++)
			{
				GadgetAIState gadgetAIState = this._gadgets[j];
				bool flag3 = gadgetAIState.Update(Time.deltaTime, flag, this.Combat);
				flag = (flag || flag3);
				if (this.CarHub.Player.IsBotControlled)
				{
					gadgetAIState.Gadget.Pressed = flag3;
					if (gadgetAIState.Gadget.Combat.HasGadgetContext((int)gadgetAIState.Gadget.Slot))
					{
						((CombatGadget)gadgetAIState.Gadget.Combat.GetGadgetContext((int)gadgetAIState.Gadget.Slot)).Pressed = flag3;
					}
				}
			}
			if (this.nextGoalTime > 0f)
			{
				this.nextGoalTime -= Time.deltaTime;
				return;
			}
			this.CheckSubState();
		}

		private bool CheckGadgetCanBeUsed(GadgetSlot gadgetSlot, GadgetNatureKind nature)
		{
			GadgetState gadgetState = this.Combat.GadgetStates.GetGadgetState(gadgetSlot).GadgetState;
			bool flag = this.Combat.Attributes.IsGadgetDisarmed(gadgetSlot, nature);
			return gadgetState == GadgetState.Ready && !flag;
		}

		private bool ProbeForward(Transform target, BotAIGoal.GadgetUseInfo info)
		{
			Vector3 vector3;
			if (info.PredictMovement && info.AttackSpeed > 0f)
			{
				CombatMovement component = target.GetComponent<CombatMovement>();
				Vector3 position = target.position;
				Vector3 position2 = base.transform.position;
				float num = (position - position2).magnitude / info.AttackSpeed;
				Vector3 vector = position + target.forward * component.SpeedZ * num;
				Vector3 vector2 = position2 + base.transform.forward * this.Combat.Movement.SpeedZ * num;
				vector3 = vector - vector2;
			}
			else
			{
				vector3 = target.position - base.transform.position;
			}
			Vector3 normalized = vector3.normalized;
			float num2 = Vector3.Dot(base.transform.forward, normalized);
			if (num2 > info.GetDotLimit)
			{
				if (!info.CrossesWalls)
				{
					foreach (RaycastHit raycastHit in Physics.RaycastAll(new Ray(base.transform.position, normalized), vector3.magnitude))
					{
						if (raycastHit.collider.gameObject.layer == 9)
						{
							return false;
						}
					}
				}
				return true;
			}
			return false;
		}

		private bool ProbeBack(Transform target, BotAIGoal.GadgetUseInfo info)
		{
			Vector3 position = target.position;
			Vector3 vector = position - base.transform.position;
			Vector3 normalized = vector.normalized;
			if (Vector3.Dot(base.transform.forward, normalized) < -info.GetDotLimit)
			{
				if (!info.CrossesWalls)
				{
					foreach (RaycastHit raycastHit in Physics.RaycastAll(new Ray(base.transform.position, normalized), vector.magnitude))
					{
						if (raycastHit.collider.gameObject.layer == 9)
						{
							return false;
						}
					}
				}
				return true;
			}
			return false;
		}

		private bool ProbeDistance(Transform target, GadgetInfo info)
		{
			CombatMovement component = target.GetComponent<CombatMovement>();
			Vector3 vector = target.TransformPoint(Vector3.forward * (component.SpeedZ * component.SpeedZ * info.LifeTime * info.LifeTime));
			float num = Vector3.SqrMagnitude(vector - base.transform.position);
			return vector.sqrMagnitude < num;
		}

		private void UseGadgets(CombatObject target)
		{
			for (int i = 0; i < this._gadgets.Count; i++)
			{
				GadgetAIState gadgetAIState = this._gadgets[i];
				if (gadgetAIState.Gadget.Nature != GadgetNatureKind.Teleport)
				{
					if (!gadgetAIState.Gadget.Activated || gadgetAIState.GadgetState.GadgetState != GadgetState.Ready)
					{
						gadgetAIState.ForceStop();
					}
					else if (this.Combat.Attributes.IsGadgetDisarmed(this._gadgets[i].Gadget.Slot, this._gadgets[i].Gadget.Nature))
					{
						gadgetAIState.Press(false);
					}
					else
					{
						bool flag = false;
						if (target != null && ((target.Team == this.Enemyteam && gadgetAIState.UseInfo.AttackKind == BotAIGoal.GadgetUseInfo.GadgetAttackKind.Damage) || (target.Team != this.Enemyteam && gadgetAIState.UseInfo.AttackKind == BotAIGoal.GadgetUseInfo.GadgetAttackKind.Heal)))
						{
							flag = this.TryUseGadget(target.transform, gadgetAIState);
						}
						if (!flag)
						{
							if ((gadgetAIState.UseInfo.AttackKind & BotAIGoal.GadgetUseInfo.GadgetAttackKind.Damage) > (BotAIGoal.GadgetUseInfo.GadgetAttackKind)0)
							{
								for (int j = 0; j < this._enemyCarsList.Count; j++)
								{
									CombatObject combatObject = this._enemyCarsList[j];
									if (!(combatObject == target))
									{
										if (combatObject.IsAlive())
										{
											flag = this.TryUseGadget(combatObject.transform, gadgetAIState);
											if (flag)
											{
												break;
											}
										}
									}
								}
							}
							if ((gadgetAIState.UseInfo.AttackKind & BotAIGoal.GadgetUseInfo.GadgetAttackKind.Heal) > (BotAIGoal.GadgetUseInfo.GadgetAttackKind)0)
							{
								for (int k = 0; k < this._friendCarsList.Count; k++)
								{
									CombatObject combatObject2 = this._friendCarsList[k];
									if (!(combatObject2 == target))
									{
										if (combatObject2.IsAlive())
										{
											flag = this.TryUseGadget(combatObject2.transform, gadgetAIState);
											if (flag)
											{
												break;
											}
										}
									}
								}
							}
							if ((gadgetAIState.UseInfo.AttackKind & BotAIGoal.GadgetUseInfo.GadgetAttackKind.Boost) > (BotAIGoal.GadgetUseInfo.GadgetAttackKind)0 && !this._closeToBomb && !this.Combat.Attributes.IsGadgetDisarmed(gadgetAIState.Gadget.Slot, gadgetAIState.Gadget.Nature))
							{
								flag = true;
								gadgetAIState.Press(true);
							}
						}
						if (!flag)
						{
							gadgetAIState.Press(false);
						}
					}
				}
			}
		}

		private bool TryUseGadget(Transform target, GadgetAIState state)
		{
			if (state.UseInfo.CheckRange)
			{
				float distanceSqr = BotAIUtils.GetDistanceSqr(base.transform, target);
				float num = Mathf.Max(GameHubBehaviour.Hub.BotAIMatchRules.MinWeaponRange, state.Gadget.GetRangeSqr());
				if (distanceSqr > num)
				{
					return false;
				}
			}
			if (state.UseInfo.CheckRadius)
			{
				float distanceSqr2 = BotAIUtils.GetDistanceSqr(base.transform, target);
				float num2 = Mathf.Max(GameHubBehaviour.Hub.BotAIMatchRules.MinWeaponRange, state.Gadget.Radius * state.Gadget.Radius);
				if (distanceSqr2 > num2)
				{
					return false;
				}
			}
			bool flag = false;
			if ((state.UseInfo.AttackPattern & BotAIGoal.GadgetUseInfo.GadgetAttackPattern.Area) != (BotAIGoal.GadgetUseInfo.GadgetAttackPattern)0)
			{
				flag = true;
			}
			if ((state.UseInfo.AttackPattern & BotAIGoal.GadgetUseInfo.GadgetAttackPattern.Forward) != (BotAIGoal.GadgetUseInfo.GadgetAttackPattern)0)
			{
				flag |= this.ProbeForward(target, state.UseInfo);
			}
			if ((state.UseInfo.AttackPattern & BotAIGoal.GadgetUseInfo.GadgetAttackPattern.Back) != (BotAIGoal.GadgetUseInfo.GadgetAttackPattern)0)
			{
				flag |= this.ProbeBack(target, state.UseInfo);
			}
			if ((state.UseInfo.AttackPattern & BotAIGoal.GadgetUseInfo.GadgetAttackPattern.Droppings) != (BotAIGoal.GadgetUseInfo.GadgetAttackPattern)0 && this.CurrentAction == BotAIGoalManager.Action.Lead)
			{
				flag |= this.ProbeDistance(target, state.Gadget.Info);
			}
			if (flag)
			{
				state.Press(true);
			}
			return flag;
		}

		private void OnDrawGizmos()
		{
			if (this._currentTarget == null)
			{
				return;
			}
			switch (this.CurrentState)
			{
			case BotAIGoalManager.State.GetBomb:
				Gizmos.color = Color.gray;
				break;
			case BotAIGoalManager.State.KillCarrier:
				Gizmos.color = Color.red;
				break;
			case BotAIGoalManager.State.DeliverBomb:
				Gizmos.color = new Color(0.5f, 1f, 1f);
				break;
			case BotAIGoalManager.State.ProtectCarrier:
				Gizmos.color = Color.green;
				break;
			}
			if (this._bombCarrier != null)
			{
				if (this._currentTarget != this._bombCarrier)
				{
					Gizmos.color *= new Color(1f, 1f, 1f, 0.5f);
				}
				Gizmos.DrawLine(base.transform.position, this._bombCarrier.transform.position);
			}
			if (this._currentTarget != null && this._currentTarget != this._bombCarrier)
			{
				switch (this.CurrentAction)
				{
				case BotAIGoalManager.Action.Idle:
					Gizmos.color = Color.gray;
					break;
				case BotAIGoalManager.Action.Heal:
					Gizmos.color = Color.green;
					break;
				case BotAIGoalManager.Action.Kill:
					Gizmos.color = Color.red;
					break;
				case BotAIGoalManager.Action.Lead:
					Gizmos.color = new Color(0.5f, 1f, 1f);
					break;
				}
				Gizmos.DrawLine(base.transform.position, this._currentTarget.position);
			}
			Vector3 vector = Camera.current.transform.up * 3f;
			switch (this.CurrentAction)
			{
			case BotAIGoalManager.Action.Heal:
				Gizmos.DrawIcon(base.transform.position + vector, "protect", false);
				break;
			case BotAIGoalManager.Action.Kill:
				Gizmos.DrawIcon(base.transform.position + vector, "attack", false);
				break;
			case BotAIGoalManager.Action.PassBomb:
				Gizmos.DrawIcon(base.transform.position + vector, "drop_bomp", false);
				break;
			case BotAIGoalManager.Action.Ambush:
				Gizmos.DrawIcon(base.transform.position + vector, "apple", false);
				break;
			case BotAIGoalManager.Action.Lead:
				Gizmos.DrawIcon(base.transform.position + vector, "lead", false);
				break;
			}
		}

		private int SortByHealth(CombatObject x, CombatObject y)
		{
			return x.Data.HP.CompareTo(y.Data.HP);
		}

		private int SortByDistance(CombatObject x, CombatObject y)
		{
			float num = Vector3.SqrMagnitude(base.transform.position - x.transform.position);
			float value = Vector3.SqrMagnitude(base.transform.position - y.transform.position);
			return num.CompareTo(value);
		}

		public void OnObjectUnspawned(UnspawnEvent evt)
		{
		}

		public void OnObjectSpawned(SpawnEvent evt)
		{
		}

		public List<CombatObject> GetEnemies()
		{
			return this._enemyCarsList;
		}

		public bool IsPathFixed()
		{
			return false;
		}

		private BotAIGoalManager.TeamSharedData _aggros;

		private float nextGoalTime;

		private List<GadgetAIState> _gadgets = new List<GadgetAIState>();

		private GadgetAIState _bombGadget;

		private Transform _currentTarget;

		private CombatObject _currentTargetCombat;

		private BotAIGoalManager.TeamState _currentTeamState;

		private BotAIGoalManager.State _currentState;

		private BotAIGoalManager.SubState _currentSubState;

		private BotAIGoalManager.Action _currentAction;

		private float lastBombPass;

		private float lastHit;

		private bool _closeToBomb;

		public static readonly BitLogger Log = new BitLogger(typeof(BotAIGoalManager));

		public PlayerData MyData;

		public TeamKind Enemyteam;

		public BotAIGoal Goals;

		public BotAIController BotAIController;

		private List<CombatObject> _enemyCarsList = new List<CombatObject>();

		private List<CombatObject> _friendCarsList = new List<CombatObject>();

		private CombatObject _bombCarrier;

		private BombObjectives _bombObjectives;

		private Transform _bombObjectiveDeliver;

		public bool TutorialDisabled;

		private bool initialized;

		private DriverRoleKind _myRole;

		private float _exitWaitingToPassAllowedTime;

		private float _holdTime = -1f;

		private float _pursuitTime;

		private List<CombatObject> _closer = new List<CombatObject>(4);

		public enum TeamState
		{
			None,
			BombGround,
			BombOurs,
			BombTheirs
		}

		public enum State
		{
			None,
			GetBomb,
			KillCarrier,
			DeliverBomb,
			ProtectCarrier
		}

		public enum SubState
		{
			None,
			GetBomb,
			HealDefendAttack,
			Deliver,
			Pass,
			GetPass,
			SelfRecover
		}

		public enum Action
		{
			Idle,
			Heal,
			Kill,
			PassBomb,
			Ambush,
			Lead,
			TurnBack,
			GetBomb,
			DeliverBomb
		}

		public class TeamSharedData
		{
			public CombatObject Carrier { get; private set; }

			public int CarrierId { get; private set; }

			public int CarrierAggroCount
			{
				get
				{
					return this._carrierAggros.Count;
				}
			}

			public void SetAggro(int target, int causer)
			{
				this.ClearFor(causer);
				if (target == this.CarrierId)
				{
					this._carrierAggros.Add(causer);
				}
				else
				{
					this.Aggros[target] = causer;
				}
				this.InverseAggros[causer] = target;
			}

			public void ClearFor(int causer)
			{
				int num;
				if (this.InverseAggros.TryGetValue(causer, out num))
				{
					this.InverseAggros.Remove(causer);
					if (num == this.CarrierId)
					{
						this._carrierAggros.Remove(causer);
					}
					else
					{
						this.Aggros.Remove(num);
					}
				}
			}

			public void SetCarrier(CombatObject carrier)
			{
				if (carrier == this.Carrier)
				{
					return;
				}
				foreach (int key in this._carrierAggros)
				{
					this.InverseAggros.Remove(key);
				}
				this._carrierAggros.Clear();
				this.Carrier = carrier;
				this.CarrierId = ((!carrier) ? -1 : carrier.Id.ObjId);
			}

			public bool IsAggroed(int target)
			{
				return this.Aggros.ContainsKey(target);
			}

			private HashSet<int> _carrierAggros = new HashSet<int>();

			public Dictionary<int, int> Aggros = new Dictionary<int, int>(4);

			public Dictionary<int, int> InverseAggros = new Dictionary<int, int>(4);
		}
	}
}
