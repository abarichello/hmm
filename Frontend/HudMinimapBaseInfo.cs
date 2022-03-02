using System;
using HeavyMetalMachines.Arena;
using HeavyMetalMachines.Match;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class HudMinimapBaseInfo : MonoBehaviour
	{
		public Vector2[] BlueTeamBase2DPoints
		{
			get
			{
				if (this._blueTeamBase2DPoints == null)
				{
					this._blueTeamBase2DPoints = new Vector2[this.BlueTeamDeadZone.Length];
					for (int i = 0; i < this.BlueTeamDeadZone.Length; i++)
					{
						this._blueTeamBase2DPoints[i] = this.BlueTeamDeadZone[i].position;
					}
				}
				return this._blueTeamBase2DPoints;
			}
		}

		public Vector2[] RedTeamBase2DPoints
		{
			get
			{
				if (this._redTeamBase2DPoints == null)
				{
					this._redTeamBase2DPoints = new Vector2[this.RedTeamDeadZone.Length];
					for (int i = 0; i < this.RedTeamDeadZone.Length; i++)
					{
						this._redTeamBase2DPoints[i] = this.RedTeamDeadZone[i].position;
					}
				}
				return this._redTeamBase2DPoints;
			}
		}

		public void Setup(TeamKind currentPlayerTeam, IGameArenaInfo arenaInfo)
		{
			if (currentPlayerTeam == arenaInfo.BaseFlipTeam)
			{
				this.SetBasePoints(arenaInfo.TeamBlueBasePoint, arenaInfo.TeamRedBasePoint, arenaInfo.TeamBlueDeadZone, arenaInfo.TeamRedDeadZone);
			}
			else
			{
				this.SetBasePoints(arenaInfo.TeamRedBasePoint, arenaInfo.TeamBlueBasePoint, arenaInfo.TeamRedDeadZone, arenaInfo.TeamBlueDeadZone);
			}
		}

		private void SetBasePoints(Vector2 redTeamBase, Vector2 blueTeamBase, Vector2[] redTeamDeadZone, Vector2[] blueTeamDeadZone)
		{
			this.RedTeamBasePoint.localPosition = redTeamBase;
			this.BlueTeamBasePoint.localPosition = blueTeamBase;
			int num = 0;
			while (num < this.RedTeamDeadZone.Length && num < redTeamDeadZone.Length)
			{
				this.RedTeamDeadZone[num].transform.localPosition = redTeamDeadZone[num];
				num++;
			}
			int num2 = 0;
			while (num2 < this.BlueTeamDeadZone.Length && num2 < blueTeamDeadZone.Length)
			{
				this.BlueTeamDeadZone[num2].transform.localPosition = blueTeamDeadZone[num2];
				num2++;
			}
			this.RecalculateBasePoints();
		}

		public void RecalculateBasePoints()
		{
			this._blueTeamBase2DPoints = null;
			this._redTeamBase2DPoints = null;
		}

		public bool IsInsideTeamArea(TeamKind teamKind, Vector3 position)
		{
			return HMMMathUtils.PolygonContainsPoint((teamKind != TeamKind.Red) ? this.BlueTeamBase2DPoints : this.RedTeamBase2DPoints, position);
		}

		public RectTransform BlueTeamBasePoint;

		public RectTransform RedTeamBasePoint;

		public RectTransform[] BlueTeamDeadZone = new RectTransform[4];

		public RectTransform[] RedTeamDeadZone = new RectTransform[4];

		private Vector2[] _blueTeamBase2DPoints;

		private Vector2[] _redTeamBase2DPoints;
	}
}
