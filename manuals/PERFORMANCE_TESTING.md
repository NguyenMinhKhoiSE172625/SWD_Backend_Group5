# Performance Testing Strategy & Report - EV Rental System

## 1. Mục tiêu Performance Testing

### 1.1. Mục đích
- Đánh giá khả năng xử lý của hệ thống dưới tải
- Xác định bottlenecks và điểm yếu
- Đảm bảo hệ thống đáp ứng yêu cầu về hiệu suất
- Tối ưu hóa trải nghiệm người dùng

### 1.2. Các loại test
1. **Load Testing** - Kiểm tra hệ thống với tải bình thường
2. **Stress Testing** - Kiểm tra giới hạn của hệ thống
3. **Spike Testing** - Kiểm tra với tải tăng đột ngột
4. **Endurance Testing** - Kiểm tra trong thời gian dài

## 2. Performance Requirements

### 2.1. Response Time
| Endpoint Type | Target | Acceptable | Maximum |
|--------------|--------|------------|---------|
| Authentication | < 100ms | < 200ms | 500ms |
| Read Operations (GET) | < 150ms | < 300ms | 1000ms |
| Write Operations (POST/PUT) | < 200ms | < 500ms | 2000ms |
| Complex Queries | < 300ms | < 800ms | 3000ms |

### 2.2. Throughput
- **Minimum:** 50 requests/second
- **Target:** 100 requests/second
- **Peak:** 200 requests/second

### 2.3. Concurrent Users
- **Normal Load:** 50 concurrent users
- **Peak Load:** 200 concurrent users
- **Stress Load:** 500 concurrent users

### 2.4. Resource Usage
- **CPU:** < 70% under normal load
- **Memory:** < 500MB
- **Database Connections:** < 50 active connections

## 3. Test Scenarios

### 3.1. Scenario 1: User Authentication Flow
**Mô tả:** Người dùng đăng nhập vào hệ thống

**Steps:**
1. POST /api/auth/login
2. Verify token received
3. GET /api/stations (with token)

**Expected Performance:**
- Login: < 200ms
- Get Stations: < 150ms
- Success Rate: > 99%

### 3.2. Scenario 2: Vehicle Booking Flow
**Mô tả:** Người dùng tìm và đặt xe

**Steps:**
1. Login
2. GET /api/stations/nearby
3. GET /api/vehicles/available?stationId=1
4. POST /api/bookings/create

**Expected Performance:**
- Total flow: < 1000ms
- Each step: < 300ms
- Success Rate: > 95%

### 3.3. Scenario 3: Vehicle Handover Flow (Critical)
**Mô tả:** Nhân viên giao xe cho khách hàng

**Steps:**
1. Staff login
2. GET /api/bookings/station/{stationId}
3. POST /api/bookings/{id}/confirm
4. POST /api/rentals/create (with inspection)

**Expected Performance:**
- Total flow: < 2000ms
- Rental creation: < 500ms
- Success Rate: > 99%

### 3.4. Scenario 4: Vehicle Return Flow (Critical)
**Mô tả:** Nhân viên nhận xe trả từ khách hàng

**Steps:**
1. Staff login
2. GET /api/rentals/active
3. POST /api/rentals/complete (with inspection)
4. POST /api/payments/create

**Expected Performance:**
- Total flow: < 2500ms
- Rental completion: < 800ms
- Success Rate: > 99%

## 4. Testing Tools

### 4.1. Apache JMeter
**Ưu điểm:**
- GUI dễ sử dụng
- Hỗ trợ nhiều protocol
- Báo cáo chi tiết
- Miễn phí

**Cách sử dụng:**
```bash
# Download JMeter
# https://jmeter.apache.org/download_jmeter.cgi

# Chạy JMeter
jmeter.bat (Windows)
./jmeter.sh (Linux/Mac)
```

### 4.2. Apache Bench (ab)
**Ưu điểm:**
- Đơn giản, nhanh
- Command-line
- Có sẵn trên Linux

**Ví dụ:**
```bash
# Test login endpoint
ab -n 1000 -c 10 -p login.json -T application/json \
   http://localhost:5085/api/auth/login

# Giải thích:
# -n 1000: Tổng 1000 requests
# -c 10: 10 concurrent requests
# -p login.json: POST data file
# -T: Content-Type
```

