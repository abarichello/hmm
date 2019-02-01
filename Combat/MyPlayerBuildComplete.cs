using System;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	public struct MyPlayerBuildComplete : Mural.IMuralMessage
	{
		public MyPlayerBuildComplete(Identifiable obj)
		{
			this.Object = obj;
		}

		public string Message
		{
			get
			{
				return "OnMyPlayerBuildComplete";
			}
		}

		public Identifiable Object;

		public const string Msg = "OnMyPlayerBuildComplete";

		public interface IMyPlayerBuildComplete
		{
			void OnMyPlayerBuildComplete(MyPlayerBuildComplete evt);
		}
	}
}
