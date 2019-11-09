using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wbooru.Galleries.SupportFeatures;
using Wbooru.Models;
using Wbooru.Models.Gallery;
using Wbooru.PluginExt;

namespace Wbooru.Galleries
{
    public abstract class Gallery
    {
        #region SupportFeatures

        private GallerySupportFeature? _support = null;
        public GallerySupportFeature SupportFeatures => (_support ?? (_support = _CheckGalleryFeatures())) ?? default;
        private GallerySupportFeature _CheckGalleryFeatures()
        {
            GallerySupportFeature support = default;

            if (this is IGalleryMark)
                support |= GallerySupportFeature.Mark;

            if (this is IGalleryVote)
                support |= GallerySupportFeature.Vote;

            if (this is IGalleryAccount)
                support |= GallerySupportFeature.Account;

            if (this is IGalleryTagSearch)
                support |= GallerySupportFeature.TagSearch;

            if (this is IGalleryTagDataPredownloadAndCache)
                support |= GallerySupportFeature.TagDataPredownloadAndCache;

            if (this is IGallerySearchImage)
                support |= GallerySupportFeature.ImageSearch;

            return support;
        }

        public T Feature<T>() where T : class, IGalleryFeature => this as T;

        #endregion

        public abstract string GalleryName { get; }

        public abstract IEnumerable<GalleryItem> GetMainPostedImages();

        public abstract GalleryImageDetail GetImageDetial(GalleryItem item);

        public abstract GalleryItem GetImage(string id);
    }
}
