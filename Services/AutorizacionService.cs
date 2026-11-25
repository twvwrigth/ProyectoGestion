using Microsoft.EntityFrameworkCore;
using Proyecto.Models;
using System.Security.Claims;

namespace Proyecto.Services;

// HU-03 (SCRUM-21) "Autorizaciones y bitácora".
// Lógica común del flujo de autorización de acciones sensibles:
//   1. El empleado intenta la acción.
//   2. Si quien ejecuta es Encargado (o Admin), se ejecuta directo.
//   3. Si no, puede traer un folio de autorización Aprobada, o las
//      credenciales del encargado en el acto (usuario + clave).
//   4. Si no trae nada, se crea una solicitud Pendiente y el encargado
//      la resuelve después en la página /Autorizaciones.
// Toda autorización es de un solo uso y toda resolución queda en Bitacora.
//
// NOTA DE SEGURIDAD (preexistente, no introducida por la HU-03): las
// contraseñas de empleado se guardan y se comparan en texto plano, igual
// que ya lo hace el login (Pages/login.cshtml.cs). Este servicio reutiliza
// esa misma comparación para no cambiar el esquema de contraseñas. Lo
// correcto a futuro es guardar hash (BCrypt) y comparar contra el hash.
public class AutorizacionService
{
    private readonly MiApplicationDbContext _context;

    public AutorizacionService(MiApplicationDbContext context)
    {
        _context = context;
    }

    // Tipos de acción sensible. Usar siempre estas constantes para que la
    // bitácora y los filtros de /Bitacora coincidan.
    public static class Acciones
    {
        public const string CancelacionVenta = "CancelacionVenta";
        public const string Devolucion = "Devolucion";
        public const string Descuento = "Descuento";
        public const string CambioFormaPago = "CambioFormaPago";
        public const string PrecioVenta = "PrecioVenta";
        public const string RetiroEfectivo = "RetiroEfectivo";
        public const string CorteConDiferencia = "CorteConDiferencia";
        public const string AjusteInventario = "AjusteInventario";
        public const string BajaProducto = "BajaProducto";
    }

    public static bool EsEncargado(ClaimsPrincipal usuario)
        => usuario.IsInRole("Encargado") || usuario.IsInRole("Admin");

    public static int IdEmpleadoDe(ClaimsPrincipal usuario)
    {
        var claim = usuario.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(claim, out int id) ? id : 0;
    }

    // Crea una solicitud Pendiente. Si ya existe una pendiente del mismo
    // empleado, tipo y folio, devuelve esa en vez de duplicarla.
    public async Task<Autorizacion> SolicitarAsync(int idSolicita, string tipoAccion,
        string? folioReferencia, string motivo, string? detalle)
    {
        var existente = await _context.Autorizaciones
            .FirstOrDefaultAsync(a => a.IdEmpleadoSolicita == idSolicita
                && a.TipoAccion == tipoAccion
                && a.Estado == "Pendiente"
                && a.FolioReferencia == folioReferencia);
        if (existente != null)
            return existente;

        var solicitud = new Autorizacion
        {
            TipoAccion = tipoAccion,
            FolioReferencia = folioReferencia,
            IdEmpleadoSolicita = idSolicita,
            FechaSolicitud = DateTime.Now,
            Estado = "Pendiente",
            Motivo = motivo,
            Detalle = detalle,
            Usada = false
        };
        _context.Autorizaciones.Add(solicitud);
        await _context.SaveChangesAsync();
        return solicitud;
    }

    // Verifica que el usuario/clave correspondan a un Encargado activo.
    // Devuelve el empleado encargado o null si no coincide.
    public async Task<Empleado?> VerificarEncargadoAsync(string usuario, string clave)
    {
        if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(clave))
            return null;

