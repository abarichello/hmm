using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Event;
using Pocketverse;
using Pocketverse.MuralContext;
using SharedUtils.Loading;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class EffectsManager : GameHubBehaviour, ICleanupListener
	{
		private void Update()
		{
			this._thisFrameDestroys.Clear();
		}

		public void Trigger(EventData data)
		{
			if (data.Content is EffectRemoveEvent)
			{
				this.DestroyEffect(data, (EffectRemoveEvent)data.Content);
				return;
			}
			EffectEvent effectEvent = data.Content as EffectEvent;
			if (effectEvent == null)
			{
				return;
			}
			GadgetSlot sourceSlot = effectEvent.SourceSlot;
			if (!effectEvent.SourceGadget)
			{
				Identifiable @object = GameHubBehaviour.Hub.ObjectCollection.GetObject(effectEvent.SourceId);
				if (!@object)
				{
					EffectsManager.Log.WarnFormat("Failed to spawn effect={0}, object={1} not found", new object[]
					{
						data.EventId,
						effectEvent.SourceId
					});
					return;
				}
				effectEvent.SourceCombat = @object.GetComponent<CombatObject>();
				if (!effectEvent.SourceCombat)
				{
					EffectsManager.Log.WarnFormat("Failed to spawn effect={0}, object={1} has no combat controller.", new object[]
					{
						data.EventId,
						effectEvent.SourceId
					});
					return;
				}
				effectEvent.SourceGadget = effectEvent.SourceCombat.GetGadget(sourceSlot);
			}
			this.StartEffect(data, effectEvent);
		}

		private void Start()
		{
			GameHubBehaviour.Hub.Events.Players.CurrentPlayerCreatedCallback += this.OnCurrentPlayerCreated;
		}

		private void OnDestroy()
		{
			GameHubBehaviour.Hub.Events.Players.CurrentPlayerCreatedCallback -= this.OnCurrentPlayerCreated;
		}

		public BaseFX GetBaseFx(int id)
		{
			BaseFX result;
			this._effects.TryGetValue(id, out result);
			return result;
		}

		private void OnCurrentPlayerCreated(PlayerEvent playerData)
		{
			if (!GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			this.m_oPlayerPositionTransform = GameHubBehaviour.Hub.Players.CurrentPlayerData.CharacterInstance.GetBitComponent<CombatObject>().Transform;
			this.m_playerPositionTransformSet = this.m_oPlayerPositionTransform;
		}

		private void StartEffect(EventData data, EffectEvent content)
		{
			if (content == null)
			{
				EffectsManager.Log.Error("Null content on EffectManager!");
				return;
			}
			if (content.EffectInfo == null)
			{
				EffectsManager.Log.Error("Null content EffectInfo on EffectManager!");
				return;
			}
			if (data == null)
			{
				EffectsManager.Log.Error((content.EffectInfo == null) ? "Null data on EffectManager!" : ("Null data on EffectManager! " + content.EffectInfo.Effect));
				return;
			}
			if (this._effects.ContainsKey(data.EventId))
			{
				EffectsManager.Log.WarnFormat("Trying to create duplicate effect data={0} Position={1} SourceGadget={2}", new object[]
				{
					data,
					content.Origin,
					(!(content.SourceGadget == null)) ? content.SourceGadget.Info.name : null
				});
				return;
			}
			if (GameHubBehaviour.Hub.Net.IsClient() && this.m_playerPositionTransformSet)
			{
				Vector3 a = this.m_oPlayerPositionTransform.position;
				a -= content.Origin;
				if (content.EffectInfo.ForceCreation || a.sqrMagnitude <= 14400f || content.EffectInfo.Instantaneous || content.LifeTime > 3f || content.LifeTime > 0f)
				{
				}
			}
			ResourcesContent.Content asset = LoadingManager.ResourceContent.GetAsset(content.EffectInfo.Effect);
			if (asset == null)
			{
				EffectsManager.Log.ErrorFormat("Could not get effect '{0}'. EffectId: {1}  SourceGadget: {2}  SourceGadgetInfo: {3}!!", new object[]
				{
					content.EffectInfo.Effect,
					content.EffectInfo.EffectId,
					(!(content.SourceGadget == null)) ? content.SourceGadget.name : null,
					(!(content.SourceGadget == null)) ? content.SourceGadget.Info.name : null
				});
				return;
			}
			if (asset == null)
			{
				UnityEngine.Debug.LogError("Resource null " + content.EffectInfo.Effect);
				return;
			}
			Transform transform = (Transform)asset.Asset;
			if (transform == null)
			{
				EffectsManager.Log.ErrorFormat("FX Asset for '{0}' not found. EffectId: {1}  SourceGadget: {2}  SourceGadgetInfo: {3}!!", new object[]
				{
					content.EffectInfo.Effect,
					content.EffectInfo.EffectId,
					(!(content.SourceGadget == null)) ? content.SourceGadget.name : null,
					(!(content.SourceGadget == null)) ? content.SourceGadget.Info.name : null
				});
				return;
			}
			BaseFX component = transform.GetComponent<BaseFX>();
			Component component2 = GameHubBehaviour.Hub.Resources.PrefabCacheInstantiate(transform, content.Origin, content.StartingRotation);
			BaseFX component3 = component2.GetComponent<BaseFX>();
			if (component3 == null)
			{
				EffectsManager.Log.ErrorFormat("data={0} Position={1} SourceGadget={2}", new object[]
				{
					data,
					content.Origin,
					(!(content.SourceGadget == null)) ? content.SourceGadget.Info.name : null
				});
				EffectsManager.Log.ErrorFormat("PrefabName={0} activeInHierarchy={1} childCount={2}, prefabHash={3}", new object[]
				{
					transform,
					transform.gameObject.activeInHierarchy,
					transform.childCount,
					transform.GetHashCode()
				});
				EffectsManager.Log.ErrorFormat("PrefabCacheInstantiated={0} type={1} hashCode={2}", new object[]
				{
					component2.name,
					component2.GetType(),
					component2.GetHashCode()
				});
				EffectsManager.Log.ErrorFormat("Children={0}", new object[]
				{
					component2.transform.childCount
				});
				return;
			}
			component3.name = string.Format("[{0}]{1}", data.EventId, content.EffectInfo.Effect);
			component3.prefabRef = component;
			component3.transform.parent = GameHubBehaviour.Hub.Drawer.Effects;
			component3.Event = data;
			component3.Data = content;
			component3.EventId = data.EventId;
			component3.Gadget = content.SourceGadget;
			component3.Source = content.SourceId;
			component3.Init();
			if (GameHubBehaviour.Hub.Net.IsClient() || GameHubBehaviour.Hub.Net.IsTest())
			{
				component3.TriggerVFX(content);
			}
			BasePerk[] componentsInChildren = component3.GetComponentsInChildren<BasePerk>(true);
			BasePerk basePerk = componentsInChildren.FirstOrDefault((BasePerk p) => p is PerkAttachToObject);
			if (basePerk)
			{
				if (basePerk.CanInitialize())
				{
					basePerk.PerkInitialized();
				}
				else
				{
					EffectsManager.Log.FatalFormat("Effect:{0} CAN'T INITIALIZE!!!", new object[]
					{
						component3.name
					});
				}
			}
			foreach (BasePerk basePerk2 in componentsInChildren)
			{
				if (!(basePerk2 == basePerk) && basePerk2.CanInitialize())
				{
					basePerk2.PerkInitialized();
					if (basePerk2 is IPerkMovement && basePerk != null)
					{
						component3.AttachedTransform.position = ((IPerkMovement)basePerk2).UpdatePosition();
					}
				}
			}
			if (!content.EffectInfo.Instantaneous)
			{
				this._effects[data.EventId] = component3;
			}
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				component3.RedCreated = (component3.BluCreated = true);
				this.SendCreate(component3.Event);
				((EffectEvent)component3.Event.Content).FirstPackageSent = true;
				if (!content.EffectInfo.Instantaneous)
				{
					GameHubBehaviour.Hub.Events.BufferEvent(data);
				}
			}
			if (GameHubBehaviour.Hub.Net.IsClient() || GameHubBehaviour.Hub.Net.IsTest())
			{
				component3.Gadget.ClientOnEffectStarted(component3);
			}
		}

		private void DestroyEffect(EventData data, EffectRemoveEvent content)
		{
			int num = data.PreviousId = content.TargetEventId;
			if (this._thisFrameDestroys.Contains(num))
			{
				return;
			}
			this._thisFrameDestroys.Add(num);
			BaseFX baseFX;
			if (!this._effects.TryGetValue(num, out baseFX))
			{
				EffectsManager.Log.WarnFormat("Trying to remove effect={0} that was never created?!", new object[]
				{
					num
				});
				return;
			}
			if (content.Origin == Vector3.zero)
			{
				content.Origin = baseFX.transform.position;
			}
			this._effects.Remove(num);
			this._hidden.Remove(num);
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				this.SendRemove(data, baseFX);
				GameHubBehaviour.Hub.Events.ForgetEvent(num);
			}
			DestroyEffect destroyEffect = baseFX.DestroyEffect(content);
			foreach (BasePerk basePerk in baseFX.GetComponentsInChildren<BasePerk>(true))
			{
				basePerk.PerkDestroyed(destroyEffect);
			}
			EffectsManager.EffectDestroyListenerHolder effectDestroyListenerHolder;
			if (this._listeners.TryGetValue(num, out effectDestroyListenerHolder))
			{
				this._listeners.Remove(num);
				effectDestroyListenerHolder.Call(content);
			}
		}

		public void OnCleanup(CleanupMessage msg)
		{
			this._effects.Clear();
			this._listeners.Clear();
			this.m_oPlayerPositionTransform = null;
			this.m_playerPositionTransformSet = false;
		}

		public void ListenToDestroy(int fxId, EffectsManager.EffectDestroyed callback)
		{
			EffectsManager.EffectDestroyListenerHolder effectDestroyListenerHolder;
			if (this._listeners.TryGetValue(fxId, out effectDestroyListenerHolder))
			{
				effectDestroyListenerHolder.EffectDestroyedListener += callback;
				return;
			}
			effectDestroyListenerHolder = new EffectsManager.EffectDestroyListenerHolder();
			effectDestroyListenerHolder.EffectDestroyedListener += callback;
			this._listeners.Add(fxId, effectDestroyListenerHolder);
		}

		public void UnlistenToDestroy(int fxId, EffectsManager.EffectDestroyed callback)
		{
			EffectsManager.EffectDestroyListenerHolder effectDestroyListenerHolder;
			if (this._listeners.TryGetValue(fxId, out effectDestroyListenerHolder))
			{
				effectDestroyListenerHolder.EffectDestroyedListener -= callback;
			}
		}

		private void SendCreate(EventData create)
		{
			GameHubBehaviour.Hub.Events.Send(create);
		}

		private void SendRemove(EventData data, BaseFX effect)
		{
			GameHubBehaviour.Hub.Events.Send(data);
		}

		private static readonly BitLogger Log = new BitLogger(typeof(EffectsManager));

		private readonly HashSet<int> _thisFrameDestroys = new HashSet<int>();

		private readonly Dictionary<int, BaseFX> _effects = new Dictionary<int, BaseFX>(200);

		private readonly Dictionary<int, BaseFX> _hidden = new Dictionary<int, BaseFX>(200);

		public const int InvalidEventId = -1;

		private Transform m_oPlayerPositionTransform;

		private bool m_playerPositionTransformSet;

		private readonly Dictionary<int, EffectsManager.EffectDestroyListenerHolder> _listeners = new Dictionary<int, EffectsManager.EffectDestroyListenerHolder>();

		public delegate void EffectDestroyed(EffectRemoveEvent data);

		private class EffectDestroyListenerHolder
		{
			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			public event EffectsManager.EffectDestroyed EffectDestroyedListener;

			public void Call(EffectRemoveEvent data)
			{
				if (this.EffectDestroyedListener != null)
				{
					this.EffectDestroyedListener(data);
				}
				this.EffectDestroyedListener = null;
			}
		}
	}
}
