using System;

namespace HeavyMetalMachines.Frontend
{
	public class UnityEditorAnimatedRawImage : AnimatedRawImage
	{
		protected override void OnEnable()
		{
			base.OnEnable();
			if (null != base.texture)
			{
				base.InitializeAnimator();
				base.StartAnimation();
			}
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			if (null != base.texture)
			{
				base.Stop();
			}
		}
	}
}
