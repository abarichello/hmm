using System;
using System.Collections;
using HeavyMetalMachines.Battlepass;
using HeavyMetalMachines.Frontend;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.UnityUI
{
	public class UnityUiBattlepassLevelProgress : MonoBehaviour, IBattlepassLevelProgress
	{
		public void Setup(BattlepassViewData.BattlepassViewDataLevels dataLevels)
		{
			int num = dataLevels.CurrentLevel;
			num++;
			bool flag = num >= dataLevels.MaxLevels;
			int num2 = (!flag) ? dataLevels.MaxXpPerLevel[num] : dataLevels.CurrentXp;
			this._currentLevelText.text = num.ToString("0");
			this._nextLevelText.text = (num + 1).ToString("0");
			this._currentXp = dataLevels.CurrentXp;
			this.SetXpText(this._currentXp, num2);
			this._progressScrollbar.size = Mathf.Lerp(this._minProgressBarValue, this._maxProgressBarValue, (float)this._currentXp / (float)num2);
			this._xpInfoGameObject.SetActive(!flag);
			this._nextLevelGameObject.SetActive(!flag);
			this._progressScrollbar.gameObject.SetActive(!flag);
			this._maxLevelGameObject.SetActive(flag);
			this._boosterGameObject.SetActive(dataLevels.HasXpBooster);
			this._maxXpPerLevel = dataLevels.MaxXpPerLevel;
		}

		private void SetXpText(int currentXp, int maxXpForCurrentLevel)
		{
			this._xpText.text = string.Format("<color=#{0}>{1}</color> / {2}", HudUtils.RGBToHex(this._xpTextColor), currentXp, maxXpForCurrentLevel);
		}

		public void AnimateLevelUp(int targetLevel, float duration)
		{
			this._xpInfoGameObject.SetActive(true);
			this._nextLevelGameObject.SetActive(true);
			this._progressScrollbar.gameObject.SetActive(true);
			this._maxLevelGameObject.SetActive(false);
			base.StartCoroutine(this.ShowProgressBarAnimationCoroutine(targetLevel, this._currentXp, duration));
		}

		public IEnumerator ShowProgressBarAnimationCoroutine(int targetLevel, int startXp, float duration)
		{
			int maxXp = this._maxXpPerLevel[targetLevel];
			float remainingDuration = duration * ((float)startXp / (float)maxXp);
			float fillAllDuration = duration - remainingDuration;
			for (float timer = 0f; timer < fillAllDuration; timer += Time.deltaTime)
			{
				float normalizedTime = timer / duration;
				this._progressScrollbar.size = normalizedTime;
				float progressValue = Mathf.Lerp((float)startXp, (float)maxXp, normalizedTime);
				this.SetXpText((int)progressValue, maxXp);
				yield return null;
			}
			targetLevel++;
			this._currentLevelText.text = targetLevel.ToString("0");
			this._nextLevelText.text = (targetLevel + 1).ToString("0");
			if (targetLevel < this._maxXpPerLevel.Length)
			{
				if (startXp > 0)
				{
					for (float timer2 = 0f; timer2 < remainingDuration; timer2 += Time.deltaTime)
					{
						float normalizedTime2 = timer2 / duration;
						float progressValue2 = Mathf.Lerp(0f, (float)startXp, normalizedTime2);
						this._progressScrollbar.size = progressValue2 / (float)startXp;
						this.SetXpText((int)progressValue2, maxXp);
						yield return null;
					}
				}
				this._progressScrollbar.size = (float)startXp / (float)maxXp;
				this.SetXpText(startXp, maxXp);
			}
			else
			{
				this._progressScrollbar.size = 1f;
				this._xpInfoGameObject.SetActive(false);
				this._nextLevelGameObject.SetActive(false);
				this._progressScrollbar.gameObject.SetActive(false);
				this._maxLevelGameObject.SetActive(true);
			}
			yield break;
		}

		[Header("[Components]")]
		[SerializeField]
		private Text _currentLevelText;

		[SerializeField]
		private Text _nextLevelText;

		[SerializeField]
		private Text _xpText;

		[SerializeField]
		private Scrollbar _progressScrollbar;

		[SerializeField]
		private GameObject _xpInfoGameObject;

		[SerializeField]
		private GameObject _nextLevelGameObject;

		[SerializeField]
		private GameObject _maxLevelGameObject;

		[SerializeField]
		private GameObject _boosterGameObject;

		[Header("[Configs]")]
		[SerializeField]
		private Color _xpTextColor;

		[Range(0f, 1f)]
		[SerializeField]
		private float _minProgressBarValue;

		[Range(0f, 1f)]
		[SerializeField]
		private float _maxProgressBarValue = 1f;

		private int[] _maxXpPerLevel;

		private int _currentXp;
	}
}
