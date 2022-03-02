using System;
using HeavyMetalMachines.Input.ControllerInput;
using Hoplon.Input;
using Hoplon.SensorSystem;
using Pocketverse;
using UnityEngine.Serialization;

namespace HeavyMetalMachines.Infra.Counselor
{
	public class CounselorConfig : GameHubScriptableObject
	{
		public int DefaultRookieCharacterPick = 6;

		public float WrongWaySensibility = 200f;

		public AudioEventAsset IntroCounselorAudioAsset;

		public AudioEventAsset JoystickIntroCounselorAudioAsset;

		public AudioEventAsset LoadingCounselorAudioAsset;

		public int[] AllowedLoadingAdvicesCharactersId;

		public CounselorConfig.ConditionalConfig[] _conditionalValues;

		public CounselorConfig.AdvicesConfig[] Advices;

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

			public AudioEventAsset AudioAsset;

			public AudioEventAsset AlternativeAudioAsset;

			public KeyboardMouseCode AlternativeKeyCode;

			public AudioEventAsset JoystickAudioAsset;

			public AudioEventAsset JoystickAlternativeAudioAsset;

			public JoystickTemplateCode JoystickAlternativeKeyCode;

			public ControllerInputActions InputAction;

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
