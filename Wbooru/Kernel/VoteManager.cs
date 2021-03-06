﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wbooru.Galleries;
using Wbooru.Galleries.SupportFeatures;
using Wbooru.Models.Gallery;

namespace Wbooru.Kernel
{
    public static class VoteManager
    {
        public static async Task<(bool action_success,string error_message)> SetVote(Gallery gallery,GalleryItem item,bool is_vote)
        {
            try
            {
                await Task.Run(() => gallery.Feature<IGalleryVote>().SetVote(item, is_vote));
                return (true,default);
            }
            catch (Exception e)
            {
                Log.Error($"Vote {(is_vote ? "up" : "down")} {gallery.GalleryName} item {item.GalleryItemID} failed:{e.Message}");
                return (false,e.Message);
            }
        }

        public static async Task<(bool action_success, string error_message)> GetVote(Gallery gallery, GalleryItem item)
        {
            try
            {
                return (await Task.Run(() => gallery.Feature<IGalleryVote>()?.IsVoted(item))??false, default);
            }
            catch (Exception e)
            {
                Log.Error($"Vote {gallery.GalleryName} item {item.GalleryItemID} vote value failed:{e.Message}");
                return (false, e.Message);
            }
        }
    }
}
