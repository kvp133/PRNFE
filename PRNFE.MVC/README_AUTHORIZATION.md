# Hệ thống phân quyền dựa trên JWT

## Tổng quan

Hệ thống phân quyền được thiết kế để tự động điều hướng người dùng đến các màn hình phù hợp dựa trên thông tin trong JWT token.

## Cấu trúc JWT Token

JWT token chứa các thông tin sau:
```json
{
  "user_name": "admin",
  "email": "kieuvietphuoc13@gmail.com",
  "full_name": "Chủ Trọ",
  "flag_admin": "True",
  "role": "Quản lý trọ",
  "permission": [
    "3000.GetAddressDetails",
    "3000.GetDistricts"
  ],
  "nbf": 1751599519,
  "exp": 1751603119,
  "iat": 1751599519,
  "iss": "UserManagement.API",
  "aud": "UserManagement.Client"
}
```

## Logic phân quyền

### 1. Kiểm tra flag_admin
- Nếu `flag_admin = "True"` → Chuyển đến màn hình quản lý người dùng (Admin)

### 2. Kiểm tra role
- Nếu `role = "Quản lý trọ"` → Chuyển đến màn hình quản lý trọ (Landlord)
- Nếu `role = "Người thuê trọ"` → Chuyển đến màn hình thông tin hóa đơn (Tenant)

## Các thành phần chính

### 1. Models
- `JwtTokenModel.cs`: Model để parse JWT token
- Các properties: `IsAdmin`, `IsLandlord`, `IsTenant` để kiểm tra role

### 2. Middleware
- `AuthorizationMiddleware.cs`: Xử lý JWT token và lưu thông tin user vào HttpContext
- Kiểm tra token expiration và tự động logout nếu token hết hạn

### 3. Attributes
- `AuthorizeAttribute.cs`: Attribute để kiểm tra quyền truy cập
- `RequireAdminAttribute`: Yêu cầu quyền Admin
- `RequireLandlordAttribute`: Yêu cầu quyền Chủ trọ
- `RequireTenantAttribute`: Yêu cầu quyền Người thuê trọ

### 4. Controllers
- `AdminController`: Quản lý người dùng và phân quyền (chỉ Admin)
- `LandlordController`: Quản lý trọ (chỉ Chủ trọ)
- `TenantController`: Xem thông tin hóa đơn (chỉ Người thuê trọ)

### 5. Views
- `Admin/adminpanel.cshtml`: Màn hình quản lý người dùng và phân quyền
- `Landlord/DormitoryManagement.cshtml`: Màn hình quản lý trọ
- `Tenant/InvoiceInfo.cshtml`: Màn hình thông tin hóa đơn

## Cách sử dụng

### 1. Thêm attribute vào controller
```csharp
[RequireAdmin]
public class AdminController : BaseController
{
    // Controller logic
}
```

### 2. Thêm attribute vào action
```csharp
[RequireLandlord]
public IActionResult DormitoryManagement()
{
    // Action logic
}
```

### 3. Kiểm tra permission cụ thể
```csharp
[Authorize(permissions: new[] { "3000.GetAddressDetails" })]
public IActionResult GetAddressDetails()
{
    // Action logic
}
```

### 4. Lấy thông tin user trong controller
```csharp
public IActionResult SomeAction()
{
    var userInfo = GetUserInfo();
    ViewBag.UserInfo = userInfo;
    return View();
}
```

## Routing

### Admin Routes
- `/Admin/UserManagement` - Quản lý người dùng và phân quyền (trang chính)
- `/Admin/GetRoles` - API lấy danh sách roles
- `/Admin/GetPermissions` - API lấy danh sách permissions
- `/Admin/CreateRole` - API tạo role mới
- `/Admin/UpdateRole` - API cập nhật role
- `/Admin/DeleteRole` - API xóa role

### Landlord Routes
- `/Landlord/DormitoryManagement` - Quản lý trọ
- `/Landlord/RoomManagement` - Quản lý phòng
- `/Landlord/TenantManagement` - Quản lý người thuê
- `/Landlord/InvoiceManagement` - Quản lý hóa đơn
- `/Landlord/Reports` - Báo cáo

### Tenant Routes
- `/Tenant/InvoiceInfo` - Thông tin hóa đơn
- `/Tenant/MyRoom` - Thông tin phòng của tôi
- `/Tenant/PaymentHistory` - Lịch sử thanh toán
- `/Tenant/Profile` - Thông tin cá nhân
- `/Tenant/Requests` - Yêu cầu

## Bảo mật

1. **Token Validation**: Middleware tự động kiểm tra tính hợp lệ của JWT token
2. **Expiration Check**: Tự động logout khi token hết hạn
3. **Role-based Access**: Chỉ cho phép truy cập các controller/action phù hợp với role
4. **Permission-based Access**: Có thể kiểm tra permission cụ thể cho từng action

## Cấu hình

### Program.cs
```csharp
// Thêm middleware
app.UseMiddleware<AuthorizationMiddleware>();

// Cấu hình routing
app.MapControllerRoute(
    name: "admin",
    pattern: "Admin/{action=UserManagement}/{id?}",
    defaults: new { controller = "Admin" });

app.MapControllerRoute(
    name: "landlord",
    pattern: "Landlord/{action=DormitoryManagement}/{id?}",
    defaults: new { controller = "Landlord" });

app.MapControllerRoute(
    name: "tenant",
    pattern: "Tenant/{action=InvoiceInfo}/{id?}",
    defaults: new { controller = "Tenant" });
```

## Xử lý lỗi

1. **Token không hợp lệ**: Tự động redirect về trang login
2. **Không có quyền truy cập**: Trả về ForbidResult (HTTP 403)
3. **Token hết hạn**: Tự động logout và redirect về trang login

## Mở rộng

Để thêm role mới:
1. Thêm property vào `JwtTokenModel`
2. Tạo attribute mới kế thừa từ `AuthorizeAttribute`
3. Tạo controller và view tương ứng
4. Cập nhật logic routing trong `HomeController` và `AuthController`

## Trang Admin Panel

Trang admin panel (`adminpanel.cshtml`) bao gồm:
- **Quản lý Roles**: Tạo, sửa, xóa roles
- **Quản lý Permissions**: Gán permissions cho roles
- **User Assignments**: Gán users cho roles
- **Dashboard**: Thống kê và báo cáo

Trang này sử dụng giao diện glassmorphism với các tính năng:
- Responsive design
- Modal dialogs
- Tab navigation
- Search functionality
- Bulk actions 