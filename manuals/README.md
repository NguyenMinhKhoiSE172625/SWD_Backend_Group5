# EV Rental System - Backend API

Hệ thống Backend API cho dự án thuê xe điện tại điểm thuê.

## 🚀 Công nghệ sử dụng

- **.NET Core 8** - Web API Framework
- **Entity Framework Core 9** - ORM với SQLite Database
- **JWT Authentication** - Xác thực và phân quyền
- **BCrypt.Net** - Mã hóa mật khẩu
- **Swagger/OpenAPI** - API Documentation

## 📁 Cấu trúc dự án (Clean Architecture)

```
EVRentalSystem/
├── src/
│   ├── EVRentalSystem.API/          # API Layer - Controllers, Program.cs
│   ├── EVRentalSystem.Application/  # Application Layer - DTOs, Interfaces
│   ├── EVRentalSystem.Domain/       # Domain Layer - Entities, Enums
│   └── EVRentalSystem.Infrastructure/ # Infrastructure - DbContext, Services
└── EVRentalSystem.db                # SQLite Database
```

## 🎯 Chức năng chính

### 1. **Người thuê (Renter)**
- Đăng ký tài khoản
- Đăng nhập
- Tìm điểm thuê trên bản đồ
- Xem xe có sẵn
- Đặt xe
- Xem lịch sử thuê xe

### 2. **Nhân viên (Station Staff)**
- Xác thực khách hàng
- Giao xe (tạo rental với inspection)
- Nhận xe trả (hoàn tất rental với inspection)
- Quản lý xe tại điểm
- Xử lý thanh toán

### 3. **Quản trị (Admin)**
- Quản lý toàn bộ hệ thống
- Báo cáo và phân tích

## 🔐 Tài khoản mẫu

### Admin
- Email: `admin@evrentalsystem.com`
- Password: `Admin@123`

### Nhân viên Quận 1
- Email: `staff1@evrentalsystem.com`
- Password: `Staff@123`

### Nhân viên Quận 3
- Email: `staff2@evrentalsystem.com`
- Password: `Staff@123`

### Người thuê 1
- Email: `nguyenvana@gmail.com`
- Password: `User@123`

### Người thuê 2
- Email: `tranthib@gmail.com`
- Password: `User@123`

## 🏃 Chạy ứng dụng

### Yêu cầu
- .NET 8 SDK

### Các bước chạy

1. **Clone hoặc mở project**
```bash
cd d:\Study\SWD
```

2. **Restore packages**
```bash
dotnet restore
```

3. **Chạy ứng dụng**
```bash
dotnet run --project src/EVRentalSystem.API
```

4. **Mở Swagger UI**
```
http://localhost:5085
```

## 📚 API Endpoints

### Authentication (`/api/auth`)
- `POST /api/auth/register` - Đăng ký tài khoản
- `POST /api/auth/login` - Đăng nhập
- `POST /api/auth/verify/{userId}` - Xác thực khách hàng (Staff/Admin)

### Stations (`/api/stations`)
- `GET /api/stations` - Lấy danh sách điểm thuê
- `GET /api/stations/{id}` - Lấy thông tin điểm thuê
- `GET /api/stations/nearby` - Tìm điểm thuê gần

### Vehicles (`/api/vehicles`)
- `GET /api/vehicles/available` - Lấy xe có sẵn
- `GET /api/vehicles/{id}` - Lấy thông tin xe
- `GET /api/vehicles/station/{stationId}` - Lấy xe tại điểm (Staff/Admin)
- `PUT /api/vehicles/{id}/status` - Cập nhật trạng thái xe (Staff/Admin)
- `PUT /api/vehicles/{id}/battery` - Cập nhật pin xe (Staff/Admin)

### Bookings (`/api/bookings`)
- `POST /api/bookings/create` - Đặt xe (Renter)
- `GET /api/bookings/{id}` - Lấy thông tin đặt xe
- `GET /api/bookings/my-bookings` - Lấy đặt xe của tôi (Renter)
- `GET /api/bookings/station/{stationId}` - Lấy đặt xe tại điểm (Staff/Admin)
- `POST /api/bookings/{id}/cancel` - Hủy đặt xe (Renter)
- `POST /api/bookings/{id}/confirm` - Xác nhận đặt xe (Staff/Admin)

### Rentals (`/api/rentals`) - **QUẢN LÝ GIAO NHẬN XE**
- `POST /api/rentals/create` - **Giao xe** (Staff/Admin)
- `POST /api/rentals/complete` - **Nhận xe trả** (Staff/Admin)
- `GET /api/rentals/{id}` - Lấy thông tin thuê xe
- `GET /api/rentals/my-rentals` - Lịch sử thuê xe (Renter)
- `GET /api/rentals/active` - Xe đang thuê (Staff/Admin)

