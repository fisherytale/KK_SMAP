using System;

using BepInEx;
using BepInEx.Logging;

using KKAPI.Maker;

namespace SMAP
{
	[BepInPlugin(GUID, Name, Version)]
	[BepInDependency("marco.kkapi", "1.17")]
	[BepInDependency("com.joan6694.illusionplugins.moreaccessories", "2.0.0")]
	public partial class SMAP : BaseUnityPlugin
	{
		public const string GUID = "SMAP";
#if DEBUG
		public const string Name = "Stimulate More Accessory Parents (Debug Build)";
#else
		public const string Name = "Stimulate More Accessory Parents";
#endif
		public const string Version = "2.0.0.0";

		internal static ManualLogSource _logger;

		private void Start()
		{
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
