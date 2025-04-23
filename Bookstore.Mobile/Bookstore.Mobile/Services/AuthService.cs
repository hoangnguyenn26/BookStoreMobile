
using Bookstore.Mobile.Interfaces.Apis;
using Bookstore.Mobile.Interfaces.Services;
using Bookstore.Mobile.Models;
using Bookstore.Mobile.Views;
using Microsoft.Extensions.Logging;
using Refit;
using System.Text.Json;

namespace Bookstore.Mobile.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthApi _authApi;
        private readonly ILogger<AuthService> _logger;

        private UserDto? _currentUser;
        private string? _authToken;
        private string? _lastErrorMessage;

        public bool IsLoggedIn => !string.IsNullOrEmpty(AuthToken) && CurrentUser != null;
        public UserDto? CurrentUser => _currentUser;
        public string? AuthToken => _authToken;
        public string? LastErrorMessage => _lastErrorMessage;

        // Key để lưu token trong SecureStorage
        private const string AuthTokenKey = "AuthToken";
        private const string UserInfoKey = "UserInfo";

        public AuthService(IAuthApi authApi, ILogger<AuthService> logger)
        {
            _authApi = authApi;
            _logger = logger;
        }

        public async Task InitializeAsync()
        {
            _logger.LogInformation("Initializing AuthService...");
            try
            {
                _authToken = await SecureStorage.Default.GetAsync(AuthTokenKey);
                var userInfoJson = await SecureStorage.Default.GetAsync(UserInfoKey);

                if (!string.IsNullOrEmpty(_authToken) && !string.IsNullOrEmpty(userInfoJson))
                {
                    _currentUser = JsonSerializer.Deserialize<UserDto>(userInfoJson);
                    _logger.LogInformation("User {Username} session loaded from storage.", _currentUser?.UserName ?? "Unknown");
                }
                else
                {
                    _logger.LogInformation("No previous session found.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize AuthService or load session.");
                await LogoutAsync();
            }
        }


        public async Task<bool> LoginAsync(LoginRequestDto loginRequest)
        {
            _logger.LogInformation("Attempting login for {LoginId}", loginRequest.LoginIdentifier);
            _lastErrorMessage = null;
            try
            {
                var response = await _authApi.Login(loginRequest);

                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    _authToken = response.Content.Token;
                    _currentUser = response.Content.User;

                    // Lưu token và thông tin user vào SecureStorage
                    await SecureStorage.Default.SetAsync(AuthTokenKey, _authToken);
                    await SecureStorage.Default.SetAsync(UserInfoKey, JsonSerializer.Serialize(_currentUser));

                    _logger.LogInformation("User {Username} logged in successfully.", _currentUser.UserName);
                    return true;
                }
                else
                {
                    string errorDetail = "Invalid username or password.";
                    if (response.Error != null)
                    {
                        _logger.LogWarning("Login failed for {LoginId}. Status: {StatusCode}. Reason: {Reason}",
                            loginRequest.LoginIdentifier, response.StatusCode, response.Error.ReasonPhrase);
                        _lastErrorMessage = errorDetail;
                    }
                    else
                    {
                        _logger.LogWarning("Login failed for {LoginId}. Status: {StatusCode}. Reason: {Reason}",
                            loginRequest.LoginIdentifier, response.StatusCode, response.ReasonPhrase);
                        _lastErrorMessage = errorDetail;
                    }
                    return false;
                }
            }
            catch (ApiException apiEx)
            {
                _logger.LogError(apiEx, "API Exception during login for {LoginId}", loginRequest.LoginIdentifier);
                _lastErrorMessage = "An error occurred during login."; // Lỗi chung
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during login for {LoginId}", loginRequest.LoginIdentifier);
                _lastErrorMessage = $"An unexpected error occurred: {ex.Message}";
                return false;
            }
        }

        public async Task<bool> RegisterAsync(RegisterRequestDto registerRequest)
        {
            _logger.LogInformation("Attempting registration for {Username}", registerRequest.UserName);
            _lastErrorMessage = null;
            try
            {
                var response = await _authApi.Register(registerRequest);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Registration successful for {Username}.", registerRequest.UserName);
                    return true;
                }
                else
                {
                    string errorDetail = "Unknown registration error";
                    if (response.Error != null)
                    {
                        _logger.LogWarning("Registration failed for {Username}. Status: {StatusCode}. Reason: {Reason}",
                            registerRequest.UserName, response.StatusCode, response.Error.ReasonPhrase);

                        if (response.Error.Content is string errorString && !string.IsNullOrWhiteSpace(errorString))
                        {
                            errorDetail = errorString;
                            try
                            {
                                var problem = JsonSerializer.Deserialize<ValidationProblemDetails>(errorDetail);
                                if (problem?.Errors != null && problem.Errors.Any())
                                {
                                    _lastErrorMessage = string.Join("; ", problem.Errors.SelectMany(e => e.Value));
                                }
                                else
                                {
                                    _lastErrorMessage = problem?.Detail ?? problem?.Title ?? errorDetail;
                                }
                            }
                            catch (JsonException jsonEx)
                            {
                                _logger.LogWarning(jsonEx, "Could not deserialize error content as JSON: {ErrorContent}", errorDetail);
                                _lastErrorMessage = errorDetail;
                            }
                        }
                        else
                        {
                            _lastErrorMessage = response.Error.ReasonPhrase ?? errorDetail;
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Registration failed for {Username}. Status: {StatusCode}. Reason: {Reason}",
                            registerRequest.UserName, response.StatusCode, response.ReasonPhrase);
                        _lastErrorMessage = response.ReasonPhrase ?? errorDetail;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during registration for {Username}", registerRequest.UserName);
                _lastErrorMessage = $"An unexpected error occurred: {ex.Message}";
                return false;
            }
        }

        public async Task LogoutAsync()
        {
            _logger.LogInformation("Logging out user {Username}", _currentUser?.UserName ?? "Unknown");
            _currentUser = null;
            _authToken = null;
            SecureStorage.Default.Remove(AuthTokenKey);
            SecureStorage.Default.Remove(UserInfoKey);

            await Task.CompletedTask;
            await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
        }
    }

    public class ValidationProblemDetails { public Dictionary<string, string[]>? Errors { get; set; } public string? Detail { get; set; } public string? Title { get; set; } }
}