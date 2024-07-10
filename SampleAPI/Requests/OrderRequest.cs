using System.ComponentModel.DataAnnotations;

namespace SampleAPI.Requests
{
    public class OrderRequest
    {
        [Required(ErrorMessage = "OrderDate is required.")]
        public string? OrderDate { get; set; }
        [Required(ErrorMessage = "Description is required.")]
        [StringLength(100, ErrorMessage = "Description can't be longer than 100 characters.")]
        public string? Description { get; set; }
        [Required(ErrorMessage = "Customer name is required.")]
        [StringLength(100, ErrorMessage = "Customer name can't be longer than 100 characters.")]
        public string? CustomerName { get; set; }
        public bool WasOrderInvoiced { get; set; } = true;
        public bool WasOrderDeleted { get; set; } = false;
    }
}
