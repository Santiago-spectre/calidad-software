using System.ComponentModel.DataAnnotations;

namespace SistemaWebPapeleria.ViewModels
{
    public class SupplierVM
    {
        [Required(ErrorMessage = "El nombre del proveedor es obligatorio")]
        [StringLength(150, ErrorMessage = "El nombre no puede superar los 150 caracteres")]
        [RegularExpression(@"^[a-zA-Z0-9áéíóúÁÉÍÓÚñÑüÜ\s\-\.]+$", ErrorMessage = "El nombre solo puede contener letras, números, espacios, guiones y puntos")]
        public string Name { get; set; }

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [StringLength(20, ErrorMessage = "El teléfono no puede superar los 20 caracteres")]
        [RegularExpression(@"^9\d{8}$", ErrorMessage = "El teléfono debe ser un número celular peruano válido (9 dígitos, empezando en 9)")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "La dirección es obligatoria")]
        [StringLength(200, ErrorMessage = "La dirección no puede superar los 200 caracteres")]
        public string Address { get; set; }

        [StringLength(200, ErrorMessage = "La descripción no puede superar los 200 caracteres")]
        public string? Description { get; set; }
    }
}
