using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.ComponentModel.DataAnnotations.Schema;

namespace Wbooru.Models
{
    [Owned]
    public class ImageSize
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public ImageSize()
        {

        }

        public ImageSize(int imageWidth, int imageHeight)
        {
            Width = imageWidth;
            Height = imageHeight;
        }
    }
}