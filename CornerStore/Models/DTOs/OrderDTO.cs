using System.ComponentModel.DataAnnotations;

namespace CornerStore.Models.DTOs;

public class OrderDTO
{
    public int Id { get; set; }

    [Required]
    public int CashierId { get; set; }
    public CashierDTO Cashier { get; set; }
    public List<OrderProductDTO> OrderProducts { get; set; }
    public decimal Total
    {
        get
        {
            decimal productPrice = 0.00M;
            foreach (OrderProductDTO orderProduct in OrderProducts)
            {
                decimal productTotalPrice = orderProduct.Product.Price * orderProduct.Quantity;
                productPrice += productTotalPrice;
            }
            return productPrice;
        }
    }
    public DateTime? PaidOnDate { get; set; }
}
