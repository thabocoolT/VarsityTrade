using System;
using System.Collections.Generic;
using System.Text;

namespace VarsityTrade.Core.Entities
{
    public class Category
    {
        public int CategoryId { get; set; }
        public int? ParentCategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? IconName { get; set; }

        public Category? ParentCategory { get; set; }
        public ICollection<Category> SubCategories { get; set; } = new List<Category>();
        public ICollection<Listing> Listings {  get; set; } = new List<Listing>();
    }
}
