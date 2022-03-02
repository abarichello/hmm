using System;
using HeavyMetalMachines.DriverHelper;
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

		public SharedPreGameGui SharedPreGameWindow;

		public TopRightButtonsController TopRightButtonsMenu;

		public GUIColorsInfo GUIColors;

		public GUIValuesInfo GUIValues;

		public DriverHelperController DriverHelper;
	}
}
