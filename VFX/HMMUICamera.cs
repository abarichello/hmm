using System;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class HMMUICamera : UICamera
	{
		public override void ProcessTouch(bool pressed, bool unpressed)
		{
			if (UICamera.currentTouchID == -2)
			{
				if (pressed)
				{
					UICamera.currentTouch.pressStarted = true;
					UICamera.Notify(UICamera.currentTouch.pressed, "OnRightPress", false);
					UICamera.currentTouch.pressed = UICamera.currentTouch.current;
					UICamera.currentTouch.dragged = UICamera.currentTouch.current;
					UICamera.currentTouch.clickNotification = UICamera.ClickNotification.BasedOnDelta;
					UICamera.currentTouch.totalDelta = Vector2.zero;
					UICamera.currentTouch.dragStarted = false;
					UICamera.Notify(UICamera.currentTouch.pressed, "OnRightPress", true);
				}
				else if (unpressed && UICamera.currentTouch.clickNotification != UICamera.ClickNotification.None)
				{
					float time = RealTime.time;
					UICamera.Notify(UICamera.currentTouch.pressed, "OnRightClick", null);
					if (UICamera.currentTouch.clickTime + 0.35f > time)
					{
						UICamera.Notify(UICamera.currentTouch.pressed, "OnDoubleRightClick", null);
					}
					UICamera.currentTouch.clickTime = time;
				}
				return;
			}
			base.ProcessTouch(pressed, unpressed);
		}
	}
}
