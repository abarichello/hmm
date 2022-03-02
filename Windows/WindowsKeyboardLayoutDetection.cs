using System;
using System.Text;
using Pocketverse;

namespace HeavyMetalMachines.Windows
{
	internal class WindowsKeyboardLayoutDetection : IKeyboardLayoutDetection
	{
		public int GetKeyboardLayoutId()
		{
			IntPtr foregroundWindow = User32.GetForegroundWindow();
			uint windowThreadProcessId = User32.GetWindowThreadProcessId(foregroundWindow, IntPtr.Zero);
			return User32.GetKeyboardLayout(windowThreadProcessId).ToInt32() & 65535;
		}

		public LanguageCode GetKeyboardLanguageCode(int layoutId)
		{
			switch (layoutId)
			{
			case 1028:
				return LanguageCode.ZH;
			default:
				switch (layoutId)
				{
				case 2052:
					return LanguageCode.ZH;
				default:
					switch (layoutId)
					{
					case 1043:
						return LanguageCode.DE;
					default:
						switch (layoutId)
						{
						case 3079:
							return LanguageCode.DE;
						default:
							switch (layoutId)
							{
							case 4103:
								return LanguageCode.DE;
							default:
								switch (layoutId)
								{
								case 5127:
									return LanguageCode.DE;
								default:
									switch (layoutId)
									{
									case 6153:
										return LanguageCode.EN;
									case 6154:
										return LanguageCode.ES;
									default:
										switch (layoutId)
										{
										case 1055:
											return LanguageCode.TR;
										default:
											switch (layoutId)
											{
											case 2067:
												return LanguageCode.DE;
											default:
												if (layoutId == 7177)
												{
													return LanguageCode.EN;
												}
												if (layoutId == 7178)
												{
													return LanguageCode.ES;
												}
												if (layoutId == 8201)
												{
													return LanguageCode.EN;
												}
												if (layoutId == 8202)
												{
													return LanguageCode.ES;
												}
												if (layoutId == 9225)
												{
													return LanguageCode.EN;
												}
												if (layoutId == 9226)
												{
													return LanguageCode.ES;
												}
												if (layoutId == 10249)
												{
													return LanguageCode.EN;
												}
												if (layoutId == 10250)
												{
													return LanguageCode.ES;
												}
												if (layoutId == 11273)
												{
													return LanguageCode.EN;
												}
												if (layoutId == 11274)
												{
													return LanguageCode.ES;
												}
												if (layoutId == 12297)
												{
													return LanguageCode.EN;
												}
												if (layoutId == 12298)
												{
													return LanguageCode.ES;
												}
												if (layoutId == 13321)
												{
													return LanguageCode.EN;
												}
												if (layoutId != 13322 && layoutId != 14346 && layoutId != 15370 && layoutId != 16394 && layoutId != 17418 && layoutId != 18442 && layoutId != 19466 && layoutId != 20490)
												{
													return LanguageCode.EN;
												}
												return LanguageCode.ES;
											case 2070:
												break;
											}
											break;
										case 1058:
											return LanguageCode.RU;
										}
										break;
									case 6156:
										return LanguageCode.FR;
									}
									break;
								case 5129:
									return LanguageCode.EN;
								case 5130:
									return LanguageCode.ES;
								case 5132:
									return LanguageCode.FR;
								}
								break;
							case 4105:
								return LanguageCode.EN;
							case 4106:
								return LanguageCode.ES;
							case 4108:
								return LanguageCode.FR;
							}
							break;
						case 3081:
							return LanguageCode.EN;
						case 3082:
							return LanguageCode.ES;
						case 3084:
							return LanguageCode.FR;
						}
						break;
					case 1045:
						return LanguageCode.PL;
					case 1046:
						break;
					case 1049:
						return LanguageCode.RU;
					}
					return LanguageCode.PT;
				case 2055:
					return LanguageCode.DE;
				case 2057:
					break;
				case 2058:
					return LanguageCode.ES;
				case 2060:
					return LanguageCode.FR;
				}
				break;
			case 1031:
				return LanguageCode.DE;
			case 1033:
				break;
			case 1034:
				return LanguageCode.ES;
			case 1036:
				return LanguageCode.FR;
			}
			return LanguageCode.EN;
		}

