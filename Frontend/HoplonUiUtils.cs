using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HeavyMetalMachines.Frontend
{
	public static class HoplonUiUtils
	{
		public static void SetPreferedWidth(string text, Text uiText, float offset = 0f)
		{
			Vector2 sizeDelta = uiText.rectTransform.sizeDelta;
			sizeDelta.x = uiText.cachedTextGenerator.GetPreferredWidth(text, uiText.GetGenerationSettings(uiText.rectTransform.rect.size)) + offset;
			uiText.rectTransform.sizeDelta = sizeDelta;
		}

		public static void TryToAddEventTriggerEntry(EventTrigger eventTrigger, ref EventTrigger.Entry entry, EventTriggerType eventTriggerType, UnityAction<BaseEventData> call)
		{
			if (!eventTrigger.triggers.Contains(entry))
			{
				entry = new EventTrigger.Entry();
				entry.eventID = eventTriggerType;
				entry.callback.AddListener(call);
				eventTrigger.triggers.Add(entry);
			}
		}

		public static void SetAlpha(this Graphic graphic, float alpha)
		{
			Color color = graphic.color;
			color.a = alpha;
			graphic.color = color;
		}

		public static void EventSystemCancelSelection()
		{
			EventSystem current = EventSystem.current;
			ExecuteEvents.Execute<ICancelHandler>(current.currentSelectedGameObject, new BaseEventData(current), ExecuteEvents.cancelHandler);
		}

		public static Vector2 GetTextPreferedSize(string text, Text uiText)
		{
			Vector2 sizeDelta = uiText.rectTransform.sizeDelta;
			sizeDelta.x = uiText.cachedTextGenerator.GetPreferredWidth(text, uiText.GetGenerationSettings(uiText.rectTransform.rect.size));
			sizeDelta.y = uiText.cachedTextGenerator.GetPreferredHeight(text, uiText.GetGenerationSettings(uiText.rectTransform.rect.size));
			return sizeDelta;
		}
	}
}
