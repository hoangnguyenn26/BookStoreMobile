<div align="center">

# **Bookstore.Mobile - Ứng dụng Quản lý Nhà sách**

Giao diện Hiện đại, Thân thiện cho Hệ thống Quản lý Nhà sách
*(A Modern and User-Friendly Interface for the Bookstore Management System)*

[![phiên bản .NET MAUI](https://img.shields.io/badge/.NET%20MAUI-.NET%208.0-purple)](https://dotnet.microsoft.com/en-us/apps/maui)
[![API Backend](https://img.shields.io/badge/API%20Backend-BookStoreManagement--API-blue)](https://github.com/hoangnguyenn26/BookStoreManagement-API)

</div>

## **Mục lục**

- [Tổng quan](#tổng-quan)
- [Tính năng Chính (Phía Client)](#tính-năng-chính-phía-client)
- [Kiến trúc Client (MAUI)](#kiến-trúc-client-maui)
- [Công nghệ Sử dụng (Client)](#công-nghệ-sử-dụng-client)
- [Ảnh chụp Màn hình (Screenshots)](#ảnh-chụp-màn-hình-screenshots)
- [Bắt đầu](#bắt-đầu)
  - [Điều kiện Tiên quyết](#điều-kiện-tiên-quyết)
  - [Cài đặt & Thiết lập](#cài-đặt--thiết-lập)
  - [Chạy Ứng dụng](#chạy-ứng-dụng)
  - [Cấu hình](#cấu-hình)
- [Đóng góp](#đóng-góp)
- [License](#license)

## **Tổng quan**

**Bookstore.Mobile** là một ứng dụng đa nền tảng được xây dựng bằng .NET MAUI, cung cấp giao diện người dùng trực quan và hiện đại để tương tác với **[BookStoreManagement-API](https://github.com/hoangnguyenn26/BookStoreManagement-API)**. Ứng dụng cho phép cả Khách hàng (Customers) và Nhân viên/Quản trị viên (Staff/Admin) truy cập và quản lý các hoạt động của nhà sách một cách hiệu quả.

Ứng dụng được thiết kế để chạy trên Android, iOS, Windows và macOS (tùy thuộc vào cấu hình và mục tiêu triển khai).

## **Tính năng Chính (Phía Client)**

Ứng dụng MAUI này triển khai các chức năng tương ứng với các API đã được cung cấp bởi backend:

*   **Khách hàng (Customer):**
    *   🔐 **Đăng ký & Đăng nhập:** Tạo tài khoản mới và đăng nhập an toàn.
    *   🏠 **Trang chủ:** Hiển thị sách mới, khuyến mãi nổi bật.
    *   📖 **Duyệt Sản phẩm:** Xem danh sách danh mục, duyệt sách theo danh mục, tìm kiếm sách.
    *   ℹ️ **Xem Chi tiết Sách:** Xem thông tin đầy đủ, hình ảnh, mô tả, giá, đánh giá.
    *   ❤️ **Danh sách Yêu thích:** Thêm/xóa sách vào danh sách yêu thích cá nhân.
    *   🛒 **Giỏ hàng:** Thêm sách vào giỏ, cập nhật số lượng, xóa sản phẩm, xem tổng tiền.
    *   👤 **Quản lý Hồ sơ:** Xem thông tin cá nhân, quản lý địa chỉ giao hàng (thêm, sửa, xóa, đặt mặc định).
    *   🛍️ **Đặt hàng (Checkout):** Chọn địa chỉ, xem lại đơn hàng, đặt hàng (tích hợp thanh toán giả lập).
    *   📋 **Lịch sử Đơn hàng:** Xem danh sách các đơn hàng đã đặt và chi tiết từng đơn.
    *   ⭐ **Gửi Đánh giá:** Viết và gửi đánh giá cho các cuốn sách.
    *   🔑 **Đăng xuất.**
*   **Quản trị viên & Nhân viên (Admin/Staff - Các chức năng này sẽ hiển thị tùy theo vai trò sau khi đăng nhập):**
    *   📊 **Dashboard Quản lý:** Xem các số liệu tổng quan nhanh (doanh thu, đơn hàng mới...).
    *   📦 **Quản lý Đơn hàng:** Xem danh sách tất cả đơn hàng, lọc theo trạng thái, xem chi tiết, cập nhật trạng thái đơn hàng.
    *   📚 **Quản lý Sản phẩm:** Xem danh sách, Thêm/Sửa/Xóa Sách, Danh mục, Tác giả. Upload ảnh bìa.
    *   🚚 **Quản lý Kho:** Tạo phiếu nhập kho, xem lịch sử phiếu nhập, thực hiện điều chỉnh tồn kho thủ công.
    *   🏷️ **Quản lý Khuyến mãi:** Xem danh sách, Thêm/Sửa/Xóa mã khuyến mãi.
    *   👥 **Quản lý Người dùng (Admin):** Xem danh sách người dùng, xem chi tiết, kích hoạt/vô hiệu hóa tài khoản, (tùy chọn) thay đổi vai trò.
    *   📈 **Xem Báo cáo:** Truy cập các báo cáo Doanh thu, Sách bán chạy, Tồn kho thấp (có thể kèm biểu đồ).

## **Kiến trúc Client (MAUI)**

Ứng dụng MAUI được xây dựng theo kiến trúc **MVVM (Model-View-ViewModel)** để đảm bảo sự phân tách rõ ràng và khả năng bảo trì:

-   **Views:** Các file XAML định nghĩa giao diện người dùng và các file code-behind (`.xaml.cs`) tối thiểu logic.
-   **ViewModels:** Các lớp chứa logic trình bày, trạng thái của View và các lệnh (Commands) được binding với View. Sử dụng `CommunityToolkit.Mvvm`.
-   **Models/DTOs:** Các lớp đại diện cho dữ liệu (thường là các DTOs được copy hoặc tham chiếu từ project API) được sử dụng trong ViewModels và Views.
-   **Services:** Các lớp chịu trách nhiệm thực hiện các tác vụ cụ thể như gọi API, quản lý trạng thái đăng nhập, điều hướng...
-   **Interfaces:** Định nghĩa các "hợp đồng" cho Services và API Clients.
-   **Handlers:** Các `DelegatingHandler` tùy chỉnh để xử lý các vấn đề xuyên suốt như đính kèm token xác thực.
-   **Converters:** Các `IValueConverter` để chuyển đổi dữ liệu cho mục đích binding trên UI.
-   **Shell:** Sử dụng .NET MAUI Shell cho cấu trúc điều hướng chính (Flyout/Tabs) và routing.

## **Công nghệ Sử dụng (Client)**

-   **Framework:** .NET MAUI (trên .NET 8.0 / .NET 9 Preview)
-   **Ngôn ngữ:** C#
-   **UI:** XAML
-   **Kiến trúc:** MVVM (sử dụng `CommunityToolkit.Mvvm`)
-   **Gọi API:** Refit (với `System.Text.Json` hoặc `Newtonsoft.Json`)
-   **Điều hướng:** .NET MAUI Shell
-   **Dependency Injection:** Tích hợp sẵn trong .NET MAUI (`Microsoft.Extensions.DependencyInjection`)
-   **Lưu trữ An toàn:** `Microsoft.Maui.Storage.SecureStorage` (cho JWT token)
-   **Lưu trữ Tùy chọn:** `Microsoft.Maui.Storage.Preferences`
-   **Xử lý Ảnh (Hiển thị):** Các control Image chuẩn của MAUI, có thể xem xét `FFImageLoading.Maui` hoặc `CommunityToolkit.Maui.ImageCaching` để tối ưu.
-   **Biểu đồ:** `Microcharts.Maui`
-   **Logging:** `Microsoft.Extensions.Logging`

## **Ảnh chụp Màn hình (Screenshots)**



## **Bắt đầu**

### **Điều kiện Tiên quyết**

-   **Cài đặt .NET MAUI Workload:** Đảm bảo bạn đã cài đặt .NET SDK (phiên bản tương ứng với project) và MAUI workload. Xem hướng dẫn tại [Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/maui/get-started/installation).
-   **Visual Studio 2022 (phiên bản 17.3 trở lên, khuyến nghị bản mới nhất)** với workload ".NET Multi-platform App UI development" được cài đặt.
-   **(Quan trọng) API Backend đang chạy:** Ứng dụng MAUI này cần kết nối đến `BookStoreManagement-API` đã được triển khai và đang chạy (trên localhost hoặc một server).
-   **Emulator/Thiết bị:**
    *   Android Emulator (cấu hình trong Android Device Manager của Visual Studio).
    *   iOS Simulator (yêu cầu máy Mac để build và chạy, hoặc dùng Pair to Mac/Dev Tunnels).
    *   Windows (Chạy trực tiếp).

### **Cài đặt & Thiết lập**

1.  **Clone Repository:**
    ```bash
    https://github.com/hoangnguyenn26/BookStoreMobile
    cd Bookstore.Mobile
    ```

2.  **Cấu hình Địa chỉ API Backend:**
    *   Mở file `MauiProgram.cs` trong project `Bookstore.Mobile`.
    *   Tìm đến dòng định nghĩa `apiBaseAddress`:
        ```csharp
        string apiBaseAddress = builder.Configuration["ApiSettings:BaseAddress"] ?? "https://localhost:7264/api";
        ```

3.  **Restore Dependencies (Nếu cần):**
    Visual Studio thường tự động restore, nhưng bạn có thể click chuột phải vào Solution -> `Restore NuGet Packages`.

### **Chạy Ứng dụng**

1.  Mở file `Bookstore.Mobile.sln` (hoặc tên solution của bạn) bằng Visual Studio 2022.
2.  **Chọn Target Framework và Thiết bị:**
    *   Trên thanh công cụ, chọn Target Framework (ví dụ: `net8.0-android`, `net8.0-ios`, `net8.0-windows`).
    *   Chọn thiết bị (Emulator Android, Simulator iOS, Local Machine cho Windows).
3.  **Đảm bảo API Backend đang chạy.**
4.  Nhấn `F5` hoặc nút Start Debugging (nút Play màu xanh).
5.  Ứng dụng sẽ được build và triển khai lên thiết bị/emulator đã chọn.

### **Cấu hình**

-   **Địa chỉ API:** Cấu hình chính nằm ở `MauiProgram.cs`. Bạn có thể đưa vào file `appsettings.json` của MAUI project nếu muốn quản lý tập trung hơn:
    *   Tạo file `appsettings.json` trong project MAUI, đặt Build Action là `MauiAsset`.
        ```json
        {
          "ApiSettings": {
            "BaseAddress": "https://localhost:7264/api"
          }
        }
        ```
    *   Nạp cấu hình trong `MauiProgram.cs`:
        ```csharp
        using System.Reflection;
        // ...
        var assembly = Assembly.GetExecutingAssembly();
        // Đảm bảo tên file appsettings.json khớp với tên trong GetManifestResourceStream
        // Ví dụ: nếu namespace gốc là Bookstore.Mobile thì tên resource sẽ là Bookstore.Mobile.appsettings.json
        using var stream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.appsettings.json"); 
        if (stream != null)
        {
            var config = new ConfigurationBuilder().AddJsonStream(stream).Build();
            builder.Configuration.AddConfiguration(config);
        }
        ```

## **Đóng góp**

## **License**
