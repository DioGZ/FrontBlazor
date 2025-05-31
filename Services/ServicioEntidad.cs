using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace FrontBlazor.Services
{
    public class ServicioEntidad
    {
        private readonly HttpClient _clienteHttp;
        private readonly JsonSerializerOptions _opcionesJson;

        public ServicioEntidad(HttpClient clienteHttp)
        {
            _clienteHttp = clienteHttp;
            _opcionesJson = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<List<Dictionary<string, object>>?> ObtenerTodosAsync(string nombreProyecto, string nombreTabla)
        {
            try
            {
                var url = $"api/{nombreProyecto}/{nombreTabla}";
                Console.WriteLine($"Realizando petición GET a: {url}");
                
                var response = await _clienteHttp.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error HTTP {response.StatusCode}: {errorContent}");
                    throw new HttpRequestException($"Error al obtener datos: {response.StatusCode} - {errorContent}");
                }

                var jsonContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Respuesta recibida: {jsonContent}");
                
                var resultado = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(jsonContent, _opcionesJson);
                if (resultado == null || !resultado.Any())
                {
                    Console.WriteLine("La respuesta se deserializó correctamente pero está vacía");
                }
                return resultado;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener datos: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                throw;
            }
        }

        public async Task<Dictionary<string, object>?> ObtenerPorClaveAsync(
            string nombreProyecto, 
            string nombreTabla, 
            string nombreClave, 
            string valorClave)
        {
            try
            {
                var url = $"api/{nombreProyecto}/{nombreTabla}/{nombreClave}/{valorClave}";
                Console.WriteLine($"Realizando petición GET a: {url}");
                
                var response = await _clienteHttp.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error HTTP {response.StatusCode}: {errorContent}");
                    throw new HttpRequestException($"Error al obtener datos: {response.StatusCode} - {errorContent}");
                }

                var jsonContent = await response.Content.ReadAsStringAsync();
                var resultado = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(jsonContent, _opcionesJson);
                return resultado?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener entidad: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> CrearAsync(
            string nombreProyecto, 
            string nombreTabla, 
            Dictionary<string, object> entidad)
        {
            try
            {
                var url = $"api/{nombreProyecto}/{nombreTabla}";
                var contenido = new StringContent(
                    JsonSerializer.Serialize(entidad),
                    Encoding.UTF8,
                    "application/json");
                
                var respuesta = await _clienteHttp.PostAsync(url, contenido);
                if (!respuesta.IsSuccessStatusCode)
                {
                    var errorContent = await respuesta.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error HTTP {respuesta.StatusCode}: {errorContent}");
                    throw new HttpRequestException($"Error al crear: {respuesta.StatusCode} - {errorContent}");
                }
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al crear entidad: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> ActualizarAsync(
            string nombreProyecto, 
            string nombreTabla, 
            string nombreClave, 
            string valorClave, 
            Dictionary<string, object> entidad)
        {
            try
            {
                var url = $"api/{nombreProyecto}/{nombreTabla}/{nombreClave}/{valorClave}";
                var contenido = new StringContent(
                    JsonSerializer.Serialize(entidad),
                    Encoding.UTF8,
                    "application/json");
                
                var respuesta = await _clienteHttp.PutAsync(url, contenido);
                if (!respuesta.IsSuccessStatusCode)
                {
                    var errorContent = await respuesta.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error HTTP {respuesta.StatusCode}: {errorContent}");
                    throw new HttpRequestException($"Error al actualizar: {respuesta.StatusCode} - {errorContent}");
                }
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al actualizar entidad: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> EliminarAsync(
            string nombreProyecto, 
            string nombreTabla, 
            string nombreClave, 
            string valorClave)
        {
            try
            {
                var url = $"api/{nombreProyecto}/{nombreTabla}/{nombreClave}/{valorClave}";
                var respuesta = await _clienteHttp.DeleteAsync(url);
                
                if (!respuesta.IsSuccessStatusCode)
                {
                    var errorContent = await respuesta.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error HTTP {respuesta.StatusCode}: {errorContent}");
                    throw new HttpRequestException($"Error al eliminar: {respuesta.StatusCode} - {errorContent}");
                }
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al eliminar entidad: {ex.Message}");
                throw;
            }
        }
    }
}