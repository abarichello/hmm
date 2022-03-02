using System;
using System.IO;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.OpenUrl.Infra;
using HeavyMetalMachines.Players.Business;
using Pocketverse;
using Zenject;

namespace HeavyMetalMachines.Infra.Quiz
{
	public class QuizUrlFileProvider : IQuizUrlFileProvider
	{
		public QuizUrlFileProvider()
		{
			this._quizFilePath = Path.Combine(Platform.Current.GetPersistentDataDirectory(), "quiz");
		}

		private string GetBaseQuizUrl()
		{
			IOpenUrlService openUrlService = this._diContainer.Resolve<IOpenUrlService>();
			string redirectUrl = openUrlService.GetRedirectUrl();
			char urlPrefix = this.GetUrlPrefix(redirectUrl);
			string arg = "feature=quiz";
			return redirectUrl + urlPrefix + arg;
		}

		public void TryCreateQuizUrlFile(Guid matchId)
		{
			if (!this._config.GetBoolValue(ConfigAccess.EnableToCreateLauncherFileQuiz))
			{
				return;
			}
			using (StreamWriter streamWriter = new StreamWriter(this._quizFilePath))
			{
				QuizUrlFileProvider._log.Debug("Creating quiz file to launcher");
				string quizUrlToLauncherFile = this.GetQuizUrlToLauncherFile(matchId);
				streamWriter.Write(quizUrlToLauncherFile);
			}
		}

		public string GetQuizUrl(Guid matchId)
		{
			IOpenUrlService openUrlService = this._diContainer.Resolve<IOpenUrlService>();
			IGetLocalPlayer getLocalPlayer = this._diContainer.Resolve<IGetLocalPlayer>();
			string baseQuizUrl = this.GetBaseQuizUrl();
			return string.Format("{0}&lang={1}&steamid={2}&matchid={3}", new object[]
			{
				baseQuizUrl,
				openUrlService.GetCurrentLanguageName(),
				getLocalPlayer.Get().UniversalId,
				matchId
			});
		}

		private char GetUrlPrefix(string redirectUrl)
		{
			return (!redirectUrl.Contains("?")) ? '?' : '&';
		}

		public void TryDeleteQuizUrlFile()
		{
			if (File.Exists(this._quizFilePath))
			{
				File.Delete(this._quizFilePath);
			}
			QuizUrlFileProvider._log.Debug("Deleting quiz file to launcher");
		}

		private string GetQuizUrlToLauncherFile(Guid matchId)
		{
			string quizUrl = this.GetQuizUrl(matchId);
			return string.Format("{0}&launcher=1", quizUrl);
		}

		private static readonly BitLogger _log = new BitLogger(typeof(QuizUrlFileProvider));

		[InjectOnClient]
		private IConfigLoader _config;

		[InjectOnClient]
		private DiContainer _diContainer;

		private const string QuizFileName = "quiz";

		private readonly string _quizFilePath;
	}
}
