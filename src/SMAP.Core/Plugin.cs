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
#if DEBUG
		public const string Name = "Stimulate More Accessory Parents (Debug Build)";
#else
		public const string Name = "Stimulate More Accessory Parents";
#endif
		public const string Version = "1.4.0.0";

		internal static ManualLogSource _logger;

		private void Start()
		{
			_logger = base.Logger;

#if KK
			if (JetPack.MoreAccessories.BuggyBootleg)
			{
#if DEBUG
				if (!JetPack.MoreAccessories.Installed)
				{
					_logger.LogError($"Backward compatibility in BuggyBootleg MoreAccessories is disabled");
					return;
				}
#else
				_logger.LogError($"Could not load {Name} {Version} because it is incompatible with MoreAccessories experimental build");
				return;
#endif
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
