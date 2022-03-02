using System;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Body
{
	public struct GadgetBodyCreation
	{
		public IHMMEventContext HmmEventContext;

		public IHMMGadgetContext HmmGadgetContext;

		public BaseParameter BodyParameter;

		public Vector3Parameter FinalPositionParameter;

		public Vector3Parameter FinalDirectionParameter;

		public CDummy.DummyKind DummyKind;

		public string CustomDummyName;

		public Transform OwnerTransform;

		public Transform ParentTransform;

		public BaseParameter DirectionParameter;

		public bool UseRelativeDirection;

		public BaseParameter PositionParameter;

		public bool UsePositionAsOffset;
	}
}
