using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ctrlC.Constants
{
	public static class PathConstants
	{
		public static string EnviromentPath { get; private set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData", "LocalLow", "Colossal Order", "Cities Skylines II");
		public static string RelativePath { get; private set; } = Path.Combine(".ctrlC~"); // To ensure the game doesnt load our custom prefabs, we put a "." infront of the folder name.
		public static string PrefabStorage { get; private set; } = Path.Combine(EnviromentPath, RelativePath);
		public static string ModPath { get; internal set; } = ""; // this is set from Mod.cs
		public static string PatreonLink { get; private set; } = "https://www.patreon.com/jk142";
		public static string XLink { get; private set; } = "https://x.com/Jk142_Cool";
    }
}
