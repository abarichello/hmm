using System;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class MortarEffectInfo : GadgetInfo
	{
		public override Type GadgetType()
		{
			return typeof(MortarEffect);
		}

		public CDummy.DummyKind MortarDummy;

		public FXInfo MortarEffect;

		public float MortarMoveSpeed;

		public float MortarRange;

		public bool MortarUseMoveSpeed;

		public Texture CursorAreaTexture;

		public Texture CursorTargetTexture;

		public float CursorTargetSize;
	}
}
