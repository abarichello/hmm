using System;
using ClientAPI;
using ClientAPI.Objects;
using ClientAPI.Service.Interfaces;

namespace HeavyMetalMachines.SkipSwordfish
{
	public class SkipSwordfishPlayerFeedback : IPlayerFeedback
	{
		public void AddFeedback(object state, PlayerFeedback playerFeedback, SwordfishClientApi.ParameterizedCallback<PlayerFeedback> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public PlayerFeedback AddFeedbackSync(PlayerFeedback playerFeedback)
		{
			throw new NotImplementedException();
		}

		public void UpdateFeedback(object state, long playerFeedbackId, PlayerFeedback playerFeedback, SwordfishClientApi.ParameterizedCallback<PlayerFeedback> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public PlayerFeedback UpdateFeedbackSync(long playerFeedbackId, PlayerFeedback playerFeedback)
		{
			throw new NotImplementedException();
		}

		public void RemoveFeedback(object state, long playerFeedbackId, SwordfishClientApi.Callback callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public void RemoveFeedbackSync(long playerFeedbackId)
		{
			throw new NotImplementedException();
		}

		public void GetPlayerFeedback(object state, long playerFeedbackId, SwordfishClientApi.ParameterizedCallback<PlayerFeedback> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public PlayerFeedback GetPlayerFeedbackSync(long playerFeedbackId)
		{
			throw new NotImplementedException();
		}

		public void GetAllPlayerFeedbackByPlayer(object state, long playerId, SwordfishClientApi.ParameterizedCallback<PlayerFeedback[]> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public PlayerFeedback[] GetAllPlayerFeedbackByPlayerSync(long playerId)
		{
			throw new NotImplementedException();
		}

		public void GetAllUninformedFeedback(object state, SwordfishClientApi.ParameterizedCallback<PlayerFeedback[]> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public PlayerFeedback[] GetAllUninformedFeedbackSync()
		{
			throw new NotImplementedException();
		}

		public void GetAllUninformedFeedback(object state, long playerId, SwordfishClientApi.ParameterizedCallback<PlayerFeedback[]> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public PlayerFeedback[] GetAllUninformedFeedbackSync(long playerId)
		{
			throw new NotImplementedException();
		}

		public void MarkFeedbackAsInformed(object state, long playerFeedbackId, SwordfishClientApi.Callback callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public void MarkFeedbackAsInformedSync(long playerFeedbackId)
		{
			throw new NotImplementedException();
		}

		public void MarkListFeedbackAsInformed(object state, long[] playersFeedbackId, SwordfishClientApi.Callback callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public void MarkListFeedbackAsInformedSync(long[] playersFeedbackId)
		{
			throw new NotImplementedException();
		}

		public void CreateReport(object state, long fromPlayerId, long toPlayerId, string reportBag, SwordfishClientApi.ParameterizedCallback<bool> callback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			throw new NotImplementedException();
		}

		public bool CreateReportSync(long fromPlayerId, long toPlayerId, string reportBag)
		{
			throw new NotImplementedException();
		}
	}
}
