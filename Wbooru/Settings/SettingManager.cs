using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Wbooru.Settings
{
    public static class SettingManager
    {
        const string CONFIG_FILE_PATH = "./setting.json";

        private static bool load = false;

        private static SettingFileEntity entity = new SettingFileEntity();

        private static JObject load_object;

        public static object LoadSetting(Type setting_type)
        {
            Debug.Assert(setting_type.IsSubclassOf(typeof(SettingBase)), "param setting_type must be subclass of SettingBase");

            if (!load)
                LoadSettingFile();

            var name = setting_type.Name;

            if (!entity.Settings.TryGetValue(name, out var setting))
            {
                //if load_object contain type we need.
                try
                {
                    setting = load_object[name]?.ToObject(setting_type) as SettingBase;
                    Log.Info($"{name} created from cached config file content.");
                }
                catch { }

                if (setting == null)
                {
                    setting = setting_type.Assembly.CreateInstance(setting_type.FullName) as SettingBase;
                    Log.Info($"{name} setting object not found , created default.");
                }

                entity.Settings[name] = setting;
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

                Log.Info($"Load config file from {Path.GetFullPath(CONFIG_FILE_PATH)}");

                using var reader = File.OpenText(CONFIG_FILE_PATH);

                load_object = (JObject)(((JObject)JsonConvert.DeserializeObject(reader.ReadToEnd()))["Settings"]) ?? new JObject();

                foreach (var item in entity.Settings.Values)
                    item.OnAfterLoad();
            }
            catch (Exception e)
            {
                Log.Error($"load settings failed:{e}");
                load_object = new JObject();
            }
        }

        internal static void SaveSettingFile()
        {
            try
            {
                foreach (var item in entity.Settings.Values)
                    item.OnBeforeSave();

                using var writer = new StreamWriter(File.Open(CONFIG_FILE_PATH,FileMode.Create));

                var str = JsonConvert.SerializeObject(entity, Formatting.Indented);
                writer.Write(str);

            }catch(Exception e)
            {
                Log.Error($"save settings failed:{e}");
            }
        }

        public static void ResetSetting(Type setting_type)
        {
            var name = setting_type.Name;

            try
            {
                var setting = setting_type.Assembly.CreateInstance(setting_type.FullName) as SettingBase;
                entity.Settings[name] = setting;

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
