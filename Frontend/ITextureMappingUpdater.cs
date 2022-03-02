using System;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public interface ITextureMappingUpdater
	{
		void TryToLoadAsset(string textureName);

		void Stop();

		void StartAnimation();

		void UpdateUv(Rect uvRect);

		void ChangeVisibility(bool visibility);

		bool IsPlaying();
	}
}
