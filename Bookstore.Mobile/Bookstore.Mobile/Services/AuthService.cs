﻿using Bookstore.Mobile.Interfaces.Apis;
using Bookstore.Mobile.Interfaces.Services;
using Bookstore.Mobile.Models;
using Microsoft.Extensions.Logging;
using Refit;
using System.Text.Json;
using FluentValidation;

namespace Bookstore.Mobile.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthApi _authApi;
        private readonly ILogger<AuthService> _logger;
        private readonly IValidator<LoginRequestDto> _loginValidator;
        private readonly IValidator<RegisterRequestDto> _registerValidator;

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

        public AuthService(
            IAuthApi authApi,
            ILogger<AuthService> logger,
            IValidator<LoginRequestDto> loginValidator,
            IValidator<RegisterRequestDto> registerValidator)
        {
            _authApi = authApi;
            _logger = logger;
            _loginValidator = loginValidator;
            _registerValidator = registerValidator;
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
                    _logger.LogInformation("User {Username} session loaded from storage with roles: {Roles}",
                         _currentUser?.UserName ?? "Unknown", string.Join(", ", _currentUser?.Roles ?? new List<string>()));
                }
                else
                {
                    _logger.LogInformation("No valid session found in storage.");
                    _currentUser = null;
                    _authToken = null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize AuthService. Clearing session.");
                _currentUser = null;
                _authToken = null;
                SecureStorage.Default.Remove(AuthTokenKey);
                SecureStorage.Default.Remove(UserInfoKey);
            }
            finally
            {
                OnAuthStateChanged();
            }
        }


        public async Task<bool> LoginAsync(LoginRequestDto loginRequest)
        {
            var validationResult = await _loginValidator.ValidateAsync(loginRequest);
            if (!validationResult.IsValid)
            {
                _lastErrorMessage = string.Join("\n", validationResult.Errors.Select(e => e.ErrorMessage));
                return false;
            }
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

                    _logger.LogInformation("User {Username} logged in successfully with roles: {Roles}",
                            _currentUser.UserName, string.Join(", ", _currentUser.Roles ?? new List<string>()));
                    OnAuthStateChanged();
                    return true;
                }
                else
                {
                    string errorDetail = "Tên đăng nhập hoặc mật khẩu không đúng";
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
                _lastErrorMessage = "Đã xảy ra lỗi trong quá trình đăng nhập";
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during login for {LoginId}", loginRequest.LoginIdentifier);
                _lastErrorMessage = $"Đã xảy ra lỗi không mong muốn: {ex.Message}";
                return false;
            }
        }

        public async Task<bool> RegisterAsync(RegisterRequestDto registerRequest)
        {
            var validationResult = await _registerValidator.ValidateAsync(registerRequest);
            if (!validationResult.IsValid)
            {
                _lastErrorMessage = string.Join("\n", validationResult.Errors.Select(e => e.ErrorMessage));
                return false;
            }
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
                    string errorDetail = "Đã xảy ra lỗi trong quá trình đăng ký";
                    if (response.Error != null)
                    {
                        _logger.LogWarning("Registration failed for {Username}. Status: {StatusCode}. Reason: {Reason}",
                            registerRequest.UserName, response.StatusCode, response.Error.ReasonPhrase);

                        if (response.Error.Content is string errorString && !string.IsNullOrWhiteSpace(errorString))
                        {
                            errorDetail = errorString;
                            try
                            {
                                // Sử dụng ValidationProblemDetails từ Models namespace
                                var problem = ValidationProblemDetails.Parse(errorString);

                                if (problem != null)
                                {
                                    // Lấy các thông báo lỗi thân thiện với người dùng
                                    var friendlyMessages = problem.GetFriendlyMessages();
                                    if (friendlyMessages.Any())
                                    {
                                        _lastErrorMessage = string.Join("\n", friendlyMessages);
                                    }
                                    else
                                    {
                                        _lastErrorMessage = problem.Detail ?? problem.Title ?? errorDetail;
                                    }
                                }
                                else
                                {
                                    _lastErrorMessage = errorDetail;
                                }
                            }
                            catch (JsonException jsonEx)
                            {
                                _logger.LogWarning(jsonEx, "Could not deserialize error content as JSON: {ErrorContent}", errorDetail);
                                // Thông báo lỗi thân thiện
                                if (errorDetail.Contains("already exists"))
                                    _lastErrorMessage = "Tên đăng nhập hoặc email đã tồn tại. Vui lòng chọn thông tin khác.";
                                else
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
                _lastErrorMessage = $"Đã xảy ra lỗi không mong muốn: {ex.Message}";
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

            OnAuthStateChanged();
            await Task.CompletedTask;
        }
        // --- Thêm cơ chế thông báo thay đổi trạng thái ---
        public event EventHandler? AuthStateChanged;

        protected virtual void OnAuthStateChanged()
        {
            AuthStateChanged?.Invoke(this, EventArgs.Empty);
        }
        public bool HasRole(string roleName)
        {
            if (!IsLoggedIn || CurrentUser?.Roles == null)
            {
                return false;
            }
            return CurrentUser.Roles.Contains(roleName, StringComparer.OrdinalIgnoreCase);
        }
    }
}