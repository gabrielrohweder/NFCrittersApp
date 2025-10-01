using System.Net.Http.Json;
using AnimalCollector.Shared.DTOs;

namespace AnimalCollector.Client.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private UserDTO? _currentUser;

    public event Action? OnAuthStateChanged;

    public AuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public UserDTO? CurrentUser => _currentUser;
    public bool IsAuthenticated => _currentUser != null;

    public async Task InitializeAsync()
    {
        try
        {
            _currentUser = await _httpClient.GetFromJsonAsync<UserDTO>("api/auth/me");
        }
        catch
        {
            _currentUser = null;
        }
        OnAuthStateChanged?.Invoke();
    }

    public async Task<AuthResponse> LoginAsync(string username, string password)
    {
        var request = new LoginRequest { Username = username, Password = password };
        var response = await _httpClient.PostAsJsonAsync("api/auth/login", request);
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();

        if (result?.Success == true)
        {
            _currentUser = result.User;
            OnAuthStateChanged?.Invoke();
        }

        return result ?? new AuthResponse { Success = false, Message = "Login failed" };
    }

    public async Task<AuthResponse> RegisterAsync(string username, string password, string nickname)
    {
        var request = new RegisterRequest { Username = username, Password = password, Nickname = nickname };
        var response = await _httpClient.PostAsJsonAsync("api/auth/register", request);
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();

        if (result?.Success == true)
        {
            _currentUser = result.User;
            OnAuthStateChanged?.Invoke();
        }

        return result ?? new AuthResponse { Success = false, Message = "Registration failed" };
    }

    public async Task LogoutAsync()
    {
        await _httpClient.PostAsync("api/auth/logout", null);
        _currentUser = null;
        OnAuthStateChanged?.Invoke();
    }
}
