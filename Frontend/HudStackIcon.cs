using System;
using Hoplon.UserInterface;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class HudStackIcon : MonoBehaviour
	{
		public void PlayAnimation(string clipName)
		{
			HudStackIcon.Log.DebugFormat("Animation received: {0}", new object[]
			{
				clipName
			});
			if (!this._isLethal && !string.IsNullOrEmpty(clipName) && clipName.Contains("lethal"))
			{
				this.SetLethal(true);
			}
		}

		public void SetCounter(int count)
		{
			if (count == 0)
			{
				this.SetLethal(false);
				base.gameObject.SetActive(false);
				return;
			}
			base.gameObject.SetActive(true);
			this._text.Text = count.ToString();
		}

		private void SetLethal(bool value)
		{
			this._isLethal = value;
			this._icon.Sprite = ((!this._isLethal) ? this._normalIcon : this._lethalIcon);
		}

		private const string LethalToken = "lethal";

		private static readonly BitLogger Log = new BitLogger(typeof(HudStackIcon));

		[SerializeField]
		private HoplonText _text;

		[SerializeField]
		private HoplonImage _icon;

		[SerializeField]
		private Sprite _normalIcon;

		[SerializeField]
		private Sprite _lethalIcon;

		private bool _isLethal;
	}
}
