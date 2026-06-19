using System.Collections.Generic;

namespace ShopManagement.Models
{
    public class ProductDetailsViewModel
    {
        public Product Product { get; set; }

        public IList<ProductVariant> Variants { get; set; }

        public ProductVariant SelectedVariant { get; set; }

        public IList<Product> RelatedProducts { get; set; }
    }
}