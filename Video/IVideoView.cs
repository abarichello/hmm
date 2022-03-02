using System;
using HeavyMetalMachines.Presenting;
using UnityEngine.Video;

namespace HeavyMetalMachines.Video
{
	public interface IVideoView
	{
		ICanvasGroup Canvas { get; }

		void StopPlayer();

		void StartPlayer(VideoClip clip);
	}
}
