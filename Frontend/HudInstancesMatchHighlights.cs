using System;
using System.Collections;
using System.Collections.Generic;
using FMod;
using HeavyMetalMachines.Bank;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Utils;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class HudInstancesMatchHighlights : GameHubBehaviour
	{
		public void Init()
		{
			GUIUtils.CreateGridPool(this.PlayersGrid, this.PlayersGrid.maxPerLine);
			this.PlayersGrid.hideInactive = false;
			List<Transform> childList = this.PlayersGrid.GetChildList();
			for (int i = 0; i < childList.Count; i++)
			{
				childList[i].transform.name = "player" + i;
				childList[i].gameObject.SetActive(false);
			}
			this.PlayersGrid.hideInactive = true;
			base.gameObject.SetActive(false);
			this._topDamagePlayerIndex = -1;
			this._topRepairPlayerIndex = -1;
			this._topDebufPlayerIndex = -1;
			this._topBombPossessionPlayerIndex = -1;
			this._didAnimation = false;
		}

		public void Hide()
		{
			base.gameObject.SetActive(false);
			if (!HudInstancesController.IsInShopState())
			{
				this._didAnimation = false;
			}
		}

		public void HideAnimating()
		{
			if (!base.gameObject.activeInHierarchy)
			{
				return;
			}
			base.StartCoroutine(this.AnimateHideCoroutine());
		}

		private IEnumerator AnimateHideCoroutine()
		{
			this.GridOutAnimation.Play();
			this.TitleOutAnimation.Play();
			while (this.GridOutAnimation.isPlaying)
			{
				yield return null;
				if (!base.gameObject.activeInHierarchy)
				{
					yield break;
				}
			}
			this.Hide();
			yield break;
		}

		public void Show()
		{
			base.gameObject.SetActive(true);
			this.TitleInAnimation.GetComponent<NGUIWidgetAlpha>().alpha = ((!this._didAnimation) ? 0f : 1f);
			this.TitleOutAnimation.GetComponent<NGUIWidgetAlpha>().alpha = 1f;
			Vector3 localPosition = this.TitleOutAnimation.transform.localPosition;
			localPosition.x = 0f;
			this.TitleOutAnimation.transform.localPosition = localPosition;
			this.PlayersGrid.GetComponent<NGUIWidgetAlpha>().alpha = 1f;
			Vector3 localPosition2 = this.PlayersGrid.transform.localPosition;
			localPosition2.x = 0f;
			this.PlayersGrid.transform.localPosition = localPosition2;
			this.PlayersGrid.hideInactive = false;
			List<Transform> childList = this.PlayersGrid.GetChildList();
			int num = -1;
			int num2 = -1;
			int num3 = -1;
			int num4 = -1;
			float num5 = 0f;
			float num6 = 0f;
			float num7 = 0f;
			float num8 = 0f;
			List<PlayerData> playersAndBots = GameHubBehaviour.Hub.Players.PlayersAndBots;
			for (int i = 0; i < playersAndBots.Count; i++)
			{
				PlayerStats bitComponent = playersAndBots[i].CharacterInstance.GetBitComponent<PlayerStats>();
				if (bitComponent.DamageDealtToPlayers > num5)
				{
					num5 = bitComponent.DamageDealtToPlayers;
					num = i;
				}
				if (bitComponent.HealingProvided > num6)
				{
					num6 = bitComponent.HealingProvided;
					num2 = i;
				}
				if (bitComponent.DebuffTime > num7)
				{
					num7 = bitComponent.DebuffTime;
					num3 = i;
				}
				if (bitComponent.BombPossessionTime > num8)
				{
					num8 = bitComponent.BombPossessionTime;
					num4 = i;
				}
			}
			for (int j = 0; j < childList.Count; j++)
			{
				HudInstancesMatchHighlightsPlayer component = childList[j].GetComponent<HudInstancesMatchHighlightsPlayer>();
				component.GetComponent<NGUIWidgetAlpha>().alpha = ((!this._didAnimation) ? 0f : 1f);
				if (num != -1)
				{
					this.SetupHighlightsPlayer(component, this._topDamagePlayerIndex, num, "INFO_TEAMS_BESTPLAYER_DAMAGE_TITTLE", num5, false, this.DamageSprite);
					this._topDamagePlayerIndex = num;
					num = -1;
				}
				else if (num2 != -1)
				{
					this.SetupHighlightsPlayer(component, this._topRepairPlayerIndex, num2, "INFO_TEAMS_BESTPLAYER_REPAIR_TITTLE", num6, false, this.RepairSprite);
					this._topRepairPlayerIndex = num2;
					num2 = -1;
				}
				else if (num3 != -1)
				{
					this.SetupHighlightsPlayer(component, this._topDebufPlayerIndex, num3, "INFO_TEAMS_BESTPLAYER_DEBUFFTIME_TITTLE", num7, true, this.DebuffSprite);
					this._topDebufPlayerIndex = num3;
					num3 = -1;
				}
				else if (num4 != -1)
				{
					this.SetupHighlightsPlayer(component, this._topBombPossessionPlayerIndex, num4, "INFO_TEAMS_BESTPLAYER_BOMBTIME_TITTLE", num8, true, this.BombPossessionSprite);
					this._topBombPossessionPlayerIndex = num4;
					num4 = -1;
				}
				else
				{
					component.gameObject.SetActive(false);
				}
			}
			this.PlayersGrid.hideInactive = true;
			this.PlayersGrid.Reposition();
			if (!this._didAnimation)
			{
				this._didAnimation = true;
				this.TitleInAnimation.Play();
				base.StartCoroutine(this.AnimateCardsCoroutine(childList));
			}
		}

		private void SetupHighlightsPlayer(HudInstancesMatchHighlightsPlayer highlightsPlayer, int currentPlayerIndex, int newPlayerIndex, string titleDraft, float value, bool isValueDateTime, Sprite roleSprite)
		{
			PlayerData playerData = GameHubBehaviour.Hub.Players.PlayersAndBots[newPlayerIndex];
			bool isReplace = !this._didAnimation && currentPlayerIndex != -1 && currentPlayerIndex != newPlayerIndex;
			string value2;
			if (isValueDateTime)
			{
				DateTime dateTime = default(DateTime).AddSeconds((double)value);
				value2 = dateTime.Minute.ToString("00") + ":" + dateTime.Second.ToString("00");
			}
			else
			{
				value2 = string.Format("{0:0}", value);
			}
			Color playerColor = (GameHubBehaviour.Hub.Players.CurrentPlayerData.Team != playerData.Team) ? GUIColorsInfo.Instance.RedTeamColor : GUIColorsInfo.Instance.BlueTeamColor;
			highlightsPlayer.Setup(Language.Get(titleDraft, TranslationSheets.MatchEndScreen), value2, playerData.Name, playerColor, playerData.Character.Asset + "_portrait_victory_00", roleSprite, isReplace);
			highlightsPlayer.gameObject.SetActive(true);
		}

		private IEnumerator AnimateCardsCoroutine(List<Transform> gridList)
		{
			yield return new WaitForSeconds(this.AnimationInStartDelayTimeInSec);
			if (!base.gameObject.activeSelf)
			{
				yield break;
			}
			for (int i = 0; i < gridList.Count; i++)
			{
				Transform gridListTransform = gridList[i];
				if (gridListTransform.gameObject.activeSelf)
				{
					gridListTransform.GetComponent<HudInstancesMatchHighlightsPlayer>().PlayInAnimation();
					yield return new WaitForSeconds(this.AnimationInCardsIntervalTimeSec);
					if (!base.gameObject.activeSelf)
					{
						yield break;
					}
				}
			}
			yield return new WaitForSeconds(this.AnimationInCardsPreReplaceTimeSec);
			if (!base.gameObject.activeSelf)
			{
				yield break;
			}
			for (int j = 0; j < gridList.Count; j++)
			{
				Transform gridListTransform2 = gridList[j];
				if (gridListTransform2.gameObject.activeSelf)
				{
					if (gridListTransform2.GetComponent<HudInstancesMatchHighlightsPlayer>().TryToPlayReplaceAnimation())
					{
						FMODAudioManager.PlayOneShotAt(this._highlightReplaceAudio, Vector3.zero, 0);
						yield return new WaitForSeconds(this.AnimationInCardsIntervalTimeSec);
						if (!base.gameObject.activeSelf)
						{
							yield break;
						}
					}
				}
			}
			yield break;
		}

		public UIGrid PlayersGrid;

		public Sprite DamageSprite;

		public Sprite RepairSprite;

		public Sprite DebuffSprite;

		public Sprite BombPossessionSprite;

		[Header("[Animation Config]")]
		public float AnimationInStartDelayTimeInSec = 1f;

		public float AnimationInCardsIntervalTimeSec = 0.25f;

		public float AnimationInCardsPreReplaceTimeSec = 0.5f;

		[SerializeField]
		private Animation TitleInAnimation;

		[SerializeField]
		private Animation TitleOutAnimation;

		[SerializeField]
		private Animation GridOutAnimation;

		private int _topDamagePlayerIndex;

		private int _topRepairPlayerIndex;

		private int _topDebufPlayerIndex;

		private int _topBombPossessionPlayerIndex;

		private bool _didAnimation;

		[SerializeField]
		private FMODAsset _highlightReplaceAudio;
	}
}
