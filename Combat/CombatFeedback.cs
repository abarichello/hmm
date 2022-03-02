using System;
using System.Collections.Generic;
using System.Diagnostics;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.UpdateStream;
using HeavyMetalMachines.VFX;
using Hoplon.Unity.Loading;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class CombatFeedback : StreamContent, IObjectSpawnListener
	{
		public static int ModifierFeedbackId
		{
			get
			{
				return CombatFeedback._modifierFeedbackId++;
			}
		}

		public CombatObject Combat
		{
			get
			{
				CombatObject result;
				if ((result = this._combat) == null)
				{
					result = (this._combat = base.GetComponent<CombatObject>());
				}
				return result;
			}
		}

		private void Update()
		{
			if (!GameHubBehaviour.Hub)
			{
				return;
			}
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				this.ClientUpdate();
				return;
			}
			this.ServerUpdate();
		}

		private void OnDestroy()
		{
			this.OnCollisionEvent = null;
		}

		public ModifierFeedbackInstance GetFeedback(int feedbackId)
		{
			for (int i = 0; i < this._feedbacks.Count; i++)
			{
				ModifierFeedbackInstance modifierFeedbackInstance = this._feedbacks[i];
				if (modifierFeedbackInstance.InstanceId == feedbackId)
				{
					return modifierFeedbackInstance;
				}
			}
			return null;
		}

		public override int GetStreamData(ref byte[] data, bool boForceSerialization)
		{
			BitStream stream = base.GetStream();
			stream.WriteCompressedInt(this._feedbacks.Count);
			for (int i = 0; i < this._feedbacks.Count; i++)
			{
				ModifierFeedbackInstance modifierFeedbackInstance = this._feedbacks[i];
				modifierFeedbackInstance.WriteToBitStream(stream);
			}
			return stream.CopyToArray(data);
		}

		public override void ApplyStreamData(byte[] data)
		{
			BitStream streamFor = base.GetStreamFor(data);
			int num = streamFor.ReadCompressedInt();
			this.NewFeedbacks.Clear();
			for (int i = 0; i < num; i++)
			{
				ModifierFeedbackInstance modifierFeedbackInstance = new ModifierFeedbackInstance();
				modifierFeedbackInstance.ReadFromBitStream(streamFor);
				this.NewFeedbacks.Add(modifierFeedbackInstance);
			}
			this.NewFeedbacksPopulated = true;
		}

		public void MarkChanged()
		{
			GameHubBehaviour.Hub.Stream.CombatFeedStream.Changed(this);
		}

		private void ServerUpdate()
		{
			int playbackTime = GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			if (this._updater.ShouldHalt())
			{
				return;
			}
			for (int i = 0; i < this._feedbacks.Count; i++)
			{
				ModifierFeedbackInstance modifierFeedbackInstance = this._feedbacks[i];
				if (modifierFeedbackInstance.EndTime >= 0 && modifierFeedbackInstance.EndTime <= playbackTime)
				{
					this._feedbacks.RemoveAt(i);
					GameHubBehaviour.Hub.Stream.CombatFeedStream.Changed(this);
					i--;
				}
			}
		}

		private void ClientUpdate()
		{
			for (int i = 0; i < this._feedbacks.Count; i++)
			{
				if (this._feedbacks[i].InstanceId == -10 && GameHubBehaviour.Hub.GameTime.GetPlaybackTime() >= this._feedbacks[i].EndTime)
				{
					if (this.ClientRemoveFeedback(this._feedbacks[i]))
					{
						i--;
					}
				}
			}
			if (!this.NewFeedbacksPopulated)
			{
				return;
			}
			for (int j = 0; j < this.NewFeedbacks.Count; j++)
			{
				ModifierFeedbackInstance modifierFeedbackInstance = this.NewFeedbacks[j];
				ModifierFeedbackInstance modifierFeedbackInstance2 = this.FindModifierFeedbackEventById(this._feedbacks, modifierFeedbackInstance.ModifierFeedbackId, modifierFeedbackInstance.Causer);
				if (modifierFeedbackInstance2 != null)
				{
					modifierFeedbackInstance2.Copy(modifierFeedbackInstance);
				}
				else
				{
					this.ClientCreateFeedback(modifierFeedbackInstance);
				}
			}
			for (int k = 0; k < this._feedbacks.Count; k++)
			{
				ModifierFeedbackInstance modifierFeedbackInstance3 = this._feedbacks[k];
				if (modifierFeedbackInstance3.InstanceId != -10)
				{
					ModifierFeedbackInstance modifierFeedbackInstance4 = this.FindModifierFeedbackEventById(this.NewFeedbacks, modifierFeedbackInstance3.ModifierFeedbackId, modifierFeedbackInstance3.Causer);
					if (modifierFeedbackInstance4 == null)
					{
						if (this.ClientRemoveFeedback(modifierFeedbackInstance3))
						{
							k--;
						}
					}
				}
			}
			this.NewFeedbacks.Clear();
			this.NewFeedbacksPopulated = false;
		}

		private ModifierFeedbackInstance FindModifierFeedbackEventById(List<ModifierFeedbackInstance> List, int id, int causerId)
		{
			for (int i = 0; i < List.Count; i++)
			{
				if (id == List[i].ModifierFeedbackId && causerId == List[i].Causer)
				{
					return List[i];
				}
			}
			return null;
		}

		private void ClientCreateFeedback(ModifierFeedbackInstance mod)
		{
			ModifierFeedbackInfo feedbackInfo = mod.FeedbackInfo;
			if (!feedbackInfo)
			{
				return;
			}
			Content asset = Loading.Content.GetAsset(feedbackInfo.Name);
			if (asset == null)
			{
				CombatFeedback.Log.ErrorFormat("Invalid asset for content {0}: Asset {1}", new object[]
				{
					feedbackInfo.name,
					feedbackInfo.Name
				});
				return;
			}
			Transform transform = (Transform)asset.Asset;
			if (transform == null)
			{
				CombatFeedback.Log.ErrorFormat("Invalid asset for content {0}: Asset {1}", new object[]
				{
					feedbackInfo.name,
					feedbackInfo.Name
				});
				return;
			}
			Transform dummy = this.Combat.GetDummy(feedbackInfo.Dummy, null);
			Transform transform2 = GameHubBehaviour.Hub.Resources.PrefabCacheInstantiate(transform, dummy.position, dummy.rotation) as Transform;
			mod.FeedbackEffect = transform2.GetComponent<ModifierFeedback>();
			mod.FeedbackEffect.prefabRef = transform;
			mod.FeedbackEffect.transform.parent = dummy;
			mod.FeedbackEffect.Instance = mod;
			mod.FeedbackEffect.InitializeFeedback();
			this._feedbacks.Add(mod);
		}

		private bool ClientRemoveFeedback(ModifierFeedbackInstance mod)
		{
			if (!mod.FeedbackEffect)
			{
				return false;
			}
			mod.FeedbackEffect.DestroyFeedbackEffect(default(DestroyFeedbackEffectMessage));
			this._feedbacks.Remove(mod);
			return true;
		}

		public void ClientAdd(ModifierFeedbackInfo info, int eventId, int causer, int starttime, int endtime, int buffcharges, GadgetSlot slot)
		{
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				return;
			}
			ModifierFeedbackInstance mod = new ModifierFeedbackInstance
			{
				Causer = causer,
				Target = this.Combat.Id.ObjId,
				EventId = eventId,
				GadgetSlot = slot,
				InstanceId = -10,
				ModifierFeedbackId = info.ModifierFeedbackId,
				StartTime = starttime,
				EndTime = endtime,
				Charges = buffcharges
			};
			this.ClientCreateFeedback(mod);
		}

		public int Add(ModifierFeedbackInfo info, int eventId, int causer, int starttime, int endtime, int buffcharges, GadgetSlot slot)
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return -1;
			}
			for (int i = 0; i < this._feedbacks.Count; i++)
			{
				ModifierFeedbackInstance modifierFeedbackInstance = this._feedbacks[i];
				bool flag = modifierFeedbackInstance.ModifierFeedbackId == info.ModifierFeedbackId;
				bool flag2 = modifierFeedbackInstance.Causer == causer;
				if (flag && flag2)
				{
					modifierFeedbackInstance.EndTime = endtime;
					modifierFeedbackInstance.Charges = buffcharges;
					GameHubBehaviour.Hub.Stream.CombatFeedStream.Changed(this);
					return modifierFeedbackInstance.InstanceId;
				}
			}
			int num = CombatFeedback._currentFeedbackId++;
			ModifierFeedbackInstance item = new ModifierFeedbackInstance
			{
				Causer = causer,
				Target = this.Combat.Id.ObjId,
				EventId = eventId,
				GadgetSlot = slot,
				InstanceId = num,
				ModifierFeedbackId = info.ModifierFeedbackId,
				StartTime = starttime,
				EndTime = endtime,
				Charges = buffcharges
			};
			this._feedbacks.Add(item);
			GameHubBehaviour.Hub.Stream.CombatFeedStream.Changed(this);
			return num;
		}

		public void Remove(int feedbackId)
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			ModifierFeedbackInstance item = this._feedbacks.Find((ModifierFeedbackInstance x) => x.InstanceId == feedbackId);
			this._feedbacks.Remove(item);
			GameHubBehaviour.Hub.Stream.CombatFeedStream.Changed(this);
		}

		public void OnObjectUnspawned(UnspawnEvent evt)
		{
			this.ClearFeedbacks();
		}

		public void ClearFeedbacks()
		{
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				this._feedbacks.Clear();
				GameHubBehaviour.Hub.Stream.CombatFeedStream.Changed(this);
				return;
			}
			for (int i = 0; i < this._feedbacks.Count; i++)
			{
				if (this.ClientRemoveFeedback(this._feedbacks[i]))
				{
					i--;
				}
			}
			this._feedbacks.Clear();
			this.NewFeedbacks.Clear();
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<Vector3, Vector3, float, byte> OnCollisionEvent;

		public void OnCollisionEventKeyFrame(Vector3 position, Vector3 direction, float intensity, byte otherLayer)
		{
			if (this.OnCollisionEvent != null)
			{
				this.OnCollisionEvent(position, direction, intensity, otherLayer);
			}
		}

		public void OnObjectSpawned(SpawnEvent evt)
		{
		}

		public static readonly BitLogger Log = new BitLogger(typeof(CombatFeedback));

		private static int _modifierFeedbackId;

		private static int _currentFeedbackId = 0;

		public CombatObject _combat;

		private List<ModifierFeedbackInstance> _feedbacks = new List<ModifierFeedbackInstance>();

		public List<ModifierFeedbackInstance> NewFeedbacks = new List<ModifierFeedbackInstance>();

		public bool NewFeedbacksPopulated;

		private TimedUpdater _updater = new TimedUpdater
		{
			PeriodMillis = 100
		};
	}
}
