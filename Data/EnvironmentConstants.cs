using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ctrlC.Data
{
	public static class EnvironmentConstants
	{
		
		public static string EnviromentPath { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData", "LocalLow", "Colossal Order", "Cities Skylines II");
		public static string RelativePath { get; } = Path.Combine(".ctrlC~");
		public static string PrefabStorage { get; } = Path.Combine(EnviromentPath, RelativePath);

		public static string ModPath { get; set; } = "";

		public static string PatreonLink { get; set; } = "https://www.patreon.com/jk142";
		public static string XLink { get; set; } = "https://x.com/Jk142_Cool";


    }
}
