using System;
using HeavyMetalMachines.Event;

namespace HeavyMetalMachines.Announcer
{
	[Serializable]
	public class QueuedAnnouncerLog
	{
		public QueuedAnnouncerLog(AnnouncerEvent announcerEvent, AnnouncerLog announcerLog)
		{
			this.AnnouncerEvent = announcerEvent;
			this.AnnouncerLog = announcerLog;
		}

		public readonly AnnouncerEvent AnnouncerEvent;

		public readonly AnnouncerLog AnnouncerLog;
	}
}
