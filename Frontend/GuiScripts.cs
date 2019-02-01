using System;
using Pocketverse;

namespace HeavyMetalMachines.Frontend
{
	public class GuiScripts : GameHubBehaviour
	{
		public ConfirmWindowReference ConfirmWindow;

		public GuiLoadingController Loading;

		public AFKControllerGui AfkControllerGui;

		public LoadingVersusController LoadingVersus;

		public ScreenResolutionController ScreenResolution;

		public EscMenuGui Esc;

		public TooltipController TooltipController;

		public JoystickSchemeController JoystickScheme;

		public SharedPreGameGui SharedPreGameWindow;

		public TopMenuController TopMenu;

		public TopRightButtonsController TopRightButtonsMenu;

		public GUIColorsInfo GUIColors;

		public GUIValuesInfo GUIValues;

		public DriverHelperController DriverHelper;

		public GUIJoystickShortcutIcons JoystickShortcutIcons;
	}
}
