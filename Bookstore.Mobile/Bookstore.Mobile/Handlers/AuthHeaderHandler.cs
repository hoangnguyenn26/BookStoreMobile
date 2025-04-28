using Bookstore.Mobile.Interfaces.Services;
using System.Net.Http.Headers;

namespace Bookstore.Mobile.Handlers
{
    public class AuthHeaderHandler : DelegatingHandler
    {
        private readonly IAuthService _authService;
        public AuthHeaderHandler(IAuthService authService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_authService.IsLoggedIn && !string.IsNullOrEmpty(_authService.AuthToken))
            {
                var authHeader = new AuthenticationHeaderValue("Bearer", _authService.AuthToken);
                request.Headers.Authorization = authHeader;
            }
            return await base.SendAsync(request, cancellationToken);
        }
    }
}