using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Wbooru.Settings
{
    [Export(typeof(SettingManager))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class SettingManager
    {
        const string CONFIG_FILE_PATH = "./setting.json";

        private bool load = false;

        private SettingFileEntity entity=new SettingFileEntity();

        public T LoadSetting<T>() where T:SettingBase,new()
        {
            if (!load)
                LoadSettingFile();

            var name = typeof(T).Name;

            if (!entity.Settings.TryGetValue(name,out var setting))
            {
                setting = new T();
                entity.Settings[name] = setting;

                Log.Info($"{name} setting object not found , created default.");
            }

            return (T)setting;
        }

        public void LoadSettingFile()
        {
            try
            {
                load = true;

                using var reader = File.OpenText(CONFIG_FILE_PATH);

                entity = JsonConvert.DeserializeObject<SettingFileEntity>(reader.ReadToEnd()) ?? entity;

                foreach (var item in entity.Settings.Values)
                    item.OnAfterLoad();
            }
            catch (Exception e)
            {
                Log.Error($"load settings failed:{e}");
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
