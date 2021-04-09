using BepInEx;
using BepInEx.Logging;

using KKAPI.Maker;

namespace SMAP
{
	[BepInDependency("com.joan6694.illusionplugins.moreaccessories")]
	[BepInIncompatibility("marco.MoreAccParents")]
	[BepInPlugin(GUID, Name, Version)]
	public partial class SMAP : BaseUnityPlugin
	{
		public const string GUID = "SMAP";
		public const string Name = "Stimulate More Accessory Parents";
		public const string Version = "1.0.0.0";

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
