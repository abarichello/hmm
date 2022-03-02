using System;
using ClientAPI;
using ClientAPI.Objects.Custom;
using ClientAPI.Service.Interfaces;
using Hoplon.Logging;

namespace HeavyMetalMachines.SkipSwordfish
{
	public class SkipSwordfishParentalControl : IParentalControl
	{
		public SkipSwordfishParentalControl(ILogger<SkipSwordfishParentalControl> logger)
		{
			this._logger = logger;
		}

		public void SetAgeRestrictionSync(AgeRestriction ageRestriction)
		{
			this._logger.Debug("SetAgeRestrictionSync");
		}

		public void GetParentalControlInfo(object state, SwordfishClientApi.ParameterizedCallback<ParentalControlInfo> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			this._logger.Debug("GetParentalControlInfo");
		}

		public ParentalControlInfo GetParentalControlInfoSync()
		{
			this._logger.Debug("GetParentalControlInfoSync");
			return new ParentalControlInfo();
		}

		public void CheckUserToUserUGCRestriction(object state, string targetUniversalId, SwordfishClientApi.ParameterizedCallback<CheckUserToUserUGCRestrictionResult> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public void CheckUserToUsersUGCRestrictions(object state, string[] targetUniversalIds, SwordfishClientApi.ParameterizedCallback<CheckUserToUsersUGCRestrictionsResults> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public void CheckUserToUsersVoiceRestrictions(object state, string[] targetUniversalIds, SwordfishClientApi.ParameterizedCallback<CheckUserToUsersUGCRestrictionsResults> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public void CheckUserToUsersTextRestrictions(object state, string[] targetUniversalIds, SwordfishClientApi.ParameterizedCallback<CheckUserToUsersUGCRestrictionsResults> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public void CheckUserToUsersRestrictions(object state, string[] targetUniversalIds, SwordfishClientApi.ParameterizedCallback<CheckUserToUsersRestrictionsResults> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		private readonly ILogger<SkipSwordfishParentalControl> _logger;
	}
}