		public string GetKeyboardLayoutName()
		{
			StringBuilder stringBuilder = new StringBuilder(50);
			User32.GetKeyboardLayoutName(stringBuilder);
			string text = stringBuilder.ToString();
			switch (text)
			{
			case "0000041C":
				return "Albanian";
			case "00000401":
				return "Arabic (101)";
			case "00010401":
				return "Arabic (102)";
			case "00020401":
				return "Arabic (102) AZERTY";
			case "0000042B":
				return "Armenian Eastern";
			case "0002042B":
				return "Armenian Phonetic";
			case "0003042B":
				return "Armenian Typewriter";
			case "0001042B":
				return "Armenian Western";
			case "0000044D":
				return "Assamese - Inscript";
			case "0001042C":
				return "Azerbaijani (Standard)";
			case "0000082C":
				return "Azerbaijani Cyrillic";
			case "0000042C":
				return "Azerbaijani Latin";
			case "0000046D":
				return "Bashkir";
			case "00000423":
				return "Belarusian";
			case "0001080C":
				return "Belgian (Comma)";
			case "00000813":
				return "Belgian (Period)";
			case "0000080C":
				return "Belgian French";
			case "00000445":
				return "Bangla (Bangladesh)";
			case "00020445":
				return "Bangla (India)";
			case "00010445":
				return "Bangla (India - Legacy)";
			case "0000201A":
				return "Bosnian (Cyrillic)";
			case "000B0C00":
				return "Buginese";
			case "00030402":
				return "Bulgarian";
			case "00010402":
				return "Bulgarian (Latin)";
			case "00020402":
				return "Bulgarian (phonetic layout)";
			case "00040402":
				return "Bulgarian (phonetic traditional)";
			case "00000402":
				return "Bulgarian (Typewriter)";
			case "00001009":
				return "Canadian French";
			case "00011009":
				return "Canadian Multilingual Standard";
			case "0000085F":
				return "Central Atlas Tamazight";
			case "0000045C":
				return "Cherokee Nation";
			case "0001045C":
				return "Cherokee Nation Phonetic";
			case "00000804":
				return "Chinese (Simplified) - US Keyboard";
			case "00000404":
				return "Chinese (Traditional) - US Keyboard";
			case "00000C04":
				return "Chinese (Traditional, Hong Kong S.A.R.)";
			case "00001404":
				return "Chinese (Traditional Macao S.A.R.) US Keyboard";
			case "00001004":
				return "Chinese (Simplified, Singapore) - US keyboard";
			case "0000041A":
				return "Croatian";
			case "00000405":
				return "Czech";
			case "00010405":
				return "Czech (QWERTY)";
			case "00020405":
				return "Czech Programmers";
			case "00000406":
				return "Danish";
			case "00000439":
				return "Devanagari-INSCRIPT";
			case "00000465":
				return "Divehi Phonetic";
			case "00010465":
				return "Divehi Typewriter";
			case "00000413":
				return "Dutch";
			case "00000C51":
				return "Dzongkha";
			case "00000425":
				return "Estonian";
			case "00000438":
				return "Faeroese";
			case "0000040B":
				return "Finnish";
			case "0001083B":
				return "Finnish with Sami";
			case "0000040C":
				return "French";
			case "00120C00":
				return "Futhark";
			case "00000437":
				return "Georgian";
			case "00020437":
				return "Georgian (Ergonomic)";
			case "00010437":
				return "Georgian (QWERTY)";
			case "00030437":
				return "Georgian Ministry of Education and Science Schools";
			case "00040437":
				return "Georgian (Old Alphabets)";
			case "00000407":
				return "German";
			case "00010407":
				return "German (IBM)";
			case "000C0C00":
				return "Gothic";
			case "00000408":
				return "Greek";
			case "00010408":
				return "Greek (220)";
			case "00030408":
				return "Greek (220) Latin";
			case "00020408":
				return "Greek (319)";
			case "00040408":
				return "Greek (319) Latin";
			case "00050408":
				return "Greek Latin";
			case "00060408":
				return "Greek Polytonic";
			case "0000046F":
				return "Greenlandic";
			case "00000474":
				return "Guarani";
			case "00000447":
				return "Gujarati";
			case "00000468":
				return "Hausa";
			case "0000040D":
				return "Hebrew";
			case "00010439":
				return "Hindi Traditional";
			case "0000040E":
				return "Hungarian";
			case "0001040E":
				return "Hungarian 101-key";
			case "0000040F":
				return "Icelandic";
			case "00000470":
				return "Igbo";
			case "000004009":
				return "India";
			case "0000085D":
				return "Inuktitut - Latin";
			case "0001045D":
				return "Inuktitut - Naqittaut";
			case "00001809":
				return "Irish";
			case "00000410":
				return "Italian";
			case "00010410":
				return "Italian (142)";
			case "00000411":
				return "Japanese";
			case "00110C00":
				return "Javanese";
			case "0000044B":
				return "Kannada";
			case "0000043F":
				return "Kazakh";
			case "00000453":
				return "Khmer";
			case "00010453":
				return "Khmer (NIDA)";
			case "00000412":
				return "Korean";
			case "00000440":
				return "Kyrgyz Cyrillic";
			case "00000454":
				return "Lao";
			case "0000080A":
				return "Latin American";
			case "00020426":
				return "Latvian (Standard)";
			case "00010426":
				return "Latvian (Legacy)";
			case "00070C00":
				return "Lisu (Basic)";
			case "00080C00":
				return "Lisu (Standard)";
			case "00010427":
				return "Lithuanian";
			case "00000427":
				return "Lithuanian IBM";
			case "00020427":
				return "Lithuanian Standard";
			case "0000046E":
				return "Luxembourgish";
			case "0000042F":
				return "Macedonia (FYROM)";
			case "0001042F":
				return "Macedonia (FYROM) - Standard";
			case "0000044C":
				return "Malayalam";
			case "0000043A":
				return "Maltese 47-Key";
			case "0001043A":
				return "Maltese 48-key";
			case "00000481":
				return "Maori";
			case "0000044E":
				return "Marathi";
			case "00000850":
				return "Mongolian (Mongolian Script - Legacy)";
			case "00020850":
				return "Mongolian (Mongolian Script - Standard)";
			case "00000450":
				return "Mongolian Cyrillic";
			case "00010C00":
				return "Myanmar";
			case "00090C00":
				return "N'ko";
			case "00000461":
				return "Nepali";
			case "00020C00":
				return "New Tai Lue";
			case "00000414":
				return "Norwegian";
			case "0000043B":
				return "Norwegian with Sami";
			case "00000448":
				return "Odia";
			case "000D0C00":
				return "Ol Chiki";
			case "000F0C00":
				return "Old Italic";
			case "000E0C00":
				return "Osmanya";
			case "00000463":
				return "Pashto (Afghanistan)";
			case "00000429":
				return "Persian";
			case "00050429":
				return "Persian (Standard)";
			case "000A0C00":
				return "Phags-pa";
			case "00010415":
				return "Polish (214)";
			case "00000415":
				return "Polish (Programmers)";
			case "00000816":
				return "Portuguese";
			case "00000416":
				return "Portuguese (Brazilian ABNT)";
			case "00010416":
				return "Portuguese (Brazilian ABNT2)";
			case "00000446":
				return "Punjabi";
			case "00000418":
				return "Romanian (Legacy)";
			case "00020418":
				return "Romanian (Programmers)";
			case "00010418":
				return "Romanian (Standard)";
			case "00000419":
				return "Russian";
			case "00020419":
				return "Russian - Mnemonic";
			case "00010419":
				return "Russian (Typewriter)";
			case "0002083B":
				return "Sami Extended Finland-Sweden";
			case "0001043B":
				return "Sami Extended Norway";
			case "00011809":
				return "Scottish Gaelic";
			case "00000C1A":
				return "Serbian (Cyrillic)";
			case "0000081A":
				return "Serbian (Latin)";
			case "0000046C":
				return "Sesotho sa Leboa";
			case "00000432":
				return "Setswana";
			case "0000045B":
				return "Sinhala";
			case "0001045B":
				return "Sinhala - wij 9";
			case "0000041B":
				return "Slovak";
			case "0001041B":
				return "Slovak (QWERTY)";
			case "00000424":
				return "Slovenian";
			case "00100C00":
				return "Sora";
			case "0001042E":
				return "Sorbian Extended";
			case "0002042E":
				return "Sorbian Standard";
			case "0000042E":
				return "Sorbian Standard (Legacy)";
			case "0000040A":
				return "Spanish";
			case "0001040A":
				return "Spanish Variation";
			case "0000041D":
				return "Swedish";
			case "0000083B":
				return "Swedish with Sami";
			case "0000100C":
				return "Swiss French";
			case "00000807":
				return "Swiss German";
			case "0000045A":
				return "Syriac";
			case "0001045A":
				return "Syriac Phonetic";
			case "00030C00":
				return "Tai Le";
			case "00000428":
				return "Tajik";
			case "00000449":
				return "Tamil";
			case "00010444":
				return "Tatar";
			case "00000444":
				return "Tatar (Legacy)";
			case "0000044A":
				return "Telugu";
			case "0000041E":
				return "Thai Kedmanee";
			case "0002041E":
				return "Thai Kedmanee (non-ShiftLock)";
			case "0001041E":
				return "Thai Pattachote";
			case "0003041E":
				return "Thai Pattachote (non-ShiftLock)";
			case "00010451":
				return "Tibetan (PRC - Standard)";
			case "00000451":
				return "Tibetan (PRC - Legacy)";
			case "00050C00":
				return "Tifinagh (Basic)";
			case "00060C00":
				return "Tifinagh (Full)";
			case "0001041F":
				return "Turkish F";
			case "0000041F":
				return "Turkish Q";
			case "00000442":
				return "Turkmen";
			case "00000480":
				return "Uyghur (Legacy)";
			case "00000422":
				return "Ukrainian";
			case "00020422":
				return "Ukrainian (Enhanced)";
			case "00000809":
				return "United Kingdom";
			case "00000452":
				return "United Kingdom Extended";
			case "00010409":
				return "United States - Dvorak";
			case "00020409":
				return "United States - International";
			case "00030409":
				return "United States-Dvorak for left hand";
			case "00040409":
				return "United States-Dvorak for right hand";
			case "00000409":
				return "United States - English";
			case "00004009":
				return "United States - india";
			case "00000420":
				return "Urdu";
			case "00010480":
				return "Uyghur";
			case "00000843":
				return "Uzbek Cyrillic";
			case "0000042A":
				return "Vietnamese";
			case "00000488":
				return "Wolof";
			case "00000485":
				return "Yakut";
			case "0000046A":
				return "Yoruba";
			case "A0000409":
				return "Workman";
			case "A0010409":
				return "Colemak";
			}
			WindowsKeyboardLayoutDetection.Log.DebugFormat("Unknown id {0}", new object[]
			{
				text
			});
			return "Unknown";
		}

		private static readonly BitLogger Log = new BitLogger(typeof(WindowsKeyboardLayoutDetection));
	}
}