### 4.3. k6 (Khuyến nghị)
**Ưu điểm:**
- Modern, developer-friendly
- JavaScript-based scripts
- Excellent reporting
- Cloud integration

**Ví dụ script:**
```javascript
import http from 'k6/http';
import { check, sleep } from 'k6';

export let options = {
  stages: [
    { duration: '2m', target: 50 },  // Ramp up to 50 users
    { duration: '5m', target: 50 },  // Stay at 50 users
    { duration: '2m', target: 100 }, // Ramp up to 100 users
    { duration: '5m', target: 100 }, // Stay at 100 users
    { duration: '2m', target: 0 },   // Ramp down to 0 users
  ],
  thresholds: {
    http_req_duration: ['p(95)<500'], // 95% of requests < 500ms
    http_req_failed: ['rate<0.01'],   // Error rate < 1%
  },
};

export default function () {
  // Login
  let loginRes = http.post('http://localhost:5085/api/auth/login', 
    JSON.stringify({
      email: 'nguyenvana@gmail.com',
      password: 'User@123'
    }),
    { headers: { 'Content-Type': 'application/json' } }
  );
  
  check(loginRes, {
    'login successful': (r) => r.status === 200,
    'token received': (r) => r.json('data.token') !== '',
  });
  
  let token = loginRes.json('data.token');
  
  // Get available vehicles
  let vehiclesRes = http.get(
    'http://localhost:5085/api/vehicles/available?stationId=1',
    { headers: { 'Authorization': `Bearer ${token}` } }
  );
  
  check(vehiclesRes, {
    'vehicles retrieved': (r) => r.status === 200,
  });
  
  sleep(1);
}
```

**Chạy test:**
```bash
k6 run performance-test.js
```

## 5. Test Plan

### 5.1. Load Test Plan

**Objective:** Kiểm tra hệ thống với tải bình thường

**Configuration:**
- Virtual Users: 50
- Duration: 30 minutes
- Ramp-up: 5 minutes
- Scenarios: Mix of all user flows

**Success Criteria:**
- Average response time < 300ms
- 95th percentile < 500ms
- Error rate < 1%
- CPU usage < 60%

### 5.2. Stress Test Plan

**Objective:** Tìm giới hạn của hệ thống

**Configuration:**
- Start: 50 users
- Increment: +50 users every 5 minutes
- Stop: When error rate > 5% or response time > 3s
- Duration: Until failure or 500 users

**Success Criteria:**
- System handles at least 200 concurrent users
- Graceful degradation (no crashes)
- Recovery after load reduction

### 5.3. Spike Test Plan

**Objective:** Kiểm tra với tải tăng đột ngột

**Configuration:**
- Normal load: 50 users
- Spike to: 300 users
- Spike duration: 2 minutes
- Recovery time: 5 minutes

**Success Criteria:**
- System remains stable during spike
- Response time recovers after spike
- No data loss or corruption

### 5.4. Endurance Test Plan

**Objective:** Kiểm tra memory leaks và stability

**Configuration:**
- Virtual Users: 50
- Duration: 4 hours
- Constant load

**Success Criteria:**
- Memory usage stable (no leaks)
- Response time consistent
- No degradation over time

## 6. Metrics to Collect

### 6.1. Application Metrics
- **Response Time:** Average, Median, 95th percentile, 99th percentile
- **Throughput:** Requests per second
- **Error Rate:** Percentage of failed requests
- **Success Rate:** Percentage of successful requests

### 6.2. System Metrics
- **CPU Usage:** Percentage
- **Memory Usage:** MB, percentage
- **Disk I/O:** Read/write operations per second
- **Network I/O:** Bandwidth usage

### 6.3. Database Metrics
- **Query Time:** Average execution time
- **Connection Pool:** Active connections, waiting connections
- **Deadlocks:** Number of deadlocks
- **Cache Hit Ratio:** Percentage

## 7. Sample Test Results

### 7.1. Load Test Results (50 Users, 30 Minutes)

