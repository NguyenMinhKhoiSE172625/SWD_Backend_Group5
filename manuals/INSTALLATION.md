# Installation Manual - EV Rental System Backend

Hướng dẫn cài đặt và triển khai hệ thống Backend API cho dự án thuê xe điện.

## 📋 Yêu cầu hệ thống

### Phần mềm cần thiết
- **.NET 8 SDK** (hoặc cao hơn)
  - Download: https://dotnet.microsoft.com/download/dotnet/8.0
- **Visual Studio 2022** (khuyến nghị) hoặc **Visual Studio Code**
- **Git** (để clone project)

### Kiểm tra cài đặt
Mở terminal/command prompt và chạy:
```bash
dotnet --version
```
Kết quả phải là `8.0.x` hoặc cao hơn.

## 🚀 Cài đặt từ Source Code

### Bước 1: Clone hoặc Copy Project

Nếu có Git repository:
```bash
git clone <repository-url>
cd SWD
```

Hoặc copy toàn bộ folder `SWD` vào máy.

### Bước 2: Restore Dependencies

```bash
cd d:\Study\SWD
dotnet restore
```

Lệnh này sẽ tải về tất cả NuGet packages cần thiết:
- Microsoft.EntityFrameworkCore.Sqlite (9.0.10)
- Microsoft.AspNetCore.Authentication.JwtBearer (8.0.11)
- BCrypt.Net-Next (4.0.3)
- Swashbuckle.AspNetCore (7.2.0)

### Bước 3: Build Project

```bash
dotnet build
```

Đảm bảo build thành công không có lỗi.

### Bước 4: Chạy ứng dụng

```bash
dotnet run --project src/EVRentalSystem.API
```

Hoặc nếu đang ở trong folder `src/EVRentalSystem.API`:
```bash
dotnet run
```

### Bước 5: Kiểm tra

Mở trình duyệt và truy cập:
```
http://localhost:5085
```

Bạn sẽ thấy Swagger UI với tất cả API endpoints.

## 🗄️ Database Setup

### SQLite Database (Mặc định)

Database SQLite sẽ được tạo tự động khi chạy ứng dụng lần đầu.

**File database:** `EVRentalSystem.db` (trong thư mục gốc của project)

**Seed data** sẽ tự động được thêm vào bao gồm:
- 3 điểm thuê
- 6 xe điện
- 5 users (1 Admin, 2 Staff, 2 Renters)

### Xóa và tạo lại Database

Nếu muốn reset database:

1. **Dừng ứng dụng** (Ctrl+C)
2. **Xóa file database:**
   ```bash
   del EVRentalSystem.db
   ```
3. **Chạy lại ứng dụng:**
   ```bash
   dotnet run --project src/EVRentalSystem.API
   ```

### Chuyển sang SQL Server (Tùy chọn)

Nếu muốn sử dụng SQL Server thay vì SQLite:

1. **Cài đặt SQL Server package:**
   ```bash
   dotnet add src/EVRentalSystem.Infrastructure/EVRentalSystem.Infrastructure.csproj package Microsoft.EntityFrameworkCore.SqlServer
   ```

2. **Sửa `appsettings.json`:**
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost;Database=EVRentalSystemDB;Trusted_Connection=True;TrustServerCertificate=True;"
   }
   ```

3. **Sửa `Program.cs`:**
   ```csharp
   builder.Services.AddDbContext<ApplicationDbContext>(options =>
       options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
   ```

4. **Tạo migration mới:**
   ```bash
   dotnet ef migrations add InitialCreate --project src/EVRentalSystem.Infrastructure --startup-project src/EVRentalSystem.API
   dotnet ef database update --project src/EVRentalSystem.Infrastructure --startup-project src/EVRentalSystem.API
   ```

## ⚙️ Cấu hình

### appsettings.json

File cấu hình chính: `src/EVRentalSystem.API/appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=EVRentalSystem.db"
  },
  "Jwt": {
    "Key": "YourSuperSecretKeyForJWTTokenGenerationMinimum32Characters!",
    "Issuer": "EVRentalSystem",
    "Audience": "EVRentalSystemUsers"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### Thay đổi JWT Secret Key (Khuyến nghị cho Production)

Trong `appsettings.json`, thay đổi giá trị `Jwt.Key`:
```json
"Jwt": {
  "Key": "YOUR_NEW_SECRET_KEY_AT_LEAST_32_CHARACTERS_LONG",
  "Issuer": "EVRentalSystem",
  "Audience": "EVRentalSystemUsers"
}
```

### Thay đổi Port

Mặc định ứng dụng chạy trên port `5085`.

Để thay đổi, sửa file `src/EVRentalSystem.API/Properties/launchSettings.json`:
```json
"applicationUrl": "http://localhost:YOUR_PORT"
```

## 🐳 Docker Deployment (Tùy chọn)

### Tạo Dockerfile

Tạo file `Dockerfile` trong thư mục gốc:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/EVRentalSystem.API/EVRentalSystem.API.csproj", "EVRentalSystem.API/"]
COPY ["src/EVRentalSystem.Application/EVRentalSystem.Application.csproj", "EVRentalSystem.Application/"]
COPY ["src/EVRentalSystem.Domain/EVRentalSystem.Domain.csproj", "EVRentalSystem.Domain/"]
COPY ["src/EVRentalSystem.Infrastructure/EVRentalSystem.Infrastructure.csproj", "EVRentalSystem.Infrastructure/"]
RUN dotnet restore "EVRentalSystem.API/EVRentalSystem.API.csproj"
COPY src/ .
WORKDIR "/src/EVRentalSystem.API"
RUN dotnet build "EVRentalSystem.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EVRentalSystem.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EVRentalSystem.API.dll"]
```

### Build và Run Docker

```bash
docker build -t ev-rental-api .
docker run -d -p 8080:80 --name ev-rental-api ev-rental-api
```

Truy cập: `http://localhost:8080`

