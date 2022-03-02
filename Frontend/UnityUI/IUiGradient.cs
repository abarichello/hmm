using System;
using UnityEngine;

namespace HeavyMetalMachines.Frontend.UnityUI
{
	public interface IUiGradient
	{
		void SetAlpha(float topAlpha, float bottomAlpha);

		void SetColors(Color topColor, Color bottomColor);
	}
}
