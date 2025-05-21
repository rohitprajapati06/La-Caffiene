namespace MyProject.Models
{
    public class PlaceOrderDto
    {
        public Guid CustomerId { get; set; }

        public List<OrderItemDto> Items { get; set; }

        public string DeliveryAddress { get; set; }

        public string PaymentMethod { get; set; }
    }
}
