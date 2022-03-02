using System;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	[DisallowMultipleComponent]
	public class UnityParticlesVFX : BaseVFX
	{
		public UnityParticlesVFX()
		{
			base.CanCollectToCache = true;
		}

		public int AliveParticles
		{
			get
			{
				int num = 0;
				for (int i = 0; i < this._emitters.Length; i++)
				{
					num += ((!this._emitters[i].isPlaying) ? 0 : this._emitters[i].particleCount);
				}
				return num;
			}
		}

		public override bool CanCollectToCache
		{
			get
			{
				return base.CanCollectToCache && (!this._waitForParticlesDeath || this.AliveParticles == 0);
			}
			protected set
			{
			}
		}

		protected override void OnActivate()
		{
			if (this._team != VFXTeam.Neutral)
			{
				bool flag;
				if (this.PrevizMode)
				{
					flag = (this.CurrentTeam == VFXTeam.Enemy);
				}
				else
				{
					TeamKind team = GameHubBehaviour.Hub.Players.GetPlayerOrBotsByObjectId(this._targetFXInfo.Owner.ObjId).Team;
					flag = (team != GameHubBehaviour.Hub.Players.CurrentPlayerTeam);
				}
				if ((flag && this._team != VFXTeam.Enemy) || (!flag && this._team != VFXTeam.Ally))
				{
					return;
				}
			}
			base.CanCollectToCache = (this.spawnStage == BaseVFX.SpawnState.OnDestroy);
			for (int i = 0; i < this._emitters.Length; i++)
			{
				this._emitters[i].Play(false);
			}
		}

		protected override void WillDeactivate()
		{
		}

		protected override void OnDeactivate()
		{
			if (base.CanCollectToCache)
			{
				return;
			}
			base.CanCollectToCache = true;
			for (int i = 0; i < this._emitters.Length; i++)
			{
				this._emitters[i].Stop(false, (!this._waitForParticlesDeath) ? 0 : 1);
			}
		}

		[SerializeField]
		private bool _waitForParticlesDeath;

		[SerializeField]
		private VFXTeam _team = VFXTeam.Neutral;

		[SerializeField]
		[ReadOnly]
		private ParticleSystem[] _emitters;

		[NonSerialized]
		private int _tagHash;
	}
}
