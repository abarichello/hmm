using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines
{
	public struct CollisionEvent
	{
		internal void ReadFromStream(BitStream stream)
		{
			this.ObjId = stream.ReadCompressedInt();
			this.Point = stream.ReadVector3();
			stream.ReadCompressedNormVec3Lossy(out this.Normal.x, out this.Normal.y, out this.Normal.z);
			this.Intensity = stream.ReadCompressedFloat();
			this.OtherLayer = stream.ReadByte();
		}

		internal void WriteToStream(BitStream stream)
		{
			stream.WriteCompressedInt(this.ObjId);
			stream.WriteVector3(this.Point);
			stream.WriteCompressedNormVec3Lossy(this.Normal.x, this.Normal.y, this.Normal.z);
			stream.WriteCompressedFloat(this.Intensity);
			stream.WriteByte(this.OtherLayer);
		}

		public int ObjId;

		public Vector3 Point;

		public Vector3 Normal;

		public float Intensity;

		public byte OtherLayer;
	}
}
