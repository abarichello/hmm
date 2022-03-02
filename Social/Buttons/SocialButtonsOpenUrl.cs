using System;
using HeavyMetalMachines.BI;
using HeavyMetalMachines.Configuring.Instances;
using HeavyMetalMachines.Utils;
using Pocketverse;

namespace HeavyMetalMachines.Social.Buttons
{
	public class SocialButtonsOpenUrl : ISocialButtonsOpenUrl
	{
		public SocialButtonsOpenUrl(IClientButtonBILogger clientButtonBiLogger, IConfigLoader configLoader)
		{
			this._clientButtonBiLogger = clientButtonBiLogger;
			this._configLoader = configLoader;
		}

		public void OpenDiscord()
		{
			this.OpenUrl(this._configLoader.GetValue(ConfigAccess.DiscordPageURL), ButtonName.FooterDiscord);
		}

		public void OpenInstagram()
		{
			ConfigInstance inst = (!SocialButtonsOpenUrl.IsLanguagePt()) ? ConfigAccess.InstagramPageURL : ConfigAccess.InstagramBRPageURL;
			this.OpenUrl(this._configLoader.GetValue(inst), ButtonName.FooterInstagram);
		}

		public void OpenFacebook()
		{
			ConfigInstance inst = (!SocialButtonsOpenUrl.IsLanguagePt()) ? ConfigAccess.FacebookPageURL : ConfigAccess.FacebookBRPageURL;
			this.OpenUrl(this._configLoader.GetValue(inst), ButtonName.FooterFacebook);
		}

		public void OpenVk()
		{
			this.OpenUrl(this._configLoader.GetValue(ConfigAccess.VkPageURL), ButtonName.FooterVK);
		}

		public void OpenSupport()
		{
			this.OpenUrl(this._configLoader.GetValue(ConfigAccess.SupportPageURL) + "?lang=" + Language.CurrentLanguage, ButtonName.FooterSupport);
		}

		public void OpenTwitter()
		{
			ConfigInstance inst = (!SocialButtonsOpenUrl.IsLanguagePt()) ? ConfigAccess.TwitterPageURL : ConfigAccess.TwitterBRPageURL;
			this.OpenUrl(this._configLoader.GetValue(inst), ButtonName.FooterTwitter);
		}

		private void OpenUrl(string url, ButtonNameInstance button)
		{
			OpenUrlUtils.OpenUrl(url);
			this._clientButtonBiLogger.LogButtonClick(button);
		}

		private static bool IsLanguagePt()
		{
			LanguageCode currentLanguage = Language.CurrentLanguage;
			return currentLanguage == LanguageCode.PT || currentLanguage == LanguageCode.PT_BR;
		}

		private readonly IClientButtonBILogger _clientButtonBiLogger;

		private readonly IConfigLoader _configLoader;
	}
}
