# TestApp - Sistema de Gesti贸n de Ex谩menes

![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4)
![WPF](https://img.shields.io/badge/WPF-Windows-0078D4)
![License](https://img.shields.io/badge/license-MIT-green)

## 馃搵 Descripci贸n

TestApp es una aplicaci贸n de escritorio desarrollada en WPF que permite gestionar mazos de preguntas y realizar ex谩menes interactivos. Dise帽ada para facilitar el estudio mediante la importaci贸n de preguntas desde archivos PDF y la realizaci贸n de ex谩menes personalizables.

## 鉁?Caracter铆sticas Principales

### v1.0.0 - Primera Versi贸n Estable

- **Gesti贸n de Mazos**
  - Crear y eliminar mazos de preguntas
  - Organizaci贸n de archivos de preguntas por mazos
  - Confirmaci贸n antes de eliminar mazos con contenido

- **Importaci贸n de Preguntas**
  - Importaci贸n autom谩tica desde archivos PDF
  - Detecci贸n inteligente del n煤mero de preguntas
  - Configuraci贸n flexible del n煤mero de preguntas a extraer
  - Di谩logos de confirmaci贸n para discrepancias

- **Sistema de Ex谩menes**
  - Ex谩menes por archivo individual o por mazo completo
  - Opciones configurables:
    - N煤mero de preguntas
    - Orden aleatorio de preguntas
    - Orden aleatorio de respuestas
    - Modo revisi贸n (ver respuestas correctas)
  - Navegaci贸n intuitiva entre preguntas

- **Edici贸n de Preguntas**
  - Visualizaci贸n de todas las preguntas de un archivo
  - Modo edici贸n para corregir respuestas
  - Actualizaci贸n en tiempo real

- **Interfaz de Usuario**
  - Dise帽o moderno con Material Design
  - Indicador de carga para operaciones largas
  - Mensajes de estado contextuales
  - Di谩logos de confirmaci贸n para operaciones cr铆ticas

## 馃殌 Requisitos del Sistema

- **Sistema Operativo**: Windows 10/11
- **.NET Runtime**: .NET 8.0 o superior
- **RAM**: M铆nimo 4 GB
- **Espacio en Disco**: 100 MB

## 馃摝 Instalaci贸n

1. Descarga la 煤ltima versi贸n desde [Releases](https://github.com/jjac10/TestApp/releases)
2. Extrae el archivo ZIP en la ubicaci贸n deseada
3. Ejecuta `TestApp.Desktop.exe`

## 馃洜锔?Tecnolog铆as Utilizadas

- **.NET 8.0** - Framework principal
- **WPF** - Interfaz de usuario
- **Material Design in XAML** - Dise帽o visual
- **CommunityToolkit.Mvvm** - Patr贸n MVVM
- **Entity Framework Core** - Acceso a datos
- **SQLite** - Base de datos
- **iText7** - Procesamiento de PDF

## 馃 Contribuir

Las contribuciones son bienvenidas. Por favor, abre un issue primero para discutir los cambios que te gustar铆a realizar.

## 馃摑 Licencia

Este proyecto est谩 bajo la Licencia MIT. Ver el archivo `LICENSE` para m谩s detalles.

## 馃懁 Autor

**Jose Joaquin Alarcon**

- GitHub: [@jjac10](https://github.com/jjac10)

## 馃搶 Roadmap

### Versiones Futuras
- [ ] Exportaci贸n de resultados de ex谩menes
- [ ] Estad铆sticas de rendimiento
- [ ] Modo de pr谩ctica con preguntas falladas
- [ ] Soporte para m煤ltiples idiomas
- [ ] Sincronizaci贸n en la nube
- [ ] Importaci贸n desde otros formatos (Word, Excel)

## 馃悰 Reportar Problemas

Si encuentras alg煤n error, por favor abre un [issue](https://github.com/jjac10/TestApp/issues) con:
- Descripci贸n del problema
- Pasos para reproducirlo
- Capturas de pantalla (si aplica)
- Versi贸n del software

---

### v2.0.0 - Segunda Versión

- **Mejoras en la Importación de PDF**
  - Detección más robusta de preguntas, evitando falsos positivos en los números de pregunta.
  - Optimización: el PDF solo se lee una vez por importación.
  - Corrección de errores en el conteo y extracción de preguntas.

- **Gestión de Mazos Mejorada**
  - Ahora no se pueden crear mazos con nombres duplicados (validación case-insensitive).
  - Mensajes de error claros si el nombre ya existe o está vacío.
  - Los mensajes de estado aparecen automáticamente y desaparecen tras unos segundos.

- **Diálogos de Confirmación Mejorados**
  - En el diálogo de confirmación de importación de PDF, ahora puedes pulsar **Enter** para confirmar o **Escape** para cancelar.
  - Mejor accesibilidad y experiencia de usuario en todos los diálogos modales.

- **Corrección de Errores de Persistencia**
  - Al eliminar archivos o mazos, se eliminan correctamente todas las preguntas, respuestas y estadísticas asociadas en la base de datos.
  - Sincronización total entre la interfaz y la base de datos tras operaciones de borrado.

- **Otras Mejoras**
  - Mejoras menores de rendimiento y estabilidad.
  - Mensajes de estado más claros y útiles en la interfaz.
  
---

**Versi贸n Actual**: 2.0.0  
**Fecha de Lanzamiento**: Febrero 2026