using System;
using System.Collections.Generic;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Utils;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class Trail
	{
		public Trail(HMMHub hub, Func<FXInfo, EffectEvent> getEffectEvent, FXInfo effectInfo, bool followCombatObject)
		{
			this._hub = hub;
			this._followCombatObject = followCombatObject;
			this._getEffectEvent = getEffectEvent;
			this._effectInfo = effectInfo;
			this._trailDroppers = new UnorderedList<TrailPieceDropper>(4);
			this._allEffects = new List<int>(100);
		}

		public void RunGadgetFixedUpdate()
		{
			foreach (TrailPieceDropper trailPieceDropper in this._trailDroppers)
			{
				if (this._trailPieceDropTimeInterval <= 0)
				{
					Vector3 position = trailPieceDropper.CombatObject.transform.position;
					position.y = 0f;
					trailPieceDropper.LastPiecePosition.y = 0f;
					float magnitude = (trailPieceDropper.LastPiecePosition - position).magnitude;
					if (magnitude < this._pieceColliderRadius && !(trailPieceDropper.LastPiecePosition == Vector3.zero))
					{
						break;
					}
				}
				else if (trailPieceDropper.TrailPieceDropTimer.ShouldHalt())
				{
					break;
				}
				Vector3 vector;
				if (this._followCombatObject)
				{
					vector = GadgetBehaviour.DummyPosition(trailPieceDropper.CombatObject, this._effectInfo);
				}
				else
				{
					vector = trailPieceDropper.TrailStartPosition + (float)(trailPieceDropper.TrailPiecesCount + 1) * this._pieceColliderRadius * trailPieceDropper.TrailDirection;
					trailPieceDropper.TrailPiecesCount++;
				}
				trailPieceDropper.LastPiecePosition = vector;
				EffectEvent effectEvent = this._getEffectEvent(this._effectInfo);
				effectEvent.Modifiers = this._modifiers;
				effectEvent.Origin = vector;
				effectEvent.Target = vector;
				effectEvent.Direction = trailPieceDropper.TrailDirection;
				effectEvent.Range = this._pieceColliderRadius;
				effectEvent.CustomVar = (byte)this._pieceColliderRadius;
				effectEvent.LifeTime = this._trailPieceLifeTime;
				this._allEffects.Add(this._hub.Events.TriggerEvent(effectEvent));
			}
		}

		public void FireCannon(Vector3 origin, Vector3 dir, int ownerId, CombatObject targetCombatObject)
		{
			if (string.IsNullOrEmpty(this._effectInfo.Effect))
			{
				return;
			}
			TrailPieceDropper trailPieceDropper = new TrailPieceDropper();
			trailPieceDropper.EffectOwnerId = ownerId;
			trailPieceDropper.CombatObject = targetCombatObject;
			trailPieceDropper.TrailStartPosition = origin;
			trailPieceDropper.LastPiecePosition = Vector3.zero;
			trailPieceDropper.TrailDirection = dir;
			trailPieceDropper.TrailPiecesCount = 0;
			trailPieceDropper.TrailPieceDropTimer.PeriodMillis = this._trailPieceDropTimeInterval;
			trailPieceDropper.TrailPieceDropTimer.Reset();
			this._trailDroppers.Add(trailPieceDropper);
		}

		public void SetLevel(ModifierData[] trailModifier, float range, float moveSpeed, float pieceColliderRadius, float trailPiecesLifeTime, float trailPieceDropTimeIntervalMillis)
		{
			this._trailPieceLifeTime = trailPiecesLifeTime;
			this._modifiers = trailModifier;
			this._pieceColliderRadius = pieceColliderRadius;
			if (this._followCombatObject)
			{
				this._trailPieceDropTimeInterval = (int)trailPieceDropTimeIntervalMillis;
				return;
			}
			float num = range / moveSpeed;
			float num2 = range / pieceColliderRadius;
			float num3 = num / num2 * 1000f;
			this._trailPieceDropTimeInterval = (int)num3;
		}

		public void OnDestroyEffect(DestroyEffectMessage evt)
		{
			if (this._allEffects.Contains(evt.RemoveData.TargetEventId))
			{
				this._allEffects.Remove(evt.RemoveData.TargetEventId);
				return;
			}
			foreach (TrailPieceDropper trailPieceDropper in this._trailDroppers)
			{
				if (evt.RemoveData.TargetEventId == trailPieceDropper.EffectOwnerId)
				{
					this._trailDroppers.Remove(trailPieceDropper);
				}
			}
		}

		public bool ItIsAValidTrailID(int eventid)
		{
			return this._allEffects.Contains(eventid);
		}

		private readonly HMMHub _hub;

		private float _trailPieceLifeTime;

		private float _pieceColliderRadius;

		private int _trailPieceDropTimeInterval;

		private readonly bool _followCombatObject;

		private ModifierData[] _modifiers;

		private readonly FXInfo _effectInfo;

		private readonly Func<FXInfo, EffectEvent> _getEffectEvent;

		private readonly UnorderedList<TrailPieceDropper> _trailDroppers;

		private readonly List<int> _allEffects;
	}
}
