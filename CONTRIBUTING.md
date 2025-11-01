# Contributing to EV Rental System

Cảm ơn bạn đã quan tâm đến việc đóng góp cho dự án! 🎉

## 🚀 Cách đóng góp

### 1. Fork Repository
Click nút "Fork" ở góc trên bên phải của trang GitHub.

### 2. Clone Fork của bạn
```bash
git clone https://github.com/your-username/ev-rental-system.git
cd ev-rental-system
```

### 3. Tạo Branch mới
```bash
git checkout -b feature/ten-tinh-nang-moi
```

Quy tắc đặt tên branch:
- `feature/` - Tính năng mới
- `bugfix/` - Sửa lỗi
- `hotfix/` - Sửa lỗi khẩn cấp
- `docs/` - Cập nhật tài liệu
- `refactor/` - Refactor code

### 4. Thực hiện thay đổi
- Viết code sạch, dễ đọc
- Follow coding conventions của dự án
- Thêm comments khi cần thiết
- Test kỹ trước khi commit

### 5. Commit thay đổi
```bash
git add .
git commit -m "feat: thêm tính năng ABC"
```

Quy tắc commit message:
- `feat:` - Tính năng mới
- `fix:` - Sửa lỗi
- `docs:` - Cập nhật tài liệu
- `style:` - Format code
- `refactor:` - Refactor code
- `test:` - Thêm tests
- `chore:` - Cập nhật build, dependencies

### 6. Push lên GitHub
```bash
git push origin feature/ten-tinh-nang-moi
```

### 7. Tạo Pull Request
1. Vào repository của bạn trên GitHub
2. Click "Compare & pull request"
3. Mô tả chi tiết những gì bạn đã thay đổi
4. Submit Pull Request

## 📋 Coding Guidelines

### C# Conventions
- Sử dụng PascalCase cho class, method, property
- Sử dụng camelCase cho biến local
- Sử dụng async/await cho operations bất đồng bộ
- Luôn handle exceptions

### API Design
- RESTful API conventions
- Sử dụng HTTP status codes đúng cách
- Validation đầy đủ cho input
- Error messages rõ ràng bằng tiếng Việt

### Database
- Sử dụng Entity Framework Core
- Migration cho mọi thay đổi schema
- Seed data cho testing

## 🧪 Testing

Trước khi submit PR:
1. Build thành công: `dotnet build`
2. Test trên Swagger UI
3. Kiểm tra validation
4. Test với các role khác nhau

## 📝 Documentation

Nếu thêm tính năng mới:
- Cập nhật README.md
- Cập nhật FRONTEND_GUIDE.md (nếu có API mới)
- Thêm XML comments cho code
- Cập nhật Swagger documentation

## ❓ Câu hỏi

Nếu có câu hỏi, tạo [Issue](https://github.com/your-username/ev-rental-system/issues) trên GitHub.

## 🙏 Cảm ơn

Mọi đóng góp đều được đánh giá cao!

