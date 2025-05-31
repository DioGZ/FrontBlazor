using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Blazored.LocalStorage;
using FrontBlazor.Models;
using System.Text.Json;

namespace FrontBlazor.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly ILocalStorageService _localStorage;

        public AuthService(HttpClient httpClient, 
                         AuthenticationStateProvider authStateProvider,
                         ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _authStateProvider = authStateProvider;
            _localStorage = localStorage;
        }        public async Task<(bool Success, string Message)> Login(LoginRequest request)
        {
            try
            {
                // Asegurarse de que la ruta sea correcta
                var response = await _httpClient.PostAsJsonAsync("api/usuario/login", request);

                // Leer el contenido de la respuesta sin importar si fue exitosa o no
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var loginResponse = System.Text.Json.JsonSerializer.Deserialize<LoginResponse>(responseContent);
                        if (loginResponse != null && !string.IsNullOrEmpty(loginResponse.Token))
                        {
                            await _localStorage.SetItemAsync("authToken", loginResponse.Token);
                            ((CustomAuthStateProvider)_authStateProvider).SetToken(loginResponse.Token);
                            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Token);
                            return (true, "Login exitoso");
                        }
                    }
                    catch
                    {
                        return (false, $"Error al procesar la respuesta del servidor: {responseContent}");
                    }
                }
                
                // Si llegamos aquí, hubo un error
                return (false, !string.IsNullOrEmpty(responseContent) ? responseContent : "Email o contraseña incorrectos");
            }
            catch (Exception ex)
            {
                return (false, $"Error de conexión: {ex.Message}");
            }
        }

        public async Task Logout()
        {
            await _localStorage.RemoveItemAsync("authToken");
            ((CustomAuthStateProvider)_authStateProvider).SetToken("");
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        public async Task<bool> InitializeAuthState()
        {
            try
            {
                var token = await _localStorage.GetItemAsync<string>("authToken");
                if (!string.IsNullOrEmpty(token))
                {
                    ((CustomAuthStateProvider)_authStateProvider).SetToken(token);
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing auth state: {ex.Message}");
                return false;
            }
        }
    }
}
