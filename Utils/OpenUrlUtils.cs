using System;
using ClientAPI.Browser;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Utils
{
	public class OpenUrlUtils
	{
		public static int HardcodedWidth
		{
			get
			{
				return 1380;
			}
		}

		public static void OpenUrl(string url)
		{
			Application.OpenURL(url);
		}

		public static void OpenSteamUrl(HMMHub hub, ConfigLoader.ConfigInstance configInstance, string urlParams, int width, int height, string windowName = "Heavy Metal Machines")
		{
			OpenUrlUtils.OpenSteamUrl(hub, configInstance, false, width, height, windowName, urlParams);
		}

		public static void OpenSteamUrl(HMMHub hub, ConfigLoader.ConfigInstance configInstance, bool useLangParam, int width, int height, string windowName = "Heavy Metal Machines", string urlParams = null)
		{
			string text = hub.Config.GetValue(configInstance);
			if (string.IsNullOrEmpty(text))
			{
				Debug.Assert(false, string.Format("OpenSteamUrl - Invalid url for config:[{0}]", configInstance.Key), Debug.TargetTeam.All);
				return;
			}
			if (useLangParam)
			{
				text = text + "?lang=" + Language.CurrentLanguage();
			}
			if (!string.IsNullOrEmpty(urlParams))
			{
				text += urlParams;
			}
			OpenUrlUtils.OpenSteamUrl(hub, text, width, height, windowName);
		}

		public static void OpenSteamUrl(HMMHub hub, string url)
		{
			OpenUrlUtils.OpenSteamUrl(hub, url, OpenUrlUtils.HardcodedWidth, (int)((float)Screen.height * 0.9f), "Heavy Metal Machines");
		}

		public static void OpenSteamUrl(HMMHub hub, string url, int width, int height, string windowName = "Heavy Metal Machines")
		{
			if (hub.ClientApi.overlay.IsOverlayEnabled())
			{
				hub.ClientApi.overlay.ShowWebPage(url);
			}
			else
			{
				OpenUrlUtils.Log.Warn("OpenSteamUrl - Overlay is NOT enabled. Using OpenURL as fallback.");
				Application.OpenURL(url);
			}
		}

		public static void OpenInGameBrowserUrl(HMMHub hub, string url, int width, int height, string windowName = "Heavy Metal Machines")
		{
			if (hub.ClientApiBrowser == null)
			{
				hub.ClientApiBrowser = new Browser("HMMBrowser");
			}
			try
			{
				if (hub.ClientApiBrowser.windows.Count > 0)
				{
					string initialUrl = hub.ClientApiBrowser.windows[0].GetInitialUrl();
					hub.ClientApiBrowser.windows[0].Show();
					if (initialUrl.ToLowerInvariant().Trim() != url.ToLowerInvariant().Trim())
					{
						hub.ClientApiBrowser.windows[0].LoadUrl(url);
					}
				}
				else
				{
					hub.ClientApiBrowser.CreateWindow(width, height, url, windowName);
				}
			}
			catch (Exception ex)
			{
				OpenUrlUtils.Log.Warn("OpenSteamUrl - In-Game Browser failed. Using OpenURL as fallback.");
				Debug.Log(ex.Message);
				Application.OpenURL(url);
			}
		}

		public static void OpenTeamsUrl(HMMHub hub, int width, int height)
		{
			string value = hub.Config.GetValue(ConfigAccess.SFTeamsUrl);
			OpenUrlUtils.OpenSteamUrl(hub, string.Format("{0}?lang={1}&id={2}&token={3}", new object[]
			{
				value,
				Language.CurrentLanguage(),
				hub.User.UniversalId,
				WWW.EscapeURL(hub.ClientApi.Token)
			}), width, height, "Heavy Metal Machines");
		}

		private static readonly BitLogger Log = new BitLogger(typeof(OpenUrlUtils));

		private static IWindow browserWindow;
	}
}
