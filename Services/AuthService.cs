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
        private const string TokenKey = "authToken";

        // Evento que notifica cambios en el estado de autenticación
        public event Action? OnAuthStateChanged;

        public AuthService(HttpClient httpClient,
                           AuthenticationStateProvider authStateProvider,
                           ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _authStateProvider = authStateProvider;
            _localStorage = localStorage;
        }

        private void NotifyAuthStateChanged()
        {
            OnAuthStateChanged?.Invoke();
        }

        public async Task<(bool Success, string Message)> Login(LoginRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/usuario/login", request);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseContent);
                        if (loginResponse != null && !string.IsNullOrEmpty(loginResponse.Token))
                        {
                            await _localStorage.SetItemAsync(TokenKey, loginResponse.Token);
                            ((CustomAuthStateProvider)_authStateProvider).SetToken(loginResponse.Token);
                            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Token);

                            NotifyAuthStateChanged();

                            return (true, "Login exitoso");
                        }
                    }
                    catch
                    {
                        return (false, $"Error al procesar la respuesta del servidor: {responseContent}");
                    }
                }

                return (false, !string.IsNullOrEmpty(responseContent) ? responseContent : "Email o contraseña incorrectos");
            }
            catch (Exception ex)
            {
                return (false, $"Error de conexión: {ex.Message}");
            }
        }

        public async Task Logout()
        {
            try
            {
                await _localStorage.RemoveItemAsync(TokenKey);
                ((CustomAuthStateProvider)_authStateProvider).SetToken(string.Empty);
                _httpClient.DefaultRequestHeaders.Authorization = null;
                await _localStorage.ClearAsync();

                NotifyAuthStateChanged();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error durante el logout: {ex.Message}");
            }
        }

        public async Task<bool> InitializeAuthState()
        {
            try
            {
                var token = await _localStorage.GetItemAsync<string>(TokenKey);
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

        public async Task<string?> GetToken()
        {
            try
            {
                return await _localStorage.GetItemAsync<string>(TokenKey);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener el token: {ex.Message}");
                return null;
            }
        }
    }
}