| Metric | Value | Status |
|--------|-------|--------|
| Total Requests | 45,000 | ✅ |
| Successful Requests | 44,775 | ✅ 99.5% |
| Failed Requests | 225 | ✅ 0.5% |
| Average Response Time | 245ms | ✅ |
| 95th Percentile | 480ms | ✅ |
| 99th Percentile | 850ms | ⚠️ |
| Throughput | 25 req/s | ⚠️ Below target |
| CPU Usage | 55% | ✅ |
| Memory Usage | 380MB | ✅ |

**Observations:**
- System performs well under normal load
- 99th percentile slightly high - investigate slow queries
- Throughput below target - consider optimization

### 7.2. Stress Test Results

| Users | Avg Response Time | Error Rate | Status |
|-------|------------------|------------|--------|
| 50 | 245ms | 0.5% | ✅ |
| 100 | 380ms | 1.2% | ✅ |
| 150 | 620ms | 2.8% | ⚠️ |
| 200 | 1,150ms | 5.5% | ❌ |

**Breaking Point:** 200 concurrent users

**Observations:**
- System stable up to 150 users
- Degradation starts at 150 users
- Breaks at 200 users (error rate > 5%)

## 8. Optimization Recommendations

### 8.1. Database Optimization
```sql
-- Add indexes for frequently queried fields
CREATE INDEX IX_Bookings_UserId_Status ON Bookings(UserId, Status);
CREATE INDEX IX_Rentals_Status_PickupTime ON Rentals(Status, PickupTime);
CREATE INDEX IX_Vehicles_StationId_Status ON Vehicles(StationId, Status);
```

### 8.2. Caching Strategy
```csharp
// Cache station data (rarely changes)
services.AddMemoryCache();

// In StationService
public async Task<List<StationResponse>> GetAllStationsAsync()
{
    return await _cache.GetOrCreateAsync("all_stations", async entry =>
    {
        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
        return await _context.Stations
            .Where(s => s.IsActive)
            .Select(s => new StationResponse { ... })
            .ToListAsync();
    });
}
```

### 8.3. Connection Pooling
```json
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=EVRentalSystem.db;Pooling=True;Max Pool Size=100;"
  }
}
```

### 8.4. Response Compression
```csharp
// Program.cs
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

app.UseResponseCompression();
```

## 9. Monitoring in Production

### 9.1. Application Insights (Azure)
```csharp
builder.Services.AddApplicationInsightsTelemetry();
```

### 9.2. Custom Metrics
```csharp
// Track rental creation time
var stopwatch = Stopwatch.StartNew();
var rental = await CreateRentalAsync(...);
stopwatch.Stop();
_logger.LogInformation("Rental created in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
```

### 9.3. Health Checks
```csharp
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>();

app.MapHealthChecks("/health");
```

## 10. Continuous Performance Testing

### 10.1. CI/CD Integration
```yaml
# GitHub Actions example
name: Performance Tests

on:
  push:
    branches: [ main ]

jobs:
  performance-test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v2
      - name: Run k6 tests
        uses: grafana/k6-action@v0.3.0
        with:
          filename: performance-test.js
          flags: --out json=results.json
      - name: Upload results
        uses: actions/upload-artifact@v2
        with:
          name: k6-results
          path: results.json
```

### 10.2. Performance Budgets
Set thresholds in CI/CD:
- Response time p95 < 500ms
- Error rate < 1%
- Throughput > 50 req/s

Fail build if thresholds exceeded.

## 11. Checklist

### Before Testing
- [ ] Test environment matches production
- [ ] Database has realistic data volume
- [ ] All services are running
- [ ] Monitoring tools configured
- [ ] Baseline metrics collected

### During Testing
- [ ] Monitor system resources
- [ ] Check error logs
- [ ] Verify data integrity
- [ ] Document observations
- [ ] Take screenshots of metrics

### After Testing
- [ ] Analyze results
- [ ] Identify bottlenecks
- [ ] Create optimization plan
- [ ] Document findings
- [ ] Share report with team

---

**Version:** 1.0  
**Last Updated:** 2025-11-01  
**Prepared by:** SWD Development Team

