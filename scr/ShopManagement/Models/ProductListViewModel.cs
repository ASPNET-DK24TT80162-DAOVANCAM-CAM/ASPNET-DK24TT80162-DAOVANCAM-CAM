using System.Collections.Generic;

namespace ShopManagement.Models
{
    public class ProductListViewModel
    {
        public IList<Product> Products { get; set; }

        public IList<Brand> Brands { get; set; }

        public IList<Category> Categories { get; set; }

        public IList<Color> Colors { get; set; }

        public IList<ShoeSize> Sizes { get; set; }

        public IList<int> SelectedBrandIds { get; set; }

        public IList<int> SelectedColorIds { get; set; }

        public IList<int> SelectedSizeIds { get; set; }

        public string QueryText { get; set; }

        public string Gender { get; set; }

        public int? Category { get; set; }

        public int? CategoryId { get; set; }

        public int? BrandId { get; set; }

        public int? ColorId { get; set; }

        public int? SizeId { get; set; }

        public decimal? MinPrice { get; set; }

        public decimal? MaxPrice { get; set; }

        public string PricePreset { get; set; }

        public string Sort { get; set; }

        public int Page { get; set; }

        public int TotalPages { get; set; }

        public int TotalItems { get; set; }
    }
}