### Payments (`/api/payments`)
- `POST /api/payments/create` - Tạo thanh toán (Staff/Admin)
- `GET /api/payments/my-payments` - Lịch sử thanh toán (Renter)
- `GET /api/payments/rental/{rentalId}` - Thanh toán của rental (Staff/Admin)

## 🔑 Sử dụng Authentication trong Swagger

1. **Đăng nhập** qua endpoint `/api/auth/login`
2. **Copy token** từ response
3. **Click nút "Authorize"** ở góc trên bên phải Swagger UI
4. **Nhập**: `Bearer {token}` (ví dụ: `Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...`)
5. **Click "Authorize"**
6. Bây giờ bạn có thể gọi các API cần authentication

## 🔄 Flow "Quản lý giao – nhận xe"

### 1. Khách hàng đặt xe
```
POST /api/bookings/create
{
  "vehicleId": 1,
  "scheduledPickupTime": "2025-11-02T10:00:00",
  "scheduledReturnTime": "2025-11-02T18:00:00"
}
```

### 2. Nhân viên xác nhận đặt xe
```
POST /api/bookings/{bookingId}/confirm
```

### 3. Nhân viên giao xe (Pickup)
```
POST /api/rentals/create
{
  "bookingId": 1,
  "vehicleId": 1,
  "pickupBatteryLevel": 100,
  "pickupInspection": {
    "imageUrls": ["url1", "url2"],
    "notes": "Xe trong tình trạng tốt",
    "damageReport": null
  }
}
```

### 4. Nhân viên nhận xe trả (Return)
```
POST /api/rentals/complete
{
  "rentalId": 1,
  "returnBatteryLevel": 45,
  "totalDistance": 50.5,
  "additionalFees": 0,
  "returnInspection": {
    "imageUrls": ["url3", "url4"],
    "notes": "Xe trả về bình thường",
    "damageReport": null
  }
}
```

### 5. Nhân viên tạo thanh toán
```
POST /api/payments/create
{
  "rentalId": 1,
  "amount": 400000,
  "type": 1,
  "paymentMethod": "Cash"
}
```

## 💾 Database

Database SQLite được tạo tự động khi chạy ứng dụng lần đầu tại: `EVRentalSystem.db`

### Seed Data bao gồm:
- 3 điểm thuê (Quận 1, Quận 3, Bình Thạnh)
- 6 xe điện (VinFast Klara, Yadea G5, Pega Plus)
- 5 users (1 Admin, 2 Staff, 2 Renters)

## 📊 Enums

### UserRole
- `Renter = 1`
- `StationStaff = 2`
- `Admin = 3`

### VehicleStatus
- `Available = 0`
- `Booked = 1`
- `InUse = 2`
- `Maintenance = 3`
- `Damaged = 4`

### BookingStatus
- `Pending = 0`
- `Confirmed = 1`
- `Cancelled = 2`
- `Completed = 3`

### RentalStatus
- `Active = 0`
- `Completed = 1`
- `Cancelled = 2`

### PaymentType
- `Deposit = 0`
- `RentalFee = 1`
- `AdditionalFee = 2`
- `Refund = 3`

### PaymentStatus
- `Pending = 0`
- `Completed = 1`
- `Failed = 2`
- `Refunded = 3`

## 🛠️ Tính năng kỹ thuật

- ✅ Clean Architecture (Domain, Application, Infrastructure, API)
- ✅ Repository Pattern via DbContext
- ✅ DTO Pattern cho Request/Response
- ✅ JWT Authentication với Role-based Authorization
- ✅ Password Hashing với BCrypt
- ✅ Standardized API Response với `ApiResponse<T>`
- ✅ Entity Framework Core với Code-First Migrations
- ✅ Swagger UI với JWT Bearer support
- ✅ CORS enabled cho frontend integration
- ✅ Auto-generated codes (BookingCode, RentalCode, PaymentCode)
- ✅ Vehicle Inspection tracking với images
- ✅ Smart pricing (hourly vs daily rates)

## 📝 Lưu ý

- API sử dụng chuẩn RESTful
- Tất cả response đều wrap trong `ApiResponse<T>` với format:
```json
{
  "success": true,
  "message": "Success message",
  "data": { ... },
  "errors": null
}
```
- Datetime format: ISO 8601 (ví dụ: `2025-11-02T10:00:00`)
- Tất cả API đều có mô tả chi tiết trong Swagger UI

## 🎓 Dành cho Frontend Team

1. **Base URL**: `http://localhost:5085`
2. **Swagger Documentation**: `http://localhost:5085`
3. **Authentication**: Sử dụng JWT Bearer Token trong header `Authorization: Bearer {token}`
4. **Response Format**: Tất cả response đều có cấu trúc `ApiResponse<T>`
5. **Error Handling**: Check `success` field, nếu `false` thì xem `errors` array

---

**Developed with ❤️ for SWD Project**

