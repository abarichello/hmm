using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using ClientAPI;
using ClientAPI.Objects;
using Pocketverse;

namespace HeavyMetalMachines.Swordfish
{
	public class SwordfishLog : GameHubObject
	{
		public SwordfishLog()
		{
			string sessionGUID = NativePlugins.GetSessionGUID();
			Match match = Regex.Match(sessionGUID, "[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}", RegexOptions.IgnoreCase);
			this._gameSessionId = ((!match.Success) ? Guid.NewGuid() : new Guid(sessionGUID));
			this._installationId = this.ReadOrGenerateInstallationGuid();
			this._sessionMsg = string.Format("GameSessionId={0} InstallationId={1}", this._gameSessionId, this._installationId);
			if (GameHubObject.Hub.Config.GetBoolValue(ConfigAccess.SFDisableBI, false) || GameHubObject.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish, false))
			{
				this._disabled = true;
				return;
			}
			GameHubObject.Hub.ClientApi.UserAccessControlCallback += this.OnUserAccessControlCallback;
			if (GameHubObject.Hub.Net.IsClient())
			{
				this.BILogClient(ClientBITags.SessionStart, true);
			}
		}

		public Guid GameSessionId
		{
			get
			{
				return this._gameSessionId;
			}
		}

		private void Awake()
		{
			this._updater = new TimedUpdater(100, true, true);
		}

		private void ErrorCallback(object state, Exception exception)
		{
			SwordfishLog.Log.Error("Callback error", exception);
		}

		private Guid ReadOrGenerateInstallationGuid()
		{
			if (!GameHubObject.Hub.Net.IsClient())
			{
				return Guid.Empty;
			}
			string text = "[id]";
			string text2 = "INSTALLATION_ID";
			string path = ".\\bi";
			if (File.Exists(path))
			{
				string[] array = null;
				try
				{
					array = File.ReadAllLines(path);
				}
				catch (Exception ex)
				{
					SwordfishLog.Log.ErrorFormat("Failed to read BI data file: {0}", new object[]
					{
						ex.ToString()
					});
				}
				if (array != null && array.Length > 1 && array[0] == text && array[1].StartsWith(text2))
				{
					string[] array2 = array[1].Split(new char[]
					{
						'='
					});
					if (array2.Length > 1)
					{
						string text3 = array2[1].Trim();
						Match match = Regex.Match(text3, "[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}", RegexOptions.IgnoreCase);
						if (match.Success)
						{
							return new Guid(text3);
						}
					}
				}
				try
				{
					File.Delete(path);
				}
				catch (Exception ex2)
				{
					SwordfishLog.Log.ErrorFormat("Failed to delete BI data file: {0}", new object[]
					{
						ex2.ToString()
					});
				}
			}
			Guid guid = Guid.NewGuid();
			try
			{
				StreamWriter streamWriter = new StreamWriter(File.OpenWrite(path));
				streamWriter.WriteLine(text);
				streamWriter.WriteLine("{0}={1}", text2, guid);
				streamWriter.Flush();
				streamWriter.Close();
			}
			catch (Exception ex3)
			{
				SwordfishLog.Log.ErrorFormat("Failed to write BI data file: {0}", new object[]
				{
					ex3.ToString()
				});
			}
			return guid;
		}

		private string BuildMessage(string message)
		{
			return (!string.IsNullOrEmpty(message)) ? string.Format("{0} {1}", this._sessionMsg, message) : this._sessionMsg;
		}

		public void BILogServerMsg(ServerBITags biTag, string msg, bool forceSendLogs)
		{
			this.LogStatistic(biTag.ToString(), this.BuildMessage(msg), false, forceSendLogs);
		}

		public void BILogClientMsg(ClientBITags biTag, string msg, bool forceSendLogs)
		{
			this.LogStatistic(biTag.ToString(), this.BuildMessage(msg), false, forceSendLogs);
		}

		public void BILogClientCloseCondition(string msg)
		{
			SwordfishLog.Log.InfoFormat("Type={0} Perm={1} Msg=\"{2}\" ShouldFlush:{3}", new object[]
			{
				ClientBITags.ClientClosed,
				false,
				this.BuildMessage(msg),
				true
			});
			if (this._disabled || this._swordfishUserId == Guid.Empty || string.IsNullOrEmpty(this._swordfishSessionId))
			{
				return;
			}
			using (StreamWriter streamWriter = File.CreateText("bicc"))
			{
				streamWriter.WriteLine(this._swordfishUserId);
				streamWriter.WriteLine(this._swordfishSessionId);
				streamWriter.WriteLine(msg);
			}
		}

