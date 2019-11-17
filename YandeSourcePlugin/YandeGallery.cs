using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wbooru;
using Wbooru.Galleries;
using Wbooru.Galleries.SupportFeatures;
using Wbooru.Models;
using Wbooru.Models.Gallery;
using Wbooru.Network;
using Wbooru.PluginExt;
using Wbooru.Settings;
using Wbooru.UI.Pages;
using Wbooru.Utils;

namespace YandeSourcePlugin
{
    [Export(typeof(Gallery))]
    public class YandeGallery : Gallery,
        IGalleryTagSearch,
        IGallerySearchImage ,
        IGalleryItemIteratorFastSkipable,
        IGalleryAccount,
        IGalleryVote
    {
        public override string GalleryName => "Yande";

        public bool IsLoggined => current_account_info!=null;

        public CustomLoginPage CustomLoginPage => null;

        public GlobalSetting setting;

        private YandeAccountInfo current_account_info;

        private CookieContainer cookie_container;

        public YandeGallery()
        {
            setting = SettingManager.LoadSetting<GlobalSetting>();
        }

        public override GalleryImageDetail GetImageDetial(GalleryItem item)
        {
            if (!((item as PictureItem)?.GalleryDetail is GalleryImageDetail detail))
            {
                if (item.GalleryName != GalleryName)
                    throw new Exception($"This item doesn't belong with gallery {GalleryName}.");

                detail = (GetImage(item.GalleryItemID) as PictureItem).GalleryDetail;
            }

            return detail;
        }

        public IEnumerable<GalleryItem> GetImagesInternal(IEnumerable<string> tags=null,int page = 1)
        {
            var limit = SettingManager.LoadSetting<YandeSetting>().PicturesCountPerRequest;
            limit = limit == 0 ? SettingManager.LoadSetting<GlobalSetting>().GetPictureCountPerLoad : limit;

            var base_url = $"https://yande.re/post.json?limit={limit}&";

            if (tags?.Any()??false)
                base_url += $"tags={string.Join("+",tags)}&";

            while (true)
            {
                JArray json=null;

                try
                {
                    var actual_url = $"{base_url}page={page}";

                    var response = RequestHelper.CreateDeafult(actual_url);
                    using var reader = new StreamReader(response.GetResponseStream());

                    json = JsonConvert.DeserializeObject(reader.ReadLine()) as JArray;

                    if (json.Count == 0)
                        break;
                }
                catch (Exception e)
                {
                    ExceptionHelper.DebugThrow(e);
                }

                foreach (var pic_info in json)
                {
                    var item = BuildItem(pic_info);

                    yield return item;
                }

                page++;
            }

            Log<YandeGallery>.Info("there is no pic that gallery could provide.");
        }

        private GalleryItem BuildItem(JToken pic_info)
        {
            PictureItem item = new PictureItem();

            item.GalleryItemID = pic_info["id"].ToString();
            item.PreviewImageDownloadLink = pic_info["preview_url"].ToString();
            item.PreviewImageSize = new Size(pic_info["preview_width"].ToObject<int>(), pic_info["preview_height"].ToObject<int>());

            var detail = new GalleryImageDetail();

            detail.ID = item.GalleryItemID;
            detail.Rate = pic_info["rating"].ToString();
            detail.Tags = pic_info["tags"].ToString().Split(' ').ToList();
            detail.Updater = pic_info["creator_id"].ToString();
            detail.CreateDate = DateTimeOffset.FromUnixTimeSeconds(pic_info["created_at"].ToObject<long>()).DateTime;
            detail.Author = pic_info["author"].ToString();
            detail.Resolution = new Size(pic_info["width"].ToObject<int>(), pic_info["height"].ToObject<int>());
            detail.Score = pic_info["score"].ToString();

            List<DownloadableImageLink> downloads = new List<DownloadableImageLink>();

            downloads.Add(new DownloadableImageLink()
            {
                Description = "Jpeg",
                Size = new Size(pic_info["jpeg_width"].ToObject<int>(), pic_info["jpeg_height"].ToObject<int>()),
                FileLength = pic_info["jpeg_file_size"].ToObject<int>(),
                DownloadLink = pic_info["jpeg_url"].ToString(),
                FullFileName = WebUtility.UrlDecode(Path.GetFileName(pic_info["jpeg_url"].ToString()))
            });

            downloads.Add(new DownloadableImageLink()
            {
                Description = "Preview",
                Size = new Size(pic_info["preview_width"].ToObject<int>(), pic_info["preview_height"].ToObject<int>()),
                FileLength = 0,
                DownloadLink = pic_info["preview_url"].ToString(),
                FullFileName = WebUtility.UrlDecode(Path.GetFileName(pic_info["preview_url"].ToString()))
            });

            downloads.Add(new DownloadableImageLink()
            {
                Description = "Sample",
                Size = new Size(pic_info["sample_width"].ToObject<int>(), pic_info["sample_height"].ToObject<int>()),
                FileLength = pic_info["sample_file_size"].ToObject<int>(),
                DownloadLink = pic_info["sample_url"].ToString(),
                FullFileName = WebUtility.UrlDecode(Path.GetFileName(pic_info["sample_url"].ToString()))
            });

            downloads.Add(new DownloadableImageLink()
            {
                Description = "File",
                Size = new Size(pic_info["width"].ToObject<int>(), pic_info["height"].ToObject<int>()),
                FileLength = pic_info["file_size"].ToObject<int>(),
                DownloadLink = pic_info["file_url"].ToString(),
                FullFileName = WebUtility.UrlDecode(Path.GetFileName(pic_info["file_url"].ToString()))
            });

            detail.DownloadableImageLinks = downloads;

            item.GalleryDetail = detail;

            item.GalleryName = GalleryName;

            item.DownloadFileName = $"{item.GalleryItemID} {string.Join(" ", detail.Tags)}";

            return item;
        }

        public IEnumerable<GalleryItem> SearchImages(IEnumerable<string> keywords)
            => GetImagesInternal(keywords);

        public override IEnumerable<GalleryItem> GetMainPostedImages() => GetImagesInternal();

        public IEnumerable<Tag> SearchTag(string keywords)
        {
            var response = RequestHelper.CreateDeafult($"https://yande.re/tag.json?order=name&limit=0&name={keywords}");
            using var reader = new StreamReader(response.GetResponseStream());

            var arr = JsonConvert.DeserializeObject(reader.ReadLine()) as JArray;

            foreach (var item in arr)
            {
                yield return new Tag()
                {
                    Name = item["name"].ToString(),
                    Type = item["type"].ToString() switch
                    {
                        "0" => TagType.General,
                        "1" => TagType.Artist,
                        //"2" => TagType.Character,
                        "3" => TagType.Copyright,
                        "4" => TagType.Character,
                        "5" => TagType.Circle,
                        "6" => TagType.Faults,
                        _ => TagType.Unknown
                    }
                };
            }
        }

        public override GalleryItem GetImage(string id)
        {
            try
            {
                var response = RequestHelper.CreateDeafult($"https://yande.re/post/show/{id}");

                using var reader = new StreamReader(response.GetResponseStream());
                var content = reader.ReadToEnd();

                const string CONTENT_HEAD = "Post.register_resp(";
                var start_index = content.LastIndexOf(CONTENT_HEAD);

                if (start_index < 0)
                {
                    Log.Warn($"Can't get any information with id {id}.");
                    return null;
                }

                start_index += CONTENT_HEAD.Length;
                StringBuilder builder = new StringBuilder(1024);
                int stack = 1;

                foreach (var ch in content.Skip(start_index))
                {
                    if (ch == ')')
                    {
                        stack--;
                        if (stack == 0)
                            break;
                    }

                    if (ch == '(')
                        stack++;

                    builder.Append(ch);
                }

                var result = JsonConvert.DeserializeObject(builder.ToString()) as JObject;
                return BuildItem((result["posts"] as JArray).FirstOrDefault());
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                ExceptionHelper.DebugThrow(e);
                return null;
            }
        }

        public IEnumerable<GalleryItem> IteratorSkip(int skip_count)
        {
            var limit_count = SettingManager.LoadSetting<GlobalSetting>().GetPictureCountPerLoad;

            var page = skip_count / limit_count + 1;
            skip_count = skip_count % SettingManager.LoadSetting<GlobalSetting>().GetPictureCountPerLoad;

            return GetImagesInternal(null, page).Skip(skip_count);
        }

        public void AccountLogin(AccountInfo info)
        {
            var buffer = SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes($"choujin-steiner--your-password--".Replace("your-password", info.Password)));
            var password_hash = string.Join("", buffer.Select(x => x.ToString("X2")));

            try
            {
                var yande_account = new YandeAccountInfo(info);

                cookie_container = new CookieContainer();

                var response = RequestHelper.CreateDeafult("https://yande.re/user/authenticate", req =>
                {
                    req.Method = "POST";
                    req.CookieContainer = cookie_container;
                    req.ContentType = "application/x-www-form-urlencoded";

                    var csrf_token = WebUtility.UrlEncode(GetCSRFToken(cookie_container));
                    var body = $"authenticity_token={csrf_token}&url=&user%5Bname%5D={info.Name}&user%5Bpassword%5D={info.Password}&commit=Login";

                    using var req_writer = new StreamWriter(req.GetRequestStream());
                    req_writer.Write(body);
                    req_writer.Flush();
                });

                var cookies = cookie_container.GetCookies(response.ResponseUri).OfType<Cookie>().ToArray();

                using var reader = new StreamReader(response.GetResponseStream());
                var content = reader.ReadToEnd();

                foreach (var cookie in cookies)
                {
                    if (cookie.Name == "pass_hash")
                    {
                        yande_account.PasswordHash = cookie.Value;
                        current_account_info = yande_account;
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private string GetCSRFToken(CookieContainer container)
        {
            var req = RequestHelper.CreateDeafult("https://yande.re/user/login", req => req.CookieContainer = container);
            var reader = new StreamReader(req.GetResponseStream());

            /*
             <meta name="csrf-token" content="2s3jOIwFfoOjCxchwh3U06H126ca3Fog7mmRM5AMKyqNKR7c3nBxOAfXEBTB4TBzBMxHbxDnhJhzb+4eEgr/UA==" />
             */

            var token = Regex.Match(reader.ReadToEnd(), @"<meta\s+name=""csrf-token""\s+content=""(.+?)""\s+/>")?.Groups[1].Value;

            return string.IsNullOrWhiteSpace(token)?throw new Exception("无法获取CSRF令牌"):token;
        }

        public void AccountLogout()
        {
            if (!IsLoggined)
                return;

            var response = RequestHelper.CreateDeafult("https://yande.re/user/logout", req =>
            {
                req.CookieContainer = cookie_container;
            }) as HttpWebResponse;

            Log.Info("Logout");

            //clean 

            current_account_info = null;
        }

        public void SetVote(GalleryItem item, bool is_mark)
        {
            if (current_account_info == null)
                throw new Exception("投票功能需要事先用户登录.");

            var score = (is_mark ? SettingManager.LoadSetting<YandeSetting>().VoteValue : 0);
            var url = "https://yande.re/post/vote.json?" + $"score={score}&id={item.GalleryItemID}&password_hash={current_account_info.PasswordHash}&login={current_account_info.Name}";

            var response = RequestHelper.CreateDeafult(url, req => req.Method = "POST");

            if (RequestHelper.GetJsonObject(response) is JObject result)
            {
                if (!result["success"].ToObject<bool>())
                    throw new Exception(result["reason"].ToString());
                else
                    Log.Info($"Voted item {item.GalleryItemID} , score {score}");
            }
            else
                Log.Error($"Can't get json object from response.");
        }

        public bool IsVoted(GalleryItem item)
        {
            if (current_account_info == null) 
                throw new Exception("投票功能需要事先用户登录.");

            var result = RequestHelper.GetJsonObject(RequestHelper.CreateDeafult($"https://yande.re/favorite/list_users.json?id={item.GalleryItemID}"));
            var user_list = result["favorited_users"].ToString().Split(',');

            return user_list
                .Any(x => x.Equals(current_account_info.Name, StringComparison.InvariantCultureIgnoreCase));
        }

        public IEnumerable<GalleryItem> GetVotedGalleryItem()
        {
            if (current_account_info == null)
                throw new Exception("投票功能需要事先用户登录.");

            var score = SettingManager.LoadSetting<YandeSetting>().VoteValue;

            return GetImagesInternal(new[] { $"vote:{score}:mikirasora","order:vote" });
        }
    }
}
