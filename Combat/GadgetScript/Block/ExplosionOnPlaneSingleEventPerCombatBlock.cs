using System;
using System.Collections.Generic;
using System.Linq;
using HeavyMetalMachines.Infra.Context;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/Physics/ExplosionOnPlaneSingleEventPerCombat")]
	public class ExplosionOnPlaneSingleEventPerCombatBlock : BaseBlock
	{
		protected override void OnEnable()
		{
			base.OnEnable();
			this._isIsBarrierParameterDefined = (null != this._isBarrier);
		}

		public override IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			IHMMGadgetContext ihmmgadgetContext = (IHMMGadgetContext)gadgetContext;
			if (ihmmgadgetContext.IsClient)
			{
				return this._nextBlock;
			}
			ExplosionOnPlaneSingleEventPerCombatBlock._allCombatsHit.Clear();
			ExplosionOnPlaneSingleEventPerCombatBlock._allCombatsHitIsBarrier.Clear();
			Vector3 value = this._origin.GetValue<Vector3>(gadgetContext);
			value.y = 0f;
			IParameterTomate<float> parameterTomate = this._radius.ParameterTomate as IParameterTomate<float>;
			float value2 = parameterTomate.GetValue(gadgetContext);
			IParameterTomate<float> parameterTomate2 = this._arc.ParameterTomate as IParameterTomate<float>;
			float num = Mathf.Clamp(parameterTomate2.GetValue(gadgetContext), 0f, 360f);
			float num2 = (float)this._numberOfRays;
			int num3 = 0;
			if (Mathf.Approximately(num, 360f))
			{
				num3 = 1;
				num2 += 1f;
			}
			float num4 = num / (num2 - 1f);
			Vector3 vector = Quaternion.Euler(0f, -(num - (float)num3 * num4) / 2f, 0f) * this._direction.GetValue<Vector3>(gadgetContext);
			Quaternion quaternion = Quaternion.Euler(0f, num4, 0f);
			int num5 = ExplosionOnPlaneSingleEventPerCombatBlock._filter.RaycastFilter(Physics.RaycastNonAlloc(value + Vector3.down * 100f, Vector3.up, ExplosionOnPlaneSingleEventPerCombatBlock._hits, 100f, this._layersToHit), ExplosionOnPlaneSingleEventPerCombatBlock._hits, ExplosionOnPlaneSingleEventPerCombatBlock._rayCombatsHit, ihmmgadgetContext, this._hitOverWall, this._notifySceneryHits);
			ExplosionOnPlaneSingleEventPerCombatBlock._filter.AddOffset(num5, ExplosionOnPlaneSingleEventPerCombatBlock._rayCombatsHit);
			this.GetCombatsHitsIsBarrier(num5, ExplosionOnPlaneSingleEventPerCombatBlock._rayCombatsHit, ExplosionOnPlaneSingleEventPerCombatBlock._allCombatsHitIsBarrier, ExplosionOnPlaneSingleEventPerCombatBlock._allCombatsHit);
			int num6 = num3;
			while ((float)num6 < num2)
			{
				Debug.DrawLine(value, value + vector * value2, Color.red, 5f);
				num5 = ExplosionOnPlaneSingleEventPerCombatBlock._filter.RaycastFilter(Physics.RaycastNonAlloc(value, vector, ExplosionOnPlaneSingleEventPerCombatBlock._hits, value2, this._layersToHit), ExplosionOnPlaneSingleEventPerCombatBlock._hits, ExplosionOnPlaneSingleEventPerCombatBlock._rayCombatsHit, ihmmgadgetContext, this._hitOverWall, this._notifySceneryHits);
				this.GetCombatsHitsIsBarrier(num5, ExplosionOnPlaneSingleEventPerCombatBlock._rayCombatsHit, ExplosionOnPlaneSingleEventPerCombatBlock._allCombatsHitIsBarrier, ExplosionOnPlaneSingleEventPerCombatBlock._allCombatsHit);
				vector = quaternion * vector;
				num6++;
			}
			for (int i = 0; i < ExplosionOnPlaneSingleEventPerCombatBlock._allCombatsHit.Count; i++)
			{
				if (!ExplosionOnPlaneSingleEventPerCombatBlock._allCombatsHit[i].isBehindWall)
				{
					if (this._isIsBarrierParameterDefined)
					{
						this._isBarrier.SetValue(ihmmgadgetContext, ExplosionOnPlaneSingleEventPerCombatBlock._allCombatsHitIsBarrier[ExplosionOnPlaneSingleEventPerCombatBlock._allCombatsHit[i].combat]);
					}
					if (this._combat != null)
					{
						this._combat.SetValue(ihmmgadgetContext, ExplosionOnPlaneSingleEventPerCombatBlock._allCombatsHit[i].combat);
					}
					ihmmgadgetContext.TriggerEvent(GadgetEvent.GetInstance(this._onCombatHitBlock.Id, ihmmgadgetContext));
				}
			}
			return this._nextBlock;
		}

		private void GetCombatsHitsIsBarrier(int numHits, ExplosionOnPlaneSingleEventPerCombatBlock.CombatHitBarrierFilter.CombatHit[] combats, Dictionary<ICombatObject, bool> combatsHitsIsBarrier, List<ExplosionOnPlaneSingleEventPerCombatBlock.CombatHitBarrierFilter.CombatHit> uniqueCombats)
		{
			for (int i = 0; i < numHits; i++)
			{
				if (combats[i].combat == null)
				{
					if (uniqueCombats.All((ExplosionOnPlaneSingleEventPerCombatBlock.CombatHitBarrierFilter.CombatHit ch) => ch.combat != null))
					{
						uniqueCombats.Add(combats[i]);
					}
				}
				else
				{
					bool flag2;
					bool flag = combatsHitsIsBarrier.TryGetValue(combats[i].combat, out flag2);
					if (!flag || flag2)
					{
						if (!flag)
						{
							uniqueCombats.Add(combats[i]);
						}
						combatsHitsIsBarrier[combats[i].combat] = combats[i].isBarrier;
					}
				}
			}
		}

		[SerializeField]
		private BaseBlock _onCombatHitBlock;

		[Header("Read")]
		[SerializeField]
		private BaseParameter _origin;

		[SerializeField]
		private BaseParameter _direction;

		[SerializeField]
		private BaseParameter _radius;

		[SerializeField]
		private BaseParameter _arc;

		[SerializeField]
		private bool _hitOverWall;

		[SerializeField]
		private bool _notifySceneryHits;

		[SerializeField]
		private LayerMask _layersToHit;

		[SerializeField]
		[Range(8f, 60f)]
		private int _numberOfRays;

		[Header("Write")]
		[SerializeField]
		private BoolParameter _isBarrier;

		[SerializeField]
		private CombatObjectParameter _combat;

		private bool _isIsBarrierParameterDefined;

		private static readonly Dictionary<ICombatObject, bool> _allCombatsHitIsBarrier = new Dictionary<ICombatObject, bool>();

		private static readonly List<ExplosionOnPlaneSingleEventPerCombatBlock.CombatHitBarrierFilter.CombatHit> _allCombatsHit = new List<ExplosionOnPlaneSingleEventPerCombatBlock.CombatHitBarrierFilter.CombatHit>();

		private static readonly ExplosionOnPlaneSingleEventPerCombatBlock.CombatHitBarrierFilter.CombatHit[] _rayCombatsHit = new ExplosionOnPlaneSingleEventPerCombatBlock.CombatHitBarrierFilter.CombatHit[50];

		private static readonly RaycastHit[] _hits = new RaycastHit[100];

		private static readonly ExplosionOnPlaneSingleEventPerCombatBlock.CombatHitBarrierFilter _filter = new ExplosionOnPlaneSingleEventPerCombatBlock.CombatHitBarrierFilter();

		private class CombatHitBarrierFilter
		{
			public void AddOffset(int offsetSize, ExplosionOnPlaneSingleEventPerCombatBlock.CombatHitBarrierFilter.CombatHit[] combats)
			{
				this._combatsHitsIndex.Clear();
				for (int i = 0; i < offsetSize; i++)
				{
					this._combatsHitsIndex.Add(combats[i].combat, i);
					this._combatsHits[i] = combats[i];
				}
			}

			public int RaycastFilter(int numHits, RaycastHit[] hits, ExplosionOnPlaneSingleEventPerCombatBlock.CombatHitBarrierFilter.CombatHit[] hitResult, IHMMGadgetContext context, bool hitOverWall, bool notifySceneryHits)
			{
				int num = 0;
				float num2 = float.PositiveInfinity;
				int i = 0;
				while (i < numHits)
				{
					ICombatObject combatObject = context.GetCombatObject(hits[i].collider);
					if (combatObject != null)
					{
						goto IL_C3;
					}
					if (!hitOverWall && hits[i].collider.gameObject.layer == 9 && hits[i].collider.GetComponent<Rigidbody>() == null)
					{
						num2 = Mathf.Min(num2, hits[i].distance);
						for (int j = 0; j > num; j++)
						{
							this._combatsHits[i].isBehindWall = (this._combatsHits[i].distance > num2);
						}
					}
					if (notifySceneryHits)
					{
						goto IL_C3;
					}
					IL_1D4:
					i++;
					continue;
					IL_C3:
					int num3 = 0;
					bool flag = combatObject != null && this._combatsHitsIndex.TryGetValue(combatObject, out num3);
					if (flag)
					{
						ExplosionOnPlaneSingleEventPerCombatBlock.CombatHitBarrierFilter.CombatHit combatHit = this._combatsHits[num3];
						if (combatHit.distance < hits[i].distance)
						{
							goto IL_1D4;
						}
					}
					else
					{
						ExplosionOnPlaneSingleEventPerCombatBlock.CombatHitBarrierFilter.CombatHit combatHit2 = new ExplosionOnPlaneSingleEventPerCombatBlock.CombatHitBarrierFilter.CombatHit
						{
							distance = hits[i].distance,
							isBarrier = false,
							combat = combatObject,
							isBehindWall = (num2 < hits[i].distance)
						};
						if (combatObject != null)
						{
							this._combatsHitsIndex.Add(combatObject, num);
						}
						this._combatsHits[num] = combatHit2;
						num3 = num;
						num++;
					}
					this._combatsHits[num3].isBarrier = BarrierUtils.IsBarrier(hits[i].collider);
					hitResult[num3] = this._combatsHits[num3];
					goto IL_1D4;
				}
				this._combatsHitsIndex.Clear();
				return num;
			}

			private readonly Dictionary<ICombatObject, int> _combatsHitsIndex = new Dictionary<ICombatObject, int>();

			private readonly ExplosionOnPlaneSingleEventPerCombatBlock.CombatHitBarrierFilter.CombatHit[] _combatsHits = new ExplosionOnPlaneSingleEventPerCombatBlock.CombatHitBarrierFilter.CombatHit[20];

			public struct CombatHit
			{
				public float distance;

				public bool isBarrier;

				public bool isBehindWall;

				public ICombatObject combat;
			}
		}
	}
}
