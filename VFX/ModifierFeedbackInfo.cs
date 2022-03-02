using System;
using System.Collections.Generic;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Localization;
using Hoplon.Localization.TranslationTable;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class ModifierFeedbackInfo : GameHubScriptableObject, IContent
	{
		public int ContentId
		{
			get
			{
				return this.ModifierFeedbackId;
			}
			set
			{
				this.ModifierFeedbackId = value;
			}
		}

		private void CheckTranslationSheet()
		{
			if (this._translationContext != TranslationContext.All)
			{
				return;
			}
			this._translationContext = ((!this.DraftTooltipTittle.StartsWith("SPONSOR")) ? TranslationContext.CharactersMatchInfo : TranslationContext.Sponsors);
		}

		public string LocalizedTooltipTittle
		{
			get
			{
				this.CheckTranslationSheet();
				return Language.Get(this.DraftTooltipTittle, this._translationContext);
			}
		}

		public string LocalizedTooltipDesciption
		{
			get
			{
				this.CheckTranslationSheet();
				return Language.Get(this.DraftTooltipDesciption, this._translationContext);
			}
		}

		private void OnEnable()
		{
			if (this.LifeTime <= 0f)
			{
				this.LifeTime = 1f;
			}
			ModifierFeedbackInfo.Feedbacks[this.ModifierFeedbackId] = this;
		}

		private void OnDisable()
		{
			ModifierFeedbackInfo.Feedbacks.Remove(this.ModifierFeedbackId);
		}

		public static readonly Dictionary<int, ModifierFeedbackInfo> Feedbacks = new Dictionary<int, ModifierFeedbackInfo>();

		[ScriptId]
		public int ModifierFeedbackId;

		public string Name;

		public CDummy.DummyKind Dummy;

		public string TooltipTittle;

		public string TooltipDesciption;

		public string DraftTooltipTittle;

		public string DraftTooltipDesciption;

		private ContextTag _translationContext = TranslationContext.All;

		public int EffectPreCacheCount = 1;

		[HideInInspector]
		public float LifeTime = 1f;
	}
}
