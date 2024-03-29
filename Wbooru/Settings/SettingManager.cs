﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Wbooru.Utils;

namespace Wbooru.Settings
{
    public static class SettingManager
    { 
        const string BACKUP_CONFIG_FILES_PATH = "./setting_backup";
        const string CONFIG_FILE_PATH = "./setting.json";

        private static bool load = false;

        private static object save_file_locker = new object();

        private static SettingFileEntity setting_object_cache = new SettingFileEntity();

        private static JToken SettingObject => load_object["Settings"] ?? (load_object["Settings"] = new JObject());

        private static JObject load_object;

        public static object LoadSetting(Type setting_type)
        {
            Debug.Assert(setting_type.IsSubclassOf(typeof(SettingBase)), "param setting_type must be subclass of SettingBase");

            if (!load)
                LoadSettingFile();

            var name = setting_type.Name;

            if (!setting_object_cache.Settings.TryGetValue(name, out var setting))
            {
                //if load_object contain type we need.
                try
                {
                    setting = SettingObject[name]?.ToObject(setting_type) as SettingBase;
                    Log.Info($"{name} created from cached config file content.");
                }
                catch { }

                if (setting == null)
                {
                    setting = setting_type.Assembly.CreateInstance(setting_type.FullName) as SettingBase;

                    Log.Info($"{name} setting object not found , created default.");
                }

                try
                {
                    setting.OnAfterLoad();
                }
                catch (Exception e)
                {
                    Log.Error($"Call {setting.GetType().Name}.OnAfterLoad() failed :{e.Message}");
                    ExceptionHelper.DebugThrow(e);
                }

                setting_object_cache.Settings[name] = setting;
            }

            return setting;
        }

        public static T LoadSetting<T>() where T:SettingBase
        {
            return LoadSetting(typeof(T)) as T;
        }

        internal static void LoadSettingFile()
        {
            try
            {
                load = true;

                var config_path = Path.GetFullPath(CONFIG_FILE_PATH);

                if (!File.Exists(config_path))
                {
                    SaveSettingFileInternal();//create new deafult file
                    Log.Info($"Created new deafult config file to {config_path}");

                    load_object = new JObject();
                }
                else
                {
                    try
                    {
                        using var reader = File.OpenText(config_path);

                        load_object = JsonConvert.DeserializeObject(reader.ReadToEnd()) as JObject ?? new JObject();

                        Log.Info($"Loaded config file from {config_path}");
                    }
                    catch (Exception e)
                    {
                        Directory.CreateDirectory(BACKUP_CONFIG_FILES_PATH);
                        var backup_file_path = Path.Combine(BACKUP_CONFIG_FILES_PATH, $"{Path.GetFileNameWithoutExtension(config_path)}.{FileNameHelper.FilterFileName(DateTime.Now.ToString("yyyy_MM_dd HH_mm_ss_fff"))}.backup.json");
                        Log.Error($"loading settings failed:{e},backup current setting file for protecting.copy file from {config_path} to {backup_file_path}");
                        File.Copy(config_path, backup_file_path, true);

                        throw;
                    }
                }
            }
            catch (Exception e)
            {
                ExceptionHelper.DebugThrow(e);
                load_object = new JObject();
            }
        }

        internal static void SaveSettingFile()
        {
            lock (save_file_locker)
            {
                try
                {
                    foreach (var pair in setting_object_cache.Settings)
                    {
                        var item = pair.Value;
                        var name = pair.Key;

                        try
                        {
                            item.OnBeforeSave();
                        }
                        catch (Exception e)
                        {
                            Log.Error($"Call {name}.OnBeforeSave() failed :{e.Message}");
                            ExceptionHelper.DebugThrow(e);
                        }
                        
                        //apply updated cache setting object into entity file object.
                        try
                        {
                            SettingObject[name] = JObject.FromObject(item);
                        }
                        catch (Exception e)
                        {
                            Log.Error($"Replace {name} failed :{e.Message}");
                            ExceptionHelper.DebugThrow(e);
                        }
                    }

                    SaveSettingFileInternal();

                    Log.Info($"Saved config content to file:{Path.GetFullPath(CONFIG_FILE_PATH)}");
                }
                catch (Exception e)
                {
                    Log.Error($"save settings failed:{e}");
                }
            }
        }

        private static void SaveSettingFileInternal()
        {
            using var writer = new StreamWriter(File.Open(CONFIG_FILE_PATH, FileMode.Create));
            var str = JsonConvert.SerializeObject(load_object, Formatting.Indented);
            writer.Write(str);
        }

        public static void ResetSetting(Type setting_type)
        {
            var name = setting_type.Name;

            try
            {
                var setting = setting_type.Assembly.CreateInstance(setting_type.FullName) as SettingBase;
                setting_object_cache.Settings[name] = setting;

                Log.Info($"Reset(reflect-create) config {name} object successfully.");
            }
            catch (Exception e)
            {
                Log.Error($"Reset(reflect-create) config {name} object failed : {e.Message}");
            }
        }

        public static void ResetSetting<T>() => ResetSetting(typeof(T));
    }
}
