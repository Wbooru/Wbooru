using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wbooru.Utils
{
    public static class ZipArchiveExtension
    {
        public static void ExtractToDirectoryEx(this ZipArchive archive, string destinationDirPath , bool overwrite)
        {
            if (!overwrite)
            {
                archive.ExtractToDirectory(destinationDirPath);
                return;
            }

            foreach (var entity in archive.Entries)
            {
                var path = Path.GetFullPath(Path.Combine(destinationDirPath, entity.FullName));

                if (string.IsNullOrWhiteSpace(entity.Name) || entity.Name.EndsWith("\\") || entity.Name.EndsWith("/"))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    Log.Debug($"Check and create directory:{path}");
                }
                else
                {
                    entity.ExtractToFile(path, true);
                    Log.Debug($"Extract zip file:{entity.FullName} -> {path}");
                }
            }
        }
    }
}
