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
    [Export(typeof(SettingManager))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class SettingManager
    {
        const string CONFIG_FILE_PATH = "./setting.json";

        private bool load = false;

        SettingFileEntity entity = new SettingFileEntity();

        JObject load_object;

        public object LoadSetting(Type setting_type)
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

        public T LoadSetting<T>() where T:SettingBase
        {
            return LoadSetting(typeof(T)) as T;
        }

        public void LoadSettingFile()
        {
            try
            {
                load = true;

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

        public void SaveSettingFile()
        {
            try
            {
                foreach (var item in entity.Settings.Values)
                    item.OnBeforeSave();

                using var writer = new StreamWriter(File.OpenWrite(CONFIG_FILE_PATH));

                var str = JsonConvert.SerializeObject(entity, Formatting.Indented);
                writer.Write(str);

            }catch(Exception e)
            {
                Log.Error($"save settings failed:{e}");
            }
        }
    }
}
