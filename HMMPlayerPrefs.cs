using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using ClientAPI;
using ClientAPI.Objects;
using HeavyMetalMachines.DataTransferObjects.Result;
using HeavyMetalMachines.Swordfish;
using Hoplon.Serialization;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines
{
	public class HMMPlayerPrefs : GameHubObject, IHMMPlayerPrefs
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private event Action OnPrefsLoaded;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private event Action OnPrefsWrongVersion;

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

		public bool IsLoaded()
		{
			return this._loaded;
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
				return;
			}
			PlayerPrefsContent playerPrefsContent = new PlayerPrefsContent
			{
				Values = this._values,
				version = 9
			};
			InventoryBag inventoryBag = new InventoryBag
			{
				Kind = 6,
				Content = playerPrefsContent.Serialize()
			};
			HMMPlayerPrefs.Log.Debug(inventoryBag.Serialize());
			PlayerCustomWS.SavePlayerPrefs(inventoryBag.Serialize(), new SwordfishClientApi.ParameterizedCallback<string>(this.OnSave), new SwordfishClientApi.ErrorCallback(this.OnError));
		}

		private void OnError(object state, Exception exception)
		{
			HMMPlayerPrefs.Log.Fatal("Failed to save preferences.", exception);
		}

		private void OnSave(object state, string obj)
		{
			NetResult netResult = (NetResult)((JsonSerializeable<!0>)obj);
			if (netResult.Success)
			{
				HMMPlayerPrefs.Log.Debug("Player preferences saved!");
			}
			else
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

		public void ExecOnceOnPrefsLoaded(Action action)
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

		public void ExecOnPrefsWrongVersion(Action action)
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
			Inventory inventoryByKind = GameHubObject.Hub.User.Inventory.GetInventoryByKind(6);
			if (inventoryByKind == null)
			{
				HMMPlayerPrefs.Log.WarnFormat("Inventory for PlayerPrefs not found", new object[0]);
				return;
			}
			InventoryBag inventoryBag = (InventoryBag)((JsonSerializeable<!0>)inventoryByKind.Bag);
			PlayerPrefsContent playerPrefsContent = null;
			try
			{
				playerPrefsContent = (PlayerPrefsContent)((JsonSerializeable<!0>)inventoryBag.Content);
			}
			catch (Exception ex)
			{
				Debug.LogErrorFormat("[HMMPlayerPrefs] {0} Loading PlayerPrefs Content, will use default content. Loaded Player Bag Content:\n{1}\n, Exception:\n{2}", new object[]
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
				Dictionary<string, string> preferences = playerPrefsContent.Values;
				if (playerPrefsContent.version < 9 && this.OnPrefsWrongVersion != null)
				{
					this.OnPrefsWrongVersion();
					preferences = null;
					this._values.Clear();
					this.SaveNow();
				}
				this.SetPreferences(preferences);
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
			this.SetPreferences(null);
		}

		public static readonly BitLogger Log = new BitLogger(typeof(HMMPlayerPrefs));

		private readonly Dictionary<string, string> _values = new Dictionary<string, string>();

		private bool _loaded;

		private bool _saveRunning;

		private bool _breakSave;

		private float _waitTime;

		private const float WaitSaveSeconds = 60f;

		private const int CurrentValidPrefsVersion = 9;
	}
}
