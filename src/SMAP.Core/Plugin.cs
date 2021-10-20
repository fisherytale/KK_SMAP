using System;

using BepInEx;
using BepInEx.Logging;

using KKAPI.Maker;

namespace SMAP
{
	[BepInPlugin(GUID, Name, Version)]
	[BepInDependency("marco.kkapi", "1.17")]
	[BepInDependency("madevil.JetPack")]
#if MoreAcc
	[BepInDependency("com.joan6694.illusionplugins.moreaccessories")]
#endif
	[BepInIncompatibility("marco.MoreAccParents")]
	public partial class SMAP : BaseUnityPlugin
	{
		public const string GUID = "SMAP";
		public const string Name = "Stimulate More Accessory Parents";
		public const string Version = "1.3.1.0";

		internal static new ManualLogSource Logger;

		private void Start()
		{
			Logger = base.Logger;
			BepInEx.Bootstrap.Chainloader.PluginInfos.TryGetValue("com.joan6694.illusionplugins.moreaccessories", out PluginInfo PluginInfo);
#if KK && !DEBUG
			if (PluginInfo?.Instance != null && PluginInfo.Metadata.Version.CompareTo(new Version("2.0")) > -1)
			{
				Logger.LogError($"Could not load {Name} {Version} because it is incompatible with MoreAccessories experimental build");
				return;
			}
#endif
			Hooks.Initialize();

			MakerAPI.MakerFinishedLoading += (_sender, _args) =>
			{
				_makerConfigWindow = gameObject.AddComponent<SMAPUI>();
			};

			MakerAPI.MakerExiting += (_sender, _args) =>
			{
				_tglSMAP = null;
				Destroy(_makerConfigWindow);
			};
		}
	}
}
