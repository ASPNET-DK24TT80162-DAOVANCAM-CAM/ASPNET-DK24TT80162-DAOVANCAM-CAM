using System.Collections.Generic;

namespace ShopManagement.Models
{
    public class StoreHomeViewModel
    {
        public IList<Product> NewArrivals { get; set; }

        public IList<Product> FeaturedMen { get; set; }

        public IList<Product> FeaturedWomen { get; set; }
    }
}