## 🌐 Deployment lên Server

### IIS (Windows Server)

1. **Publish project:**
   ```bash
   dotnet publish src/EVRentalSystem.API/EVRentalSystem.API.csproj -c Release -o ./publish
   ```

2. **Cài đặt .NET 8 Hosting Bundle** trên server:
   - Download: https://dotnet.microsoft.com/download/dotnet/8.0

3. **Tạo Application Pool** trong IIS:
   - .NET CLR Version: No Managed Code
   - Managed Pipeline Mode: Integrated

4. **Tạo Website** trong IIS:
   - Physical path: Trỏ đến folder `publish`
   - Application Pool: Chọn pool vừa tạo

5. **Copy file `appsettings.json`** và cấu hình connection string phù hợp

### Linux (Ubuntu/Debian)

1. **Cài đặt .NET 8 Runtime:**
   ```bash
   wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
   sudo dpkg -i packages-microsoft-prod.deb
   sudo apt-get update
   sudo apt-get install -y aspnetcore-runtime-8.0
   ```

2. **Publish project:**
   ```bash
   dotnet publish src/EVRentalSystem.API/EVRentalSystem.API.csproj -c Release -o /var/www/ev-rental-api
   ```

3. **Tạo systemd service:**
   ```bash
   sudo nano /etc/systemd/system/ev-rental-api.service
   ```

   Nội dung:
   ```ini
   [Unit]
   Description=EV Rental System API

   [Service]
   WorkingDirectory=/var/www/ev-rental-api
   ExecStart=/usr/bin/dotnet /var/www/ev-rental-api/EVRentalSystem.API.dll
   Restart=always
   RestartSec=10
   SyslogIdentifier=ev-rental-api
   User=www-data
   Environment=ASPNETCORE_ENVIRONMENT=Production

   [Install]
   WantedBy=multi-user.target
   ```

4. **Start service:**
   ```bash
   sudo systemctl enable ev-rental-api
   sudo systemctl start ev-rental-api
   sudo systemctl status ev-rental-api
   ```

5. **Cấu hình Nginx reverse proxy:**
   ```bash
   sudo nano /etc/nginx/sites-available/ev-rental-api
   ```

   Nội dung:
   ```nginx
   server {
       listen 80;
       server_name your-domain.com;

       location / {
           proxy_pass http://localhost:5085;
           proxy_http_version 1.1;
           proxy_set_header Upgrade $http_upgrade;
           proxy_set_header Connection keep-alive;
           proxy_set_header Host $host;
           proxy_cache_bypass $http_upgrade;
           proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
           proxy_set_header X-Forwarded-Proto $scheme;
       }
   }
   ```

   ```bash
   sudo ln -s /etc/nginx/sites-available/ev-rental-api /etc/nginx/sites-enabled/
   sudo nginx -t
   sudo systemctl restart nginx
   ```

## 🔧 Troubleshooting

### Lỗi: "Unable to connect to database"

**Giải pháp:**
- Kiểm tra connection string trong `appsettings.json`
- Đảm bảo SQL Server đang chạy (nếu dùng SQL Server)
- Với SQLite, đảm bảo ứng dụng có quyền ghi file

### Lỗi: "JWT Bearer error"

**Giải pháp:**
- Kiểm tra `Jwt.Key` trong `appsettings.json` phải ít nhất 32 ký tự
- Đảm bảo token được gửi đúng format: `Bearer {token}`

### Lỗi: "CORS policy"

**Giải pháp:**
- Kiểm tra CORS đã được enable trong `Program.cs`
- Thêm origin của frontend vào CORS policy nếu cần

### Port đã được sử dụng

**Giải pháp:**
- Thay đổi port trong `launchSettings.json`
- Hoặc kill process đang dùng port 5085:
  ```bash
  # Windows
  netstat -ano | findstr :5085
  taskkill /PID <PID> /F

  # Linux
  sudo lsof -i :5085
  sudo kill -9 <PID>
  ```

## 📊 Performance Testing

### Sử dụng Apache Bench (ab)

```bash
# Test login endpoint
ab -n 1000 -c 10 -p login.json -T application/json http://localhost:5085/api/auth/login
```

### Sử dụng JMeter

1. Download Apache JMeter
2. Tạo Thread Group với số lượng users
3. Thêm HTTP Request samplers cho các endpoints
4. Chạy test và xem báo cáo

### Metrics cần theo dõi

- **Response Time:** < 200ms cho các API đơn giản
- **Throughput:** > 100 requests/second
- **Error Rate:** < 1%
- **CPU Usage:** < 70%
- **Memory Usage:** < 500MB

## 📝 Checklist Deployment

- [ ] Build project thành công
- [ ] Tất cả tests pass (nếu có)
- [ ] Database connection string đúng
- [ ] JWT secret key đã thay đổi (production)
- [ ] CORS policy phù hợp với frontend domain
- [ ] Logging được cấu hình đúng
- [ ] HTTPS được enable (production)
- [ ] Backup database strategy
- [ ] Monitoring và alerting setup

---

**Hỗ trợ:** Nếu gặp vấn đề, vui lòng kiểm tra logs trong console hoặc file logs.

