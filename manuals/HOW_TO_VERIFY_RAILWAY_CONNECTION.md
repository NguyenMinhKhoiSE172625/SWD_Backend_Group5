# 🔍 Cách Verify Đã Kết Nối Đến Railway Database

## ✅ Các dấu hiệu đã kết nối thành công

### 1. Kiểm tra Logs khi chạy ứng dụng

Khi chạy `dotnet run`, bạn sẽ thấy:

```
✅ "📊 Sử dụng PostgreSQL database" - Đã detect PostgreSQL
✅ "🔗 Connection String: postgresql://***:***@gondola.proxy.rlwy.net:26018/railway" - Connection string Railway
✅ "📦 Database: railway" - Database name là "railway" (KHÔNG phải "EVRentalSystemDB")
✅ "✅ Database 'railway' đã sẵn sàng!" - Database đã kết nối
✅ "✅ Tất cả migrations đã được áp dụng thành công!" - Migrations đã chạy
✅ "✅ Dữ liệu mẫu đã được khởi tạo thành công!" - Seed data đã chạy
```

**⚠️ Nếu thấy:**
- ❌ "📦 Database: EVRentalSystemDB" → **CHƯA kết nối Railway** (đang dùng SQL Server local)
- ❌ "📦 Database: railway" → **Đã kết nối Railway** ✅

### 2. Kiểm tra Railway Dashboard

1. Vào Railway Dashboard → Database → Tab **"Metrics"**
2. Xem các metrics:
   - **Connections**: Phải > 0 khi app đang chạy
   - **Queries**: Phải có queries khi bạn test API
   - **Database Size**: Phải tăng khi có dữ liệu mới

### 3. Test API và Verify Dữ liệu

1. **Chạy ứng dụng:**
   ```bash
   dotnet run --project src/EVRentalSystem.API
   ```

2. **Test API:**
   - Mở Swagger: http://localhost:5085/swagger
   - Test các endpoints (ví dụ: GET /api/stations, POST /api/auth/register)

3. **Kiểm tra dữ liệu trong Railway:**
   - Vào Railway Dashboard → Database → Tab **"Database"** (nếu có query editor)
   - Hoặc dùng tool như pgAdmin, DBeaver, VS Code extension
   - Kết nối bằng connection string từ Railway
   - Chạy query:
     ```sql
     SELECT * FROM "Users" LIMIT 5;
     SELECT * FROM "Stations" LIMIT 5;
     ```
   - Nếu thấy dữ liệu → **Đã kết nối Railway** ✅

### 4. Kiểm tra Connection String

**Kiểm tra User Secrets:**
```bash
cd src/EVRentalSystem.API
dotnet user-secrets list
```

Phải thấy:
```
ConnectionStrings:DefaultConnection = postgresql://postgres:...@gondola.proxy.rlwy.net:26018/railway
```

**Kiểm tra trong code (sau khi fix):**
- Logs sẽ hiển thị connection string (ẩn password)
- Connection string phải chứa: `gondola.proxy.rlwy.net` hoặc `railway.internal`

## 🐛 Troubleshooting

### Vấn đề: Logs hiển thị "Database: EVRentalSystemDB"

**Nguyên nhân:** Code chưa extract database name từ PostgreSQL URL format đúng cách.

**Giải pháp:**
1. Dừng ứng dụng (Ctrl+C)
2. Code đã được fix, restart ứng dụng:
   ```bash
   dotnet run --project src/EVRentalSystem.API
   ```
3. Kiểm tra logs lại - phải thấy "Database: railway"

### Vấn đề: Không thấy connections trong Railway Dashboard

**Nguyên nhân:** 
- App chưa chạy
- Connection string sai
- Database chưa sẵn sàng

**Giải pháp:**
1. Đảm bảo app đang chạy
2. Test API (tạo request) để tạo connections
3. Đợi vài giây rồi refresh Railway Dashboard

### Vấn đề: "Cannot connect to database"

**Nguyên nhân:**
- Connection string sai
- Database chưa sẵn sàng
- Firewall/network issue

**Giải pháp:**
1. Kiểm tra connection string trong User Secrets
2. Đảm bảo dùng `DATABASE_PUBLIC_URL` (không phải `DATABASE_URL`)
3. Kiểm tra database status trong Railway Dashboard

## ✅ Checklist Verify

- [ ] Logs hiển thị "📊 Sử dụng PostgreSQL database"
- [ ] Logs hiển thị "📦 Database: railway" (KHÔNG phải "EVRentalSystemDB")
- [ ] Logs hiển thị connection string có "gondola.proxy.rlwy.net" hoặc "railway.internal"
- [ ] Railway Dashboard → Metrics → Connections > 0
- [ ] Test API thành công
- [ ] Dữ liệu được lưu vào database Railway (kiểm tra bằng query)

## 🎯 Quick Test

1. **Chạy app:**
   ```bash
   dotnet run --project src/EVRentalSystem.API
   ```

2. **Xem logs - phải thấy:**
   - "📊 Sử dụng PostgreSQL database"
   - "📦 Database: railway"
   - "✅ Database 'railway' đã sẵn sàng!"

3. **Test API:**
   - Mở: http://localhost:5085/swagger
   - Test: GET /api/stations
   - Nếu có dữ liệu trả về → **Đã kết nối Railway** ✅

4. **Kiểm tra Railway Dashboard:**
   - Vào Database → Metrics
   - Xem Connections và Queries
   - Nếu có số liệu → **Đã kết nối Railway** ✅

---

## 📝 Kết luận

**Cách đơn giản nhất để verify:**
1. ✅ Logs hiển thị "Database: railway" (không phải "EVRentalSystemDB")
2. ✅ Connection string chứa "gondola.proxy.rlwy.net" hoặc "railway.internal"
3. ✅ Test API thành công và dữ liệu được lưu
4. ✅ Railway Dashboard hiển thị connections và queries

Nếu có tất cả 4 điều trên → **Đã kết nối Railway thành công!** ✅

---

**Xem thêm:**
- [RAILWAY_DEPLOYMENT_GUIDE.md](./RAILWAY_DEPLOYMENT_GUIDE.md) - Hướng dẫn deploy
- [RAILWAY_DATABASE_ONLY.md](./RAILWAY_DATABASE_ONLY.md) - Chỉ deploy database

