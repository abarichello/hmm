﻿using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public struct UnspawnEvent : Mural.IMuralMessage
	{
		public UnspawnEvent(Vector3 pos, SpawnReason reason, int causer)
		{
			this.Position = pos;
			this.Reason = reason;
			this.Causer = causer;
		}

		public string Message
		{
			get
			{
				return "OnObjectUnspawned";
			}
		}

		private const string Msg = "OnObjectUnspawned";

		public SpawnReason Reason;

		public Vector3 Position;

		public int Causer;
	}
}
