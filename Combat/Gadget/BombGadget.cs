using System;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class BombGadget : BasicLink, TriggerEnterCallback.ITriggerEnterCallbackListener, LinkCreatedCallback.ILinkCreatedCallbackListener
	{
		public BombGadgetInfo BombGadgetInfo
		{
			get
			{
				return base.Info as BombGadgetInfo;
			}
		}

		public BombGadget.State CurrentState
		{
			get
			{
				return this._currentState;
			}
			private set
			{
				this._currentState = value;
			}
		}

		public bool IsPowerShooting
		{
			get
			{
				return this._holdingStarted;
			}
		}

		public override void SetInfo(GadgetInfo info)
		{
			base.SetInfo(info);
			GameHubBehaviour.Hub.BombManager.ListenToBombDelivery += this.OnBombDelivered;
			this._spinningSpeedThreshold = new Upgradeable(this.BombGadgetInfo.SpinningSpeedThresholdUpgrade, this.BombGadgetInfo.SpinningSpeedThreshold, this.BombGadgetInfo.UpgradesValues);
			this._spinningTimeActivation = new Upgradeable(this.BombGadgetInfo.SpinningTimeActivationUpgrade, this.BombGadgetInfo.SpinningTimeActivation, this.BombGadgetInfo.UpgradesValues);
			this._spinningTimeDeactivation = new Upgradeable(this.BombGadgetInfo.SpinningTimeDeactivationUpgrade, this.BombGadgetInfo.SpinningTimeDeactivation, this.BombGadgetInfo.UpgradesValues);
			this._spinningReleaseLifetime = new Upgradeable(this.BombGadgetInfo.SpinningReleaseLifetimeUpgrade, this.BombGadgetInfo.SpinningReleaseLifetime, this.BombGadgetInfo.UpgradesValues);
			this._bombLinkCreatedModifiers = ModifierData.CreateData(this.BombGadgetInfo.BombLinkCreatedModifiers);
			this._warmupModifiers = ModifierData.CreateData(this.BombGadgetInfo.WarmupModifiers, this.BombGadgetInfo);
			this._linkCancelWarmupModifiers = ModifierData.CreateData(this.BombGadgetInfo.LinkCancelWarmupModifiers);
			this._grabberMissOnHitBombModifiers = ModifierData.CreateData(this.BombGadgetInfo.GrabberMissOnHitBombModifiers, this.BombGadgetInfo);
			this._jammedModifiers = ModifierData.CreateData(this.BombGadgetInfo.JammedModifiers, this.BombGadgetInfo);
			this._spinningModifiers = ModifierData.CreateData(this.BombGadgetInfo.SpinningModifiers, this.BombGadgetInfo);
			this._spinningReleaseModifiers = ModifierData.CreateData(this.BombGadgetInfo.SpinningReleaseModifiers, this.BombGadgetInfo);
			this._bombHoldWarmupModifiers = ModifierData.CreateData(this.BombGadgetInfo.BombHoldWarmupModifiers, this.BombGadgetInfo);
			this._bombHolderModifiers = ModifierData.CreateData(this.BombGadgetInfo.BombHolderModifiers, this.BombGadgetInfo);
			this._linkBrokenModifiers = ModifierData.CreateData(this.BombGadgetInfo.LinkBrokenModifiers, this.BombGadgetInfo);
			this._pushBackCos = -Mathf.Cos(this.BombGadgetInfo.PushBackAngle * 0.0174532924f);
			this._leewayOffsetCos = -Mathf.Cos(this.BombGadgetInfo.LeewayOffsetAngle * 0.0174532924f);
			this._sqrJammerRange = this.BombGadgetInfo.JammedRange * this.BombGadgetInfo.JammedRange;
			this._powerStartTime = -1f;
			this._lastPower = 0f;
			this._lastPowerMax = false;
			this._holdingStarted = false;
			this.CurrentState = BombGadget.State.Disabled;
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
			this._spinningSpeedThreshold.SetLevel(upgradeName, level);
			this._spinningTimeActivation.SetLevel(upgradeName, level);
			this._spinningTimeDeactivation.SetLevel(upgradeName, level);
			this._spinningReleaseLifetime.SetLevel(upgradeName, level);
			this._bombLinkCreatedModifiers.SetLevel(upgradeName, level);
			this._warmupModifiers.SetLevel(upgradeName, level);
			this._linkCancelWarmupModifiers.SetLevel(upgradeName, level);
			this._grabberMissOnHitBombModifiers.SetLevel(upgradeName, level);
			this._jammedModifiers.SetLevel(upgradeName, level);
			this._spinningModifiers.SetLevel(upgradeName, level);
			this._spinningReleaseModifiers.SetLevel(upgradeName, level);
			this._bombHoldWarmupModifiers.SetLevel(upgradeName, level);
			this._bombHolderModifiers.SetLevel(upgradeName, level);
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			GameHubBehaviour.Hub.BombManager.ListenToBombDelivery -= this.OnBombDelivered;
		}

		public void Disable(BombGadget.DisableReason reason)
		{
			if (this._isDisabled)
			{
				return;
			}
			this._isDisabled = true;
			this.CurrentState = BombGadget.State.Disabled;
			base.DestroyExistingFiredEffects();
			GameHubBehaviour.Hub.BombManager.StopCompetingForBomb(this.Combat);
			if (GameHubBehaviour.Hub.BombManager.IsCarryingBomb(this.Combat.Id.ObjId))
			{
				if (this.Combat.IsAlive())
				{
					SpawnReason reason2;
					if (reason == BombGadget.DisableReason.Input && GameHubBehaviour.Hub.BombManager.ActiveBomb.State == BombInstance.BombState.Meteor)
					{
						reason2 = SpawnReason.InputDrop;
					}
					else if (reason == BombGadget.DisableReason.Input && this._spinningEffect != -1 && !this._link.IsBroken)
					{
						reason2 = SpawnReason.InputDrop;
						GameHubBehaviour.Hub.BombManager.ActiveBomb.State = BombInstance.BombState.Meteor;
					}
					else
					{
						if (reason == BombGadget.DisableReason.LinkBroke)
						{
							reason2 = SpawnReason.BrokenLink;
						}
						else if (reason == BombGadget.DisableReason.Input)
						{
							reason2 = SpawnReason.InputDrop;
						}
						else
						{
							reason2 = SpawnReason.TriggerDrop;
						}
						GameHubBehaviour.Hub.BombManager.ActiveBomb.State = BombInstance.BombState.Idle;
					}
					GameHubBehaviour.Hub.BombManager.OnBombDropped(base.Id.ObjId, reason2);
				}
				else
				{
					GameHubBehaviour.Hub.BombManager.ActiveBomb.State = BombInstance.BombState.Idle;
					GameHubBehaviour.Hub.BombManager.OnBombDropped(base.Id.ObjId, SpawnReason.Death);
				}
			}
			if (this._link != null)
			{
				this._link.Break();
			}
			if (this._linkEffect != -1)
			{
			}
			this._jammedEffect = (this._spinningEffect = (this._linkEffect = (this._grabberEffect = (this._warmupEffect = (this._linkCreatedEffect = -1)))));
			this._link = null;
			this._timeLinked = 0f;
			this._powerStartTime = -1f;
			this._holdingStarted = false;
			this._lastPower = 0f;
			this._lastPowerMax = false;
			this._dropped = (reason == BombGadget.DisableReason.Dropper);
			this._canBreakLink = false;
			this._canShoot = false;
			this.Combat.GadgetStates.SetGadgetState(this._gadgetState, base.Slot, GadgetState.Ready, 0L, 0, 0f, 0, null);
		}

		protected override int FireGadget()
		{
			if (this._linkEffect != -1)
			{
				return -1;
			}
			this.TargetId = GameHubBehaviour.Hub.BombManager.BombMovement.Combat.Id.ObjId;
			this._grabberEffect = base.FireGadget();
			this.Combat.GadgetStates.SetGadgetState(this._gadgetState, base.Slot, GadgetState.Waiting, 0L, 0, 0f, 0, null);
			if (this.Combat.Movement.HasLinkWith(GameHubBehaviour.Hub.BombManager.BombMovement))
			{
				for (int i = 0; i < 0; i++)
				{
					if (this.Combat.Movement.Links[i].Point1.Movement == GameHubBehaviour.Hub.BombManager.BombMovement || this.Combat.Movement.Links[i].Point2.Movement == GameHubBehaviour.Hub.BombManager.BombMovement)
					{
						this.Combat.Movement.Links[i].Break();
						this.Combat.GadgetStates.SetGadgetState(this._gadgetState, base.Slot, GadgetState.Ready, 0L, 0, 0f, 0, null);
					}
				}
			}
			return this._grabberEffect;
		}

		protected override int FireWarmup()
		{
			if (this._powerStartTime > 0f || this._lastPower > 0f)
			{
				return -1;
			}
			bool flag = this._canBreakLink && this._linkEffect != -1 && this._timeLinked > this.BombGadgetInfo.MinimumLinkedTime;
			if (flag)
			{
				this._canBreakLink = false;
				this._powerStartTime = Time.time;
				this._lastPower = 0f;
				this._lastPowerMax = false;
				this._holdingStarted = false;
				this.CurrentState = BombGadget.State.PowerShot;
				if (!string.IsNullOrEmpty(this.BombGadgetInfo.BombHoldWarmupEffect.Effect))
				{
					int effectID = this.FireExtraGadget(this.BombGadgetInfo.BombHoldWarmupEffect, this._bombHoldWarmupModifiers, null, delegate(EffectEvent x)
					{
						x.LifeTime = this.BombGadgetInfo.DropTime;
					});
					base.ExistingFiredEffectsAdd(effectID);
				}
			}
			if (!this._canShoot)
			{
				return -1;
			}
			this.Disable(BombGadget.DisableReason.Input);
			this._warmupEffect = this.FireWarmup(delegate(EffectEvent data)
			{
				data.Modifiers = this._warmupModifiers;
			});
			base.ExistingFiredEffectsAdd(this._warmupEffect);
			this._timeGrabbing = 0f;
			this.CurrentState = BombGadget.State.Grabbing;
			return this._warmupEffect;
		}

		public void OnLinkCreatedCallback(LinkCreatedCallback evt)
		{
			this._timeLinked = 0f;
			this._link = evt.Link;
			this._link.Compression = 0f;
			Vector3 normalized = (this._link.Point2.Position - this._link.Point1.Position).normalized;
			float bombDirCos = Vector3.Dot(normalized, this.Combat.Transform.forward);
			this.ProcessLinkLengthOffset(bombDirCos);
			this._linkCreatedEffect = this.FireExtraGadget(this.BombGadgetInfo.BombLinkCreated, this._bombLinkCreatedModifiers, null, delegate(EffectEvent data)
			{
				data.TargetId = evt.Point2.Id.ObjId;
				data.Origin = evt.Point2.Transform.position;
				data.LifeTime = this.BombGadgetInfo.BombLinkCreatedLifetime;
			});
			base.ExistingFiredEffectsAdd(this._linkCreatedEffect);
		}

		public override void OnTriggerEnterCallback(TriggerEnterCallback evt)
		{
			if (GameHubBehaviour.Hub.BombManager.IsSomeoneCarryingBomb() || !this.Combat.IsAlive() || this.GrabbingThroughWall(evt.Combat))
			{
				return;
			}
			this.ProcessJamming();
			CombatMovement bombMovement = GameHubBehaviour.Hub.BombManager.BombMovement;
			BaseFX baseFx = GameHubBehaviour.Hub.Effects.GetBaseFx(this._grabberEffect);
			if ((bombMovement.HasLinkWithTag(base.MyInfo.TagLink) || this.Combat.Movement.HasLinkWithTag(base.MyInfo.TagLink)) && !base.StealLink && this._jammedEffect == -1)
			{
				this._jammedEffect = this.FireExtraGadget(this.BombGadgetInfo.GrabberMiss, this._grabberMissOnHitBombModifiers, this._grabberMissOnHitBombModifiers, delegate(EffectEvent data)
				{
					data.TargetId = evt.Combat.Id.ObjId;
				});
				base.ExistingFiredEffectsAdd(this._jammedEffect);
				if (baseFx != null)
				{
					baseFx.TriggerDefaultDestroy(GameHubBehaviour.Hub.BombManager.BombMovement.Id.ObjId);
					base.ExistingFiredEffectsRemove(this._grabberEffect);
				}
			}
			if (this._jammedEffect == -1 && !GameHubBehaviour.Hub.BombManager.IsPickDisabled(this.Combat))
			{
				this._linkEffect = base.FireExtraGadget(bombMovement.Id.ObjId);
				base.ExistingFiredEffectsAdd(this._linkEffect);
				this._powerStartTime = 0f;
				this._holdingStarted = false;
				this._lastPower = 0f;
				this._lastPowerMax = false;
				GameHubBehaviour.Hub.BombManager.GrabBomb(this.Combat);
				if (baseFx != null)
				{
					baseFx.TriggerDefaultDestroy(bombMovement.Id.ObjId);
				}
				this._grabberEffect = -1;
				base.ExistingFiredEffectsRemove(this._grabberEffect);
				this.Combat.GadgetStates.SetGadgetState(this._gadgetState, base.Slot, GadgetState.Ready, 0L, 0, 0f, 0, null);
				this.CurrentState = BombGadget.State.Linked;
			}
		}

		private bool GrabbingThroughWall(CombatObject bomb)
		{
			Vector3 position = this.Combat.transform.position;
			Vector3 direction = bomb.transform.position - position;
			RaycastHit raycastHit;
			return Physics.Raycast(position, direction, out raycastHit, direction.magnitude, 512);
		}

		protected override void OnMyEffectDestroyed(DestroyEffect evt)
		{
			if (evt.RemoveData.TargetEventId == this._linkCreatedEffect && this._link != null)
			{
				this._link.Compression = base.MyInfo.Compression;
			}
			else if (evt.RemoveData.TargetEventId == this._linkEffect)
			{
				if (this._spinningEffect != -1 || this._lastPower > 0f)
				{
					Vector3 direction = GameHubBehaviour.Hub.BombManager.BombMovement.LastVelocity.normalized;
					float power = 1f;
					if (this._lastPower > 0f)
					{
						direction = this.Combat.Transform.forward;
						power = Mathf.Max(1f, this._lastPower);
					}
					int effectID = this.FireMeteor(direction, power, this._lastPowerMax);
					base.ExistingFiredEffectsRemove(effectID);
				}
				this.Disable(BombGadget.DisableReason.Input);
			}
		}

		public int FireMeteor(Vector3 direction, float power, bool fullPower = false)
		{
			ModifierData[] modifierData = this._spinningReleaseModifiers;
			if (power > 1f)
			{
				modifierData = ModifierData.CreateConvoluted(this._spinningReleaseModifiers, power);
				GameHubBehaviour.Hub.BombManager.BombMovement.ResetImpulseAndVelocity();
			}
			int num = this.FireExtraGadget((!fullPower) ? this.BombGadgetInfo.SpinningReleaseEffect : this.BombGadgetInfo.SpinningReleaseMaxPowerEffect, modifierData, null, delegate(EffectEvent data)
			{
				data.Origin = GameHubBehaviour.Hub.BombManager.BombMovement.Combat.Transform.position;
				data.TargetId = GameHubBehaviour.Hub.BombManager.BombMovement.Id.ObjId;
				data.LifeTime = this._spinningReleaseLifetime;
				data.Direction = direction;
			});
			BaseFX baseFx = GameHubBehaviour.Hub.Effects.GetBaseFx(GameHubBehaviour.Hub.BombManager.ActiveBomb.lastMeteorEventId);
			if (baseFx)
			{
				baseFx.TriggerDefaultDestroy(-1);
			}
			GameHubBehaviour.Hub.BombManager.ActiveBomb.lastMeteorEventId = num;
			GameHubBehaviour.Hub.BombManager.ActiveBomb.State = BombInstance.BombState.Meteor;
			GameHubBehaviour.Hub.Effects.ListenToDestroy(num, new EffectsManager.EffectDestroyed(GameHubBehaviour.Hub.BombManager.OnMeteorEnded));
			return num;
		}

		protected void ProcessSpinning()
		{
			if (this._linkEffect != -1 && this._link != null && Mathf.Abs(this._link.Point2.PerpendicularSpeed) > this._spinningSpeedThreshold)
			{
				this._timeSpinning += Time.deltaTime;
				if (this._timeSpinning >= this._spinningTimeActivation && this._spinningEffect == -1)
				{
					this._spinningEffect = this.FireExtraGadget(this.BombGadgetInfo.SpinningEffect, this._spinningModifiers, null, delegate(EffectEvent data)
					{
						data.Origin = GameHubBehaviour.Hub.BombManager.BombMovement.Combat.Transform.position;
						data.TargetId = GameHubBehaviour.Hub.BombManager.BombMovement.Id.ObjId;
					});
					base.ExistingFiredEffectsAdd(this._spinningEffect);
					GameHubBehaviour.Hub.BombManager.OnSpinningBegins(this.Combat.Id.ObjId);
				}
				this._timeNotSpinning = 0f;
			}
			else if (this._spinningEffect != -1)
			{
				this._timeNotSpinning += Time.deltaTime;
				if (this._timeNotSpinning > this._spinningTimeDeactivation)
				{
					BaseFX baseFx = GameHubBehaviour.Hub.Effects.GetBaseFx(this._spinningEffect);
					if (baseFx == null)
					{
						return;
					}
					baseFx.TriggerDefaultDestroy(GameHubBehaviour.Hub.BombManager.BombMovement.Id.ObjId);
					base.ExistingFiredEffectsRemove(this._spinningEffect);
					this._spinningEffect = -1;
					GameHubBehaviour.Hub.BombManager.OnSpinningEnd(this.Combat.Id.ObjId);
				}
			}
			else
			{
				this._timeSpinning = 0f;
				this._timeNotSpinning = 0f;
			}
		}

		protected void ProcessJamming()
		{
			bool flag = false;
			bool flag2 = (GameHubBehaviour.Hub.BombManager.BombMovement.Combat.Transform.position - this.Combat.Transform.position).sqrMagnitude <= this._sqrJammerRange;
			bool flag3 = GameHubBehaviour.Hub.BombManager.IsSomeoneCarryingBomb();
			bool flag4 = this._grabberEffect != -1;
			bool flag5 = this._jammedEffect == -1;
			bool flag6 = GameHubBehaviour.Hub.BombManager.IsOtherTeamCompetingForBomb(this.Combat);
			if (flag4 && !flag3 && flag2)
			{
				flag = GameHubBehaviour.Hub.BombManager.CompeteForBomb(this.Combat);
			}
			else
			{
				GameHubBehaviour.Hub.BombManager.StopCompetingForBomb(this.Combat);
			}
			if (!flag5 && (!flag || !flag6))
			{
				this.Disable(BombGadget.DisableReason.Input);
				if (base.Pressed)
				{
					base.ExistingFiredEffectsAdd(this.FireGadget());
				}
			}
			else if (flag4 && flag5 && flag && flag6 && !flag3)
			{
				this._jammedEffect = this.FireExtraGadget(this.BombGadgetInfo.JammedEffect, this._jammedModifiers, this._jammedModifiers, delegate(EffectEvent data)
				{
					data.Origin = GameHubBehaviour.Hub.BombManager.BombMovement.Combat.transform.position;
					data.TargetId = GameHubBehaviour.Hub.BombManager.BombMovement.Id.ObjId;
				});
				base.ExistingFiredEffectsAdd(this._jammedEffect);
				BaseFX baseFx = GameHubBehaviour.Hub.Effects.GetBaseFx(this._grabberEffect);
				if (baseFx != null)
				{
					baseFx.TriggerDefaultDestroy(GameHubBehaviour.Hub.BombManager.BombMovement.Id.ObjId);
					base.ExistingFiredEffectsRemove(this._grabberEffect);
				}
			}
		}

		protected void ProcessPushBack()
		{
			if (this._powerStartTime > 0f)
			{
				return;
			}
			if (this._link != null && this._link.PositionDiff != Vector3.zero)
			{
				Vector3 normalized = this._link.PositionDiff.normalized;
				float num = Vector3.Dot(normalized, this.Combat.Transform.forward);
				if (this.BombGadgetInfo.PushBackForce > 0f && num > this._pushBackCos)
				{
					Vector3 vector = (Vector3.Dot(normalized, this.Combat.Transform.right) <= 0f) ? Vector3.Cross(normalized, this.Combat.Transform.up) : Vector3.Cross(normalized, -this.Combat.Transform.up);
					float num2 = 1f + num;
					float num3 = Vector3.Dot(vector, this._link.Point2.Velocity - this._link.Point1.Velocity);
					if (num3 <= 0f || this.BombGadgetInfo.PushBack == BombGadgetInfo.PushBackType.Accelerate)
					{
						float num4 = this.BombGadgetInfo.PushBackForce * num2;
						if (this.BombGadgetInfo.PushBack == BombGadgetInfo.PushBackType.SetSpeed)
						{
							num4 -= num3;
						}
						this._link.Point2.Movement.Push(vector, false, num4, true);
					}
				}
				this.ProcessLinkLengthOffset(num);
			}
		}

		private void ProcessLinkLengthOffset(float bombDirCos)
		{
			if (this._powerStartTime > 0f)
			{
				return;
			}
			if (!this._link.IsFixedPivot && bombDirCos > this._leewayOffsetCos)
			{
				this._link.SetLengthOffset(this.BombGadgetInfo.LeewayOffset);
			}
			else if (this.BombGadgetInfo.ProgressiveLeeway && this._leewayOffsetCos > -1f)
			{
				this._link.SetLengthOffset((bombDirCos + 1f) / (this._leewayOffsetCos + 1f) * this.BombGadgetInfo.LeewayOffset);
			}
			else
			{
				this._link.SetLengthOffset(0f);
			}
		}

		private void ProcessPowerShot()
		{
			if (this._powerStartTime <= 0f)
			{
				return;
			}
			BombGadgetInfo info = this.BombGadgetInfo;
			float num = Time.time - this._powerStartTime;
			if (!this._holdingStarted)
			{
				if (num >= info.DropTime)
				{
					base.DestroyExistingFiredEffects();
					this._holdingStarted = true;
					this._linkEffect = this.FireExtraGadget(info.BombHolderEffect, this._bombHolderModifiers, null, delegate(EffectEvent data)
					{
						data.TargetId = GameHubBehaviour.Hub.BombManager.BombMovement.Id.ObjId;
					});
					base.ExistingFiredEffectsAdd(this._linkEffect);
					return;
				}
				if (this.Combat.Attributes.IsGadgetDisarmed(base.Slot, base.Nature))
				{
					this.CurrentState = BombGadget.State.Linked;
					this._powerStartTime = -1f;
					return;
				}
				if (!base.Pressed)
				{
					GameHubBehaviour.Hub.BombManager.LastDropData.Populated = true;
					GameHubBehaviour.Hub.BombManager.LastDropData.DropperPosition = this.Combat.Transform.position;
					GameHubBehaviour.Hub.BombManager.LastDropData.HoldTime = num;
					GameHubBehaviour.Hub.BombManager.LastDropData.PowerShot = false;
					GameHubBehaviour.Hub.BombManager.LastDropData.DropperForwardAngle = this.Combat.transform.forward.Rotation();
					this._powerStartTime = -1f;
					this._lastPower = 0f;
					this._lastPowerMax = false;
					this._linkEffect = this.FireExtraGadget(info.GrabberCancelWarmup, this._linkCancelWarmupModifiers, null, delegate(EffectEvent data)
					{
						data.TargetId = GameHubBehaviour.Hub.BombManager.BombMovement.Id.ObjId;
						data.LifeTime = info.GrabberCancelWarmupLifetime;
					});
					base.DestroyExistingFiredEffects();
				}
				return;
			}
			else
			{
				if (num < info.DropTime + info.HoldStartTime)
				{
					return;
				}
				if (!base.Pressed || num >= info.TotalPowerHoldTime)
				{
					float num2 = num - info.DropTime;
					this._lastPowerMax = (num2 >= info.TotalPowerUpTime);
					float num3 = Mathf.Clamp(num2, 0f, info.TotalPowerUpTime);
					this._lastPower = num3 * info.HoldPowerPerSecond;
					this._linkEffect = this.FireExtraGadget(info.GrabberCancelWarmup, this._linkCancelWarmupModifiers, null, delegate(EffectEvent data)
					{
						data.TargetId = GameHubBehaviour.Hub.BombManager.BombMovement.Id.ObjId;
						data.LifeTime = info.GrabberCancelWarmupLifetime;
					});
					this._powerStartTime = -1f;
					GameHubBehaviour.Hub.BombManager.LastDropData.Populated = true;
					GameHubBehaviour.Hub.BombManager.LastDropData.DropperPosition = this.Combat.Transform.position;
					GameHubBehaviour.Hub.BombManager.LastDropData.HoldTime = Mathf.Min(num, info.TotalPowerHoldTime);
					GameHubBehaviour.Hub.BombManager.LastDropData.PowerShot = true;
					GameHubBehaviour.Hub.BombManager.LastDropData.DropperForwardAngle = this.Combat.transform.forward.Rotation();
				}
				return;
			}
		}

		protected override void GadgetUpdate()
		{
			base.GadgetUpdate();
			this.ProcessJamming();
			this.ProcessSpinning();
			this.ProcessPowerShot();
			this._canBreakLink |= ((this._link != null && !base.Pressed) || (this._link != null && this._timeLinked > this.BombGadgetInfo.TotalPowerHoldTime));
			if (this._link != null)
			{
				this._timeLinked += Time.deltaTime;
			}
			if (this._link != null && (this._link.IsBroken || this._linkEffect == -1))
			{
				if (!string.IsNullOrEmpty(this.BombGadgetInfo.LinkBrokenEffect.Effect))
				{
					int effectID = this.FireExtraGadget(this.BombGadgetInfo.LinkBrokenEffect, this._linkBrokenModifiers, null, delegate(EffectEvent x)
					{
						x.LifeTime = this.BombGadgetInfo.LinkBrokenLifeTime;
					});
					base.ExistingFiredEffectsRemove(effectID);
				}
				this.Disable(BombGadget.DisableReason.LinkBroke);
			}
			if (this._linkEffect == -1 && this._warmupEffect == -1 && (!base.Pressed || this._dropped) && !GameHubBehaviour.Hub.BombManager.IsGrabberDisabled(this.Combat))
			{
				this._canShoot = true;
				this._canBreakLink = false;
			}
			if (this._grabberEffect != -1)
			{
				this._timeGrabbing += Time.deltaTime;
				if (!base.Pressed && this._timeGrabbing > this.BombGadgetInfo.MinimumGrabberUseTime)
				{
					this.Disable(BombGadget.DisableReason.Input);
				}
				else if (this.Combat.Attributes.IsGadgetDisarmed(base.Slot, base.Nature))
				{
					this.Disable(BombGadget.DisableReason.Input);
				}
			}
			this.ProcessPushBack();
			if (this._isDisabled)
			{
				GameHubBehaviour.Hub.BombManager.EnableBombGrabber(this.Combat);
				this._isDisabled = false;
			}
		}

		private void OnBombDelivered(int causer, TeamKind team, Vector3 deliveryPosition)
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			if (this._linkEffect != -1)
			{
			}
			this._jammedEffect = (this._spinningEffect = (this._linkEffect = (this._grabberEffect = (this._warmupEffect = (this._linkCreatedEffect = -1)))));
			this.Combat.GadgetStates.SetGadgetState(this._gadgetState, base.Slot, GadgetState.Ready, 0L, 0, 0f, 0, null);
			this.Disable(BombGadget.DisableReason.Input);
		}

		public override void OnObjectUnspawned(UnspawnEvent evt)
		{
			base.OnObjectUnspawned(evt);
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			base.Pressed = false;
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			this.Disable(BombGadget.DisableReason.Input);
		}

		public override bool IsBombBlocked(BaseFX baseFX)
		{
			return true;
		}

		public static readonly BitLogger Log = new BitLogger(typeof(BombGadget));

		private BombGadget.State _currentState;

		private int _linkEffect = -1;

		private int _grabberEffect = -1;

		private int _warmupEffect = -1;

		private int _spinningEffect = -1;

		private int _linkCreatedEffect = -1;

		private int _jammedEffect = -1;

		private float _powerStartTime;

		private float _lastPower;

		private bool _lastPowerMax;

		private bool _holdingStarted;

		private bool _canBreakLink;

		private bool _canShoot = true;

		private bool _dropped;

		private bool _isDisabled;

		private float _timeSpinning;

		private float _timeNotSpinning;

		private float _timeGrabbing;

		private float _timeLinked;

		private CombatLink _link;

		private float _pushBackCos;

		private float _leewayOffsetCos;

		private float _sqrJammerRange;

		protected Upgradeable _spinningSpeedThreshold;

		protected Upgradeable _spinningTimeActivation;

		protected Upgradeable _spinningTimeDeactivation;

		protected Upgradeable _spinningReleaseLifetime;

		private ModifierData[] _bombLinkCreatedModifiers;

		private ModifierData[] _warmupModifiers;

		private ModifierData[] _linkCancelWarmupModifiers;

		private ModifierData[] _grabberMissOnHitBombModifiers;

		private ModifierData[] _jammedModifiers;

		private ModifierData[] _spinningModifiers;

		private ModifierData[] _spinningReleaseModifiers;

		private ModifierData[] _bombHoldWarmupModifiers;

		private ModifierData[] _bombHolderModifiers;

		private ModifierData[] _linkBrokenModifiers;

		public enum State
		{
			Disabled,
			Grabbing,
			Linked,
			PowerShot
		}

		public enum DisableReason
		{
			Input,
			Dropper,
			LinkBroke
		}
	}
}
