# Plan de la aplicación de conversión de archivos

## Descripción general

Esta aplicación de consola convertirá archivos de longitud fija y CSV a formatos JSON y XML. Cumplirá con los principios SOLID y utilizará patrones de diseño para garantizar la extensibilidad y el mantenimiento.

## Estructura del proyecto

*   Crear una estructura de carpetas bien organizada para separar las responsabilidades.
*   Definir interfaces para los componentes clave para facilitar la extensibilidad y el cumplimiento de los principios SOLID.

## Manejo de argumentos

*   Utilizar una librería para el análisis de argumentos de línea de comandos (ej: `System.CommandLine`) para facilitar la definición y el manejo de los argumentos.
*   Definir los argumentos necesarios:
    *   Archivo de entrada (obligatorio).
    *   Formato de salida (obligatorio: JSON o XML).

## Lectura de archivos

*   Crear interfaces para los lectores de archivos (ej: `IFileReader`).
*   Implementar clases concretas para leer archivos de longitud fija (`FixedLengthFileReader`) y archivos CSV (`CsvFileReader`).
*   Definir una configuración para describir la estructura del archivo (ej: nombre de los campos, longitud de cada campo para archivos de longitud fija, delimitador para archivos CSV).
*   Implementar la lógica para leer los archivos y extraer los datos según la configuración.

## Conversión a JSON/XML

*   Crear interfaces para los convertidores a JSON y XML.
*   Implementar las clases concretas para realizar la conversión.
*   Utilizar librerías para la generación de JSON y XML (ej: `System.Text.Json` y `System.Xml`).

## Principios SOLID y patrones de diseño

*   **Single Responsibility Principle (SRP):** Cada clase debe tener una única responsabilidad.
*   **Open/Closed Principle (OCP):** La aplicación debe estar abierta a la extensión, pero cerrada a la modificación.
*   **Liskov Substitution Principle (LSP):** Las subclases deben ser sustituibles por sus clases base.
*   **Interface Segregation Principle (ISP):** Es mejor tener muchas interfaces específicas que una interfaz general.
*   **Dependency Inversion Principle (DIP):** Las clases de alto nivel no deben depender de las clases de bajo nivel. Ambas deben depender de abstracciones.
*   **Patrones de diseño:**
    *   **Factory Pattern:** Para crear instancias de los lectores de archivos (CSV o longitud fija) y los convertidores (JSON o XML) según los argumentos de línea de comandos.
    *   **Strategy Pattern:** Para definir diferentes estrategias de conversión (ej: diferentes formatos de salida JSON/XML).

## Manejo de errores

*   Implementar un manejo de errores robusto para capturar y reportar errores durante la lectura, conversión y escritura de archivos.

## Logging

*   Utilizar la librería Serilog para registrar información relevante sobre la ejecución de la aplicación (ej: inicio, fin, errores).
*   Configurar Serilog para escribir logs a la consola y/o a un archivo.

## Pruebas unitarias

*   Escribir pruebas unitarias para verificar la correcta funcionalidad de los componentes clave.

## Diagrama de clases (Mermaid)

```mermaid
classDiagram
    class Program {
        +Main(string[] args)
    }
    class ArgumentParser {
        +ParseArguments(string[] args) Arguments
    }
    class Arguments {
        +InputFile string
        +OutputFileFormat string
    }
    class IFileReader {
        +Read(string inputFile, FileConfiguration config) List~Dictionary~string, string~~
    }
    class FixedLengthFileReader {
        +Read(string inputFile, FileConfiguration config) List~Dictionary~string, string~~
    }
    class CsvFileReader {
        +Read(string inputFile, FileConfiguration config) List~Dictionary~string, string~~
    }
    class FileConfiguration {
        +Fields List~FieldDefinition~
    }
    class FieldDefinition {
        +Name string
        +Length int
    }
    class IConverter {
        +Convert(List~Dictionary~string, string~~ data) string
    }
    class JsonConverter {
        +Convert(List~Dictionary~string, string~~ data) string
    }
    class XmlConverter {
        +Convert(List~Dictionary~string, string~~ data) string
    }
    class ConverterFactory {
        +CreateConverter(string format) IConverter
    }
    class FileReaderFactory {
        +CreateReader(string fileType) IFileReader
    }
    class Logger {
        +LogInformation(string message)
        +LogError(string message)
    }

    Program -- ArgumentParser
    ArgumentParser -- Arguments
    Program -- FileReaderFactory
    FileReaderFactory -- IFileReader
    IFileReader <|-- FixedLengthFileReader
    IFileReader <|-- CsvFileReader
    Program -- ConverterFactory
    ConverterFactory -- IConverter
    IConverter <|-- JsonConverter
    IConverter <|-- XmlConverter
    Program -- Logger
    FixedLengthFileReader -- FileConfiguration
    CsvFileReader -- FileConfiguration
    FileConfiguration -- FieldDefinition