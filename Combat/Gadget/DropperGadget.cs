using System;
using System.Collections.Generic;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	public abstract class DropperGadget : GadgetBehaviour
	{
		private DropperGadgetInfo DropperGadgetInfo
		{
			get
			{
				return base.Info as DropperGadgetInfo;
			}
		}

		protected float _currentLifeTime
		{
			get
			{
				return base.LifeTime - 0.001f * (float)((long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime() - this._firstDropTimeMillis);
			}
		}

		public override void SetInfo(GadgetInfo info)
		{
			base.SetInfo(info);
			DropperGadgetInfo dropperGadgetInfo = this.DropperGadgetInfo;
			this._upgDropTime = new Upgradeable(dropperGadgetInfo.DropTimeUpgrade, dropperGadgetInfo.DropTime, dropperGadgetInfo.UpgradesValues);
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
			this._upgDropTime.SetLevel(upgradeName, level);
		}

		protected Vector3 DummyPosition(CombatObject combatObject)
		{
			return combatObject.Dummy.GetDummy(this.DropperGadgetInfo.Dummy, null).position;
		}

		protected override void GadgetUpdate()
		{
			if (!GameHubBehaviour.Hub || GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			this.CurrentTime = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			base.GadgetUpdate();
			this.UpdateGadget(this.CurrentTime);
			if (this.CurrentCooldownTime > this.CurrentTime || !base.Pressed)
			{
				return;
			}
			if (!this.Combat.Controller.ConsumeEP((float)base.ActivationCost))
			{
				return;
			}
			this.CurrentCooldownTime = (long)(this.Cooldown * 1000f) + this.CurrentTime;
			this._firstDropTimeMillis = this.CurrentTime;
			this._trapDroppers = new List<DropperGadget.TrapDropper>();
			this.StartDrop(this.Combat, this._upgDropTime, 0f);
		}

		protected void StartDrop(CombatObject combatObject, float dropTime, float initialDropDelay)
		{
			long num = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			DropperGadget.TrapDropper trapDropper = new DropperGadget.TrapDropper();
			trapDropper.LastDropPosition = Vector3.one * float.MaxValue;
			trapDropper.StartMillis = num + (long)(initialDropDelay * 1000f);
			trapDropper.EndMillis = trapDropper.StartMillis + (long)(dropTime * 1000f);
			trapDropper.CombatObject = combatObject;
			this._trapDroppers.Add(trapDropper);
		}

		private void UpdateGadget(long millis)
		{
			if (this._trapDroppers == null)
			{
				return;
			}
			this._trapDroppers.RemoveAll((DropperGadget.TrapDropper t) => millis >= t.EndMillis + (long)this.DropperGadgetInfo.ImmunityMillis);
			for (int i = 0; i < this._trapDroppers.Count; i++)
			{
				DropperGadget.TrapDropper trapDropper = this._trapDroppers[i];
				if (trapDropper.StartMillis <= millis)
				{
					if (trapDropper.EndMillis > millis)
					{
						Vector3 vector = this.DummyPosition(trapDropper.CombatObject);
						float num = Vector3.SqrMagnitude(vector - trapDropper.LastDropPosition);
						float num2 = this.DropperGadgetInfo.DropDistance * this.DropperGadgetInfo.DropDistance;
						if (num >= num2)
						{
							trapDropper.LastDropPosition = vector;
							this.DropTrap(trapDropper, vector);
						}
					}
				}
			}
		}

		protected abstract void DropTrap(DropperGadget.TrapDropper trapDropper, Vector3 position);

		private Upgradeable _upgDropTime;

		private long _firstDropTimeMillis;

		protected List<DropperGadget.TrapDropper> _trapDroppers;

		protected class TrapDropper
		{
			public Vector3 LastDropPosition;

			public long EndMillis;

			public CombatObject CombatObject;

			public long StartMillis;
		}
	}
}
