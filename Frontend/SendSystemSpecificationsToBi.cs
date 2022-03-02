using System;
using System.Text;
using HeavyMetalMachines.BI;
using HeavyMetalMachines.Frontend.Apis;
using HeavyMetalMachines.Players.Business;
using HeavyMetalMachines.Swordfish;
using NativePlugins;
using UnityEngine;
using UnityEngine.Rendering;

namespace HeavyMetalMachines.Frontend
{
	public class SendSystemSpecificationsToBi : ISendSystemSpecificationsToBi
	{
		public SendSystemSpecificationsToBi(IScreenResolutionController screenResolutionController, IClientBILogger clientBiLogger, IUnidentifiedPlayerBiLogger unidentifiedPlayerBiLogger, IGetLocalPlayer getLocalPlayer)
		{
			this._screenResolutionController = screenResolutionController;
			this._clientBiLogger = clientBiLogger;
			this._unidentifiedPlayerBiLogger = unidentifiedPlayerBiLogger;
			this._getLocalPlayer = getLocalPlayer;
		}

		public void Send()
		{
			Resolution currentResolution = this._screenResolutionController.GetCurrentResolution();
			IPlayer player = this._getLocalPlayer.Get();
			int systemMemorySize = SystemInfo.systemMemorySize;
			ulong availablePhysicalMemory = Platform.Current.GetAvailablePhysicalMemory();
			string operatingSystem = SystemInfo.operatingSystem;
			string processorType = SystemInfo.processorType;
			int processorCount = SystemInfo.processorCount;
			int processorFrequency = SystemInfo.processorFrequency;
			GraphicsDeviceType graphicsDeviceType = SystemInfo.graphicsDeviceType;
			string graphicsDeviceName = SystemInfo.graphicsDeviceName;
			string graphicsDeviceVendor = SystemInfo.graphicsDeviceVendor;
			int dedicatedVideoMemorySize = UnityInterface.GetDedicatedVideoMemorySize();
			string graphicsDeviceVersion = SystemInfo.graphicsDeviceVersion;
			int graphicsDeviceID = SystemInfo.graphicsDeviceID;
			int graphicsDeviceVendorID = SystemInfo.graphicsDeviceVendorID;
			int num = (Display.displays == null) ? 1 : Display.displays.Length;
			ScreenResolutionController.QualityLevels qualityLevel = ScreenResolutionController.QualityLevel;
			this._unidentifiedPlayerBiLogger.BiLogClientMsg(new UnidentifiedPlayerSpecsLog
			{
				InnerAction = 53,
				SystemMemorySize = systemMemorySize,
				FreeMemorySize = availablePhysicalMemory.ToString(),
				OperatingSystem = operatingSystem,
				ProcessorType = processorType,
				ProcessorCount = processorCount,
				ProcessorFrequency = processorFrequency,
				GraphicsApi = graphicsDeviceType.ToString(),
				GraphicsDeviceName = graphicsDeviceName,
				GraphicsDeviceVendor = graphicsDeviceVendor,
				GraphicsMemorySize = dedicatedVideoMemorySize,
				GraphicsDeviceVersion = graphicsDeviceVersion,
				GraphicsDeviceId = graphicsDeviceID,
				GraphicsDeviceVendorId = graphicsDeviceVendorID,
				GraphicsDisplayCount = num,
				GraphicsDisplayWidth = currentResolution.Width,
				GraphicsDisplayHeight = currentResolution.Height,
				GraphicsQuality = qualityLevel.ToString(),
				UniversalId = player.UniversalId
			});
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("{");
			stringBuilder.AppendFormat("\"SystemMemorySize\": {0},", systemMemorySize);
			stringBuilder.AppendFormat("\"FreeMemorySize\": {0},", availablePhysicalMemory);
			stringBuilder.AppendFormat("\"OperatingSystem\": \"{0}\",", operatingSystem);
			stringBuilder.AppendFormat("\"ProcessorType\": \"{0}\",", processorType);
			stringBuilder.AppendFormat("\"ProcessorCount\": {0},", processorCount);
			stringBuilder.AppendFormat("\"ProcessorFrequency\": {0},", processorFrequency);
			stringBuilder.AppendFormat("\"GraphicsAPI\": \"{0}\",", graphicsDeviceType);
			stringBuilder.AppendFormat("\"GraphicsDeviceName\": \"{0}\",", graphicsDeviceName);
			stringBuilder.AppendFormat("\"GraphicsDeviceVendor\": \"{0}\",", graphicsDeviceVendor);
			stringBuilder.AppendFormat("\"GraphicsMemorySize\": {0},", dedicatedVideoMemorySize);
			stringBuilder.AppendFormat("\"GraphicsDeviceVersion\": \"{0}\",", graphicsDeviceVersion);
			stringBuilder.AppendFormat("\"GraphicsDeviceID\": {0},", graphicsDeviceID);
			stringBuilder.AppendFormat("\"GraphicsDeviceVendorID\": {0},", graphicsDeviceVendorID);
			stringBuilder.AppendFormat("\"GraphicsDisplayCount\": {0},", num);
			stringBuilder.AppendFormat("\"GraphicsDisplayWidth\": {0},", currentResolution.Width);
			stringBuilder.AppendFormat("\"GraphicsDisplayHeight\": {0},", currentResolution.Height);
			stringBuilder.AppendFormat("\"GraphicsQuality\": \"{0}\",", qualityLevel);
			stringBuilder.AppendFormat("\"PlayerId\": \"{0}\"", player.UniversalId);
			stringBuilder.Append("}");
			string text = stringBuilder.ToString();
			this._clientBiLogger.BILogClientMsg(53, text, true);
		}

		private readonly IScreenResolutionController _screenResolutionController;

		private readonly IClientBILogger _clientBiLogger;

		private readonly IUnidentifiedPlayerBiLogger _unidentifiedPlayerBiLogger;

		private readonly IGetLocalPlayer _getLocalPlayer;
	}
}