		public void OnListenToSwordfishConnected()
		{
			GameHubObject.Hub.UserService.GetMyUserId(null, delegate(object state, Guid guid)
			{
				this._swordfishUserId = guid;
			}, delegate(object state, Exception exception)
			{
				SwordfishLog.Log.Error("Could not get swordfish user id", exception);
			});
			GameHubObject.Hub.UserService.GetMySessionId(null, delegate(object state, string session)
			{
				this._swordfishSessionId = session;
			}, delegate(object state, Exception exception)
			{
				SwordfishLog.Log.Error("Could not get swordfish session id", exception);
			});
		}

		public void BILogClient(ClientBITags biTag, bool forceSendLogs)
		{
			this.BILogClientMsg(biTag, null, forceSendLogs);
		}

		public void BILogClientMatchMsg(ClientBITags biTag, string msg, bool forceSendLogs)
		{
			string text = string.Format("MatchId={0}", GameHubObject.Hub.Swordfish.Msg.ClientMatchId);
			this.BILogClientMsg(biTag, (!string.IsNullOrEmpty(msg)) ? (text + " " + msg) : text, forceSendLogs);
		}

		public void BILogClientMatch(ClientBITags biTag, bool forceSendLogs)
		{
			this.BILogClientMatchMsg(biTag, null, forceSendLogs);
		}

		public void LogStatistic(string type, string msg, bool perm, bool forceSendLogs)
		{
			SwordfishLog.Log.InfoFormat("Type={0} Perm={1} Msg=\"{2}\" ShouldFlush:{3}", new object[]
			{
				type,
				perm,
				msg,
				forceSendLogs
			});
			if (this._disabled)
			{
				return;
			}
			ClientLog item = new ClientLog
			{
				Type = type,
				Message = msg
			};
			this._logs.Add(item);
			if (forceSendLogs)
			{
				this.SendLogs();
			}
		}

		private void SendLogs()
		{
			if (this._disabled || this._logs.Count == 0 || !GameHubObject.Hub.Swordfish.Connection.Connected)
			{
				return;
			}
			if (this._failedLogs.Count > 0)
			{
				this._logs.AddRange(this._failedLogs);
				this._failedLogs.Clear();
			}
			ClientLog[] array = this._logs.ToArray();
			GameHubObject.Hub.ClientApi.log.LogAll(array, array, new SwordfishClientApi.Callback(this.LogSuccess), new SwordfishClientApi.ErrorCallback(this.LogError));
			this._logs.Clear();
		}

		private void LogError(object failedLogs, Exception exception)
		{
			SwordfishLog.Log.Warn("Failed to send logs.", exception);
			this._failedLogs.AddRange((ClientLog[])failedLogs);
		}

		private void LogSuccess(object state)
		{
		}

		public void Update()
		{
			if (this._disabled)
			{
				return;
			}
			if (this._updater.ShouldHalt())
			{
				return;
			}
			this.SendLogs();
		}

		private void OnUserAccessControlCallback(UserAccessControlMessage uacmessage)
		{
			this._disabled = true;
		}

		public void BILogFunnel(FunnelBITags biTag, string publisherUserId = null)
		{
			if (this._disabled)
			{
				return;
			}
			GameHubObject.Hub.ClientApi.log.LogInstallation(biTag, biTag.ToString(), this._sessionMsg, DateTime.Now, new SwordfishClientApi.Callback(this.BILogFunnelSuccess), new SwordfishClientApi.ErrorCallback(this.BILogFunnelError));
		}

		private void BILogFunnelSuccess(object state)
		{
		}

		private void BILogFunnelError(object state, Exception exception)
		{
			SwordfishLog.Log.ErrorFormat("Failed to send FunnelTag:{0}. Exception:{1}", new object[]
			{
				state,
				exception
			});
		}

		private static readonly BitLogger Log = new BitLogger(typeof(SwordfishLog));

		private List<ClientLog> _logs = new List<ClientLog>();

		private List<ClientLog> _failedLogs = new List<ClientLog>();

		private bool _disabled;

		private Guid _gameSessionId;

		private Guid _installationId;

		private string _sessionMsg;

		private const string GUID_REGEX = "[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}";

		private string _swordfishSessionId;

		private Guid _swordfishUserId = Guid.Empty;

		private TimedUpdater _updater;
	}
}
