using System;
using System.Collections.Generic;
using System.Text;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class ChatFilter : GameHubBehaviour
	{
		private void Start()
		{
			for (int i = 0; i < 8; i++)
			{
				this.InstantiateIcon();
			}
		}

		private void OnDestroy()
		{
			this._sprites.Clear();
		}

		private void InstantiateIcon()
		{
			GameObject gameObject = Object.Instantiate<GameObject>(this.IconPrefab.gameObject);
			gameObject.transform.parent = this.IconsParent.transform;
			gameObject.transform.position = Vector3.zero;
			gameObject.transform.localScale = Vector3.one;
			gameObject.gameObject.SetActive(false);
			this._sprites.Push(gameObject.GetComponent<UI2DSprite>());
		}

		public string OnMessageReceived(string inputMessage, ref List<ChatFilter.ActiveIcons> activeIcons)
		{
			string text = ChatFilter.ProcessText(inputMessage, this.ClampedChatLabel).ToString();
			string[] array = this.ChatLabel.processedText.Split(new char[]
			{
				'\n'
			});
			string[] array2 = text.Split(new char[]
			{
				'\n'
			});
			int num = array.Length - 1;
			for (int i = 0; i < array2.Length; i++)
			{
				string text2 = array2[i];
				string text3 = array2[i];
				string text4 = array2[i];
				for (int j = 0; j < this.Icons.Count; j++)
				{
					ChatFilter.IconConfig iconConfig = this.Icons[j];
					if (text2.Contains(iconConfig.IconCode))
					{
						UI2DSprite sprite = this.GetSprite();
						text4 = ChatFilter.ProcessIcon(text3, num + i, this.ChatLabel, iconConfig, sprite);
						ChatFilter.ActiveIcons activeIcons2 = new ChatFilter.ActiveIcons();
						activeIcons2.Index = num + i;
						activeIcons2.Sprite = sprite;
						activeIcons2.IconConfig = iconConfig;
						activeIcons2.IconXPos = sprite.leftAnchor.absolute;
						activeIcons.Add(activeIcons2);
						text = text.Replace(text3, text4);
						text3 = text4;
					}
				}
			}
			return text;
		}

		private UI2DSprite GetSprite()
		{
			if (this._sprites.Count <= 0)
			{
				this.InstantiateIcon();
			}
			UI2DSprite ui2DSprite = this._sprites.Pop();
			ui2DSprite.gameObject.SetActive(true);
			return ui2DSprite;
		}

		private void ReturnSprite(UI2DSprite sprite)
		{
			sprite.gameObject.SetActive(false);
			this._sprites.Push(sprite);
		}

		public void OnLineRemoved(int lineIndex, ref List<ChatFilter.ActiveIcons> activeIcons)
		{
			for (int i = 0; i < activeIcons.Count; i++)
			{
				ChatFilter.ActiveIcons activeIcons2 = activeIcons[i];
				if (activeIcons2.Index == lineIndex)
				{
					this.ReturnSprite(activeIcons2.Sprite);
					activeIcons.RemoveAt(i);
					i--;
				}
			}
		}

		public void UpdateIconsPosition(int iconIndexModification, ref List<ChatFilter.ActiveIcons> activeIcons, bool isCurrentActiveTab)
		{
			int num = this.ChatLabel.fontSize + this.ChatLabel.spacingY;
			for (int i = 0; i < activeIcons.Count; i++)
			{
				ChatFilter.ActiveIcons activeIcons2 = activeIcons[i];
				activeIcons2.Index += iconIndexModification;
				int topPosition = num * activeIcons2.Index;
				int iconXPos = activeIcons2.IconXPos;
				if (activeIcons2.Sprite == null)
				{
					activeIcons2.Sprite = this.GetSprite();
				}
				ChatFilter.SetSpriteAnchorProperties(activeIcons2.Sprite, this.ChatLabel, topPosition, iconXPos, activeIcons2.IconConfig);
				activeIcons[i] = activeIcons2;
			}
			if (!isCurrentActiveTab)
			{
				this.Clear(ref activeIcons);
			}
		}

		public void Clear(ref List<ChatFilter.ActiveIcons> activeIcons)
		{
			for (int i = 0; i < activeIcons.Count; i++)
			{
				ChatFilter.ActiveIcons activeIcons2 = activeIcons[i];
				if (!(activeIcons2.Sprite == null))
				{
					this.ReturnSprite(activeIcons2.Sprite);
					activeIcons2.Sprite = null;
				}
			}
		}

		public static void ClearStringBuilder(StringBuilder value)
		{
			value.Length = 0;
		}

		private static StringBuilder ProcessText(string inputMessage, UILabel clampedLabel)
		{
			string text = string.Empty;
			ChatFilter.ClearStringBuilder(ChatFilter.Sb);
			for (;;)
			{
				inputMessage = inputMessage.Substring(text.Length).TrimStart(new char[0]);
				if (string.IsNullOrEmpty(inputMessage))
				{
					break;
				}
				clampedLabel.text = inputMessage;
				clampedLabel.TryUpdateText();
				text = clampedLabel.processedText;
				bool flag = text != inputMessage;
				int num = text.LastIndexOf(' ');
				if (flag && num != -1)
				{
					text = text.Remove(num);
				}
				if (flag)
				{
					ChatFilter.Sb.Append(text);
					ChatFilter.Sb.Append("\r\n");
				}
				else
				{
					ChatFilter.Sb.Append(text);
					ChatFilter.Sb.Append(" ");
				}
				if (!flag)
				{
					goto Block_5;
				}
			}
			return ChatFilter.Sb;
			Block_5:
			return ChatFilter.Sb;
		}

		public static string ProcessIcon(string inputMessage, int labelLinesCount, UILabel label, ChatFilter.IconConfig iconConfig, UI2DSprite sprite)
		{
			label.UpdateNGUIText();
			string arg = string.Empty;
			if (!iconConfig.IsBackground)
			{
				arg = ChatFilter.CreateAStringWithSameSize(" ", iconConfig.IconWidth);
			}
			float num = 0f;
			string[] array = inputMessage.Split(new string[]
			{
				iconConfig.IconCode
			}, StringSplitOptions.None);
			string text = string.Empty;
			for (int i = 0; i < array.Length; i++)
			{
				string text2 = array[i];
				text = string.Format("{0}{1}", text, text2);
				if (i + 1 < array.Length)
				{
					num += ChatFilter.SetIconPosition(text2, label, sprite, iconConfig, num, labelLinesCount);
				}
				text = string.Format("{0}{1}", text, arg);
			}
			return text;
		}

		public static float SetIconPosition(string linePiece, UILabel label, UI2DSprite sprite, ChatFilter.IconConfig iconConfig, float lineSizeAlreadyProcessed, int lineNumber)
		{
			label.UpdateNGUIText();
			Vector2 vector = NGUIText.CalculatePrintedSize(linePiece);
			int num = label.fontSize + label.spacingY;
			float result = lineSizeAlreadyProcessed + vector.x + (float)iconConfig.IconWidth;
			int topPosition = num * lineNumber;
			int leftPosition = (int)(lineSizeAlreadyProcessed + vector.x);
			ChatFilter.SetSpriteAnchorProperties(sprite, label, topPosition, leftPosition, iconConfig);
			return result;
		}

		public static void SetSpriteAnchorProperties(UI2DSprite sprite, UILabel label, int topPosition, int leftPosition, ChatFilter.IconConfig iconConfig)
		{
			sprite.sprite2D = iconConfig.IconSprite;
			sprite.color = iconConfig.ColorTint;
			sprite.leftAnchor.target = label.transform;
			if (iconConfig.IgnoreInitialXPosition)
			{
				leftPosition = 0;
			}
			if (iconConfig.IgnoreInitialYPosition)
			{
				topPosition = 0;
			}
			topPosition += iconConfig.YDiff;
			leftPosition += iconConfig.XDiff;
			sprite.topAnchor.absolute = -topPosition;
			sprite.bottomAnchor.absolute = -topPosition - iconConfig.IconHeight;
			sprite.leftAnchor.absolute = leftPosition;
			sprite.rightAnchor.absolute = leftPosition + iconConfig.IconWidth;
		}

		private static string CreateAStringWithSameSize(string characterToFill, int sizeToReach)
		{
			Vector2 vector = NGUIText.CalculatePrintedSize(characterToFill);
			int num = Mathf.CeilToInt((float)sizeToReach / vector.x);
			string text = string.Empty;
			for (int i = 0; i < num; i++)
			{
				text = string.Format("{0}{1}", text, characterToFill);
			}
			return text;
		}

		public UILabel ChatLabel;

		public UILabel ClampedChatLabel;

		public UI2DSprite IconPrefab;

		public GameObject IconsParent;

		private readonly Stack<UI2DSprite> _sprites = new Stack<UI2DSprite>(8);

		private static readonly StringBuilder Sb = new StringBuilder();

		public List<ChatFilter.IconConfig> Icons;

		[Serializable]
		public struct IconConfig
		{
			public string IconCode;

			public Sprite IconSprite;

			public int IconWidth;

			public int IconHeight;

			public bool IgnoreInitialXPosition;

			public bool IgnoreInitialYPosition;

			public int XDiff;

			public int YDiff;

			public bool IsBackground;

			public Color ColorTint;
		}

		public class ActiveIcons
		{
			public int Index;

			public UI2DSprite Sprite;

			public int IconXPos;

			public ChatFilter.IconConfig IconConfig;
		}
	}
}
