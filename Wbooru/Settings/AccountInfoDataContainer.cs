using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Wbooru.Galleries;
using Wbooru.Galleries.SupportFeatures;
using Wbooru.Utils;

namespace Wbooru.Settings
{
    public class AccountInfoDataContainer : SettingBase
    {
        public class AccountInfoData
        {
            public Type AccountInfoType { get; set; }
            public string Data { get; set; }
        }

        [JsonProperty("Data")]
        public JObject DataObject { get; set; } = new JObject();

        public void SaveAccountInfoData(Gallery gallery, AccountInfo info)
        {
            Debug.Assert(gallery.SupportFeatures.HasFlag(GallerySupportFeature.Account), $"gallery {gallery.GalleryName} not support Account");

            DataObject[gallery.GalleryName] = JToken.FromObject(new AccountInfoData()
            {
                AccountInfoType = info.GetType(),
                Data = EncryptString(JsonConvert.SerializeObject(info))
            });

            Log.Info($"Saved gallery {gallery.GalleryName} account info {info.GetType().Name}");
            Setting.ForceSave();
        }

        TripleDESCryptoServiceProvider symm = new TripleDESCryptoServiceProvider()
        {
            Key = Convert.FromBase64String("PSLzUgjNHM8d9YYQhpTrfDlGSl57yxM8"),
            IV = Convert.FromBase64String("7R2f6eAg83Y=")
        };

        private byte[] Decrypt(byte[] data)
        {
            var length = data.Length;
            using var ms = new MemoryStream(data);

            CryptoStream cs = new CryptoStream(ms,
                symm.CreateDecryptor(symm.Key, symm.IV),
                CryptoStreamMode.Read);

            byte[] result = new byte[length];
            cs.Read(result, 0, result.Length);
            return result;
        }

        private byte[] Encrypt(byte[] data, int length)
        {
            using var ms = new MemoryStream();

            var cs = new CryptoStream(ms,
                symm.CreateEncryptor(symm.Key, symm.IV),
                CryptoStreamMode.Write);

            cs.Write(data, 0, length);
            cs.FlushFinalBlock();

            byte[] ret = ms.ToArray();

            cs.Close();
            ms.Close();

            return ret;
        }

        private string EncryptString(string text)
        {
            var buffer = Encoding.UTF8.GetBytes(text);
            return Convert.ToBase64String(Encrypt(buffer, buffer.Length));
        }

        private string DecryptString(string data)
        {
            return Encoding.UTF8.GetString(Decrypt(Convert.FromBase64String(data))).TrimEnd('\0');
        }

        public AccountInfo TryGetAccountInfoData(Gallery gallery)
        {
            if (!DataObject.TryGetValue(gallery.GalleryName, out var token))
                return null;

            if (!(token.ToObject<AccountInfoData>() is AccountInfoData data))
                return null;

            try
            {
                var type = data.AccountInfoType;

                var json = DecryptString(data.Data);
                var t = JsonConvert.DeserializeObject(json, type) as AccountInfo;

                Log.Info($"Deserialize account info {gallery.GalleryName} -> {type.Name}");

                return t;
            }
            catch (Exception e)
            {
                Log.Error($"Can't load account info for gallery {gallery.GalleryName}");
                ExceptionHelper.DebugThrow(e);

                return null;
            }
        }

        public static AccountInfoDataContainer Instance => Setting<AccountInfoDataContainer>.Current;

        public void CleanAccountInfo(Gallery currentGallery)
        {
            DataObject.Remove(currentGallery.GalleryName);
        }
    }
}
