using System;
using HeavyMetalMachines.Presenting;
using Hoplon.Input.UiNavigation;
using UniRx;

namespace HeavyMetalMachines.Profile.View
{
	public interface ILegacyProfileView
	{
		IActivatable MainActivatable { get; }

		IButton BackButton { get; }

		IButton AvatarChangeButton { get; }

		IObservable<Unit> Show();

		IObservable<Unit> Hide();

		IActivatable AvatarLoading { get; }

		ILabel PlayerTagLabel { get; }

		IButton CopyPlayerTagButton { get; }

		IUiNavigationGroupHolder UiNavigationGroupHolder { get; }

		void SetPortraitAndAvatarImage(string avatarImageName, string portraitImageName);
	}
}
