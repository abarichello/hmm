using System;
using UnityEngine;

namespace HeavyMetalMachines.GameCamera.Behaviour
{
	public interface IBombScoreCameraBehaviour
	{
		void LookAtExplosion(Transform explosionTransform);

		void FollowBomb(Transform bombTransform);

		void StopBehaviour();
	}
}
