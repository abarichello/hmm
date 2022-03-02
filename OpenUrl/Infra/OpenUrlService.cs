using System;
using System.Collections.Generic;
using Assets.Standard_Assets.Scripts.Infra;
using HeavyMetalMachines.Swordfish.Session;
using HeavyMetalMachines.Utils;
using Pocketverse;
using UnityEngine.Networking;

namespace HeavyMetalMachines.OpenUrl.Infra
{
	public class OpenUrlService : IOpenUrlService
	{
		public OpenUrlService(IConfigLoader configLoader, ILoginSessionIdProvider loginSessionIdProvider, IPublisher publisher)
		{
			this._configLoader = configLoader;
			this._loginSessionIdProvider = loginSessionIdProvider;
			this._publisher = publisher;
		}

		public string GetRedirectUrl()
		{
			string gameClientRedirectUrl = this._publisher.GetGameClientRedirectUrl();
			if (gameClientRedirectUrl != null)
			{
				return gameClientRedirectUrl;
			}
			return this._configLoader.GetValue(ConfigAccess.RedirectUrl);
		}

		public void OpenUrl(string url)
		{
			OpenUrlUtils.OpenUrl(url);
		}

		public string GetCurrentLanguageName()
		{
			return Language.CurrentLanguage.ToString();
		}

		public string GetToken()
		{
			string token = this._loginSessionIdProvider.GetToken();
			string text = UnityWebRequest.EscapeURL(token);
			return UnityWebRequest.EscapeURL(text);
		}

		private readonly IConfigLoader _configLoader;

		private readonly ILoginSessionIdProvider _loginSessionIdProvider;

		private readonly Dictionary<int, Action> _urlActions;

		private readonly Dictionary<int, string> _featureNames;

		private readonly string _redirectUrl;

		private readonly bool _hasQuestionMark;

		private readonly IPublisher _publisher;
	}
}
