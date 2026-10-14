using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;

namespace Proyecto.Pages.Laboratorios
{
    public class IndexModel : PageModel
    {
        // Modelo temporal para representar los datos de la HU-02
        public class LaboratorioTemp
        {
            public int Id { get; set; }
            public string Nombre { get; set; }
            public string Tipo { get; set; }
            public string Telefono { get; set; }
            public string TiempoEntrega { get; set; }
            public bool Activo { get; set; }
        }

        public List<LaboratorioTemp> Laboratorios { get; set; }

        public void OnGet()
        {
            // Datos simulados para el primer sprint
            Laboratorios = new List<LaboratorioTemp>
            {
                new LaboratorioTemp { Id = 1, Nombre = "Laboratorio Óptico Central", Tipo = "Interno", Telefono = "555-1234", TiempoEntrega = "2 días", Activo = true },
                new LaboratorioTemp { Id = 2, Nombre = "Micas Express", Tipo = "Externo", Telefono = "555-5678", TiempoEntrega = "3 días", Activo = true },
                new LaboratorioTemp { Id = 3, Nombre = "Visión Clara", Tipo = "Externo", Telefono = "555-9012", TiempoEntrega = "5 días", Activo = false }
            };
        }
    }
}