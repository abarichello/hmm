using System;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public interface IPerkWithCollision
	{
		int Priority();

		[Obsolete]
		void OnHit(Collider other, Vector3 hitPoint, Vector3 hitNormal, bool isOverlapping, bool isBarrier);

		void OnStay(Collider other, Vector3 hitPoint, Vector3 hitNormal, bool isOverlapping, bool isBarrier);

		void OnEnter(Collider other, Vector3 hitPoint, Vector3 hitNormal, bool isOverlapping, bool isBarrier);

		void OnExit(Collider other, Vector3 hitPoint, Vector3 hitNormal, bool isOverlapping, bool isBarrier);
	}
}
