# Tổng Hợp Validation và Error Handling

## ✅ Những gì đã cải thiện

### 1. **Validation Infrastructure**

#### ValidateModelAttribute Filter
- **File**: `src/EVRentalSystem.API/Filters/ValidateModelAttribute.cs`
- **Chức năng**: Tự động validate model state và trả về lỗi chuẩn
- **Response format**:
```json
{
  "success": false,
  "message": "Dữ liệu không hợp lệ",
  "data": null,
  "errors": [
    "Email không hợp lệ",
    "Mật khẩu phải có ít nhất 6 ký tự"
  ]
}
```

#### ValidationError Class
- **File**: `src/EVRentalSystem.Application/DTOs/Common/ValidationError.cs`
- **Cấu trúc**: `{ Field, Message }`
- **Mục đích**: Chuẩn hóa cấu trúc lỗi validation

### 2. **Data Annotations trên DTOs**

#### RegisterRequest
**File**: `src/EVRentalSystem.Application/DTOs/Auth/RegisterRequest.cs`

| Field | Validation | Error Message |
|-------|-----------|---------------|
| FullName | Required, 2-100 chars | "Họ tên là bắt buộc", "Họ tên phải từ 2-100 ký tự" |
| Email | Required, EmailAddress | "Email là bắt buộc", "Email không hợp lệ" |
| PhoneNumber | Required, Phone, Regex | "Số điện thoại là bắt buộc", "Số điện thoại không hợp lệ" |
| Password | Required, 6+ chars, Regex | "Mật khẩu là bắt buộc", "Mật khẩu phải có ít nhất 6 ký tự, bao gồm chữ hoa, chữ thường, số và ký tự đặc biệt" |
| DriverLicenseNumber | Required, Max 20 | "Số giấy phép lái xe là bắt buộc", "Số giấy phép lái xe không được quá 20 ký tự" |
| IdCardNumber | Required, Regex | "Số CMND/CCCD là bắt buộc", "Số CMND/CCCD phải là 9-12 chữ số" |

**Regex Patterns**:
- Phone: `^(0|\+84)[0-9]{9,10}$` (Số Việt Nam)
- Password: `^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,}$`
- IdCard: `^[0-9]{9,12}$`

#### LoginRequest
**File**: `src/EVRentalSystem.Application/DTOs/Auth/LoginRequest.cs`

| Field | Validation | Error Message |
|-------|-----------|---------------|
| Email | Required, EmailAddress | "Email là bắt buộc", "Email không hợp lệ" |
| Password | Required | "Mật khẩu là bắt buộc" |

#### CreateBookingRequest
**File**: `src/EVRentalSystem.Application/DTOs/Booking/CreateBookingRequest.cs`

| Field | Validation | Error Message |
|-------|-----------|---------------|
| VehicleId | Required, Range(1, int.Max) | "ID xe là bắt buộc", "ID xe phải lớn hơn 0" |
| StationId | Required, Range(1, int.Max) | "ID điểm thuê là bắt buộc", "ID điểm thuê phải lớn hơn 0" |
| ScheduledPickupTime | Required | "Thời gian nhận xe là bắt buộc" |
| ScheduledReturnTime | Optional | - |
| Notes | Optional, Max 500 | "Ghi chú không được quá 500 ký tự" |

#### CreateRentalRequest
**File**: `src/EVRentalSystem.Application/DTOs/Rental/CreateRentalRequest.cs`

| Field | Validation | Error Message |
|-------|-----------|---------------|
| VehicleId | Required, Range(1, int.Max) | "ID xe là bắt buộc", "ID xe phải lớn hơn 0" |
| BookingId | Optional | - |
| PickupBatteryLevel | Required, Range(0, 100) | "Mức pin khi nhận xe là bắt buộc", "Mức pin phải từ 0-100%" |
| PickupImages | Optional | - |
| PickupNotes | Optional, Max 1000 | "Ghi chú nhận xe không được quá 1000 ký tự" |

#### CompleteRentalRequest
**File**: `src/EVRentalSystem.Application/DTOs/Rental/CompleteRentalRequest.cs`

