using System;
using System.Collections.Generic;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using HeavyMetalMachines.Characters;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class PickScreenHints : GameHubBehaviour
	{
		public void UpdateTips(List<int> closedPilots)
		{
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			for (int i = 0; i < closedPilots.Count; i++)
			{
				int key = closedPilots[i];
				IItemType itemType;
				if (GameHubBehaviour.Hub.InventoryColletion.AllCharactersByCharacterId.TryGetValue(key, out itemType))
				{
					DriverRoleKind role = itemType.GetComponent<CharacterItemTypeComponent>().Role;
					if (role != 1)
					{
						if (role != 2)
						{
							if (role == null)
							{
								flag3 = true;
							}
						}
						else
						{
							flag2 = true;
						}
					}
					else
					{
						flag = true;
					}
				}
			}
			this.UpdateTip(flag, this._carrierTip);
			this.UpdateTip(flag2, this._tacklerTip);
			this.UpdateTip(flag3, this._supportTip);
			this._grid.Reposition();
			this._tipsGroupAnimator.SetBool("active", !flag || !flag2 || !flag3);
		}

		private void UpdateTip(bool condition, PickScreenHintReference reference)
		{
			if (condition)
			{
				reference.Hide();
			}
			else if (!reference.isVisible)
			{
				reference.Show();
			}
		}

		[SerializeField]
		private UIGrid _grid;

		[SerializeField]
		private Animator _tipsGroupAnimator;

		[SerializeField]
		private PickScreenHintReference _carrierTip;

		[SerializeField]
		private PickScreenHintReference _tacklerTip;

		[SerializeField]
		private PickScreenHintReference _supportTip;
	}
}
