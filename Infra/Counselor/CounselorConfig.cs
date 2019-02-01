using System;
using HeavyMetalMachines.Options;
using Hoplon.SensorSystem;
using Pocketverse;
using UnityEngine;
using UnityEngine.Serialization;

namespace HeavyMetalMachines.Infra.Counselor
{
	public class CounselorConfig : GameHubScriptableObject
	{
		public int DefaultRookieCharacterPick = 6;

		public float WrongWaySensibility = 200f;

		public FMODVoiceOverAsset IntroCounselorAudioAsset;

		public FMODVoiceOverAsset LoadingCounselorAudioAsset;

		public int[] AllowedLoadingAdvicesCharactersId;

		public CounselorConfig.ConditionalConfig[] _conditionalValues;

		public CounselorConfig.AdvicesConfig[] Advices;

		[Header("GUI")]
		public Sprite ShortcutMouse0Sprite;

		public Sprite ShortcutMouse1Sprite;

		public Sprite ShortcutMouse2Sprite;

		[Serializable]
		public struct ConditionalConfig
		{
			public ServerCounselorController.CounselorConditions condition;

			public ServerCounselorController.ScannerParameters scanner;

			public NumericCondition.NumericSensorType numericType;

			public float value;
		}

		[Serializable]
		public struct AdvicesConfig
		{
			[FormerlySerializedAs("TranslationKey")]
			public string TranslationKey;

			public FMODVoiceOverAsset AudioAsset;

			public FMODVoiceOverAsset AlternativeAudioAsset;

			public string AlternativeKeyTrigger;

			public ControlAction ControlAction;

			public string CursorText;

			public float WarmupSeconds;

			public int MaxUsesPerGame;

			public bool CheckAlive;

			public bool AllCharactersAllowed;

			public int[] AllowedCharactersId;

			public ServerCounselorController.CounselorConditions[] Conditions;

			public string DependencyAdviceName;
		}
	}
}
