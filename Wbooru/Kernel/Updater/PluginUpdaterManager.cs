using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wbooru.Network;
using Wbooru.PluginExt;
using Wbooru.Settings;
using Wbooru.Utils;

namespace Wbooru.Kernel.Updater
{
    public static class PluginUpdaterManager
    {
        public static Dictionary<Type, ReleaseInfo> UpdatablePluginsInfo { get; } = new Dictionary<Type, ReleaseInfo>();

        public static void CheckPluginUpdatable(IPluginUpdatable plugin)
        {
            var type = plugin.GetType();
            UpdatablePluginsInfo[type] = null;

            var current_version = plugin.CurrentPluginVersion;
            var releases_list = plugin.GetReleaseInfoList().Where(x => x.Version > current_version).OrderBy(x => x.ReleaseDate);

            if (!releases_list.Any())
                return;

            var release_info = (SettingManager.LoadSetting<GlobalSetting>().UpdatableTargetVersion == GlobalSetting.UpdatableTarget.Preview ? releases_list.FirstOrDefault(x => x.Type == ReleaseInfo.ReleaseType.Preview) : null) ?? releases_list.FirstOrDefault(x => x.Type == ReleaseInfo.ReleaseType.Stable);

            UpdatablePluginsInfo[type] = release_info;
        }

        internal static void BeginPluginUpdate(IEnumerable<IPluginUpdatable> updatables,Action<string> reporter = null)
        {
            updatables = updatables.Where(x => UpdatablePluginsInfo.TryGetValue(x.GetType(), out var r) && r is ReleaseInfo release);
            var update_params = updatables.Select(x => ((PluginInfo)x, UpdatablePluginsInfo[x.GetType()])).ToArray();

            var command_line = "-update_plugin ";

            foreach (var (plugin,release_info) in update_params)
            {
                var file_save_path = Path.GetTempFileName();
                var response = RequestHelper.CreateDeafult(release_info.DownloadURL);

                var length = response.ContentLength;
                var buffer = new byte[1024];

                reporter?.Invoke($"Started download plugin {plugin.GetType().Name} update file : {release_info.DownloadURL}");

                using (var net_stream = response.GetResponseStream())
                {
                    using (var file_stream = File.OpenWrite(file_save_path))
                    {
                        int read = 0;
                        do
                        {
                            read = net_stream.Read(buffer, 0, buffer.Length);
                            file_stream.Write(buffer, 0, read);
                        } while (read != 0);
                    }
                }

                command_line += $"-update_plugin_zip_file=\"{file_save_path}\" ";
                reporter?.Invoke($"Finished all download update files");
            }

            var exe_path = Process.GetCurrentProcess().MainModule.FileName;

            Process.Start(exe_path, command_line);
            App.UnusualSafeExit();
        }

        internal static void ApplyPluginUpdate()
        {
            var zip_files = CommandLine.ValueOptions
                .Where(x => x.Name == "update_plugin_zip_file")
                .Select(x => x.Value);

            var exe_path = Directory.GetParent(Process.GetCurrentProcess().MainModule.FileName).FullName;

            foreach (var file in zip_files)
            {
                try
                {
                    using (ZipArchive archive = ZipFile.Open(file, ZipArchiveMode.Read))
                    {
                        archive.ExtractToDirectory(exe_path);
                    }
                }
                catch (Exception e)
                {
                    Log.Error($"Applied zip file {file} failed : "+e.Message);
                }

                Log.Info("Applied zip file extract:" + file);
            }

            Process.Start(Process.GetCurrentProcess().MainModule.FileName);
            App.UnusualSafeExit();
        }
    }
}
