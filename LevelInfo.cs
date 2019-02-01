using System;
using UnityEngine;

namespace HeavyMetalMachines
{
	public class LevelInfo : ScriptableObject
	{
		[ScriptId]
		public int Id;

		public string FileName;

		public int PlayerCount;
	}
}
