using System;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	[AddComponentMenu("NGUI/Interaction/HMM Play Tween")]
	[ExecuteInEditMode]
	public class HMMUIPlayTween : UIPlayTween
	{
		public void Play()
		{
			if (!Application.isPlaying)
			{
				Debug.LogError("Only work in play mode");
				return;
			}
			base.Play(true);
			if (!this.tweenTarget)
			{
				return;
			}
			if (this._tweens == null || this._tweens.Length == 0)
			{
				this._tweens = this.tweenTarget.GetComponentsInChildren<UITweener>();
			}
			for (int i = 0; i < this._tweens.Length; i++)
			{
				UITweener tween = this._tweens[i];
				if (tween.tweenGroup == this.tweenGroup)
				{
					tween.style = UITweener.Style.Once;
					tween.AddOnFinished(delegate()
					{
						UIPlayTween.RemoveUITweener(tween);
					});
					UIPlayTween.AddUITweener(tween);
				}
			}
		}

		private UITweener[] _tweens;
	}
}
