using System;
using System.Collections.Generic;
using System.IO;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Announcer
{
	[Serializable]
	public class AnnouncerInfo : GameHubScriptableObject
	{
		public AnnouncerLog GetAnnouncerLog(AnnouncerLog.AnnouncerEventKinds announcereventKind)
		{
			AnnouncerLog result;
			try
			{
				AnnouncerLog announcerLog = this.DicAnnouncerLogs[announcereventKind];
				result = announcerLog;
			}
			catch (Exception)
			{
				AnnouncerInfo._logger.Warn("AnnouncerInfo - GetAnnouncerLog ERROR: " + announcereventKind + " not found in dictionary.");
				result = null;
			}
			return result;
		}

		public bool TryGetAnnouncerLog(AnnouncerLog.AnnouncerEventKinds announcereventKind, out AnnouncerLog log)
		{
			if (this.DicAnnouncerLogs.TryGetValue(announcereventKind, out log))
			{
				return true;
			}
			AnnouncerInfo._logger.Warn("AnnouncerInfo - TryGetAnnouncerLog WARN: " + announcereventKind + " not found in dictionary.");
			return false;
		}

		private void OnEnable()
		{
			this.ReloadDic();
		}

		public void ReloadDic()
		{
			this.DicAnnouncerLogs = new Dictionary<AnnouncerLog.AnnouncerEventKinds, AnnouncerLog>();
			for (int i = 0; i < this.AnnouncerLogs.Count; i++)
			{
				try
				{
					this.DicAnnouncerLogs.Add(this.AnnouncerLogs[i].AnnouncerEventKind, this.AnnouncerLogs[i]);
				}
				catch (Exception)
				{
					Debug.Log(string.Concat(new object[]
					{
						"AnnouncerInfo - ERROR reloading DIC - Duplicated key: ",
						this.AnnouncerLogs[i].AnnouncerEventKind,
						" - Name: ",
						this.AnnouncerLogs[i].Name
					}));
				}
			}
		}

		public void GenerateLogListOnFile()
		{
			StreamWriter streamWriter = new StreamWriter("log_announces.txt", false);
			streamWriter.WriteLine(" Nome - Tipo ");
			for (int i = 0; i < this.AnnouncerLogs.Count; i++)
			{
				if (this.AnnouncerLogs[i].LogAnnouncerEventKind != AnnouncerLog.AnnouncerEventKinds.None)
				{
					streamWriter.WriteLine(string.Concat(new object[]
					{
						" Nome: ",
						this.AnnouncerLogs[i].Name,
						" Tipo: ",
						this.AnnouncerLogs[i].LogAnnouncerEventKind
					}));
				}
			}
			Debug.Log("Log list on file...GENERATED.");
			streamWriter.Close();
		}

		public void FixIndexers()
		{
			int num = 0;
			for (int i = 0; i < this.AnnouncerLogs.Count; i++)
			{
				AnnouncerLog announcerLog = this.AnnouncerLogs[i];
				announcerLog.Index = num++;
			}
		}

		public List<AnnouncerLog> AnnouncerLogs;

		private static BitLogger _logger = new BitLogger("AnnouncerInfo");

		private Dictionary<AnnouncerLog.AnnouncerEventKinds, AnnouncerLog> DicAnnouncerLogs = new Dictionary<AnnouncerLog.AnnouncerEventKinds, AnnouncerLog>();
	}
}
