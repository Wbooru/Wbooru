using CommandLine;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using Wbooru.Network;
using Wbooru.Settings;

namespace Wbooru.Kernel.ProgramUpdater
{
    public static class ProgramUpdater
    {
        private const string UPDATE_EXE_NAME = "updater_temp.exe";
        private const string EXE_NAME = "Wbooru.exe";

        public static Version CurrentProgramVersion => typeof(ProgramUpdater).Assembly.GetName().Version;

        private static ReleaseInfo cached_updatable_release_info;

        public static bool IsUpdatable()
        {
            cached_updatable_release_info = new ReleaseInfo()
            {
                DownloadURL = "https://github.com/MikiraSora/ReOsuStoryboardPlayer/releases/download/v2.4.3/ReOsuStoryboardPlayer.zip"
            };

            return true;

            var option = SettingManager.LoadSetting<GlobalSetting>();

            var releases_url = $"https://api.github.com/repos/MikiraSora/Wbooru/releases";

            try
            {
                if (!(RequestHelper.GetJsonContainer<JArray>(RequestHelper.CreateDeafult(releases_url, req =>
                {
                    req.UserAgent = "WbooruProgramUpdater";
                })) is JArray array))
                    return false;

                var releases = array
                    .Select(x => TryGetReleaseInfo(x))
                    .OfType<ReleaseInfo>().ToArray();

                if (!releases.Any())
                {
                    Log.Info($"There is no any release info ,skip update check.");
                    return false;
                }

                var updatable_releases = releases.Where(x => x.Version > CurrentProgramVersion)
                    .OrderByDescending(x => x.Version);

                cached_updatable_release_info = (option.UpdatableTargetVersion == GlobalSetting.UpdatableTarget.Preview ? releases.FirstOrDefault(x => x.Type == ReleaseInfo.ReleaseType.Preview) : null) ?? releases.FirstOrDefault(x => x.Type == ReleaseInfo.ReleaseType.Stable);

                if (cached_updatable_release_info == null)
                {
                    Log.Info($"There is no any updatable({option.UpdatableTargetVersion}) release info ,skip update check.");
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                Log.Error("Updater occured error : " + e.Message);
                return false;
            }
        }

        public static void BeginUpdate(Action<long, long> reporter = null,Func<bool> restart_comfirm=null)
        {
            if (cached_updatable_release_info == null)
                throw new Exception("Must call IsUpdatable() before.");

            var file_save_path = Path.GetTempFileName();

            #region Download Packaged File

            var response = RequestHelper.CreateDeafult(cached_updatable_release_info.DownloadURL);

            var length = response.ContentLength;
            var buffer = new byte[1024];
            long total_read = 0;

            using (var net_stream = response.GetResponseStream())
            {
                using (var file_stream = File.OpenWrite(file_save_path))
                {
                    int read = 0;
                    do
                    {
                        read = net_stream.Read(buffer, 0, buffer.Length);
                        file_stream.Write(buffer, 0, read);
                        total_read += read;
                        reporter?.Invoke(total_read, length);
                    } while (read != 0);
                }
            }

            #endregion

            #region Unzip Packaged File

            var unzip_target_path = Path.Combine(Path.GetTempPath(), DateTime.Now.GetHashCode().ToString());
            Directory.CreateDirectory(unzip_target_path);

            using (ZipArchive archive = ZipFile.Open(file_save_path, ZipArchiveMode.Read))
            {
                archive.ExtractToDirectory(unzip_target_path);
            }

            #endregion

            #region Create new temp exe

            var exe_file_path = Directory.EnumerateFiles(unzip_target_path, EXE_NAME, SearchOption.AllDirectories).FirstOrDefault();
            unzip_target_path = Directory.GetParent(exe_file_path).FullName;
            var updater_exe_file = Path.Combine(unzip_target_path, UPDATE_EXE_NAME);
            File.Copy(exe_file_path, updater_exe_file, true);

            #endregion

            #region Comfirm and Restart

            if (restart_comfirm?.Invoke()??true)
            {
                var current_path = AppDomain.CurrentDomain.BaseDirectory;

                var command_line = $"-update -UpdateTargetPath \"{current_path}\"";

                Process.Start(new ProcessStartInfo(updater_exe_file, command_line));

                App.UnusualSafeExit();
            }

            #endregion
        }

        public static void ApplyUpdate()
        {
            var command_line = string.Join(" ", Environment.GetCommandLineArgs());

            var current_exe_name = Path.GetFileName(Process.GetCurrentProcess().Modules[0].FileName);
            var current_path = AppDomain.CurrentDomain.BaseDirectory;
            var target_path = Regex.Match(command_line, @"-UpdateTargetPath\s+\""(.+?)\""\s*").Groups[1].Value;

            var files = Directory.EnumerateFiles(current_path).Where(x => Path.GetFileName(x) != current_exe_name).ToArray();

            for (int i = 0; i < files.Length; i++)
            {
                var source_file = files[i];
                var display_file_path = string.Empty;

                try
                {
                    display_file_path = RelativePath(current_path, source_file);
                    Log.Info($"Copy file({i + 1}/{files.Length}):{display_file_path}");
                    CopyRelativeFile(source_file, current_path, target_path);
                }
                catch (Exception e)
                {
                    Log.Error($"Copy file \"{display_file_path}\" failed:{e.Message}");
                }
            }

            MessageBox.Show("更新成功!");
        }

        private static void CopyRelativeFile(string source_file_path, string source_root_folder, string destination_root_folder)
        {
            if (!File.Exists(source_file_path))
                return;

            var source_relative_path = RelativePath(source_root_folder, source_file_path);

            var distination_file_path = Path.GetFullPath(Path.Combine(destination_root_folder, source_relative_path));

            var distination_dir_path = Path.GetDirectoryName(distination_file_path);

            Directory.CreateDirectory(distination_dir_path);

            File.Copy(source_file_path, distination_file_path, true);
        }

        private static string RelativePath(string absolutePath, string relativeTo)
        {
            string[] absoluteDirectories = absolutePath.Split('\\');
            string[] relativeDirectories = relativeTo.Split('\\');

            int length = absoluteDirectories.Length < relativeDirectories.Length ? absoluteDirectories.Length : relativeDirectories.Length;

            int lastCommonRoot = -1;
            int index;

            for (index = 0; index < length; index++)
                if (absoluteDirectories[index] == relativeDirectories[index])
                    lastCommonRoot = index;
                else
                    break;

            if (lastCommonRoot == -1)
                throw new ArgumentException("Paths do not have a common base");

            StringBuilder relativePath = new StringBuilder();

            for (index = lastCommonRoot + 1; index < absoluteDirectories.Length; index++)
                if (absoluteDirectories[index].Length > 0)
                    relativePath.Append("..\\");

            for (index = lastCommonRoot + 1; index < relativeDirectories.Length - 1; index++)
                relativePath.Append(relativeDirectories[index] + "\\");
            relativePath.Append(relativeDirectories[relativeDirectories.Length - 1]);

            return relativePath.ToString();
        }

        private static ReleaseInfo TryGetReleaseInfo(JToken x)
        {
            try
            {
                return new ReleaseInfo
                {
                    Description = x["body"].ToString(),
                    ReleaseDate = DateTime.Parse(x["published_at"].ToString()),
                    Type = x["prerelease"].ToObject<bool>() ? ReleaseInfo.ReleaseType.Preview : ReleaseInfo.ReleaseType.Stable,
                    ReleaseURL = x["html_url"].ToString(),
                    Version = Version.Parse(x["tag_name"].ToString().TrimStart('v')),
                    DownloadURL = ((x["assets"] as JArray)?["browser_download_url"])?.ToString()
                };
            }
            catch (Exception e)
            {
                Log.Error($"Can't parse release info from json content: {e.Message} \n {x.ToString()}");
                return null;
            }
        }

        class ReleaseInfo
        {
            public enum ReleaseType
            {
                Stable, Preview
            }

            public ReleaseType Type { get; set; }
            public Version Version { get; set; }
            public DateTime ReleaseDate { get; set; }
            public string DownloadURL { get; set; }
            public string ReleaseURL { get; set; }
            public string Description { get; set; }
        }
    }
}
