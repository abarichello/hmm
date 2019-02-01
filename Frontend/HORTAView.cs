using System;
using Pocketverse;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.Frontend
{
	public class HORTAView : GameHubBehaviour
	{
		private void OnEnable()
		{
			this.PlayButton.SetActive(false);
			this.MatchData.gameObject.SetActive(false);
			this.Component.OnVersionMismatched += this.ListenToVersionMismatched;
			this.Component.OnReadFileException += this.ListenToReadFileException;
			this.Component.OnMatchFileLoaded += this.ListenToMatchFileLoaded;
		}

		private void OnDisable()
		{
			this.Component.OnVersionMismatched -= this.ListenToVersionMismatched;
			this.Component.OnReadFileException -= this.ListenToReadFileException;
			this.Component.OnMatchFileLoaded -= this.ListenToMatchFileLoaded;
		}

		private void ListenToMatchFileLoaded(IMatchInformation match)
		{
			this.PlayButton.SetActive(true);
			this.MatchData.gameObject.SetActive(true);
			this.MatchData.text = string.Format("Match={0}\nVersion={1}\nMatchData={2}\nStates={3}\nKeyFrames={4}\n", new object[]
			{
				match.MatchId,
				match.Version,
				match.Data,
				match.States,
				match.KeyFrames
			});
		}

		private void ListenToReadFileException(Exception exception)
		{
			this.WarningText.gameObject.SetActive(true);
			this.WarningText.text = exception.Message;
		}

		private void ListenToVersionMismatched(string fileVer, string buildVer)
		{
			this.WarningText.gameObject.SetActive(true);
			this.WarningText.text = string.Format("Version mismatch current={0} file={1}", buildVer, fileVer);
		}

		public void OnButtonLoad()
		{
			this.WarningText.text = string.Empty;
			this.MatchData.text = string.Empty;
			this.Component.LoadFile(this.FilePath.text);
		}

		public void OnButtonPlay()
		{
			this.Component.Play();
		}

		public static readonly BitLogger Log = new BitLogger(typeof(HORTAView));

		public HORTAComponent Component;

		public InputField FilePath;

		public GameObject PlayButton;

		public Text MatchData;

		public Text WarningText;
	}
}
