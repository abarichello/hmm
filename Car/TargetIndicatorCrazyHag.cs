using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Utils;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Car
{
	public class TargetIndicatorCrazyHag : TargetIndicator
	{
		private void Awake()
		{
			this._mPoTimedUpdater = new TimedUpdater(100, true, false);
		}

		protected override void PostInitPlayers()
		{
			base.PostInitPlayers();
			if (GameHubBehaviour.Hub.Net.IsServer() || !this.m_poCurrentPlayer)
			{
				return;
			}
			this._isCrazyHag = (this.m_poCurrentPlayer.Player.GetCharacter() == CharacterTarget.CrazyHag);
			if (!this._isCrazyHag)
			{
				return;
			}
			this.m_cnHitExceptionList = new List<int>();
			this.m_cnHitExceptionList.Add(this.m_poCurrentPlayer.Combat.Id.ObjId);
			this.m_nCursorLayer = LayerMask.NameToLayer("AttackCursor");
			this.m_poAimCursorMaterial = new Material(Shader.Find("Particles/Alpha Blended"));
			this.m_nCursorLayer = LayerMask.NameToLayer("AttackCursor");
			this.m_fOriginalRange = this.m_fRange;
		}

		protected override void UpdateRange()
		{
			float num = this.m_poGadgetBehaviour.GetRange();
			if (num == 0f)
			{
				num = this.m_poGadgetBehaviour.Radius;
			}
			this.m_fRange = num;
			this.m_fSquaredRange = this.m_fRange;
			this.m_fSquaredRange *= this.m_fRange;
		}

		protected override long CalculateTarget(Vector3 stSourcePosition, ref Vector3 stClosestTargetPosition, ref Vector3 stPointingDirection)
		{
			if (!this._mPoTimedUpdater.ShouldHalt())
			{
				stPointingDirection = this.m_stPreviousPosition;
				return this.m_fPreviousClosestTarget;
			}
			this.m_fPreviousClosestTarget = -1L;
			Identifiable identifiable = null;
			switch (this.UsedGadgetSlot)
			{
			case GadgetSlot.CustomGadget0:
			{
				GadgetBehaviour poGadgetBehaviour = this.m_poGadgetBehaviour;
				FXInfo effect = this.m_poGadgetBehaviour.Info.Effect;
				List<int> cnHitExceptionList = this.m_cnHitExceptionList;
				if (TargetIndicatorCrazyHag.<>f__mg$cache0 == null)
				{
					TargetIndicatorCrazyHag.<>f__mg$cache0 = new GadgetBehaviour.CriteriaFunction(GadgetBehaviour.LowestRelativeHp);
				}
				GadgetBehaviour.CriteriaFunction funcToCalc = TargetIndicatorCrazyHag.<>f__mg$cache0;
				if (TargetIndicatorCrazyHag.<>f__mg$cache1 == null)
				{
					TargetIndicatorCrazyHag.<>f__mg$cache1 = new GadgetBehaviour.CriteriaFunction(GadgetBehaviour.ShortestMagnitude);
				}
				identifiable = poGadgetBehaviour.GetTarget(effect, stSourcePosition, cnHitExceptionList, funcToCalc, TargetIndicatorCrazyHag.<>f__mg$cache1);
				goto IL_167;
			}
			case GadgetSlot.CustomGadget1:
			{
				GadgetBehaviour poGadgetBehaviour2 = this.m_poGadgetBehaviour;
				FXInfo effect2 = this.m_poGadgetBehaviour.Info.Effect;
				List<int> cnHitExceptionList2 = this.m_cnHitExceptionList;
				if (TargetIndicatorCrazyHag.<>f__mg$cache2 == null)
				{
					TargetIndicatorCrazyHag.<>f__mg$cache2 = new GadgetBehaviour.CriteriaFunction(GadgetBehaviour.ShortestMagnitude);
				}
				identifiable = poGadgetBehaviour2.GetTarget(effect2, stSourcePosition, cnHitExceptionList2, TargetIndicatorCrazyHag.<>f__mg$cache2, null);
				goto IL_167;
			}
			case GadgetSlot.BoostGadget:
			{
				GadgetBehaviour poGadgetBehaviour3 = this.m_poGadgetBehaviour;
				FXInfo effect3 = this.m_poGadgetBehaviour.Info.Effect;
				List<int> cnHitExceptionList3 = this.m_cnHitExceptionList;
				if (TargetIndicatorCrazyHag.<>f__mg$cache3 == null)
				{
					TargetIndicatorCrazyHag.<>f__mg$cache3 = new GadgetBehaviour.CriteriaFunction(GadgetBehaviour.ShortestMagnitude);
				}
				identifiable = poGadgetBehaviour3.GetTarget(effect3, stSourcePosition, cnHitExceptionList3, TargetIndicatorCrazyHag.<>f__mg$cache3, null);
				goto IL_167;
			}
			}
			TargetIndicator.Log.FatalFormat("Unexpected Gadget attached to target Indicator. slot: {0}", new object[]
			{
				this.UsedGadgetSlot
			});
			IL_167:
			int i = 0;
			int num = this.m_apoPlayers.Length;
			while (i < num)
			{
				if (this.m_apoPlayers[i].Id == identifiable)
				{
					stPointingDirection = this.m_apoPlayers[i].transform.position - stSourcePosition;
					this.m_stPreviousPosition = stPointingDirection;
					this.m_fPreviousClosestTarget = (long)i;
					break;
				}
				i++;
			}
			return this.m_fPreviousClosestTarget;
		}

		protected override void LateUpdateVirtual()
		{
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				return;
			}
			if (!this.m_poCurrentPlayer || !this._isCrazyHag || !this.m_poCurrentPlayer.IsAlive())
			{
				return;
			}
			this.UpdateRange();
			bool flag = this.m_poCurrentPlayer.IsCursorEnabled(this.m_poGadgetState, this.m_poGadgetBehaviour);
			Vector3 position = this.m_poCurrentPlayer.Transform.position;
			Vector3 forward = Vector3.forward;
			Vector3 down = Vector3.down;
			long num = this.CalculateTarget(position, ref down, ref forward);
			base.RotateElementTowardsTarget(num, ref forward);
			if (num != -1L)
			{
				Matrix4x4 identity = Matrix4x4.identity;
				Vector3 position2 = this.m_apoPlayers[(int)(checked((IntPtr)num))].Transform.position;
				Vector3 vector = position2 + this.AimOffset + TargetIndicatorCrazyHag.m_stMeshCursorGroundOffset;
				Mesh mesh = (!flag) ? this.AimMeshDisabled : this.AimMesh;
				this.m_poAimCursorMaterial.mainTexture = ((!flag) ? this.AimTextureDisabled : this.AimTexture);
				identity.SetTRS(vector, TargetIndicatorCrazyHag.m_stMeshCursorRotation, this.AimMeshScale);
				Graphics.DrawMesh(mesh, identity, this.m_poAimCursorMaterial, this.m_nCursorLayer, null);
			}
			float num2 = 1f;
			Matrix4x4 identity2 = Matrix4x4.identity;
			Vector3 vector2 = position + this.RangeOffset + TargetIndicatorCrazyHag.m_stMeshCursorGroundOffset;
			Mesh mesh2 = (!flag) ? this.RangeMeshDisabled : this.RangeMesh;
			this.m_poAimCursorMaterial.mainTexture = ((!flag) ? this.RangeTextureDisabled : this.RangeTexture);
			if (this.m_fOriginalRange != 0f && this.m_fRange != 0f)
			{
				num2 = this.m_fRange / this.m_fOriginalRange;
			}
			else if (!this.m_boWarnedOnce)
			{
				this.m_boWarnedOnce = true;
				TargetIndicator.Log.WarnFormat("Possible invalid range. Original: {0}    ; Current: {1}", new object[]
				{
					this.m_fRange,
					this.m_fOriginalRange
				});
			}
			identity2.SetTRS(vector2, TargetIndicatorCrazyHag.m_stMeshCursorRotation, this.RangeMeshScale * num2);
			Graphics.DrawMesh(mesh2, identity2, this.m_poAimCursorMaterial, this.m_nCursorLayer, null);
		}

		protected override void StartVirtual()
		{
			base.StartVirtual();
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				return;
			}
			Debug.Assert(this.AimMesh != null, "Trying to set a null aim mesh for TargetIndicatorCrazyHag.", Debug.TargetTeam.All);
			Debug.Assert(this.AimTexture != null, "Trying to set a null aim texture for TargetIndicatorCrazyHag.", Debug.TargetTeam.All);
			Debug.Assert(this.AimMeshDisabled != null, "Trying to set a null disabled aim mesh for TargetIndicatorCrazyHag.", Debug.TargetTeam.All);
			Debug.Assert(this.AimTextureDisabled != null, "Trying to set a null disabled aim texture for TargetIndicatorCrazyHag", Debug.TargetTeam.All);
			Debug.Assert(this.RangeMesh != null, "Trying to set a null range mesh for TargetIndicatorCrazyHag.", Debug.TargetTeam.All);
			Debug.Assert(this.RangeTexture != null, "Trying to set a null range texture for TargetIndicatorCrazyHag.", Debug.TargetTeam.All);
			Debug.Assert(this.RangeMeshDisabled != null, "Trying to set a null disabled range mesh for TargetIndicatorCrazyHag.", Debug.TargetTeam.All);
			Debug.Assert(this.RangeTextureDisabled != null, "Trying to set a null disabled range texture for TargetIndicatorCrazyHag.", Debug.TargetTeam.All);
		}

		public Mesh AimMesh;

		public Texture AimTexture;

		public Mesh AimMeshDisabled;

		public Texture AimTextureDisabled;

		public Mesh RangeMesh;

		public Texture RangeTexture;

		public Mesh RangeMeshDisabled;

		public Texture RangeTextureDisabled;

		public Vector3 AimMeshScale = Vector3.one;

		public Vector3 AimOffset = Vector3.zero;

		public Vector3 RangeMeshScale = Vector3.one;

		public Vector3 RangeOffset = Vector3.zero;

		private static Vector3 m_stMeshCursorGroundOffset = new Vector3(0f, 0.5f, 0f);

		private static Quaternion m_stMeshCursorRotation = Quaternion.Euler(-90f, 0f, 0f);

		private Material m_poAimCursorMaterial;

		private int m_nCursorLayer = -1;

		private bool m_boWarnedOnce;

		private float m_fOriginalRange = -1f;

		private float m_fRange = -1f;

		private List<int> m_cnHitExceptionList;

		private TimedUpdater _mPoTimedUpdater;

		private long m_fPreviousClosestTarget = -1L;

		private Vector3 m_stPreviousPosition = Vector3.forward;

		private bool _isCrazyHag;

		[CompilerGenerated]
		private static GadgetBehaviour.CriteriaFunction <>f__mg$cache0;

		[CompilerGenerated]
		private static GadgetBehaviour.CriteriaFunction <>f__mg$cache1;

		[CompilerGenerated]
		private static GadgetBehaviour.CriteriaFunction <>f__mg$cache2;

		[CompilerGenerated]
		private static GadgetBehaviour.CriteriaFunction <>f__mg$cache3;
	}
}
