using BepInEx;
using BepInEx.Logging;

using KKAPI.Maker;

namespace SMAP
{
	[BepInPlugin(GUID, Name, Version)]
	[BepInDependency("marco.kkapi")]
	[BepInDependency("madevil.JetPack")]
	[BepInDependency("com.joan6694.illusionplugins.moreaccessories")]
	[BepInIncompatibility("marco.MoreAccParents")]
	public partial class SMAP : BaseUnityPlugin
	{
		public const string GUID = "SMAP";
		public const string Name = "Stimulate More Accessory Parents";
		public const string Version = "1.2.1.0";

		internal static new ManualLogSource Logger;

		private void Start()
		{
			Logger = base.Logger;

			Hooks.Initialize();

			MakerAPI.MakerFinishedLoading += (s, e) =>
			{
				_makerConfigWindow = gameObject.AddComponent<SMAPUI>();
			};

			MakerAPI.MakerExiting += (s, e) =>
			{
				_tglSMAP = null;
				Destroy(_makerConfigWindow);
			};
		}
	}
}
