using SistemaWebPapeleria.Models;

namespace SistemaWebPapeleria.ViewModels
{
    public class ReporteDiarioVM
    {
        public DateTime Date { get; set; }
        public List<Sale> Sales { get; set; }
    }
}
