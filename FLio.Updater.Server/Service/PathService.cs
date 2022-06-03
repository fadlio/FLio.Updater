using System.IO;
using System.Reflection;

namespace FLio.Updater.Server.Service
{
    public static class PathService
    {
        public static readonly string BaseFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        public static readonly string BinFolder = Path.Join(BaseFolder, "bin");
        public static readonly string BinLogsFolder = Path.Join(BaseFolder, "logs", "bin");
        public static readonly string LoaderLogsFolder = Path.Join(BaseFolder, "logs", "loader");
    }
}
