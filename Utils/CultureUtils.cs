using System;
using System.Collections.Generic;
using System.Globalization;
using HeavyMetalMachines.Platform;
using Pocketverse;

namespace HeavyMetalMachines.Utils
{
	public class CultureUtils
	{
		static CultureUtils()
		{
			foreach (CultureInfo cultureInfo in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
			{
				RegionInfo regionInfo = new RegionInfo(cultureInfo.LCID);
				if (!CultureUtils.CultureInfoNameByIsoCurrency.ContainsKey(regionInfo.ISOCurrencySymbol))
				{
					CultureUtils.CultureInfoNameByIsoCurrency.Add(regionInfo.ISOCurrencySymbol, cultureInfo.Name);
				}
			}
		}

		public static string GetCultureInfoName(string isoCurrency)
		{
			string result;
			if (!CultureUtils.CultureInfoNameByIsoCurrency.TryGetValue(isoCurrency, out result))
			{
				Debug.Assert(false, string.Format("CurrencyUtils error. ISOCurrency not found: {0}", isoCurrency), Debug.TargetTeam.All);
				return CultureUtils.GetSystemCulture().Name;
			}
			return result;
		}

		public static string FormatValue(string isoCurrency, decimal currencyValue)
		{
			CultureInfo provider = new CultureInfo(CultureUtils.GetCultureInfoName(isoCurrency));
			string text = string.Format(provider, "{0:C}", new object[]
			{
				currencyValue
			});
			if (isoCurrency == "JPY")
			{
				text = text.Replace('\\', '¥');
			}
			return text;
		}

		public static CultureInfo GetSystemCulture()
		{
			CultureInfo result;
			try
			{
				result = new CultureInfo(WindowsPlatform.GetSystemDefaultLCID());
			}
			catch (Exception ex)
			{
				CultureUtils.Log.ErrorFormat("Error on get system culture info. Using default. - Exception: {0}", new object[]
				{
					ex
				});
				result = CultureInfo.CurrentCulture;
			}
			return result;
		}

		public static readonly BitLogger Log = new BitLogger(typeof(CultureUtils));

		private static readonly Dictionary<string, string> CultureInfoNameByIsoCurrency = new Dictionary<string, string>();
	}
}
