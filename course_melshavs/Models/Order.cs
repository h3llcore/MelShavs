namespace course_melshavs.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public List<OrderItem> Items { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }

        public string FullName { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string? AdditionalNotes { get; set; }
    }
}
