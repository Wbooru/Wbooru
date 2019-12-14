using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wbooru.Kernel.Updater.PluginMarket;
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

            var release_info = (SettingManager.LoadSetting<GlobalSetting>().UpdatableTargetVersion == GlobalSetting.UpdatableTarget.Preview ? releases_list.FirstOrDefault(x => x.ReleaseType == ReleaseType.Preview) : null) ?? releases_list.FirstOrDefault(x => x.ReleaseType == ReleaseType.Stable);

            UpdatablePluginsInfo[type] = release_info;
        }

        internal static void BeginPluginUpdate(IEnumerable<IPluginUpdatable> updatables,Action<string> reporter = null)
        {
            updatables = updatables.Where(x => UpdatablePluginsInfo.TryGetValue(x.GetType(), out var r) && r is ReleaseInfo release);
            var update_params = updatables.Select(x => ((PluginInfo)x, UpdatablePluginsInfo[x.GetType()])).ToArray();

            var command_line = "-update_plugin ";

            reporter?.Invoke($"开始下载插件压缩包....");

            foreach (var (plugin,release_info) in update_params)
            {
                var file_save_path = Path.GetTempFileName();
                reporter?.Invoke($"开始下载插件{plugin.GetType().Name} 的压缩包 {release_info.DownloadURL} 至 :{file_save_path}");

                try
                {
                    var response = RequestHelper.CreateDeafult(release_info.DownloadURL);

                    var length = response.ContentLength;
                    reporter?.Invoke($"ContentLength = {length}");
                    
                    var buffer = new byte[1024];

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

                    reporter?.Invoke($"插件 {plugin.GetType().Name} 下载成功!");
                }
                catch (Exception)
                {

                    throw;
                }

                command_line += $"-update_plugin_zip_file=\"{file_save_path}\" ";
            }

            reporter?.Invoke($"Finished all download update files");

            var exe_path = Process.GetCurrentProcess().MainModule.FileName;

            reporter?.Invoke($"command line = {command_line}");
            reporter?.Invoke($"exe path = {command_line}");
            Process.Start(exe_path, command_line);

            reporter?.Invoke("Started another Wbooru program. Now wait 3 seconds , and then exit.");
            Thread.Sleep(3000);

            App.UnusualSafeExit();
        }

        internal static void InstallPluginRelease(PluginMarketRelease release,Action<string> reporter)
        {
            var file_save_path = Path.GetTempFileName();

            var response = RequestHelper.CreateDeafult(release.DownloadURL);

            var length = response.ContentLength;

            var buffer = new byte[1024];

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

            var command_line = $"-update_plugin -update_plugin_zip_file=\"{file_save_path}\" ";
            reporter?.Invoke($"Finished all download update files");

            var exe_path = Process.GetCurrentProcess().MainModule.FileName;

            reporter?.Invoke($"command line = {command_line}");
            reporter?.Invoke($"exe path = {command_line}");
            Process.Start(exe_path, command_line);

            reporter?.Invoke("Started another Wbooru program. Now wait 3 seconds , and then exit.");
            Thread.Sleep(3000);

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
