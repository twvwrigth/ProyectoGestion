using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using System.Data;

namespace Proyecto.Pages
{
    // Modelos para mapear la BD
    public class Mica
    {
        public int IdMica { get; set; }
        public string Material { get; set; } = string.Empty;
        public string Tratamiento { get; set; } = string.Empty;
        public string RangoGraduacion { get; set; } = string.Empty;
        public decimal Precio { get; set; }
    }

    public class ParametroNegocio
    {
        public decimal PrecioExamen { get; set; } = 250.00m;
        public int VigenciaBonificacion { get; set; } = 30;
        public int AnticipoMinimo { get; set; } = 50;
    }

    public class Promocion
    {
        public int IdPromocion { get; set; }
        public string NombrePromocion { get; set; } = string.Empty;
        public string TipoPromocion { get; set; } = string.Empty;
        public decimal PorcentajePromo { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public bool Acumulable { get; set; }
        public List<int> ProductosAplicaIds { get; set; } = new List<int>(); // IDs de los productos
    }

    public class ProductoBasico
    {
        public int IdProducto { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }

    public class PreciosReglasPromocionesModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public PreciosReglasPromocionesModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Listas para mostrar en la Vista
        public List<Mica> ListaMicas { get; set; } = new List<Mica>();
        public ParametroNegocio Parametros { get; set; } = new ParametroNegocio();
        public List<Promocion> ListaPromociones { get; set; } = new List<Promocion>();
        public List<ProductoBasico> ListaProductos { get; set; } = new List<ProductoBasico>(); // Para el select múltiple

        public string ActiveTab { get; set; } = "micas";

        // ==========================================
        // ON GET: Cargar datos al abrir la página
        // ==========================================
        public void OnGet()
        {
            CargarDatosBasicos();
        }

        private void CargarDatosBasicos()
        {
            string connectionString = _configuration.GetConnectionString("ConexionMySQL");
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                // 1. Cargar Micas
                string queryMicas = "SELECT idMica, material, tratamiento, rangoGraduacion, precio FROM mica";
                using (var command = new MySqlCommand(queryMicas, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ListaMicas.Add(new Mica
                        {
                            IdMica = reader.GetInt32("idMica"),
                            Material = reader.GetString("material"),
                            Tratamiento = reader.GetString("tratamiento"),
                            RangoGraduacion = reader.GetString("rangoGraduacion"),
                            Precio = reader.GetDecimal("precio")
                        });
                    }
                }

                // 2. Cargar Parámetros Globales (Siempre ID 1)
                string queryParam = "SELECT precioExamen, vigenciaBonificacion, anticipoMinimo FROM parametro_negocio WHERE idParametro = 1";
                using (var command = new MySqlCommand(queryParam, connection))
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        Parametros.PrecioExamen = reader.GetDecimal("precioExamen");
                        Parametros.VigenciaBonificacion = reader.GetInt32("vigenciaBonificacion");
                        Parametros.AnticipoMinimo = reader.GetInt32("anticipoMinimo");
                    }
                }

                // 3. Cargar Promociones
                string queryPromo = "SELECT idPromocion, nombrePromocion, tipoPromocion, porcentajePromo, fechaInicio, fechaFin, acumulable FROM promocion";
                using (var command = new MySqlCommand(queryPromo, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ListaPromociones.Add(new Promocion
                        {
                            IdPromocion = reader.GetInt32("idPromocion"),
                            NombrePromocion = reader.IsDBNull("nombrePromocion") ? "" : reader.GetString("nombrePromocion"),
                            TipoPromocion = reader.IsDBNull("tipoPromocion") ? "" : reader.GetString("tipoPromocion"),
                            PorcentajePromo = reader.GetDecimal("porcentajePromo"),
                            FechaInicio = reader.IsDBNull("fechaInicio") ? null : reader.GetDateTime("fechaInicio"),
                            FechaFin = reader.IsDBNull("fechaFin") ? null : reader.GetDateTime("fechaFin"),
                            Acumulable = reader.GetBoolean("acumulable")
                        });
                    }
                }

