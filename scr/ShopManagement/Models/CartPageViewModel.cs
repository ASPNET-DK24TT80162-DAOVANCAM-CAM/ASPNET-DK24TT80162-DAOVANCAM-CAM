using System.Collections.Generic;

namespace ShopManagement.Models
{
    public class CartPageViewModel
    {
        public IList<CartItem> Items { get; set; }

        public decimal Subtotal { get; set; }
    }
}