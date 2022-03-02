using System;
using System.Collections.Generic;
using System.Text;
using HeavyMetalMachines.Infra.Context;
using Pocketverse;
using UniRx;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	[Serializable]
	public class BombScoreBoard : IScoreBoard, IBitStreamSerializable
	{
		public BombScoreBoard()
		{
			this._gameStateObservation = new Subject<ScoreBoardState>();
		}

		public float LastScoreUpdateTime
		{
			get
			{
				return this._lastScoreUpdateTime;
			}
		}

		public int ScoreRed
		{
			get
			{
				return this.BombScoreRed;
			}
		}

		public int ScoreBlue
		{
			get
			{
				return this.BombScoreBlue;
			}
		}

		public int Round
		{
			get
			{
				return this._round;
			}
		}

		public int RoundState
		{
			get
			{
				return (int)this._state.CurrentState;
			}
		}

		public BombScoreboardState CurrentState
		{
			get
			{
				return this._state.CurrentState;
			}
			set
			{
				if (this._state.CurrentState == value)
				{
					return;
				}
				this._state.PreviousState = this._state.CurrentState;
				this._state.CurrentState = value;
				this._gameStateObservation.OnNext(this._state);
			}
		}

		public IObservable<ScoreBoardState> StateChangedObservation
		{
			get
			{
				return this._gameStateObservation;
			}
		}

		public bool IsInOvertime { get; set; }

		public BombScoreboardState PreviouState
		{
			get
			{
				return this._state.PreviousState;
			}
		}

		public void SetTimeout(long currentMatchTime, int delayMillis)
		{
			this.Timeout = currentMatchTime + (long)delayMillis;
			this.Dirty = true;
		}

		public float BombTimeRed
		{
			get
			{
				float num = 0f;
				for (int i = 0; i < this.Rounds.Count; i++)
				{
					num += this.Rounds[i].BombTimeRed;
				}
				return num;
			}
		}

		public float BombTimeBlue
		{
			get
			{
				float num = 0f;
				for (int i = 0; i < this.Rounds.Count; i++)
				{
					num += this.Rounds[i].BombTimeBlue;
				}
				return num;
			}
		}

		public float GetTotalBombTime(int carId)
		{
			float num = 0f;
			for (int i = 0; i < this.Rounds.Count; i++)
			{
				PlayerRoundStats playerRoundStats;
				if (this.Rounds[i].Players.TryGetValue(carId, out playerRoundStats))
				{
					num += playerRoundStats.BombTime;
				}
			}
			return num;
		}

		public void ResetRounds()
		{
			this.Rounds.Clear();
			this._current = null;
			this._round = 0;
			this.Dirty = true;
		}

		public void NextRound()
		{
			this._round++;
			this._current = null;
			this.Dirty = true;
		}

		public RoundStats CurrentRound
		{
			get
			{
				if (this._current == null)
				{
					this._current = new RoundStats();
					this.Rounds.Add(this._current);
				}
				return this._current;
			}
		}

		public void WriteToBitStream(BitStream bs)
		{
			bs.WriteCompressedInt(this.BombScoreBlue);
			bs.WriteCompressedInt(this.BombScoreRed);
			bs.WriteCompressedLong(this.Timeout);
			bs.WriteCompressedInt(this._round);
			bs.WriteBits(3, (int)this.CurrentState);
			bs.WriteCompressedInt(this.RoundStartTimeMillis);
			bs.WriteBool(this.IsInOvertime);
			bs.WriteBool(this.MatchOver);
			bs.WriteBits(3, this.Rounds.Count);
			for (int i = 0; i < this.Rounds.Count; i++)
			{
				this.Rounds[i].WriteToBitStream(bs);
			}
		}

		public void ReadFromBitStream(BitStream bs)
		{
			int bombScoreBlue = this.BombScoreBlue;
			int bombScoreRed = this.BombScoreRed;
			this.BombScoreBlue = bs.ReadCompressedInt();
			this.BombScoreRed = bs.ReadCompressedInt();
			this.Timeout = bs.ReadCompressedLong();
			this._round = bs.ReadCompressedInt();
			this.CurrentState = (BombScoreboardState)bs.ReadBits(3);
			this.RoundStartTimeMillis = bs.ReadCompressedInt();
			this.IsInOvertime = bs.ReadBool();
			this.MatchOver = bs.ReadBool();
			int num = bs.ReadBits(3);
			for (int i = 0; i < num; i++)
			{
				if (this.Rounds.Count == i)
				{
					this.Rounds.Add(new RoundStats());
				}
				RoundStats roundStats = this.Rounds[i];
				roundStats.ReadFromBitStream(bs);
			}
			if (bombScoreBlue != this.BombScoreBlue || bombScoreRed != this.BombScoreRed)
			{
				this._lastScoreUpdateTime = Time.unscaledTime;
			}
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat("[RED={0} BLUE={1} ROUND={2} STATE={3}]", new object[]
			{
				this.BombScoreRed,
				this.BombScoreBlue,
				this._round,
				this.CurrentState
			});
			return stringBuilder.ToString();
		}

		public static BitLogger Log = new BitLogger(typeof(BombScoreBoard));

		public int BombScoreRed;

		public int BombScoreBlue;

		private float _lastScoreUpdateTime;

		public long Timeout;

		private Subject<ScoreBoardState> _gameStateObservation;

		private int _round;

		private ScoreBoardState _state;

		public int RoundStartTimeMillis;

		public int OvertimeStartTimeMillis;

		public bool MatchOver;

		public List<RoundStats> Rounds = new List<RoundStats>(5);

		private RoundStats _current;

		public bool Dirty;
	}
}
