using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Render
{
	public class AnimationDisableRenderer : MonoBehaviour
	{
		private void OnEnable()
		{
			this.CheckForNullRenderers();
			if (this._showRenderersOnEnable)
			{
				this.ShowRenderers();
			}
		}

		private bool IsRendererListEmpty()
		{
			return this._renderers == null || this._renderers.Length == 0;
		}

		private void CheckForNullRenderers()
		{
			if (this.IsRendererListEmpty())
			{
				return;
			}
			for (int i = 0; i < this._renderers.Length; i++)
			{
				if (this._renderers[i] == null)
				{
					AnimationDisableRenderer.Log.ErrorFormat("ART - Null renderer set on {0}. Index {1}", new object[]
					{
						base.gameObject.name,
						i
					});
				}
			}
		}

		private void SetRenderersEnable(bool enable)
		{
			if (this.IsRendererListEmpty())
			{
				return;
			}
			for (int i = 0; i < this._renderers.Length; i++)
			{
				this._renderers[i].enabled = enable;
			}
		}

		public void HideRenderers()
		{
			this.SetRenderersEnable(false);
		}

		public void ShowRenderers()
		{
			this.SetRenderersEnable(true);
		}

		private static readonly BitLogger Log = new BitLogger(typeof(AnimationDisableRenderer));

		[SerializeField]
		private bool _showRenderersOnEnable = true;

		[SerializeField]
		private Renderer[] _renderers;
	}
}
