using System.IO;
using System.Reflection;

namespace MHLab.PATCH.Settings
{
	public class Settings : Singleton<Settings>
	{
		public static string APP_PATH = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + Path.DirectorySeparatorChar;

		public static string ASSETS_PATH = "Assets" + Path.DirectorySeparatorChar + "MHLab" + Path.DirectorySeparatorChar + "PATCH" + Path.DirectorySeparatorChar;

		public static string LANGUAGE_PATH = ASSETS_PATH + "Resources" + Path.DirectorySeparatorChar + "Localizatron" + Path.DirectorySeparatorChar + "Locale" + Path.DirectorySeparatorChar;

		public static string SAVING_LANGUAGE_PATH = Path.DirectorySeparatorChar + "Resources" + Path.DirectorySeparatorChar + "Localizatron" + Path.DirectorySeparatorChar + "Locale" + Path.DirectorySeparatorChar;

		public static string LANGUAGE_EXTENSION = ".txt";

		public static string LANGUAGE_DEFAULT = "en_EN";
	}
}
