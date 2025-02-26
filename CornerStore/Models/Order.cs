using System.ComponentModel.DataAnnotations;

namespace CornerStore.Models;

public class Order
{
    public int Id { get; set; }

    [Required]
    public int CashierId { get; set; }
    public Cashier Cashier { get; set; }
    public List<OrderProduct> OrderProducts { get; set; }
    public decimal Total
    {
        get
        {
            decimal productPrice = 0.00M;
            if (OrderProducts == null)
            {
                return productPrice;
            }
            foreach (OrderProduct orderProduct in OrderProducts)
            {
                if (orderProduct.Product == null)
                {
                    return productPrice;
                }

                decimal productTotalPrice = orderProduct.Product.Price * orderProduct.Quantity;
                productPrice += productTotalPrice;
            }
            return productPrice;
        }
    }
    public DateTime? PaidOnDate { get; set; }
}
