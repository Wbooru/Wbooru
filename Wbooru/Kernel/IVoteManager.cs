using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wbooru.Galleries;
using Wbooru.Kernel.DI;
using Wbooru.Models.Gallery;

namespace Wbooru.Kernel
{
    public interface IVoteManager : IImplementInjectable
    {
        Task<(bool action_success, string error_message)> SetVote(Gallery gallery, GalleryItem item, bool is_vote);
        Task<(bool action_success, string error_message)> GetVote(Gallery gallery, GalleryItem item);
    }
}
