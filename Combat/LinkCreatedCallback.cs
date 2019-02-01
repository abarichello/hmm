using System;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	public struct LinkCreatedCallback : Mural.IMuralMessage
	{
		public LinkCreatedCallback(CombatObject point1, CombatObject point2, CombatLink link)
		{
			this.Point1 = point1;
			this.Point2 = point2;
			this.Link = link;
		}

		public string Message
		{
			get
			{
				return "OnLinkCreatedCallback";
			}
		}

		public CombatObject Point1;

		public CombatObject Point2;

		public CombatLink Link;

		public const string Msg = "OnLinkCreatedCallback";

		public interface ILinkCreatedCallbackListener
		{
			void OnLinkCreatedCallback(LinkCreatedCallback evt);
		}
	}
}
