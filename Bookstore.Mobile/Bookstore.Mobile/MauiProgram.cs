
using Bookstore.Mobile.Interfaces.Apis;
using Bookstore.Mobile.Interfaces.Services;
using Bookstore.Mobile.Services;
using Bookstore.Mobile.ViewModels;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Refit;
namespace Bookstore.Mobile
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                // (Optional) Khởi tạo Maui Community Toolkit nếu dùng các tính năng của nó
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    // Thêm các font custom khác nếu có
                });

            // ----- Đăng ký Dependency Injection -----

            string apiBaseAddress = builder.Configuration["ApiSettings:BaseAddress"] ?? "https://localhost:7264/api";
#if DEBUG && ANDROID
            if (apiBaseAddress.StartsWith("https://localhost"))
            {
                apiBaseAddress = apiBaseAddress.Replace("https://localhost", "http://10.0.2.2");
            }
#endif
            // --- Thêm cấu hình Refit ---
            var refitSettings = new RefitSettings(new NewtonsoftJsonContentSerializer());

            builder.Services.AddRefitClient<IAuthApi>(refitSettings)
                            .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBaseAddress));

            builder.Services.AddRefitClient<IBooksApi>(refitSettings)
                            .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBaseAddress));
#if DEBUG
            builder.Logging.AddDebug();
#endif

            // ViewModels (Transient - Mỗi lần mở trang tạo mới ViewModel)
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<RegisterViewModel>();
            builder.Services.AddSingleton<IAuthService, AuthService>();

            // ----- Kết thúc Đăng ký DI -----


            return builder.Build();
        }
    }
}