using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wbooru.Galleries;
using Wbooru.Galleries.SupportFeatures;
using Wbooru.Kernel.DI;
using Wbooru.Models.Gallery;

namespace Wbooru.Kernel.ManagerImpl
{
    [PriorityExport(typeof(IVoteManager), Priority = 0)]
    [PartCreationPolicy(CreationPolicy.Shared)]
    internal class VoteManager : IVoteManager
    {
        public async Task<(bool action_success, string error_message)> SetVote(Gallery gallery, GalleryItem item, bool is_vote)
        {
            try
            {
                await gallery.Feature<IGalleryVote>().SetVoteAsync(item, is_vote);
                return (true, default);
            }
            catch (Exception e)
            {
                Log.Error($"Vote {(is_vote ? "up" : "down")} {gallery.GalleryName} item {item.GalleryItemID} failed:{e.Message}");
                return (false, e.Message);
            }
        }

        public async Task<(bool action_success, string error_message)> GetVote(Gallery gallery, GalleryItem item)
        {
            try
            {
                return (await gallery.Feature<IGalleryVote>()?.IsVotedAsync(item), default);
            }
            catch (Exception e)
            {
                Log.Error($"Vote {gallery.GalleryName} item {item.GalleryItemID} vote value failed:{e.Message}");
                return (false, e.Message);
            }
        }
    }
}
