using System.ComponentModel.DataAnnotations;

namespace SampleAPI.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public string? Description { get; set; }
        public string? CustomerName { get; set; }
        public bool WasOrderInvoiced { get; set; }
        public bool WasOrderDeleted { get; set; }
    }
}