| Field | Validation | Error Message |
|-------|-----------|---------------|
| RentalId | Required, Range(1, int.Max) | "ID giao dịch thuê xe là bắt buộc", "ID giao dịch thuê xe phải lớn hơn 0" |
| ReturnBatteryLevel | Required, Range(0, 100) | "Mức pin khi trả xe là bắt buộc", "Mức pin phải từ 0-100%" |
| TotalDistance | Required, Range(0, 10000) | "Tổng quãng đường là bắt buộc", "Quãng đường phải từ 0-10000 km" |
| AdditionalFees | Optional, Range(0, 100000000) | "Phí phát sinh phải từ 0-100,000,000 VNĐ" |
| AdditionalFeesReason | Optional, Max 500 | "Lý do phí phát sinh không được quá 500 ký tự" |
| ReturnImages | Optional | - |
| ReturnNotes | Optional, Max 1000 | "Ghi chú trả xe không được quá 1000 ký tự" |
| DamageReport | Optional, Max 2000 | "Báo cáo hư hỏng không được quá 2000 ký tự" |

#### CreatePaymentRequest
**File**: `src/EVRentalSystem.Application/DTOs/Payment/CreatePaymentRequest.cs`

| Field | Validation | Error Message |
|-------|-----------|---------------|
| RentalId | Optional | - |
| Amount | Required, Range(0.01, 100000000) | "Số tiền là bắt buộc", "Số tiền phải từ 0.01-100,000,000 VNĐ" |
| Type | Required, Range(0, 3) | "Loại thanh toán là bắt buộc", "Loại thanh toán không hợp lệ (0-3)" |
| PaymentMethod | Required, Max 50 | "Phương thức thanh toán là bắt buộc", "Phương thức thanh toán không được quá 50 ký tự" |
| Notes | Optional, Max 500 | "Ghi chú không được quá 500 ký tự" |

**Payment Type Values**:
- 0 = Deposit (Đặt cọc)
- 1 = RentalFee (Phí thuê)
- 2 = AdditionalFee (Phí phát sinh)
- 3 = Refund (Hoàn tiền)

### 3. **Controllers với ValidateModel Attribute**

Tất cả controllers đã được thêm `[ValidateModel]` attribute:

✅ **AuthController** - Đăng ký, đăng nhập, xác thực
✅ **BookingsController** - Tạo, xác nhận, hủy đặt xe
✅ **RentalsController** - Giao xe, nhận xe trả
✅ **PaymentsController** - Tạo thanh toán
✅ **VehiclesController** - Cập nhật trạng thái, pin
✅ **StationsController** - Quản lý điểm thuê

### 4. **Service Layer Improvements**

#### PaymentService
**File**: `src/EVRentalSystem.Infrastructure/Services/PaymentService.cs`

**Thay đổi quan trọng**:
- ✅ Đã sửa để xử lý `Type` là `int` thay vì `string`
- ✅ Validate payment type bằng `Enum.IsDefined()`
- ✅ Trả về `null` nếu payment type không hợp lệ

```csharp
// Validate payment type
if (!Enum.IsDefined(typeof(PaymentType), request.Type))
{
    return null;
}

var paymentType = (PaymentType)request.Type;
```

### 5. **Error Response Format**

Tất cả API endpoints trả về format chuẩn:

**Success Response**:
```json
{
  "success": true,
  "message": "Thành công",
  "data": { ... },
  "errors": null
}
```

**Validation Error Response**:
```json
{
  "success": false,
  "message": "Dữ liệu không hợp lệ",
  "data": null,
  "errors": [
    "Email không hợp lệ",
    "Mật khẩu phải có ít nhất 6 ký tự"
  ]
}
```

**Business Logic Error Response**:
```json
{
  "success": false,
  "message": "Không thể đặt xe. Xe có thể đã được đặt hoặc không khả dụng",
  "data": null,
  "errors": null
}
```

## 📋 Cách Test Validation

### 1. Mở Swagger UI
```
http://localhost:5085/swagger
```

### 2. Test RegisterRequest

**Test Case 1: Missing Required Fields**
```json
{
  "fullName": "",
  "email": "",
  "password": ""
}
```
**Expected**: Lỗi validation cho tất cả required fields

