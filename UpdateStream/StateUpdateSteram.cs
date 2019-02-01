using System;
using System.Collections.Generic;
using Pocketverse;

namespace HeavyMetalMachines.UpdateStream
{
	public class StateUpdateSteram : GameHubObject
	{
		public void AddObject(IStateContent obj)
		{
			if (obj == null)
			{
				return;
			}
			ContentKey key = new ContentKey
			{
				ClassId = obj.ClassId,
				ObjId = obj.ObjId
			};
			this._objects[key] = obj;
			this._allObjects.Add(obj);
			if (this._unknownUpdates.ContainsKey(key))
			{
				UnknownUpdate unknownUpdate = this._unknownUpdates[key];
				this._objects[key].ApplyStreamData(unknownUpdate.Contents);
				this._objects[key].Version = unknownUpdate.Version;
				this._unknownUpdates.Remove(key);
			}
		}

		public void Remove(IStateContent obj)
		{
			ContentKey key = new ContentKey
			{
				ClassId = obj.ClassId,
				ObjId = obj.ObjId
			};
			this._objects.Remove(key);
			this._allObjects.Remove(obj);
			this._changedObjects.Remove(obj);
		}

		public void Remove(int id, byte classId)
		{
			ContentKey key = new ContentKey
			{
				ClassId = classId,
				ObjId = id
			};
			this._objects.Remove(key);
			this._allObjects.RemoveAll((IStateContent x) => x.ObjId == id && x.ClassId == classId);
			this._changedObjects.RemoveAll((IStateContent x) => x.ObjId == id && x.ClassId == classId);
		}

		public void Cleanup()
		{
			this._objects.Clear();
			this._changedObjects.Clear();
			this._allObjects.Clear();
			this._unknownUpdates.Clear();
		}

		public void Changed(IStateContent data)
		{
			if (data == null || this._changedObjects.Contains(data) || !this._allObjects.Contains(data))
			{
				return;
			}
			this._changedObjects.Add(data);
		}

		public void Changed(int id, byte classId)
		{
			ContentKey key = new ContentKey
			{
				ClassId = classId,
				ObjId = id
			};
			IStateContent data;
			if (this._objects.TryGetValue(key, out data))
			{
				this.Changed(data);
			}
		}

		public void FillSendStreamFull(BitStream stream)
		{
			for (int i = 0; i < this._allObjects.Count; i++)
			{
				IStateContent stateContent = this._allObjects[i];
				if (stateContent != null && !stateContent.IsCached())
				{
					stream.WriteBool(true);
					stream.WriteCompressedInt(stateContent.ObjId);
					stream.WriteByte(stateContent.ClassId);
					stream.WriteShort(stateContent.Version);
					byte[] streamData = stateContent.GetStreamData();
					stream.WriteByteArray(streamData);
				}
			}
			stream.WriteBool(false);
			this._changedObjects.Clear();
		}

		public bool FillSendStream(BitStream stream)
		{
			bool result = false;
			for (int i = 0; i < this._changedObjects.Count; i++)
			{
				IStateContent stateContent = this._changedObjects[i];
				if (stateContent != null && !stateContent.IsCached())
				{
					result = true;
					stream.WriteBool(true);
					stream.WriteCompressedInt(stateContent.ObjId);
					stream.WriteByte(stateContent.ClassId);
					IStateContent stateContent2 = stateContent;
					stream.WriteShort(stateContent2.Version += 1);
					byte[] streamData = stateContent.GetStreamData();
					stream.WriteByteArray(streamData);
				}
			}
			stream.WriteBool(false);
			this._changedObjects.Clear();
			return result;
		}

		public void ReadStream(BitStream stream)
		{
			while (stream.ReadBool())
			{
				int objId = stream.ReadCompressedInt();
				byte classId = stream.ReadByte();
				short num = stream.ReadShort();
				byte[] array = stream.ReadByteArray();
				ContentKey key = new ContentKey
				{
					ClassId = classId,
					ObjId = objId
				};
				IStateContent stateContent;
				if (this._objects.TryGetValue(key, out stateContent))
				{
					if (stateContent.Version < num)
					{
						if (array != null)
						{
							stateContent.ApplyStreamData(array);
						}
						stateContent.Version = num;
					}
				}
				else if (this._unknownUpdates.ContainsKey(key))
				{
					if (this._unknownUpdates[key].Version <= num)
					{
						this._unknownUpdates[key] = new UnknownUpdate
						{
							ObjId = objId,
							Version = num,
							Contents = array
						};
					}
				}
				else
				{
					this._unknownUpdates.Add(key, new UnknownUpdate
					{
						ObjId = objId,
						Version = num,
						Contents = array
					});
				}
			}
		}

		private readonly List<IStateContent> _allObjects = new List<IStateContent>(128);

		private readonly List<IStateContent> _changedObjects = new List<IStateContent>(128);

		private readonly Dictionary<ContentKey, IStateContent> _objects = new Dictionary<ContentKey, IStateContent>(128);

		private readonly Dictionary<ContentKey, UnknownUpdate> _unknownUpdates = new Dictionary<ContentKey, UnknownUpdate>();
	}
}
