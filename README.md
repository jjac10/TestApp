# TestApp - Sistema de Gestión de Exámenes

![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4)
![WPF](https://img.shields.io/badge/WPF-Windows-0078D4)
![License](https://img.shields.io/badge/license-MIT-green)

## 📋 Descripción

TestApp es una aplicación de escritorio desarrollada en WPF que permite gestionar mazos de preguntas y realizar exámenes interactivos. Diseñada para facilitar el estudio mediante la importación de preguntas desde archivos PDF y la realización de exámenes personalizables.

## ✨ Características Principales

### v1.0.0 - Primera Versión Estable

- **Gestión de Mazos**
  - Crear y eliminar mazos de preguntas
  - Organización de archivos de preguntas por mazos
  - Confirmación antes de eliminar mazos con contenido

- **Importación de Preguntas**
  - Importación automática desde archivos PDF
  - Detección inteligente del número de preguntas
  - Configuración flexible del número de preguntas a extraer
  - Diálogos de confirmación para discrepancias

- **Sistema de Exámenes**
  - Exámenes por archivo individual o por mazo completo
  - Opciones configurables:
    - Número de preguntas
    - Orden aleatorio de preguntas
    - Orden aleatorio de respuestas
    - Modo revisión (ver respuestas correctas)
  - Navegación intuitiva entre preguntas

- **Edición de Preguntas**
  - Visualización de todas las preguntas de un archivo
  - Modo edición para corregir respuestas
  - Actualización en tiempo real

- **Interfaz de Usuario**
  - Diseño moderno con Material Design
  - Indicador de carga para operaciones largas
  - Mensajes de estado contextuales
  - Diálogos de confirmación para operaciones críticas

## 🚀 Requisitos del Sistema

- **Sistema Operativo**: Windows 10/11
- **.NET Runtime**: .NET 8.0 o superior
- **RAM**: Mínimo 4 GB
- **Espacio en Disco**: 100 MB

## 📦 Instalación

1. Descarga la última versión desde [Releases](https://github.com/jjac10/TestApp/releases)
2. Extrae el archivo ZIP en la ubicación deseada
3. Ejecuta `TestApp.Desktop.exe`

## 🛠️ Tecnologías Utilizadas

- **.NET 8.0** - Framework principal
- **WPF** - Interfaz de usuario
- **Material Design in XAML** - Diseño visual
- **CommunityToolkit.Mvvm** - Patrón MVVM
- **Entity Framework Core** - Acceso a datos
- **SQLite** - Base de datos
- **iText7** - Procesamiento de PDF

## 🤝 Contribuir

Las contribuciones son bienvenidas. Por favor, abre un issue primero para discutir los cambios que te gustaría realizar.

## 📝 Licencia

Este proyecto está bajo la Licencia MIT. Ver el archivo `LICENSE` para más detalles.

## 👤 Autor

**Jose Joaquin Alarcon**

- GitHub: [@jjac10](https://github.com/jjac10)

## 📌 Roadmap

### Versiones Futuras
- [ ] Exportación de resultados de exámenes
- [ ] Estadísticas de rendimiento
- [ ] Modo de práctica con preguntas falladas
- [ ] Soporte para múltiples idiomas
- [ ] Sincronización en la nube
- [ ] Importación desde otros formatos (Word, Excel)

## 🐛 Reportar Problemas

Si encuentras algún error, por favor abre un [issue](https://github.com/jjac10/TestApp/issues) con:
- Descripción del problema
- Pasos para reproducirlo
- Capturas de pantalla (si aplica)
- Versión del software

---

**Versión Actual**: 1.0.0  
**Fecha de Lanzamiento**: Febrero 2026