using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class ChainLink : BasicCannon
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<int> ListenToMultipleChainLinkDamaged;

		private ChainLinkInfo MyInfo
		{
			get
			{
				return base.Info as ChainLinkInfo;
			}
		}

		protected override void Awake()
		{
			base.Awake();
			this.TargetNotFoundDelay = new TimedUpdater(100, true, false);
		}

		public override void SetInfo(GadgetInfo info)
		{
			base.SetInfo(info);
			this.ChainLinkExtraModifier = ModifierData.CreateData(this.MyInfo.ChainLinkExtraModifier, base.CannonInfo);
			this.MaxChainLinks = new Upgradeable(this.MyInfo.MaxChainLinksUpgrade, this.MyInfo.MaxChainLinks, this.MyInfo.UpgradesValues);
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
			this.MaxChainLinks.SetLevel(upgradeName, level);
			this.ChainLinkExtraModifier.SetLevel(upgradeName, level);
		}

		protected override int FireGadget()
		{
			if (this.TargetNotFoundDelay.ShouldHalt())
			{
				return -1;
			}
			ChainLink.ChainStruct chainStruct = new ChainLink.ChainStruct();
			List<int> list = new List<int>(4)
			{
				this.Combat.Id.ObjId
			};
			FXInfo effect = this.MyInfo.Effect;
			Vector3 position = this.Combat.transform.position;
			List<int> ignoreIds = list;
			if (ChainLink.<>f__mg$cache0 == null)
			{
				ChainLink.<>f__mg$cache0 = new GadgetBehaviour.CriteriaFunction(GadgetBehaviour.ShortestMagnitude);
			}
			Identifiable target = base.GetTarget(effect, position, ignoreIds, ChainLink.<>f__mg$cache0, null);
			if (!target)
			{
				base.OnGadgetUsed(-1);
				return -1;
			}
			int num = this.FireChainLink(this._damage, target.transform.position, target.ObjId, this.Combat.Id.ObjId);
			list.Add(target.ObjId);
			chainStruct.EventId = num;
			chainStruct.LastWarmupId = -1;
			chainStruct.PreviousTargets = list;
			this.ChainLinksCount.Add(chainStruct);
			return num;
		}

		protected override void InnerOnDestroyEffect(DestroyEffectMessage evt)
		{
			for (int i = 0; i < this.ChainLinksCount.Count; i++)
			{
				ChainLink.ChainStruct chainStruct = this.ChainLinksCount[i];
				if (chainStruct.EventId == evt.RemoveData.TargetEventId)
				{
					if ((float)chainStruct.ChainLinksCount >= this.MaxChainLinks.Get())
					{
						this.ChainLinksCount.Remove(chainStruct);
						return;
					}
					int num = -1;
					if (chainStruct.LastWarmupId != -1)
					{
						FXInfo effect = this.MyInfo.Effect;
						Vector3 target = evt.EffectData.Target;
						List<int> previousTargets = chainStruct.PreviousTargets;
						if (ChainLink.<>f__mg$cache1 == null)
						{
							ChainLink.<>f__mg$cache1 = new GadgetBehaviour.CriteriaFunction(GadgetBehaviour.ShortestMagnitude);
						}
						Identifiable target2 = base.GetTarget(effect, target, previousTargets, ChainLink.<>f__mg$cache1, null);
						if (!target2)
						{
							this.ChainLinksCount.Remove(chainStruct);
							return;
						}
						int chainLinksCount = chainStruct.ChainLinksCount;
						if (chainLinksCount != 0)
						{
							if (chainLinksCount == 1)
							{
								num = this.FireChainLink(this.ChainLinkExtraModifier, target2.transform.position, target2.ObjId, evt.EffectData.SourceId);
							}
						}
						else
						{
							num = this.FireChainLink(this.ExtraModifier, target2.transform.position, target2.ObjId, evt.EffectData.SourceId);
						}
						chainStruct.LastWarmupId = -1;
						chainStruct.PreviousTargets.Add(target2.ObjId);
						chainStruct.ChainLinksCount++;
						if (this.ListenToMultipleChainLinkDamaged != null)
						{
							this.ListenToMultipleChainLinkDamaged(chainStruct.ChainLinksCount);
						}
					}
					else
					{
						int chainLinksCount2 = chainStruct.ChainLinksCount;
						if (chainLinksCount2 != 0)
						{
							if (chainLinksCount2 == 1)
							{
								num = base.FireWithSpecifiedTarget(evt.EffectData.TargetId, () => this.FireExtraWarmup(this.MyInfo.ExtraWarmupEffect, evt.EffectData, this.MyInfo.ExtraWarmupSeconds));
							}
						}
						else
						{
							num = base.FireWithSpecifiedTarget(evt.EffectData.TargetId, () => this.FireExtraWarmup(this.MyInfo.ExtraEffect, evt.EffectData, this.ExtraLifeTime));
						}
						chainStruct.LastWarmupId = num;
					}
					base.ExistingFiredEffectsAdd(num);
					chainStruct.EventId = num;
				}
			}
		}

		protected virtual int FireChainLink(ModifierData[] modifier, Vector3 targetPos, int targetId, int sourceId)
		{
			EffectEvent effectEvent = base.GetEffectEvent(this.MyInfo.Effect);
			effectEvent.MoveSpeed = this._moveSpeed.Get();
			effectEvent.Range = this.GetRange();
			effectEvent.Target = targetPos;
			effectEvent.SourceId = sourceId;
			effectEvent.TargetId = targetId;
			effectEvent.LifeTime = ((base.LifeTime <= 0f) ? (effectEvent.Range / effectEvent.MoveSpeed) : base.LifeTime);
			effectEvent.Modifiers = ModifierData.CopyData(modifier);
			return GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
		}

		protected virtual int FireExtraWarmup(FXInfo effect, EffectEvent effectEvent, float lifetime)
		{
			EffectEvent effectEvent2 = base.GetEffectEvent(effect);
			effectEvent2.MoveSpeed = this._moveSpeed.Get();
			effectEvent2.Range = this.GetRange();
			effectEvent2.Target = effectEvent.Target;
			effectEvent2.SourceId = effectEvent.TargetId;
			effectEvent2.LifeTime = lifetime;
			return GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent2);
		}

		protected readonly List<ChainLink.ChainStruct> ChainLinksCount = new List<ChainLink.ChainStruct>(4);

		protected Upgradeable MaxChainLinks;

		protected ModifierData[] ChainLinkExtraModifier;

		protected TimedUpdater TargetNotFoundDelay;

		[CompilerGenerated]
		private static GadgetBehaviour.CriteriaFunction <>f__mg$cache0;

		[CompilerGenerated]
		private static GadgetBehaviour.CriteriaFunction <>f__mg$cache1;

		public class ChainStruct
		{
			public int EventId;

			public int LastWarmupId;

			public int ChainLinksCount;

			public List<int> PreviousTargets;
		}
	}
}
