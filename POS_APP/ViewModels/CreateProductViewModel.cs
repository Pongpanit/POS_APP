using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace POS_APP.ViewModels
{
    public class CreateProductViewModel
    {
        [Required(ErrorMessage = "Product name is required")]
        [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
        public string Name { get; set; }

        [Range(0.01, 999999.99, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        public string? Description { get; set; }

        [Display(Name = "Upload Image")]
        public IFormFile? ImageFile { get; set; }

        [Required]
        public int CategoryId { get; set; }
    }
}
