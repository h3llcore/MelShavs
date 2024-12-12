using System.ComponentModel.DataAnnotations;

namespace course_melshavs.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; } // уникальный идентификатор товара

        [Required]
        [StringLength(100)]
        public string Name { get; set; } // название шаурмы

        [Required]
        [StringLength(500)]
        public string Description { get; set; } // описание (состав шаурмы)

        [Required]
        [StringLength(255)]
        public string ImagePath { get; set; } // путь к изображению шаурмы

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Вага має бути більшим за 0")]
        public decimal Weight { get; set; } // вес шаурмы (граммы)

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Ціна має бути більшою за 0")]
        public decimal Price { get; set; } // цена шаурмы

    }
}
