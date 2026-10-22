using System.ComponentModel.DataAnnotations;

namespace SistemaWebPapeleria.ViewModels
{
    public class SaleItemVM //venta producto
    {
        [Required(ErrorMessage = "El producto es obligatorio")]
        public int ProductId { get; set; }

        
        [Range(1, 9000, ErrorMessage = "La cantidad no es válida")]
        public int Quantity { get; set; }


        [Range(0.01, 999999.99, ErrorMessage = "El precio unitario debe ser mayor a 0")]
        public decimal UnitPrice { get; set; }
    }
}
