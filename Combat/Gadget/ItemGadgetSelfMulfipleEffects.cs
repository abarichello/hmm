using System;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class ItemGadgetSelfMulfipleEffects : GadgetBehaviour
	{
		public ItemGadgetSelfMultipleEffectsInfo MyInfo
		{
			get
			{
				return base.Info as ItemGadgetSelfMultipleEffectsInfo;
			}
		}

		private void Start()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			this._transform = this.Combat.transform;
			this._currentFeedbacks = new int[this.MyInfo.MultipleInfos.Length];
			this._currentEffects = new int[this.MyInfo.MultipleInfos.Length];
			for (int i = 0; i < this.MyInfo.MultipleInfos.Length; i++)
			{
				this._currentFeedbacks[i] = -1;
				this._currentEffects[i] = -1;
			}
		}

		public override void SetInfo(GadgetInfo info)
		{
			base.SetInfo(info);
			this._modifiers = new ModifierData[this.MyInfo.MultipleInfos.Length][];
			for (int i = 0; i < this._modifiers.Length; i++)
			{
				this._modifiers[i] = ModifierData.CreateData(this.MyInfo.MultipleInfos[i].Modifiers, this.MyInfo);
			}
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
			for (int i = 0; i < this._modifiers.Length; i++)
			{
				this._modifiers[i].SetLevel(upgradeName, level);
			}
		}

		protected override void Awake()
		{
			base.Awake();
			this.CurrentCooldownTime = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
		}

		protected override void GadgetUpdate()
		{
			if (!GameHubBehaviour.Hub || GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			this.CurrentTime = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			base.GadgetUpdate();
			if (this.CurrentCooldownTime > this.CurrentTime)
			{
				return;
			}
			if (!base.Pressed)
			{
				this.CurrentCooldownTime = this.CurrentTime;
				return;
			}
			if (!this.Combat.Controller.ConsumeEP((float)base.ActivationCost))
			{
				return;
			}
			long num = this.CurrentTime - this.CurrentCooldownTime;
			this.CurrentCooldownTime = (long)(this.Cooldown * 1000f) + this.CurrentTime - num;
			this.FireGadget();
		}

		protected override int FireGadget()
		{
			for (int i = 0; i < this.MyInfo.MultipleInfos.Length; i++)
			{
				EffectEvent effectEvent = base.GetEffectEvent(this.MyInfo.MultipleInfos[i].Effect);
				effectEvent.Origin = (effectEvent.Target = this._transform.position);
				effectEvent.Direction = base.CalcDirection(effectEvent.Origin, effectEvent.Target + this._transform.forward);
				effectEvent.LifeTime = this.MyInfo.MultipleInfos[i].LifeTime;
				effectEvent.Range = this.MyInfo.MultipleInfos[i].Range;
				effectEvent.Modifiers = this._modifiers[i];
				effectEvent.TargetId = this.Combat.Id.ObjId;
				int linkedTo = this.MyInfo.MultipleInfos[i].LinkedTo;
				if (linkedTo >= 0 && linkedTo < i)
				{
					effectEvent.TargetEventId = this._currentEffects[linkedTo];
				}
				this._currentEffects[i] = GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
				this._currentFeedbacks[i] = ((!(this.MyInfo.MultipleInfos[i].Feedback != null)) ? -1 : this.Combat.Feedback.Add(this.MyInfo.MultipleInfos[i].Feedback, -1, this.Combat.Id.ObjId, GameHubBehaviour.Hub.GameTime.GetPlaybackTime(), GameHubBehaviour.Hub.GameTime.GetPlaybackTime() + (int)(this.MyInfo.MultipleInfos[i].LifeTime * 1000f), 0, base.Slot));
			}
			base.OnGadgetUsed(-1);
			return -1;
		}

		private void RemoveFeedback(int index)
		{
			if (this._currentFeedbacks[index] != -1)
			{
				this.Combat.Feedback.Remove(this._currentFeedbacks[index]);
				this._currentFeedbacks[index] = -1;
			}
		}

		public override void OnObjectUnspawned(UnspawnEvent evt)
		{
			base.OnObjectUnspawned(evt);
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			for (int i = 0; i < this._currentFeedbacks.Length; i++)
			{
				this.RemoveFeedback(i);
			}
		}

		protected override void InnerOnDestroyEffect(DestroyEffectMessage evt)
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			for (int i = 0; i < this._currentEffects.Length; i++)
			{
				if (evt.RemoveData.TargetEventId == this._currentEffects[i])
				{
					this._currentEffects[i] = -1;
					this.RemoveFeedback(i);
					return;
				}
			}
		}

		private Transform _transform;

		private ModifierData[][] _modifiers;

		private int[] _currentFeedbacks;

		private int[] _currentEffects;
	}
}
