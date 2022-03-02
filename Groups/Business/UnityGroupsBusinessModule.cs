using System;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Social.Groups;
using HeavyMetalMachines.Social.Groups.Business;
using Hoplon.Assertions;
using Hoplon.DependencyInjection;

namespace HeavyMetalMachines.Groups.Business
{
	public class UnityGroupsBusinessModule : IInjectionModule, IInjectionBindable
	{
		public UnityGroupsBusinessModule(IInjectionBinder injectionBinder)
		{
			Assert.ConstructorParametersAreNotNull(new object[]
			{
				injectionBinder
			});
			this._injectionBinder = injectionBinder;
		}

		public void Bind()
		{
			this._injectionBinder.BindTransient<IGetAvailableFriendsToInviteToGroup, GetAvailableUsersToInviteToGroup>();
			this._injectionBinder.BindTransient<ISendGroupInvite, SendGroupInvite>();
			this._injectionBinder.BindTransient<ICancelGroupInvite, CancelGroupInvite>();
			this._injectionBinder.BindTransient<IGetGroupInviteExpiration, GetGroupInviteExpiration>();
			this._injectionBinder.BindTransient<IIsFriendAvailableToInviteToGroup, IsFriendAvailableToInviteToGroup>();
			this._injectionBinder.BindTransient<IIsUserPendingInviteToGroup, IsUserPendingInviteToGroup>();
			this._injectionBinder.BindSingle<ICanLocalPlayerJoinGroup, CanLocalPlayerJoinGroup>();
			this._injectionBinder.BindTransient<IObserverGroupOwnerChange, LegacyObserveGroupOwnerChange>();
			this._injectionBinder.BindTransient<IObserveGroupInvitesCooldown, ObserveGroupInvitesCooldown>();
			this.ToogleInjection();
		}

		private void ToogleInjection()
		{
			this._injectionBinder.BindTransient<IIsGroupMemberReadyToPlay, IsGroupMemberReadyToPlay>();
			this._injectionBinder.BindTransient<IIsGroupReadyToPlay, IsGroupReadyToPlay>();
		}

		private readonly IInjectionBinder _injectionBinder;
	}
}
