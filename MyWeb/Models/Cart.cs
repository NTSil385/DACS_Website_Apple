using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyWeb.Models
{
    public class CartItems
    {
        public Product _shopping_product { get; set; }
        public int _shopping_quantity { get; set; }
    }
    public class Cart
    {
        List<CartItems> items = new List<CartItems>();
        public IEnumerable<CartItems> Items
        {
            get { return items; }
        }

        public void Add(Product _pro, int _quantity = 1)
        {
            var item = items.FirstOrDefault(s => s._shopping_product.ID == _pro.ID);
            if(item == null)
            {
                items.Add(new CartItems
                {
                    _shopping_product= _pro,
                    _shopping_quantity= _quantity,
                }
                );
            }else
                {
                    item._shopping_quantity += _quantity;
                }
        }
        public void Update_quantity_Cart(int id, int _quantity)
        {
            var item = items.Find(s => s._shopping_product.ID == id);
            if(item != null)
            {
                item._shopping_quantity = _quantity;
            }
        }
        public double Total_Money()
        {
            var total = items.Sum(s => s._shopping_product.Price * s._shopping_quantity);
            return (double)total ;
        }
        public void Remove_CartItems(int id)
        {
            items.RemoveAll(s => s._shopping_product.ID==id);
        }
        public int Count_Quantity()
        {
            return items.Sum(s => s._shopping_quantity);
        }
        public void ClearCart()
        {
            items.Clear();
        }

    }
}