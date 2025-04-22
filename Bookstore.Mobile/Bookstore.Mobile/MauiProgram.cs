
using Bookstore.Mobile.Interfaces.Apis;
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
            //builder.Services.AddTransient<HomeViewModel>();  
            //builder.Services.AddTransient<CategoriesViewModel>();  
            //builder.Services.AddTransient<BooksViewModel>();  
            //builder.Services.AddTransient<BookDetailsViewModel>();  
            //builder.Services.AddTransient<WishlistViewModel>();  
            //builder.Services.AddTransient<CartViewModel>();  
            //builder.Services.AddTransient<ProfileViewModel>();  
            //builder.Services.AddTransient<AddressViewModel>();  
            //builder.Services.AddTransient<OrderHistoryViewModel>();  
            //builder.Services.AddTransient<OrderDetailsViewModel>();  
            //builder.Services.AddTransient<CheckoutViewModel>();  


            //// Views (Transient - Trang được tạo mới mỗi lần điều hướng tới)
            //builder.Services.AddTransient<LoginPage>();  
            //builder.Services.AddTransient<RegisterPage>();  
            //builder.Services.AddTransient<HomePage>();  
            //builder.Services.AddTransient<CategoriesPage>();  
            //builder.Services.AddTransient<BooksPage>();  
            //builder.Services.AddTransient<BookDetailsPage>();  
            //builder.Services.AddTransient<WishlistPage>();  
            //builder.Services.AddTransient<CartPage>();  
            //builder.Services.AddTransient<ProfilePage>();  
            //builder.Services.AddTransient<AddressListPage>();  
            //builder.Services.AddTransient<OrderHistoryPage>();  
            //builder.Services.AddTransient<OrderDetailsPage>();  
            //builder.Services.AddTransient<CheckoutPage>();  

            // ----- Kết thúc Đăng ký DI -----


            return builder.Build();
        }
    }
}