                // 4. Cargar Productos para el Select "Aplica A"
                string queryProd = "SELECT idProducto, nombre FROM producto";
                using (var command = new MySqlCommand(queryProd, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ListaProductos.Add(new ProductoBasico
                        {
                            IdProducto = reader.GetInt32("idProducto"),
                            Nombre = reader.GetString("nombre")
                        });
                    }
                }
            }
        }

        // ==========================================
        // HANDLERS (POST) PARA GUARDAR DATOS
        // ==========================================

        public IActionResult OnPostCrearMica(string Material, string Tratamiento, string RangoGraduacion, decimal Precio)
        {
            string connectionString = _configuration.GetConnectionString("ConexionMySQL");
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "INSERT INTO mica (material, tratamiento, rangoGraduacion, precio) VALUES (@mat, @trat, @rango, @precio)";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@mat", Material);
                    command.Parameters.AddWithValue("@trat", Tratamiento);
                    command.Parameters.AddWithValue("@rango", RangoGraduacion);
                    command.Parameters.AddWithValue("@precio", Precio);
                    command.ExecuteNonQuery();
                }
            }
            TempData["SuccessMessage"] = "Mica registrada exitosamente.";
            ActiveTab = "micas";
            return RedirectToPage();
        }

        public IActionResult OnPostGuardarParametros(decimal PrecioExamen, int VigenciaBonificacion, int AnticipoMinimo)
        {
            string connectionString = _configuration.GetConnectionString("ConexionMySQL");
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "UPDATE parametro_negocio SET precioExamen = @precio, vigenciaBonificacion = @dias, anticipoMinimo = @anticipo WHERE idParametro = 1";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@precio", PrecioExamen);
                    command.Parameters.AddWithValue("@dias", VigenciaBonificacion);
                    command.Parameters.AddWithValue("@anticipo", AnticipoMinimo);
                    command.ExecuteNonQuery();
                }
            }
            TempData["SuccessMessage"] = "Parámetros actualizados exitosamente.";
            ActiveTab = "parametros";
            return RedirectToPage();
        }

        public IActionResult OnPostCrearPromocion(string NombrePromocion, string TipoPromocion, decimal PorcentajePromo, DateTime FechaInicio, DateTime FechaFin, bool Acumulable, List<int> ProductosIds)
        {
            string connectionString = _configuration.GetConnectionString("ConexionMySQL");
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // 1. Insertar Promoción
                        string queryPromo = "INSERT INTO promocion (nombrePromocion, tipoPromocion, porcentajePromo, fechaInicio, fechaFin, acumulable) VALUES (@nom, @tipo, @valor, @inicio, @fin, @acum)";
                        int newPromoId = 0;
                        using (var command = new MySqlCommand(queryPromo, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@nom", NombrePromocion);
                            command.Parameters.AddWithValue("@tipo", TipoPromocion);
                            command.Parameters.AddWithValue("@valor", PorcentajePromo);
                            command.Parameters.AddWithValue("@inicio", FechaInicio);
                            command.Parameters.AddWithValue("@fin", FechaFin);
                            command.Parameters.AddWithValue("@acum", Acumulable);
                            command.ExecuteNonQuery();
                            newPromoId = (int)command.LastInsertedId;
                        }

                        // 2. Insertar en tabla puente (promocion_producto) si hay productos seleccionados
                        if (ProductosIds != null && ProductosIds.Count > 0)
                        {
                            string queryPuente = "INSERT INTO promocion_producto (idPromocion, idProducto) VALUES (@idPromo, @idProd)";
                            foreach (int idProd in ProductosIds)
                            {
                                using (var command = new MySqlCommand(queryPuente, connection, transaction))
                                {
                                    command.Parameters.AddWithValue("@idPromo", newPromoId);
                                    command.Parameters.AddWithValue("@idProd", idProd);
                                    command.ExecuteNonQuery();
                                }
                            }
                        }

                        transaction.Commit();
                        TempData["SuccessMessage"] = "Promoción o paquete registrado exitosamente.";
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            ActiveTab = "promociones";
            return RedirectToPage();
        }
    }
}