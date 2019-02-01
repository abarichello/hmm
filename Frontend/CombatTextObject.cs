using System;
using System.Collections;
using System.Text;
using Pocketverse;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.Frontend
{
	public class CombatTextObject : GameHubBehaviour
	{
		protected void Awake()
		{
			if (CombatTextObject._combatTextStringBuilder == null)
			{
				CombatTextObject._combatTextStringBuilder = new StringBuilder(10);
			}
		}

		protected void OnDestroy()
		{
			CombatTextObject._combatTextStringBuilder = null;
		}

		public void ShowCombatText(CombatTextType combatTextType, float value, Transform target, AnimationClip animationClip, Color color, float stackTimeInSec)
		{
			this._value = value;
			this._startValueScaleModifier = this.GetValueScaleModifier(this._value);
			this._passedTopAnimationCurve = false;
			this._pauseOnTopAnimationCurve = false;
			this._combatTextType = combatTextType;
			this._stackTimeInSec = stackTimeInSec;
			base.gameObject.SetActive(true);
			this.InfoText.text = CombatTextObject.ParseValue(combatTextType, this._value);
			this.InfoText.color = color;
			this.FollowTarget.SetTargetTransform(target);
			this.CurrentAnimation.clip = animationClip;
			this.CurrentAnimation.AddClip(animationClip, animationClip.name);
			this.CurrentAnimation.Play();
		}

		public CombatTextType GetCombatTextType()
		{
			return this._combatTextType;
		}

		public void Pause(bool pause)
		{
			base.StopCoroutine(this.AnimatePulseText(this.PulseTimeInSec));
			if (pause)
			{
				IEnumerator enumerator = this.CurrentAnimation.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						object obj = enumerator.Current;
						AnimationState animationState = (AnimationState)obj;
						if (animationState.clip == this.CurrentAnimation.clip)
						{
							this._pauseNormalizedTime = animationState.normalizedTime;
							this._pausedTextLocalScale = this.InfoText.rectTransform.localScale;
							break;
						}
					}
				}
				finally
				{
					IDisposable disposable;
					if ((disposable = (enumerator as IDisposable)) != null)
					{
						disposable.Dispose();
					}
				}
				this.CurrentAnimation.Stop();
			}
			else
			{
				IEnumerator enumerator2 = this.CurrentAnimation.GetEnumerator();
				try
				{
					while (enumerator2.MoveNext())
					{
						object obj2 = enumerator2.Current;
						AnimationState animationState2 = (AnimationState)obj2;
						if (animationState2.clip == this.CurrentAnimation.clip)
						{
							animationState2.normalizedTime = this._pauseNormalizedTime;
							break;
						}
					}
				}
				finally
				{
					IDisposable disposable2;
					if ((disposable2 = (enumerator2 as IDisposable)) != null)
					{
						disposable2.Dispose();
					}
				}
				this.CurrentAnimation.Play();
			}
		}

		public bool PassedTopAnimationCurve()
		{
			return this._passedTopAnimationCurve;
		}

		public void SetToPauseOnTopAnimationCurve(float value)
		{
			this._value += value;
			this.InfoText.text = CombatTextObject.ParseValue(this._combatTextType, this._value);
			this._pauseTimeInSec = this._stackTimeInSec;
			if (!this.CurrentAnimation.isPlaying)
			{
				base.StopCoroutine(this.AnimatePulseText(this.PulseTimeInSec));
				base.StartCoroutine(this.AnimatePulseText(this.PulseTimeInSec));
			}
			this._pauseOnTopAnimationCurve = true;
		}

		private IEnumerator AnimatePulseText(float timeInSec)
		{
			float halfTotalTime = timeInSec * 0.5f;
			RectTransform textTransform = this.InfoText.rectTransform;
			float deltaTime = 0f;
			float scaleModifier = 1f + (this.GetValueScaleModifier(this._value) - this._startValueScaleModifier);
			Vector3 baseLocalScale = this._pausedTextLocalScale * scaleModifier;
			Vector3 increaseLocalScale = new Vector3(0.2f, 0.2f, 0.2f);
			Vector3 newLocalScale = default(Vector3);
			while (timeInSec > 0f)
			{
				timeInSec -= Time.deltaTime;
				if (timeInSec > halfTotalTime)
				{
					deltaTime += Time.deltaTime;
				}
				else
				{
					deltaTime -= Time.deltaTime;
				}
				float deltaTimeNormalized = deltaTime / halfTotalTime;
				newLocalScale.x = baseLocalScale.x + increaseLocalScale.x * deltaTimeNormalized;
				newLocalScale.y = baseLocalScale.x + increaseLocalScale.y * deltaTimeNormalized;
				newLocalScale.z = baseLocalScale.z + increaseLocalScale.z * deltaTimeNormalized;
				textTransform.localScale = newLocalScale;
				yield return null;
			}
			this.InfoText.rectTransform.localScale = this._pausedTextLocalScale * (1f + (this.GetValueScaleModifier(this._value) - this._startValueScaleModifier));
			yield break;
		}

		private float GetValueScaleModifier(float value)
		{
			for (int i = 0; i < this.CombatTextSettings.CombatPulseInfoList.Length; i++)
			{
				CombatPulseInfo combatPulseInfo = this.CombatTextSettings.CombatPulseInfoList[i];
				if (value >= (float)combatPulseInfo.MinValue && value <= (float)combatPulseInfo.MaxValue)
				{
					return combatPulseInfo.ScaleModifier;
				}
			}
			return 1f;
		}

		private static string ParseValue(CombatTextType combatTextType, float value)
		{
			CombatTextObject._combatTextStringBuilder.Length = 0;
			char value2 = ' ';
			if (CombatTextObject.TryToGetSign(combatTextType, ref value2))
			{
				CombatTextObject._combatTextStringBuilder.Append(value2);
			}
			int num = Mathf.RoundToInt(value);
			if (num == 0)
			{
				num = 1;
			}
			CombatTextObject._combatTextStringBuilder.Append(num);
			return CombatTextObject._combatTextStringBuilder.ToString();
		}

		private static bool TryToGetSign(CombatTextType type, ref char sign)
		{
			if (type != CombatTextType.RepairSelf && type != CombatTextType.RepairAlly)
			{
				return false;
			}
			sign = '+';
			return true;
		}

		public void OnAnimationTop()
		{
			if (this._pauseOnTopAnimationCurve)
			{
				this.Pause(true);
				base.StartCoroutine(this.PausedCoroutine());
				return;
			}
			this._passedTopAnimationCurve = true;
		}

		private IEnumerator PausedCoroutine()
		{
			while (this._pauseTimeInSec > 0f)
			{
				this._pauseTimeInSec -= Time.deltaTime;
				yield return null;
			}
			this._passedTopAnimationCurve = true;
			this.Pause(false);
			yield break;
		}

		public void OnAnimationFinish()
		{
			base.gameObject.SetActive(false);
			this.FollowTarget.SetTargetTransform(null);
			base.transform.localScale = Vector3.one;
			this.InfoText.rectTransform.localScale = Vector3.one;
			this.CanvasGroup.alpha = 1f;
			this._passedTopAnimationCurve = false;
			this._pauseOnTopAnimationCurve = false;
			base.StopCoroutine(this.AnimatePulseText(this.PulseTimeInSec));
		}

		private const int CombatTextStringBuilderCapacity = 10;

		public CombatTextSettings CombatTextSettings;

		public float PulseTimeInSec = 0.2f;

		public Text InfoText;

		public CombatTextFollowTarget FollowTarget;

		public Animation CurrentAnimation;

		public CanvasGroup CanvasGroup;

		private CombatTextType _combatTextType;

		private bool _passedTopAnimationCurve;

		private bool _pauseOnTopAnimationCurve;

		private float _pauseTimeInSec;

		private float _stackTimeInSec;

		private float _value;

		private float _startValueScaleModifier;

		private static StringBuilder _combatTextStringBuilder;

		private float _pauseNormalizedTime;

		private Vector3 _pausedTextLocalScale;
	}
}
