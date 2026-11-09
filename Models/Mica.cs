using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proyecto.Models
{
    [Table("micas")] // O el nombre exacto de tu tabla en MySQL
    public class Mica
    {
        [Key]
        public int IdMica { get; set; }

        [Required]
        [MaxLength(100)]
        public string Material { get; set; } = "";

        [MaxLength(100)]
        public string Tratamiento { get; set; } = "";

        [MaxLength(100)]
        public string RangoGraduacion { get; set; } = "";

        [Column(TypeName = "decimal(18,2)")]
        public decimal Precio { get; set; }
    }
}