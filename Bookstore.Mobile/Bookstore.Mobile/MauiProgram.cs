
using Bookstore.Mobile.Handlers;
using Bookstore.Mobile.Interfaces.Apis;
using Bookstore.Mobile.Interfaces.Services;
using Bookstore.Mobile.Mappings;
using Bookstore.Mobile.Models;
using Bookstore.Mobile.Services;
using Bookstore.Mobile.ViewModels;
using Bookstore.Mobile.Views;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Refit;
using System.Text.Json;

namespace Bookstore.Mobile
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // ----- Đăng ký Dependency Injection -----

            // Logging
#if DEBUG
            builder.Logging.AddDebug();
#endif
            var logger = builder.Services.BuildServiceProvider().GetService<ILogger<MauiApp>>();

            // Lấy địa chỉ API gốc (ưu tiên HTTPS)
            string apiBaseAddress = builder.Configuration["ApiSettings:BaseAddress"] ?? "https://localhost:7264/api";
            string httpApiBaseAddress = builder.Configuration["ApiSettings:HttpBaseAddress"] ?? "http://localhost:5244/api";

            // --- XỬ LÝ KẾT NỐI CHO ANDROID DEBUG ---
#if DEBUG && ANDROID 
            // Emulator dùng 10.0.2.2 để trỏ về localhost của máy host
            // và phải dùng HTTP nếu không cấu hình HTTPS phức tạp
            logger?.LogWarning("Android DEBUG detected. Using HTTP address for API connection.");
            apiBaseAddress = httpApiBaseAddress.Replace("http://localhost", "http://10.0.2.2");
            logger?.LogInformation("API Base Address set to: {ApiBaseAddress}", apiBaseAddress);

            // Đăng ký Refit clients với địa chỉ HTTP đã sửa đổi
            ConfigureDefaultRefitClients(builder.Services, apiBaseAddress);

#else
            // Cấu hình cho các platform khác hoặc Release build (dùng HTTPS mặc định)
            logger?.LogInformation("Using default HTTPS address for API connection: {ApiBaseAddress}", apiBaseAddress);
            ConfigureDefaultRefitClients(builder.Services, apiBaseAddress);
#endif

            // ----- Register AutoMapper -----
            builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

            // ----- Register Services -----
            builder.Services.AddSingleton<IAuthService, AuthService>();

            // Đăng ký Handler là Transient
            builder.Services.AddTransient<AuthHeaderHandler>();

            // ----- Register ViewModels & Views (Transient) -----
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<RegisterViewModel>();
            builder.Services.AddTransient<HomeViewModel>();
            builder.Services.AddTransient<CategoriesViewModel>();
            builder.Services.AddTransient<BooksViewModel>();
            builder.Services.AddTransient<BookDetailsViewModel>();
            builder.Services.AddTransient<CartViewModel>();
            builder.Services.AddTransient<ProfileViewModel>();
            builder.Services.AddTransient<AddressListViewModel>();
            builder.Services.AddTransient<AddEditAddressViewModel>();
            builder.Services.AddTransient<CheckoutViewModel>();

            // ... (Các ViewModel khác)

            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RegisterPage>();
            builder.Services.AddTransient<HomePage>();
            builder.Services.AddTransient<CategoriesPage>();
            builder.Services.AddTransient<BooksPage>();
            builder.Services.AddTransient<BookDetailsPage>();
            builder.Services.AddTransient<CartPage>();
            builder.Services.AddTransient<ProfilePage>();
            builder.Services.AddTransient<AddressListPage>();
            builder.Services.AddTransient<AddEditAddressPage>();
            builder.Services.AddTransient<CheckoutPage>();

            // ... (Các View khác)

            return builder.Build();
        }

        // Helper đăng ký Refit client
        private static void ConfigureDefaultRefitClients(IServiceCollection services, string apiBaseAddress)
        {
            var refitSettings = new RefitSettings(new SystemTextJsonContentSerializer(new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }));

            services.AddRefitClient<IAuthApi>(refitSettings)
                    .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBaseAddress));
            services.AddRefitClient<IDashboardApi>(refitSettings)
                    .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBaseAddress));
            services.AddRefitClient<ICategoriesApi>(refitSettings)
                    .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBaseAddress));
            services.AddRefitClient<IBooksApi>(refitSettings)
                   .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBaseAddress));
            // --- Client CẦN Auth Header ---
            //var httpClientBuilderBooks = services.AddRefitClient<IBooksApi>(refitSettings)
            //       .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBaseAddress));
            //httpClientBuilderBooks.AddHttpMessageHandler<AuthHeaderHandler>();

            var httpClientBuilderWishlist = services.AddRefitClient<IWishlistApi>(refitSettings)
                    .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBaseAddress));
            httpClientBuilderWishlist.AddHttpMessageHandler<AuthHeaderHandler>();

            var httpClientBuilderCarts = services.AddRefitClient<ICartApi>(refitSettings)
                    .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBaseAddress));
            httpClientBuilderCarts.AddHttpMessageHandler<AuthHeaderHandler>();

            var httpClientBuilderAddresses = services.AddRefitClient<IAddressApi>(refitSettings)
                    .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBaseAddress));
            httpClientBuilderAddresses.AddHttpMessageHandler<AuthHeaderHandler>();

            var httpClientBuilderOrders = services.AddRefitClient<IOrderApi>(refitSettings)
                    .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBaseAddress));
            httpClientBuilderOrders.AddHttpMessageHandler<AuthHeaderHandler>();
        }
    }

}