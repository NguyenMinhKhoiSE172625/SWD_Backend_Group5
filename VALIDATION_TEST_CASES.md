# Test Cases cho Validation

## 🚀 Cách Test

1. Mở Swagger UI: **http://localhost:5085/swagger**
2. Chọn endpoint muốn test
3. Click "Try it out"
4. Copy/paste test case JSON vào Request body
5. Click "Execute"
6. Xem Response

---

## 1. Auth Controller

### POST /api/Auth/register

#### ❌ Test Case 1: Tất cả fields trống
```json
{
  "fullName": "",
  "email": "",
  "phoneNumber": "",
  "password": "",
  "driverLicenseNumber": "",
  "idCardNumber": ""
}
```
**Expected Response**: 400 Bad Request
```json
{
  "success": false,
  "message": "Dữ liệu không hợp lệ",
  "errors": [
    "Họ tên là bắt buộc",
    "Email là bắt buộc",
    "Số điện thoại là bắt buộc",
    "Mật khẩu là bắt buộc",
    "Số giấy phép lái xe là bắt buộc",
    "Số CMND/CCCD là bắt buộc"
  ]
}
```

#### ❌ Test Case 2: Email không hợp lệ
```json
{
  "fullName": "Nguyen Van A",
  "email": "invalid-email",
  "phoneNumber": "0901234567",
  "password": "Test@123",
  "driverLicenseNumber": "B123456",
  "idCardNumber": "123456789"
}
```
**Expected**: "Email không hợp lệ"

#### ❌ Test Case 3: Số điện thoại không hợp lệ
```json
{
  "fullName": "Nguyen Van A",
  "email": "test@example.com",
  "phoneNumber": "123",
  "password": "Test@123",
  "driverLicenseNumber": "B123456",
  "idCardNumber": "123456789"
}
```
**Expected**: "Số điện thoại không hợp lệ"

#### ❌ Test Case 4: Mật khẩu yếu (không đủ ký tự)
```json
{
  "fullName": "Nguyen Van A",
  "email": "test@example.com",
  "phoneNumber": "0901234567",
  "password": "123",
  "driverLicenseNumber": "B123456",
  "idCardNumber": "123456789"
}
```
**Expected**: "Mật khẩu phải có ít nhất 6 ký tự"

#### ❌ Test Case 5: Mật khẩu yếu (không có chữ hoa)
```json
{
  "fullName": "Nguyen Van A",
  "email": "test@example.com",
  "phoneNumber": "0901234567",
  "password": "test@123",
  "driverLicenseNumber": "B123456",
  "idCardNumber": "123456789"
}
```
**Expected**: "Mật khẩu phải có ít nhất 6 ký tự, bao gồm chữ hoa, chữ thường, số và ký tự đặc biệt"

#### ❌ Test Case 6: CMND/CCCD không hợp lệ
```json
{
  "fullName": "Nguyen Van A",
  "email": "test@example.com",
  "phoneNumber": "0901234567",
  "password": "Test@123",
  "driverLicenseNumber": "B123456",
  "idCardNumber": "ABC123"
}
```
**Expected**: "Số CMND/CCCD phải là 9-12 chữ số"

#### ✅ Test Case 7: Dữ liệu hợp lệ
```json
{
  "fullName": "Nguyen Van A",
  "email": "newuser@example.com",
  "phoneNumber": "0901234567",
  "password": "Test@123",
  "driverLicenseNumber": "B123456",
  "idCardNumber": "123456789"
}
```
**Expected**: 200 OK (hoặc "Email đã tồn tại" nếu email đã dùng)

---

### POST /api/Auth/login

#### ❌ Test Case 1: Email trống
```json
{
  "email": "",
  "password": "Test@123"
}
```
**Expected**: "Email là bắt buộc"

#### ❌ Test Case 2: Email không hợp lệ
```json
{
  "email": "invalid-email",
  "password": "Test@123"
}
```
**Expected**: "Email không hợp lệ"

#### ✅ Test Case 3: Đăng nhập hợp lệ
```json
{
  "email": "renter1@example.com",
  "password": "Test@123"
}
```
**Expected**: 200 OK với token

---

## 2. Bookings Controller

### POST /api/Bookings/create

**Lưu ý**: Cần đăng nhập với role Renter và thêm Bearer token vào Authorization