        var encargado = await _context.Empleados
            .Include(e => e.IdRolNavigation)
            .FirstOrDefaultAsync(e => e.Usuario == usuario.Trim() && e.Activo);
        if (encargado == null)
            return null;
        if (encargado.IdRolNavigation?.NombreRol != "Encargado")
            return null;
        // Comparación en texto plano, igual que el login (ver nota arriba).
        if (encargado.Contrasena != clave)
            return null;
        return encargado;
    }

    // Aprueba o rechaza una solicitud Pendiente (página /Autorizaciones).
    public async Task<(bool Ok, string? Error)> ResolverAsync(int idAutorizacion,
        string usuarioEncargado, string claveEncargado, bool aprobar, string? motivoRechazo = null)
    {
        var solicitud = await _context.Autorizaciones.FindAsync(idAutorizacion);
        if (solicitud == null)
            return (false, $"No existe la solicitud #{idAutorizacion}.");
        if (solicitud.Estado != "Pendiente")
            return (false, $"La solicitud #{idAutorizacion} ya fue resuelta ({solicitud.Estado}).");

        var encargado = await VerificarEncargadoAsync(usuarioEncargado, claveEncargado);
        if (encargado == null)
            return (false, "Usuario o clave del encargado incorrectos, o el usuario no es Encargado.");
        if (encargado.IdEmpleado == solicitud.IdEmpleadoSolicita)
            return (false, "No puedes autorizar tu propia solicitud.");

        solicitud.Estado = aprobar ? "Aprobada" : "Rechazada";
        solicitud.FechaResolucion = DateTime.Now;
        solicitud.IdEncargadoAutoriza = encargado.IdEmpleado;
        if (!aprobar && !string.IsNullOrWhiteSpace(motivoRechazo))
            solicitud.Detalle = motivoRechazo.Trim();

        await _context.SaveChangesAsync();

        await RegistrarEnBitacoraAsync(
            solicitud.IdEmpleadoSolicita,
            encargado.IdEmpleado,
            solicitud.TipoAccion,
            solicitud.Estado,
            solicitud.FolioReferencia,
            solicitud.Motivo,
            aprobar
                ? $"Solicitud #{solicitud.IdAutorizacion} aprobada por {encargado.Nombre}."
                : $"Solicitud #{solicitud.IdAutorizacion} rechazada por {encargado.Nombre}." +
                  (string.IsNullOrWhiteSpace(motivoRechazo) ? "" : $" Motivo: {motivoRechazo.Trim()}"));

        return (true, null);
    }

    // Autoriza en el acto: verifica las credenciales del encargado y crea
    // la autorización directamente como Aprobada (sin pasar por Pendiente).
    public async Task<(bool Ok, string? Error, Autorizacion? Autorizacion)> AutorizarEnElActoAsync(
        string tipoAccion, string? folioReferencia, int idSolicita,
        string usuarioEncargado, string claveEncargado, string motivo, string? detalle)
    {
        var encargado = await VerificarEncargadoAsync(usuarioEncargado, claveEncargado);
        if (encargado == null)
            return (false, "Usuario o clave del encargado incorrectos, o el usuario no es Encargado.", null);
        if (encargado.IdEmpleado == idSolicita)
            return (false, "No puedes autorizar tu propia acción.", null);

        var autorizacion = new Autorizacion
        {
            TipoAccion = tipoAccion,
            FolioReferencia = folioReferencia,
            IdEmpleadoSolicita = idSolicita,
            IdEncargadoAutoriza = encargado.IdEmpleado,
            FechaSolicitud = DateTime.Now,
            FechaResolucion = DateTime.Now,
            Estado = "Aprobada",
            Motivo = motivo,
            Detalle = detalle,
            Usada = false
        };
        _context.Autorizaciones.Add(autorizacion);
        await _context.SaveChangesAsync();

        await RegistrarEnBitacoraAsync(
            idSolicita, encargado.IdEmpleado, tipoAccion, "Aprobada", folioReferencia, motivo,
            $"Autorización #{autorizacion.IdAutorizacion} aprobada en el acto por {encargado.Nombre}.");

        return (true, null, autorizacion);
    }

    // Devuelve la autorización solo si existe, está Aprobada, es del tipo
    // pedido y todavía no se usó.
    public async Task<Autorizacion?> ObtenerAprobadaAsync(int idAutorizacion, string tipoAccion)
    {
        var aut = await _context.Autorizaciones.FindAsync(idAutorizacion);
        if (aut == null || aut.Estado != "Aprobada" || aut.Usada)
            return null;
        if (aut.TipoAccion != tipoAccion)
            return null;
        return aut;
    }

    public async Task MarcarUsadaAsync(int idAutorizacion)
    {
        var aut = await _context.Autorizaciones.FindAsync(idAutorizacion);
        if (aut != null && !aut.Usada)
        {
            aut.Usada = true;
            await _context.SaveChangesAsync();
        }
    }

    // Puerta de entrada única para los handlers de acciones sensibles.
    // Devuelve (Autorizado, Autorizacion, Mensaje):
    //   - Autorizado = true: ejecutar la acción. Autorizacion es null
    //     cuando quien ejecuta ya es Encargado (directo) o trae la
    //     autorización aprobada en el acto / por folio.
    //   - Autorizado = false: NO ejecutar. Mensaje dice qué pasó (solicitud
    //     creada con su folio, o credenciales incorrectas).
    public async Task<(bool Autorizado, Autorizacion? Autorizacion, string? Mensaje)> ResolverOSolicitarAsync(
        ClaimsPrincipal usuarioActual, int idSolicita, string tipoAccion, string? folioReferencia,
        int? idAutorizacion, string? usuarioEncargado, string? claveEncargado,
        string motivo, string? detalle)
    {
        // 1. El Encargado (o Admin) ejecuta directo, sin solicitud.
        if (EsEncargado(usuarioActual))
            return (true, null, null);

        // 2. Trae un folio de autorización aprobada.
        if (idAutorizacion.HasValue)
        {
            var aut = await ObtenerAprobadaAsync(idAutorizacion.Value, tipoAccion);
            if (aut == null)
                return (false, null, $"La autorización #{idAutorizacion.Value} no existe, no está aprobada para {tipoAccion} o ya fue usada.");
            return (true, aut, null);
        }

        // 3. Trae las credenciales del encargado en el acto.
        if (!string.IsNullOrWhiteSpace(usuarioEncargado) && !string.IsNullOrWhiteSpace(claveEncargado))
        {
            var (ok, error, aut) = await AutorizarEnElActoAsync(
                tipoAccion, folioReferencia, idSolicita, usuarioEncargado!, claveEncargado!, motivo, detalle);
            if (!ok)
                return (false, null, error);
            return (true, aut, null);
        }

        // 4. Nada de lo anterior: se crea la solicitud Pendiente.
        var solicitud = await SolicitarAsync(idSolicita, tipoAccion, folioReferencia, motivo, detalle);
        return (false, null,
            $"Se requiere autorización del encargado. Se creó la solicitud #{solicitud.IdAutorizacion} (pendiente). " +
            "Pide al encargado que la apruebe en Autorizaciones y vuelve a intentarlo con ese folio.");
    }

    // Llamar DESPUÉS de ejecutar la acción sensible: marca la
    // autorización como usada (si la hubo) y escribe el registro en
    // Bitacora. Cuando quien ejecutó ya es Encargado, la autorización es
    // null y el encargado registrado es él mismo.
    public async Task CerrarAutorizacionAsync(Autorizacion? autorizacion, int idEmpleado,
        string tipoAccion, string? folioReferencia, string motivo, string cambioRealizado)
    {
        int idEncargado = autorizacion?.IdEncargadoAutoriza ?? idEmpleado;
        if (autorizacion != null)
            await MarcarUsadaAsync(autorizacion.IdAutorizacion);
        await RegistrarEnBitacoraAsync(idEmpleado, idEncargado, tipoAccion, "Aprobada",
            folioReferencia, motivo, cambioRealizado);
    }

    // Escribe una fila en Bitacora. Llenar FechaReporte (fecha) y FechaHora.
    public async Task RegistrarEnBitacoraAsync(int idEmpleado, int idEncargado,
        string tipoAccion, string estado, string? folioReferencia, string motivo, string cambioRealizado)
    {
        var ahora = DateTime.Now;
        _context.Bitacoras.Add(new Bitacora
        {
            IdEmpleado = idEmpleado,
            IdEncargado = idEncargado,
            FechaReporte = DateOnly.FromDateTime(ahora),
            FechaHora = ahora,
            TipoAccion = tipoAccion,
            Estado = estado,
            FolioReferencia = folioReferencia,
            Motivo = motivo,
            CambioRealizado = cambioRealizado
        });
        await _context.SaveChangesAsync();
    }
}
