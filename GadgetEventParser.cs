using System;
using System.Collections.Generic;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Combat.GadgetScript;
using HeavyMetalMachines.Utils;
using Pocketverse;

namespace HeavyMetalMachines
{
	public class GadgetEventParser : KeyFrameParser
	{
		public GadgetEventParser()
		{
			this._eventBuffer.OnMemberRemoved += this.OnEventRemoved;
		}

		public override KeyFrameType Type
		{
			get
			{
				return KeyFrameType.GadgetEvent;
			}
		}

		private void OnEventRemoved(IHMMEventContext ev)
		{
			this._eventsGadgets.Remove(ev);
		}

		public override void Process(BitStream stream)
		{
			int num = stream.ReadInt();
			int num2 = stream.ReadInt();
			int bitReadSize = stream.GetBitReadSize();
			stream.SetReadedSize(bitReadSize);
			Identifiable @object = GameHubObject.Hub.ObjectCollection.GetObject(num);
			if (@object == null)
			{
				GadgetEventParser.Log.ErrorFormat("Owner null id={0} gadget={1}", new object[]
				{
					num,
					num2
				});
				return;
			}
			IGadgetOwner component = @object.GetComponent<IGadgetOwner>();
			IHMMGadgetContext gadgetContext = component.GetGadgetContext(num2);
			GadgetEvent gadgetEvent = new GadgetEvent(0, gadgetContext);
			gadgetEvent.ReadFromBitStream(stream);
			gadgetContext.SetLastBodyId(gadgetEvent);
			gadgetContext.TriggerEvent(gadgetEvent);
		}

		public override bool RewindProcess(IFrame frame)
		{
			BitStream readData = frame.GetReadData();
			int id = readData.ReadInt();
			int id2 = readData.ReadInt();
			Identifiable @object = GameHubObject.Hub.ObjectCollection.GetObject(id);
			IGadgetOwner component = @object.GetComponent<IGadgetOwner>();
			IHMMGadgetContext gadgetContext = component.GetGadgetContext(id2);
			GadgetEvent gadgetEvent = new GadgetEvent(0, gadgetContext);
			gadgetEvent.ReadFromBitStream(readData);
			gadgetEvent.Undo();
			return frame.PreviousFrameId >= 0;
		}

		public int NextId()
		{
			return GameHubObject.Hub.PlaybackManager.NextId();
		}

		public void SendEvent(IHMMGadgetContext gadgetContext, IHMMEventContext eventContext)
		{
			BitStream stream = base.GetStream();
			this.WriteToBitStream(stream, gadgetContext, eventContext);
			if (this._eventBuffer.AddMember(eventContext))
			{
				this._eventsGadgets.Add(eventContext, gadgetContext);
			}
			GameHubObject.Hub.PlaybackManager.SendKeyFrame(this.Type, true, eventContext.Id, eventContext.PreviousEventId, stream.ByteArray);
		}

		public void SendAllEvents(byte address)
		{
			for (int i = 0; i < this._eventBuffer.LiveHolders.Count; i++)
			{
				IHMMEventContext ihmmeventContext = this._eventBuffer.LiveHolders[i];
				IHMMGadgetContext gadgetContext = this._eventsGadgets[ihmmeventContext];
				BitStream stream = base.GetStream();
				this.WriteToBitStream(stream, gadgetContext, ihmmeventContext);
				GameHubObject.Hub.PlaybackManager.SendFullKeyFrame(address, this.Type, ihmmeventContext.Id, ihmmeventContext.PreviousEventId, ihmmeventContext.CreationTime, stream.ByteArray);
				GadgetEventParser.Log.InfoFormat("Sending full frame Gadget BlockIndex = {0}", new object[]
				{
					ihmmeventContext.BlockIndex
				});
			}
			for (int j = 0; j < GameHubObject.Hub.Players.PlayersAndBots.Count; j++)
			{
				CombatObject combat = CombatRef.GetCombat(GameHubObject.Hub.Players.PlayersAndBots[j].CharacterInstance);
				CombatGadget[] array = new CombatGadget[combat.CustomGadgets.Values.Count];
				combat.CustomGadgets.Values.CopyTo(array, 0);
				for (int k = 0; k < array.Length; k++)
				{
					GadgetEvent gadgetEvent = new GadgetEvent(-1, array[k], array[k].GetAllUIParameters());
					BitStream stream = base.GetStream();
					this.WriteToBitStream(stream, array[k], gadgetEvent);
					GameHubObject.Hub.PlaybackManager.SendFullKeyFrame(address, this.Type, gadgetEvent.Id, gadgetEvent.PreviousEventId, gadgetEvent.CreationTime, stream.ByteArray);
				}
			}
		}

		private void WriteToBitStream(BitStream stream, IHMMGadgetContext gadgetContext, IHMMEventContext eventContext)
		{
			stream.WriteInt(gadgetContext.OwnerId);
			stream.WriteInt(gadgetContext.Id);
			eventContext.WriteToBitStream(stream);
		}

		private static readonly BitLogger Log = new BitLogger(typeof(GadgetEventParser));

		private NodeHolderLifeCycle<IHMMEventContext> _eventBuffer = new NodeHolderLifeCycle<IHMMEventContext>();

		private Dictionary<IHMMEventContext, IHMMGadgetContext> _eventsGadgets = new Dictionary<IHMMEventContext, IHMMGadgetContext>();
	}
}
