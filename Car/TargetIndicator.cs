using System;
using Assets.Standard_Assets.Scripts.HMM.PlotKids;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Utils;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Car
{
	public class TargetIndicator : GameHubBehaviour
	{
		protected virtual void StartVirtual()
		{
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				base.enabled = false;
				return;
			}
			Debug.Assert(this.TargetObject != null, "Null Target Object on target indicator: you must set a target object if you use this component.", Debug.TargetTeam.All);
			if (this.UsedGadgetSlot == GadgetSlot.None)
			{
				this.m_fSquaredRange = this.Range * this.Range;
			}
			if (GameHubBehaviour.Hub.Events.Players.CarCreationFinished)
			{
				this.InitPlayers();
			}
			else
			{
				GameHubBehaviour.Hub.Events.Players.ListenToAllPlayersSpawned += this.OnAllPlayersSpawned;
				this.m_boRegisteredCallback = true;
			}
			this.m_stCurrentQuaterion = Quaternion.LookRotation(this.TargetObject.transform.forward, Vector3.up);
		}

		private void Start()
		{
			this.StartVirtual();
		}

		protected virtual void UpdateRange()
		{
			this.m_fSquaredRange = this.m_poGadgetBehaviour.GetRangeSqr();
		}

		protected virtual void PostInitPlayers()
		{
		}

		private void InitPlayers()
		{
			bool isSpectating = SpectatorController.IsSpectating;
			int num = GameHubBehaviour.Hub.Players.PlayersAndBots.Count;
			if (!isSpectating)
			{
				num--;
			}
			this.m_apoPlayers = new CombatObject[num];
			int i = 0;
			int num2 = 0;
			while (i < GameHubBehaviour.Hub.Players.PlayersAndBots.Count)
			{
				if (GameHubBehaviour.Hub.Players.CurrentPlayerData.PlayerCarId == GameHubBehaviour.Hub.Players.PlayersAndBots[i].PlayerCarId)
				{
					this.m_poCurrentPlayer = GameHubBehaviour.Hub.Players.PlayersAndBots[i].CharacterInstance.GetBitComponent<CombatObject>();
				}
				else
				{
					this.m_apoPlayers[num2] = GameHubBehaviour.Hub.Players.PlayersAndBots[i].CharacterInstance.GetBitComponent<CombatObject>();
					num2++;
				}
				i++;
			}
			if (this.UsedGadgetSlot != GadgetSlot.None && !isSpectating)
			{
				CombatObject bitComponent = GameHubBehaviour.Hub.Players.CurrentPlayerData.CharacterInstance.GetBitComponent<CombatObject>();
				switch (this.UsedGadgetSlot)
				{
				case GadgetSlot.CustomGadget0:
					this.m_poGadgetBehaviour = bitComponent.CustomGadget0;
					this.m_poGadgetState = bitComponent.GadgetStates.G0StateObject;
					break;
				case GadgetSlot.CustomGadget1:
					this.m_poGadgetBehaviour = bitComponent.CustomGadget1;
					this.m_poGadgetState = bitComponent.GadgetStates.G1StateObject;
					break;
				case GadgetSlot.CustomGadget2:
					this.m_poGadgetBehaviour = bitComponent.CustomGadget2;
					this.m_poGadgetState = bitComponent.GadgetStates.G2StateObject;
					break;
				case GadgetSlot.BoostGadget:
					this.m_poGadgetBehaviour = bitComponent.BoostGadget;
					this.m_poGadgetState = bitComponent.GadgetStates.GBoostStateObject;
					break;
				default:
					Debug.Assert(false, string.Format("TargetIndicator for [{0}] -> UsedGadgetSlot is invalid: [{1}]", GameHubBehaviour.Hub.Players.CurrentPlayerData.Name, this.UsedGadgetSlot), Debug.TargetTeam.All);
					this.m_fSquaredRange = this.Range * this.Range;
					this.m_fOriginalSquaredRange = this.m_fSquaredRange;
					return;
				}
				this.UpdateRange();
				this.m_fOriginalSquaredRange = this.m_fSquaredRange;
			}
			this.PostInitPlayers();
		}

		private void OnAllPlayersSpawned()
		{
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				return;
			}
			this.InitPlayers();
		}

		private void Destroy()
		{
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				return;
			}
			if (this.m_boRegisteredCallback)
			{
				GameHubBehaviour.Hub.Events.Players.ListenToAllPlayersSpawned -= this.OnAllPlayersSpawned;
			}
			for (long num = 0L; num < this.m_apoPlayers.LongLength; num += 1L)
			{
				this.m_apoPlayers[(int)(checked((IntPtr)num))] = null;
			}
			this.m_apoPlayers = null;
			this.m_poCurrentPlayer = null;
		}

		protected virtual long CalculateTarget(Vector3 stSourcePosition, ref Vector3 stClosestTargetPosition, ref Vector3 stPointingDirection)
		{
			long result = -1L;
			float num = float.MaxValue;
			Vector3 vector = Vector3.forward;
			Vector3 vector2 = Vector3.down;
			for (long num2 = 0L; num2 < this.m_apoPlayers.LongLength; num2 += 1L)
			{
				checked
				{
					if (this.m_apoPlayers[(int)((IntPtr)num2)].IsAlive())
					{
						TargetIndicator.ETarget targetType = this.TargetType;
						if (targetType != TargetIndicator.ETarget.TargetClosestAlly)
						{
							if (targetType != TargetIndicator.ETarget.TargetClosestEnemy)
							{
								Debug.Assert(false, "Unknown target type.", Debug.TargetTeam.All);
								goto IL_10A;
							}
							if (this.m_apoPlayers[(int)((IntPtr)num2)].Team == this.m_poCurrentPlayer.Team)
							{
								goto IL_10A;
							}
						}
						else if (this.m_apoPlayers[(int)((IntPtr)num2)].Team != this.m_poCurrentPlayer.Team)
						{
							goto IL_10A;
						}
						vector2 = this.m_apoPlayers[(int)((IntPtr)num2)].Transform.position;
						vector = vector2 - stSourcePosition;
						float num3 = Vector3.SqrMagnitude(vector);
						if (num3 < num)
						{
							num = num3;
							if (num3 <= this.m_fSquaredRange)
							{
								stClosestTargetPosition = vector2;
								stPointingDirection = vector;
								result = num2;
							}
						}
					}
					IL_10A:;
				}
			}
			return result;
		}

		protected void RotateElementTowardsTarget(long nClosestTarget, ref Vector3 stPointingDirection)
		{
			Quaternion quaternion = Quaternion.identity;
			if (nClosestTarget == -1L)
			{
				Quaternion quaternion2 = Quaternion.LookRotation(base.transform.forward, Vector3.up);
				quaternion = Quaternion.RotateTowards(this.m_stCurrentQuaterion, quaternion2, this.MaxRotationSpeed);
				this.m_stCurrentQuaterion = quaternion;
				this.TargetObject.transform.rotation = quaternion;
				return;
			}
			quaternion = Quaternion.LookRotation(stPointingDirection.normalized, Vector3.up);
			Quaternion quaternion3 = Quaternion.RotateTowards(this.m_stCurrentQuaterion, quaternion, this.MaxRotationSpeed);
			this.m_stCurrentQuaterion = quaternion3;
			this.TargetObject.transform.rotation = quaternion3;
		}

		protected virtual void LateUpdateVirtual()
		{
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				return;
			}
			Vector3 position = this.m_poCurrentPlayer.Transform.position;
			Vector3 forward = Vector3.forward;
			Vector3 down = Vector3.down;
			long nClosestTarget = this.CalculateTarget(position, ref down, ref forward);
			this.RotateElementTowardsTarget(nClosestTarget, ref forward);
		}

		private void LateUpdate()
		{
			this.LateUpdateVirtual();
		}

		protected static readonly BitLogger Log = new BitLogger(typeof(TargetIndicator));

		public GameObject TargetObject;

		public TargetIndicator.ETarget TargetType;

		public float Range = 12f;

		[Tooltip("Use GadgetSlot range. The attribute \"Range\" above will be ignored!")]
		public GadgetSlot UsedGadgetSlot;

		[Range(0f, 360f)]
		public float MaxRotationSpeed = 360f;

		protected CombatObject[] m_apoPlayers;

		protected CombatObject m_poCurrentPlayer;

		protected float m_fSquaredRange;

		protected float m_fOriginalSquaredRange;

		private Quaternion m_stCurrentQuaterion = Quaternion.identity;

		private bool m_boRegisteredCallback;

		protected GadgetBehaviour m_poGadgetBehaviour;

		protected GadgetData.GadgetStateObject m_poGadgetState;

		public enum ETarget
		{
			TargetClosestEnemy,
			TargetClosestAlly
		}
	}
}
