# Sistema de Gestión para Óptica

Bienvenido al repositorio oficial del sistema de gestión para nuestra óptica. Esta herramienta está diseñada para administrar pacientes, inventario de armazones/lentes, graduaciones y ventas de forma eficiente, rápida y sin enredos.

---

## Tecnologías y Arquitectura

El proyecto está desarrollado utilizando tecnologías modernas de Microsoft:
* **Lenguaje:** C# (.NET)
* **Framework:** ASP.NET Core (Razor Pages / MVC según tu estructura de vistas)
* **Base de Datos:** SQL Server (Gestionada mediante script SQL)
* **Frontend:** HTML5, CSS3, JavaScript / Bootstrap (en la carpeta `wwwroot`)

---

## Estructura del Repositorio

Para que no te pierdas navegando entre los archivos, aquí está la radiografía de la estructura:

```text
ProyectoGestion/
│
├── Models/             # Clases y modelos de datos (Entidades de la óptica)
├── Pages/              # Vistas o páginas de la interfaz de usuario
├── Properties/         # Configuraciones de propiedades del proyecto
├── wwwroot/            # Archivos estáticos (CSS, JS, imágenes)
│
├── .gitignore          # Archivos ignorados por Git
├── Program.cs          # Punto de entrada y configuración de servicios de la app
├── Proyecto.csproj     # Archivo de proyecto de C#
├── appsettings.json    # Cadenas de conexión y configuración general
└── script_bd.sql       # Script SQL para inicializar la base de datos
```

---

## Requisitos Previos

Asegúrate de tener instalado en tu máquina lo siguiente antes de clonar y correr el proyecto:
1. [.NET SDK](https://dotnet.microsoft.com/) (versión compatible con tu `.csproj`)
2. [SQL Server](https://www.microsoft.com/es-mx/sql-server) (LocalDB o SQL Server Express)
3. Un IDE potente como [Visual Studio](https://visualstudio.microsoft.com/) o [Visual Studio Code](https://code.visualstudio.com/)

---

## Puesta en Marcha (Instalación)

Sigue estos pasos rápidos para levantar el proyecto localmente:

1. **Clona el repositorio:**
   ```bash
   git clone https://github.com/tu-usuario/ProyectoGestion.git
   cd ProyectoGestion
   ```

2. **Configura la Base de Datos:**
   * Abre tu gestor de base de datos favorito (SSMS, Azure Data Studio, etc.).
   * Ejecuta el script que se encuentra en la raíz: `script_bd.sql`. Esto creará la base de datos y sus tablas necesarias.

3. **Configura la Conexión:**
   * Abre el archivo `appsettings.json` y actualiza la cadena de conexión (`ConnectionStrings`) apuntando a tu instancia local de SQL Server.

4. **Ejecuta el proyecto:**
   * Si estás usando Visual Studio, solo dale al botón de **Play / IIS Express**.
   * O desde la terminal en la raíz del proyecto, corre:
     ```bash
     dotnet run
     ```
