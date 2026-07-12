using System;
using System.Collections.Generic;
using System.Text;

namespace VarsityTrade.Core.Entities
{
    public class ListingImage
    {
        public int ListingImageId { get; set; }
        public int ListinId { get; set; }
        public string ImagePath { get; set; } = string.Empty;
        public string? ImageAltText { get; set; }
        public bool IsCoverImage { get; set; } = false;
        public int SortOrder { get; set; } = 0;
        public DateTime UploadedAt { get; set; }= DateTime.UtcNow;

        public Listing Listing { get; set; } = null!;

    }
}