**Test Case 2: Invalid Email Format**
```json
{
  "fullName": "Nguyen Van A",
  "email": "invalid-email",
  "password": "Test@123"
}
```
**Expected**: "Email không hợp lệ"

**Test Case 3: Weak Password**
```json
{
  "fullName": "Nguyen Van A",
  "email": "test@example.com",
  "password": "123"
}
```
**Expected**: "Mật khẩu phải có ít nhất 6 ký tự, bao gồm chữ hoa, chữ thường, số và ký tự đặc biệt"

**Test Case 4: Invalid Phone Number**
```json
{
  "fullName": "Nguyen Van A",
  "email": "test@example.com",
  "phoneNumber": "123",
  "password": "Test@123"
}
```
**Expected**: "Số điện thoại không hợp lệ"

**Test Case 5: Valid Data**
```json
{
  "fullName": "Nguyen Van A",
  "email": "test@example.com",
  "phoneNumber": "0901234567",
  "password": "Test@123",
  "driverLicenseNumber": "B123456",
  "idCardNumber": "123456789"
}
```
**Expected**: Success (hoặc "Email đã tồn tại" nếu email đã được dùng)

### 3. Test CreateBookingRequest

**Test Case 1: Invalid VehicleId**
```json
{
  "vehicleId": 0,
  "stationId": 1,
  "scheduledPickupTime": "2025-11-02T10:00:00"
}
```
**Expected**: "ID xe phải lớn hơn 0"

**Test Case 2: Notes Too Long**
```json
{
  "vehicleId": 1,
  "stationId": 1,
  "scheduledPickupTime": "2025-11-02T10:00:00",
  "notes": "Lorem ipsum dolor sit amet... (> 500 chars)"
}
```
**Expected**: "Ghi chú không được quá 500 ký tự"

### 4. Test CreatePaymentRequest

**Test Case 1: Invalid Payment Type**
```json
{
  "amount": 100000,
  "type": 99,
  "paymentMethod": "Cash"
}
```
**Expected**: "Loại thanh toán không hợp lệ (0-3)"

**Test Case 2: Amount Out of Range**
```json
{
  "amount": 0,
  "type": 1,
  "paymentMethod": "Cash"
}
```
**Expected**: "Số tiền phải từ 0.01-100,000,000 VNĐ"

## ✅ Checklist Hoàn Thành

- [x] Thêm Data Annotations cho tất cả DTOs
- [x] Tạo ValidateModelAttribute filter
- [x] Áp dụng ValidateModel cho tất cả controllers
- [x] Sửa PaymentService để xử lý Type là int
- [x] Tất cả error messages bằng tiếng Việt
- [x] Response format chuẩn cho tất cả endpoints
- [x] Build thành công
- [x] Ứng dụng chạy thành công

## 🔄 Những gì có thể cải thiện thêm (Tùy chọn)

1. **Custom Validation Attributes**:
   - Validate ScheduledPickupTime phải trong tương lai
   - Validate ScheduledReturnTime > ScheduledPickupTime
   - Validate AdditionalFees yêu cầu AdditionalFeesReason

2. **Service Layer Error Handling**:
   - Thay đổi services trả về Result<T> thay vì null
   - Thêm specific error messages cho từng trường hợp lỗi
   - Ví dụ: "Email đã tồn tại" vs "Sai mật khẩu" vs "Tài khoản chưa được xác thực"

3. **Localization**:
   - Tách error messages ra resource files
   - Hỗ trợ đa ngôn ngữ (Việt/Anh)

4. **Logging**:
   - Log validation errors
   - Log business logic errors
   - Giúp debug và monitor

## 🎯 Kết Luận

Hệ thống validation hiện tại đã:
- ✅ **Hoàn chỉnh**: Tất cả DTOs đều có validation
- ✅ **Chuẩn hóa**: Error response format nhất quán
- ✅ **Rõ ràng**: Error messages bằng tiếng Việt, dễ hiểu
- ✅ **Tự động**: ValidateModel attribute tự động validate
- ✅ **An toàn**: Validate cả format và business rules

Frontend team có thể dễ dàng:
- Đọc error messages từ `errors` array
- Hiển thị lỗi cho từng field
- Xử lý validation errors một cách nhất quán

