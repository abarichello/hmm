using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using ClientAPI;
using ClientAPI.Objects;
using HeavyMetalMachines.Swordfish;
using HeavyMetalMachines.Swordfish.Player;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines
{
	public class HMMPlayerPrefs : GameHubObject
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private event System.Action OnPrefsLoaded;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private event System.Action OnPrefsWrongVersion;

		public void SetString(string key, string value)
		{
			this._values[key] = value;
		}

		public void SetFloat(string key, float value)
		{
			this._values[key] = value.ToString();
		}

		public void SetInt(string key, int value)
		{
			this._values[key] = value.ToString();
		}

		public void DeleteKey(string key)
		{
			this._values.Remove(key);
		}

		private string Get(string key)
		{
			string result;
			if (this._values.TryGetValue(key, out result))
			{
				return result;
			}
			return null;
		}

		public string GetString(string key, string defaultValue = null)
		{
			string text = this.Get(key);
			if (text == null)
			{
				return defaultValue;
			}
			return text;
		}

		public float GetFloat(string key, float defaultValue = 0f)
		{
			string s = this.Get(key);
			float result;
			if (float.TryParse(s, out result))
			{
				return result;
			}
			return defaultValue;
		}

		public int GetInt(string key, int defaultValue = 0)
		{
			string s = this.Get(key);
			int result;
			if (int.TryParse(s, out result))
			{
				return result;
			}
			return defaultValue;
		}

		public int GetIntWithoutDefault(string key)
		{
			string s = this.Get(key);
			int result;
			if (int.TryParse(s, out result))
			{
				return result;
			}
			return -1;
		}

		public float GetFloatWithoutDefault(string key)
		{
			string s = this.Get(key);
			float result;
			if (float.TryParse(s, out result))
			{
				return result;
			}
			return -1f;
		}

		public bool HasKey(string key)
		{
			return this._values.ContainsKey(key);
		}

		public void Save()
		{
			if (!this._loaded)
			{
				return;
			}
			this._waitTime = Time.realtimeSinceStartup + 60f;
			if (!this._saveRunning)
			{
				GameHubObject.Hub.StartCoroutine(this.SaveInputs());
			}
		}

		public void SaveNow()
		{
			if (!this._loaded)
			{
				return;
			}
			this._breakSave = true;
			if (GameHubObject.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish))
			{
				this.SkipSwordfishSave();
				return;
			}
			PlayerPrefsContent playerPrefsContent = new PlayerPrefsContent
			{
				Values = this._values
			};
			playerPrefsContent.version = 2;
			InventoryBag inventoryBag = new InventoryBag
			{
				Kind = InventoryBag.InventoryKind.PlayerPrefs,
				Content = playerPrefsContent.ToString()
			};
			PlayerCustomWS.SavePlayerPrefs(inventoryBag.ToString(), new SwordfishClientApi.ParameterizedCallback<string>(this.OnSave), new SwordfishClientApi.ErrorCallback(this.OnError));
		}

		private void SkipSwordfishSave()
		{
			foreach (KeyValuePair<string, string> keyValuePair in this._values)
			{
				GameHubObject.Hub.Config.SetDebugSetting(keyValuePair.Key, keyValuePair.Value);
			}
			GameHubObject.Hub.Config.SaveDebugIni();
		}

		private void OnError(object state, Exception exception)
		{
			HMMPlayerPrefs.Log.Fatal("Failed to save preferences.", exception);
		}

		private void OnSave(object state, string obj)
		{
			NetResult netResult = (NetResult)((JsonSerializeable<T>)obj);
			if (!netResult.Success)
			{
				HMMPlayerPrefs.Log.ErrorFormat("Failed to save preferences, error={0} msg={1}", new object[]
				{
					netResult.Error,
					netResult.Msg
				});
			}
		}

		private IEnumerator SaveInputs()
		{
			this._saveRunning = true;
			this._breakSave = false;
			while (this._waitTime > Time.realtimeSinceStartup && !this._breakSave)
			{
				yield return UnityUtils.WaitForOneSecond;
			}
			this._saveRunning = false;
			if (this._breakSave)
			{
				yield break;
			}
			this.SaveNow();
			yield break;
		}

		public void ExecOnPrefsLoaded(System.Action action)
		{
			if (action == null)
			{
				return;
			}
			if (this._loaded)
			{
				action();
				return;
			}
			this.OnPrefsLoaded += action;
		}

		public void ExecOnPrefsWrongVersion(System.Action action)
		{
			if (action == null)
			{
				return;
			}
			if (this._loaded)
			{
				action();
				return;
			}
			this.OnPrefsWrongVersion += action;
		}

		public void Load()
		{
			this._loaded = true;
			Inventory inventoryByKind = GameHubObject.Hub.User.Inventory.GetInventoryByKind(InventoryBag.InventoryKind.PlayerPrefs);
			if (inventoryByKind == null)
			{
				HMMPlayerPrefs.Log.WarnFormat("Inventory for PlayerPrefs not found", new object[0]);
				return;
			}
			InventoryBag inventoryBag = (InventoryBag)((JsonSerializeable<T>)inventoryByKind.Bag);
			PlayerPrefsContent playerPrefsContent = null;
			try
			{
				playerPrefsContent = (PlayerPrefsContent)((JsonSerializeable<T>)inventoryBag.Content);
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.LogErrorFormat("[HMMPlayerPrefs] {0} Loading PlayerPrefs Content, will use default content. Loaded Player Bag Content:\n{1}\n, Exception:\n{2}", new object[]
				{
					ex.GetType().Name,
					inventoryBag.Content,
					ex
				});
			}
			if (playerPrefsContent == null)
			{
				this.SetPreferences(null);
			}
			else
			{
				this.SetPreferences(playerPrefsContent.Values);
				if (playerPrefsContent.version < 2 && this.OnPrefsWrongVersion != null)
				{
					this.OnPrefsWrongVersion();
				}
			}
			this.OnPrefsWrongVersion = null;
		}

		private void SetPreferences(Dictionary<string, string> values)
		{
			if (values != null)
			{
				foreach (KeyValuePair<string, string> keyValuePair in values)
				{
					this._values[keyValuePair.Key] = keyValuePair.Value;
				}
			}
			if (this.OnPrefsLoaded != null)
			{
				this.OnPrefsLoaded();
				this.OnPrefsLoaded = null;
			}
		}

		public void SkipSwordfishLoad()
		{
			this._loaded = true;
			Dictionary<string, string> debugSettings = GameHubObject.Hub.Config.GetDebugSettings();
			this.SetPreferences(debugSettings);
		}

		public static readonly BitLogger Log = new BitLogger(typeof(HMMPlayerPrefs));

		private readonly Dictionary<string, string> _values = new Dictionary<string, string>();

		private bool _loaded;

		private bool _saveRunning;

		private bool _breakSave;

		private float _waitTime;

		private const float WaitSaveSeconds = 60f;

		private const int CurrentValidPrefsVersion = 2;
	}
}
