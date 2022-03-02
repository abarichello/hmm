using System;
using HeavyMetalMachines.Utils;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class GameModesHoverView : MonoBehaviour
	{
		public void OnPvpButtonHoverOver()
		{
			GUIUtils.PlayAnimation(this._pvpArtAnimation, false, 1f, string.Empty);
		}

		public void OnPvpButtonHoverOut()
		{
			GUIUtils.PlayAnimation(this._pvpArtAnimation, true, 1f, string.Empty);
		}

		public void OnCustomMatchButtonHoverOver()
		{
			GUIUtils.PlayAnimation(this._customMatchArtAnimation, false, 1f, string.Empty);
		}

		public void OnCustomMatchButtonHoverOut()
		{
			GUIUtils.PlayAnimation(this._customMatchArtAnimation, true, 1f, string.Empty);
		}

		public void AnimateIn(Animation animation)
		{
			GUIUtils.PlayAnimation(animation, false, 1f, string.Empty);
		}

		public void AnimateOut(Animation animation)
		{
			GUIUtils.PlayAnimation(animation, true, 1f, string.Empty);
		}

		[SerializeField]
		private Animation _pvpArtAnimation;

		[SerializeField]
		private Animation _customMatchArtAnimation;
	}
}
