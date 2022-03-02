using System;

namespace HeavyMetalMachines.Infra.Quiz
{
	public interface IQuizUrlFileProvider
	{
		void TryCreateQuizUrlFile(Guid matchId);

		string GetQuizUrl(Guid matchId);

		void TryDeleteQuizUrlFile();
	}
}
