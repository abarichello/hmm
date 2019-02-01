using System;
using System.Collections.Generic;
using HeavyMetalMachines.VFX;
using UnityEngine;

namespace HeavyMetalMachines.Utils
{
	public static class IconChatUtility
	{
		public static string ProcessMessage(string inputMessage, string iconCode, int iconWidth, UILabel label, List<ChatFilter.IconConfig> sprites)
		{
			label.UpdateNGUIText();
			string arg = IconChatUtility.CreateAStringWithSameSize(" ", iconWidth);
			string[] array = inputMessage.Split(new string[]
			{
				iconCode
			}, StringSplitOptions.None);
			string text = string.Empty;
			foreach (string arg2 in array)
			{
				text = string.Format("{0}{1}", text, arg2);
				text = string.Format("{0}{1}", text, arg);
			}
			return text;
		}

		public static float SetIconPosition(string linePiece, int iconWidth, UILabel label, UI2DSprite sprite, float lineSizeAlreadyProcessed)
		{
			label.UpdateNGUIText();
			Vector2 vector = NGUIText.CalculatePrintedSize(linePiece);
			int num = label.fontSize + label.spacingY;
			int num2 = label.height / num - 1;
			float result = lineSizeAlreadyProcessed + vector.x + (float)iconWidth;
			int topPosition = num * num2;
			int leftPosition = (int)(lineSizeAlreadyProcessed + vector.x);
			IconChatUtility.SetSpriteAnchorProperties(sprite, label, topPosition, leftPosition, iconWidth);
			return result;
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

		public static void SetSpriteAnchorProperties(UI2DSprite sprite, UILabel label, int topPosition, int leftPosition, int iconWidth)
		{
			sprite.leftAnchor.target = label.transform;
			sprite.topAnchor.absolute = -topPosition;
			sprite.bottomAnchor.absolute = -topPosition - iconWidth;
			sprite.leftAnchor.absolute = leftPosition;
			sprite.rightAnchor.absolute = leftPosition + iconWidth;
		}
	}
}
