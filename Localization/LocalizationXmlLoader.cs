using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Hoplon.Localization.TranslationTable;
using Hoplon.Logging;
using Hoplon.ToggleableFeatures;

namespace HeavyMetalMachines.Localization
{
	public class LocalizationXmlLoader : ILocalizationLoader
	{
		public LocalizationXmlLoader(ILocalizationFileReader fileReader, ILogger<LocalizationXmlLoader> logger, IIsFeatureToggled isFeatureToggled)
		{
			this._fileReader = fileReader;
			this._logger = logger;
			this._isFeatureToggled = isFeatureToggled;
		}

		public LocaleTranslationData LoadLocale(string locale)
		{
			return this.LoadContexts(locale);
		}

		private LocaleTranslationData LoadContexts(string locale)
		{
			List<string> list = this._fileReader.GetLocaleContexts(locale).ToList<string>();
			if (!list.Any<string>())
			{
				string text = string.Format("No contexts in locale: \"{0}\".", locale);
				this._logger.Error(text);
				throw new EmptyLocaleException(text);
			}
			LocaleTranslationData localeTranslationData = new LocaleTranslationData
			{
				Locale = locale
			};
			foreach (string contextName in list)
			{
				this.LoadContextEntries(locale, contextName, localeTranslationData);
			}
			return localeTranslationData;
		}

		private void LoadContextEntries(string locale, string contextName, LocaleTranslationData localeTranslationData)
		{
			ContextTranslationData contextTranslationData = LocalizationXmlLoader.AddOrCreateContext(contextName, localeTranslationData);
			string localizationFileContent = this._fileReader.GetLocalizationFileContent(locale, contextName);
			using (XmlReader xmlReader = XmlReader.Create(new StringReader(localizationFileContent)))
			{
				while (xmlReader.ReadToFollowing("entry"))
				{
					this.LoadEntry(xmlReader, localeTranslationData, contextTranslationData);
				}
				if (contextTranslationData.Entries.Count == 0)
				{
					this._logger.Warn(string.Format("No entries in localization file. Locale: \"{0}\"; Context: \"{1}\".", locale, contextName));
				}
			}
		}

		private void LoadEntry(XmlReader reader, LocaleTranslationData localeData, ContextTranslationData contextData)
		{
			string attribute = reader.GetAttribute("name");
			string attribute2 = reader.GetAttribute("feature");
			reader.MoveToElement();
			string text = reader.ReadElementContentAsString();
			if (!string.IsNullOrEmpty(attribute2) && !this.ShouldOverrideEntry(attribute2))
			{
				return;
			}
			text = LocalizationXmlLoader.RestoreEscapedCharacters(text);
			contextData.Entries[attribute] = text;
		}

		private bool ShouldOverrideEntry(string featureName)
		{
			return this._isFeatureToggled.Check(featureName);
		}

		private static ContextTranslationData AddOrCreateContext(string contextName, LocaleTranslationData localeTranslationData)
		{
			ContextTranslationData contextTranslationData;
			if (localeTranslationData.Contexts.TryGetValue(contextName, out contextTranslationData))
			{
				return contextTranslationData;
			}
			contextTranslationData = new ContextTranslationData();
			localeTranslationData.Contexts[contextName] = contextTranslationData;
			return contextTranslationData;
		}

		private static string RestoreEscapedCharacters(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				return text;
			}
			StringBuilder stringBuilder = new StringBuilder(text);
			stringBuilder.Replace("&apos;", "'").Replace("&quot;", "\"").Replace("&gt;", ">").Replace("&lt;", "<").Replace("&amp;", "&").Replace("&#39;", "'").Replace("&#34;", "\"").Replace("&#62;", ">").Replace("&#60;", "<").Replace("&#38;", "&");
			return stringBuilder.ToString();
		}

		private readonly ILocalizationFileReader _fileReader;

		private readonly ILogger<LocalizationXmlLoader> _logger;

		private readonly IIsFeatureToggled _isFeatureToggled;
	}
}
