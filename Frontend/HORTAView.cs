using System;
using System.IO;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using Pocketverse;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace HeavyMetalMachines.Frontend
{
	public class HORTAView : GameHubBehaviour
	{
		private void OnEnable()
		{
			this.PlayButton.SetActive(false);
			this.MatchData.gameObject.SetActive(false);
			this.FilePath.text = this.Component.DefaultFile;
			this.Component.OnVersionMismatched += this.ListenToVersionMismatched;
			this.Component.OnReadFileException += this.ListenToReadFileException;
			this.Component.OnMatchFileLoaded += this.ListenToMatchFileLoaded;
			this.PlayReplayFileAtConfigIfRunningOnConsoles();
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
			this.Load(this.FilePath.text);
		}

		public void OnButtonPlay()
		{
			this.Component.Play();
		}

		private void Load(string fileUri)
		{
			this.WarningText.text = string.Empty;
			this.MatchData.text = string.Empty;
			if (string.IsNullOrEmpty(fileUri))
			{
				Debug.LogError("HORTA replay file uri is null or empty.");
				return;
			}
			Debug.LogErrorFormat("HORTA is loading replay file at '{0}'.", new object[]
			{
				fileUri
			});
			this.Component.LoadFile(fileUri);
		}

		private void PlayReplayFileAtConfigIfRunningOnConsoles()
		{
			if (!Platform.Current.IsConsole())
			{
				return;
			}
			string fileUri = Path.Combine(Application.streamingAssetsPath, this._config.GetValue(ConfigAccess.HORTAFileUri));
			this.Load(fileUri);
			this.Component.Play();
		}

		public static readonly BitLogger Log = new BitLogger(typeof(HORTAView));

		[Inject]
		private IConfigLoader _config;

		[InjectOnClient]
		private HORTAComponent Component;

		public InputField FilePath;

		public GameObject PlayButton;

		public Text MatchData;

		public Text WarningText;
	}
}
