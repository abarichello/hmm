using System;
using System.Collections.Generic;
using Pocketverse;

namespace HeavyMetalMachines.UpdateStream
{
	public class UpdateDataStream<T> : GameHubObject where T : GameHubBehaviour, IStreamContent
	{
		public UpdateDataStream()
		{
			int bufferSize = GameHubObject.Hub.Config.GetIntValue(ConfigAccess.SfNetMaximumTransmissionUnit) * 15;
			UpdateDataStreamBuffer.AddRef(bufferSize);
		}

		public bool HasChanges
		{
			get
			{
				return this.ChangedObjects.Count > 0;
			}
		}

		~UpdateDataStream()
		{
			UpdateDataStreamBuffer.Release();
		}

		public void AddObject(T obj)
		{
			if (obj == null)
			{
				return;
			}
			this.Objects[obj.Id.ObjId] = obj;
			this.AllObjects.Add(obj);
			if (this.ObjectsUpdatedButNotFound.ContainsKey(obj.Id.ObjId))
			{
				Objects objects = this.ObjectsUpdatedButNotFound[obj.Id.ObjId];
				T t = this.Objects[obj.Id.ObjId];
				t.ApplyStreamData(objects.Contents);
				T t2 = this.Objects[obj.Id.ObjId];
				t2.Version = objects.Version;
				this.ObjectsUpdatedButNotFound.Remove(obj.Id.ObjId);
			}
		}

		public void AddObject(Identifiable obj)
		{
			this.AddObject(obj.GetBitComponent<T>());
		}

		public void Remove(int id)
		{
			this.Objects.Remove(id);
			this.AllObjects.RemoveAll((T x) => x.Id.ObjId == id);
		}

		public void Cleanup()
		{
			this.Objects.Clear();
			this.ChangedObjects.Clear();
			this.AllObjects.Clear();
			this.ObjectsUpdatedButNotFound.Clear();
		}

		public void Changed(T data)
		{
			if (data == null || this.ChangedObjects.Contains(data) || !this.AllObjects.Contains(data))
			{
				return;
			}
			this.ChangedObjects.Add(data);
		}

		public void Changed(int id)
		{
			T data;
			if (this.Objects.TryGetValue(id, out data))
			{
				this.Changed(data);
			}
		}

		public void FillSendStreamFull(BitStream stream)
		{
			for (int i = 0; i < this.AllObjects.Count; i++)
			{
				T t = this.AllObjects[i];
				if (!(t == null) && !t.Id.IsCached)
				{
					stream.WriteBool(true);
					stream.WriteCompressedInt(t.Id.ObjId);
					stream.WriteShort(t.Version);
					int streamData = t.GetStreamData(ref UpdateDataStreamBuffer.Buffer, true);
					stream.WriteByteArray(UpdateDataStreamBuffer.Buffer, streamData);
				}
			}
			stream.WriteBool(false);
			this.ChangedObjects.Clear();
		}

		public bool FillSendStream(BitStream stream)
		{
			bool result = false;
			for (int i = 0; i < this.ChangedObjects.Count; i++)
			{
				T t = this.ChangedObjects[i];
				if (!(t == null) && !t.Id.IsCached)
				{
					result = true;
					stream.WriteBool(true);
					stream.WriteCompressedInt(t.Id.ObjId);
					stream.WriteShort(t.Version += 1);
					int streamData = t.GetStreamData(ref UpdateDataStreamBuffer.Buffer, false);
					stream.WriteByteArray(UpdateDataStreamBuffer.Buffer, streamData);
				}
			}
			stream.WriteBool(false);
			this.ChangedObjects.Clear();
			return result;
		}

		public void ReadStream(BitStream stream)
		{
			while (stream.ReadBool())
			{
				int num = stream.ReadCompressedInt();
				short num2 = stream.ReadShort();
				byte[] array = stream.ReadByteArray();
				T t;
				if (this.Objects.TryGetValue(num, out t))
				{
					if (array != null)
					{
						t.ApplyStreamData(array);
					}
					t.Version = num2;
					if (t is StreamContent)
					{
						(t as StreamContent).OnChanged.Invoke(t);
					}
				}
				else if (this.ObjectsUpdatedButNotFound.ContainsKey(num))
				{
					if (this.ObjectsUpdatedButNotFound[num].Version <= num2)
					{
						this.ObjectsUpdatedButNotFound[num] = new Objects
						{
							ObjId = num,
							Version = num2,
							Contents = array
						};
					}
				}
				else
				{
					this.ObjectsUpdatedButNotFound.Add(num, new Objects
					{
						ObjId = num,
						Version = num2,
						Contents = array
					});
				}
			}
		}

		protected List<T> AllObjects = new List<T>(128);

		protected Dictionary<int, T> Objects = new Dictionary<int, T>(128);

		protected List<T> ChangedObjects = new List<T>(128);

		protected Dictionary<int, Objects> ObjectsUpdatedButNotFound = new Dictionary<int, Objects>();
	}
}