#### ❌ Test Case 1: VehicleId = 0
```json
{
  "vehicleId": 0,
  "stationId": 1,
  "scheduledPickupTime": "2025-11-02T10:00:00",
  "scheduledReturnTime": "2025-11-02T18:00:00"
}
```
**Expected**: "ID xe phải lớn hơn 0"

#### ❌ Test Case 2: StationId = 0
```json
{
  "vehicleId": 1,
  "stationId": 0,
  "scheduledPickupTime": "2025-11-02T10:00:00"
}
```
**Expected**: "ID điểm thuê phải lớn hơn 0"

#### ❌ Test Case 3: Thiếu ScheduledPickupTime
```json
{
  "vehicleId": 1,
  "stationId": 1
}
```
**Expected**: "Thời gian nhận xe là bắt buộc"

#### ❌ Test Case 4: Notes quá dài (> 500 ký tự)
```json
{
  "vehicleId": 1,
  "stationId": 1,
  "scheduledPickupTime": "2025-11-02T10:00:00",
  "notes": "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum. Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua."
}
```
**Expected**: "Ghi chú không được quá 500 ký tự"

#### ✅ Test Case 5: Dữ liệu hợp lệ
```json
{
  "vehicleId": 1,
  "stationId": 1,
  "scheduledPickupTime": "2025-11-02T10:00:00",
  "scheduledReturnTime": "2025-11-02T18:00:00",
  "notes": "Cần xe sạch sẽ"
}
```
**Expected**: 200 OK

---

## 3. Rentals Controller

### POST /api/Rentals/create

**Lưu ý**: Cần đăng nhập với role StationStaff hoặc Admin

#### ❌ Test Case 1: VehicleId = 0
```json
{
  "vehicleId": 0,
  "pickupBatteryLevel": 100,
  "pickupNotes": "Xe trong tình trạng tốt"
}
```
**Expected**: "ID xe phải lớn hơn 0"

#### ❌ Test Case 2: PickupBatteryLevel > 100
```json
{
  "vehicleId": 1,
  "pickupBatteryLevel": 150,
  "pickupNotes": "Xe trong tình trạng tốt"
}
```
**Expected**: "Mức pin phải từ 0-100%"

#### ❌ Test Case 3: PickupBatteryLevel < 0
```json
{
  "vehicleId": 1,
  "pickupBatteryLevel": -10,
  "pickupNotes": "Xe trong tình trạng tốt"
}
```
**Expected**: "Mức pin phải từ 0-100%"

#### ❌ Test Case 4: PickupNotes quá dài (> 1000 ký tự)
```json
{
  "vehicleId": 1,
  "pickupBatteryLevel": 100,
  "pickupNotes": "Lorem ipsum... (> 1000 chars)"
}
```
**Expected**: "Ghi chú nhận xe không được quá 1000 ký tự"

#### ✅ Test Case 5: Dữ liệu hợp lệ
```json
{
  "vehicleId": 1,
  "bookingId": 1,
  "pickupBatteryLevel": 100,
  "pickupImages": "https://example.com/image1.jpg,https://example.com/image2.jpg",
  "pickupNotes": "Xe trong tình trạng tốt, không có vết xước"
}
```
**Expected**: 200 OK

---

### POST /api/Rentals/complete

#### ❌ Test Case 1: RentalId = 0
```json
{
  "rentalId": 0,
  "returnBatteryLevel": 80,
  "totalDistance": 50
}
```
**Expected**: "ID giao dịch thuê xe phải lớn hơn 0"

#### ❌ Test Case 2: ReturnBatteryLevel > 100
```json
{
  "rentalId": 1,
  "returnBatteryLevel": 150,
  "totalDistance": 50
}
```
**Expected**: "Mức pin phải từ 0-100%"

#### ❌ Test Case 3: TotalDistance > 10000
```json
{
  "rentalId": 1,
  "returnBatteryLevel": 80,
  "totalDistance": 15000
}
```
**Expected**: "Quãng đường phải từ 0-10000 km"

#### ❌ Test Case 4: AdditionalFees âm
```json
{
  "rentalId": 1,
  "returnBatteryLevel": 80,
  "totalDistance": 50,
  "additionalFees": -100000
}
```
**Expected**: "Phí phát sinh phải từ 0-100,000,000 VNĐ"

