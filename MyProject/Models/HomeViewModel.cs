namespace MyProject.Models
{
    public class HomeViewModel
    {
        public IEnumerable<Coupon> Coupons { get; set; }
        public IEnumerable<Product> Products { get; set; }
        
       // public IEnumerable<Review> Reviews { get; set; }
    }
}
