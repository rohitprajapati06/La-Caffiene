namespace MyProject.Models
{
    public class CartItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int Price { get; set; }
        public int Total => Price * Quantity;
    }
}
