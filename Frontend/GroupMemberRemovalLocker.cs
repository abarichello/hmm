using System;
using System.Collections;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Infra;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Social;
using HeavyMetalMachines.Utils;
using HeavyMetalMachines.VFX;
using HeavyMetalMachines.VFX.PlotKids;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class GroupMemberRemovalLocker : IGroupMemberRemovalLocker
	{
		public GroupMemberRemovalLocker(GameObject selfLeaveGroupButtonGameObject, GroupMemberGuiItem[] groupMemberGuiItems, MonoBehaviour routineController)
		{
			this._selfLeaveGroupButtonGameObject = selfLeaveGroupButtonGameObject;
			this._groupMemberGuiItems = groupMemberGuiItems;
			this._routineController = routineController;
		}

		public void SetLockingInterval(int interval)
		{
			this._leaveButtonLockIntervalInSec = interval;
		}

		public void StartPreventMemberRemoval()
		{
			this.StopPreventMemberRemovalCoroutine();
			GroupManager groupManager = ManagerController.Get<GroupManager>();
			GroupStatus selfGroupStatus = groupManager.GetSelfGroupStatus();
			this._preventMemberRemovalCoroutine = this._routineController.StartCoroutine(this.PreventMemberRemovalCooldown(selfGroupStatus));
		}

		public void InterruptPreventMemberRemoval()
		{
			if (this._preventMemberRemovalCoroutine == null)
			{
				return;
			}
			this.StopPreventMemberRemovalCoroutine();
			GroupManager groupManager = ManagerController.Get<GroupManager>();
			GroupStatus selfGroupStatus = groupManager.GetSelfGroupStatus();
			this.UnlockMemberRemoval(selfGroupStatus);
		}

		private void StopPreventMemberRemovalCoroutine()
		{
			if (this._preventMemberRemovalCoroutine == null)
			{
				return;
			}
			this._routineController.StopCoroutine(this._preventMemberRemovalCoroutine);
			this._preventMemberRemovalCoroutine = null;
		}

		private IEnumerator PreventMemberRemovalCooldown(GroupStatus groupStatus)
		{
			this.LockMemberRemoval(groupStatus);
			yield return CoroutineUtil.WaitForRealSeconds((float)this._leaveButtonLockIntervalInSec);
			this.UnlockMemberRemoval(groupStatus);
			yield break;
		}

		private void LockMemberRemoval(GroupStatus groupStatus)
		{
			if (groupStatus == GroupStatus.None)
			{
				return;
			}
			this._selfLeaveGroupButtonGameObject.SetActive(false);
			if (groupStatus != GroupStatus.Owner)
			{
				return;
			}
			for (int i = 0; i < this._groupMemberGuiItems.Length; i++)
			{
				this._groupMemberGuiItems[i].DisableRemoveFromGroupButton();
			}
		}

		private void UnlockMemberRemoval(GroupStatus groupStatus)
		{
			if (groupStatus == GroupStatus.None)
			{
				return;
			}
			this._selfLeaveGroupButtonGameObject.SetActive(true);
			if (groupStatus != GroupStatus.Owner)
			{
				return;
			}
			for (int i = 0; i < this._groupMemberGuiItems.Length; i++)
			{
				this._groupMemberGuiItems[i].EnableRemoveFromGroupButton();
			}
		}

		private int _leaveButtonLockIntervalInSec = 3;

		private readonly GameObject _selfLeaveGroupButtonGameObject;

		private readonly GroupMemberGuiItem[] _groupMemberGuiItems;

		private readonly MonoBehaviour _routineController;

		private Coroutine _preventMemberRemovalCoroutine;
	}
}