#### ✅ Test Case 5: Dữ liệu hợp lệ (không có phí phát sinh)
```json
{
  "rentalId": 1,
  "returnBatteryLevel": 80,
  "totalDistance": 50,
  "returnImages": "https://example.com/return1.jpg",
  "returnNotes": "Xe trả trong tình trạng tốt"
}
```
**Expected**: 200 OK

#### ✅ Test Case 6: Dữ liệu hợp lệ (có phí phát sinh)
```json
{
  "rentalId": 1,
  "returnBatteryLevel": 60,
  "totalDistance": 50,
  "additionalFees": 200000,
  "additionalFeesReason": "Xe bị xước nhẹ ở cửa",
  "returnImages": "https://example.com/damage1.jpg",
  "returnNotes": "Xe có vết xước nhẹ",
  "damageReport": "Cửa trước bên phải có vết xước dài 5cm"
}
```
**Expected**: 200 OK

---

## 4. Payments Controller

### POST /api/Payments/create

**Lưu ý**: Cần đăng nhập với role StationStaff hoặc Admin

#### ❌ Test Case 1: Amount = 0
```json
{
  "amount": 0,
  "type": 1,
  "paymentMethod": "Cash"
}
```
**Expected**: "Số tiền phải từ 0.01-100,000,000 VNĐ"

#### ❌ Test Case 2: Type không hợp lệ (> 3)
```json
{
  "amount": 100000,
  "type": 99,
  "paymentMethod": "Cash"
}
```
**Expected**: "Loại thanh toán không hợp lệ (0-3)"

#### ❌ Test Case 3: Type không hợp lệ (< 0)
```json
{
  "amount": 100000,
  "type": -1,
  "paymentMethod": "Cash"
}
```
**Expected**: "Loại thanh toán không hợp lệ (0-3)"

#### ❌ Test Case 4: PaymentMethod trống
```json
{
  "amount": 100000,
  "type": 1,
  "paymentMethod": ""
}
```
**Expected**: "Phương thức thanh toán là bắt buộc"

#### ✅ Test Case 5: Thanh toán đặt cọc (Type = 0)
```json
{
  "rentalId": 1,
  "amount": 500000,
  "type": 0,
  "paymentMethod": "Cash",
  "notes": "Đặt cọc khi nhận xe"
}
```
**Expected**: 200 OK

#### ✅ Test Case 6: Thanh toán phí thuê (Type = 1)
```json
{
  "rentalId": 1,
  "amount": 200000,
  "type": 1,
  "paymentMethod": "BankTransfer",
  "notes": "Thanh toán phí thuê 8 giờ"
}
```
**Expected**: 200 OK

---

## 📊 Payment Type Reference

| Value | Type | Mô tả |
|-------|------|-------|
| 0 | Deposit | Đặt cọc |
| 1 | RentalFee | Phí thuê xe |
| 2 | AdditionalFee | Phí phát sinh |
| 3 | Refund | Hoàn tiền |

---

## 🔐 Cách lấy Bearer Token

1. Đăng nhập bằng POST /api/Auth/login
2. Copy `token` từ response
3. Click nút "Authorize" ở góc trên bên phải Swagger UI
4. Nhập: `Bearer {token}` (thay {token} bằng token vừa copy)
5. Click "Authorize"

**Test Accounts**:
- **Renter**: `renter1@example.com` / `Test@123`
- **Staff**: `staff1@example.com` / `Test@123`
- **Admin**: `admin@example.com` / `Test@123`

---

## ✅ Validation Summary

| DTO | Total Fields | Required | Optional | Validated |
|-----|--------------|----------|----------|-----------|
| RegisterRequest | 6 | 6 | 0 | ✅ |
| LoginRequest | 2 | 2 | 0 | ✅ |
| CreateBookingRequest | 5 | 3 | 2 | ✅ |
| CreateRentalRequest | 5 | 2 | 3 | ✅ |
| CompleteRentalRequest | 9 | 3 | 6 | ✅ |
| CreatePaymentRequest | 5 | 3 | 2 | ✅ |

**Tổng cộng**: 32 fields, 19 required, 13 optional - **TẤT CẢ ĐÃ ĐƯỢC VALIDATE** ✅

