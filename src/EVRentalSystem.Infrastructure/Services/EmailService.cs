using System.Net;
using System.Net.Mail;
using EVRentalSystem.Application.DTOs.Common;
using EVRentalSystem.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EVRentalSystem.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    public async Task<bool> SendPasswordResetEmailAsync(string toEmail, string resetToken, string userName)
    {
        var subject = "Đặt lại mật khẩu - EV Rental System";
        
        // Tạo reset link (thay đổi domain theo môi trường)
        var resetLink = $"{GetFrontendUrl()}/reset-password?token={resetToken}&email={Uri.EscapeDataString(toEmail)}";
        
        var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
        .content {{ background-color: #f9f9f9; padding: 30px; }}
        .button {{ 
            display: inline-block; 
            padding: 12px 30px; 
            background-color: #4CAF50; 
            color: white; 
            text-decoration: none; 
            border-radius: 5px;
            margin: 20px 0;
        }}
        .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
        .warning {{ color: #d32f2f; font-weight: bold; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🔐 Đặt lại mật khẩu</h1>
        </div>
        <div class='content'>
            <p>Xin chào <strong>{userName}</strong>,</p>
            
            <p>Bạn đã yêu cầu đặt lại mật khẩu cho tài khoản EV Rental System của mình.</p>
            
            <p>Vui lòng click vào nút bên dưới để đặt lại mật khẩu:</p>
            
            <div style='text-align: center;'>
                <a href='{resetLink}' class='button'>Đặt lại mật khẩu</a>
            </div>
            
            <p>Hoặc copy link sau vào trình duyệt:</p>
            <p style='background-color: #e0e0e0; padding: 10px; word-break: break-all;'>
                {resetLink}
            </p>
            
            <p class='warning'>⚠️ Link này sẽ hết hạn sau 1 giờ.</p>
            
            <p>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này. Tài khoản của bạn vẫn an toàn.</p>
        </div>
        <div class='footer'>
            <p>© 2024 EV Rental System. All rights reserved.</p>
            <p>Email này được gửi tự động, vui lòng không trả lời.</p>
        </div>
    </div>
</body>
</html>";

        return await SendEmailAsync(toEmail, subject, body);
    }

    public async Task<bool> SendWelcomeEmailAsync(string toEmail, string userName)
    {
        var subject = "Chào mừng đến với EV Rental System! 🎉";
        
        var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
        .content {{ background-color: #f9f9f9; padding: 30px; }}
        .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🚗 Chào mừng bạn!</h1>
        </div>
        <div class='content'>
            <p>Xin chào <strong>{userName}</strong>,</p>
            
            <p>Cảm ơn bạn đã đăng ký tài khoản tại <strong>EV Rental System</strong>!</p>
            
            <p>Bạn đã có thể:</p>
            <ul>
                <li>✅ Đặt xe điện trực tuyến</li>
                <li>✅ Xem lịch sử thuê xe</li>
                <li>✅ Quản lý thông tin cá nhân</li>
                <li>✅ Thanh toán trực tuyến</li>
            </ul>
            
            <p>Hãy bắt đầu trải nghiệm dịch vụ thuê xe điện của chúng tôi ngay hôm nay!</p>
            
            <p>Nếu bạn có bất kỳ câu hỏi nào, đừng ngần ngại liên hệ với chúng tôi.</p>
        </div>
        <div class='footer'>
            <p>© 2024 EV Rental System. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

        return await SendEmailAsync(toEmail, subject, body);
    }

    public async Task<bool> SendBookingConfirmationEmailAsync(string toEmail, string userName, string bookingCode)
    {
        var subject = $"Xác nhận đặt xe - Mã đặt: {bookingCode}";
        
        var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
        .content {{ background-color: #f9f9f9; padding: 30px; }}
        .booking-code {{ 
            background-color: #4CAF50; 
            color: white; 
            padding: 15px; 
            text-align: center; 
            font-size: 24px; 
            font-weight: bold;
            border-radius: 5px;
            margin: 20px 0;
        }}
        .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>✅ Đặt xe thành công!</h1>
        </div>
        <div class='content'>
            <p>Xin chào <strong>{userName}</strong>,</p>
            
            <p>Cảm ơn bạn đã đặt xe tại EV Rental System!</p>
            
            <p>Mã đặt xe của bạn là:</p>
            <div class='booking-code'>{bookingCode}</div>
            
            <p>Vui lòng mang theo:</p>
            <ul>
                <li>📱 Mã đặt xe này</li>
                <li>🪪 CMND/CCCD</li>
                <li>🚗 Giấy phép lái xe</li>
            </ul>
            
            <p>Khi đến điểm thuê để nhận xe.</p>
            
            <p>Chúc bạn có chuyến đi an toàn và vui vẻ!</p>
        </div>
        <div class='footer'>
            <p>© 2024 EV Rental System. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

        return await SendEmailAsync(toEmail, subject, body);
    }

    private async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
    {
        // Kiểm tra cấu hình email
        if (string.IsNullOrEmpty(_emailSettings.SmtpHost) || 
            string.IsNullOrEmpty(_emailSettings.SmtpUsername))
        {
            _logger.LogWarning("Email service chưa được cấu hình. Email sẽ không được gửi.");
            _logger.LogInformation("Email would be sent to: {Email}", toEmail);
            _logger.LogInformation("Subject: {Subject}", subject);
            return false;
        }

        try
        {
            using var smtpClient = new SmtpClient(_emailSettings.SmtpHost, _emailSettings.SmtpPort)
            {
                EnableSsl = _emailSettings.EnableSsl,
                Credentials = new NetworkCredential(
                    _emailSettings.SmtpUsername,
                    _emailSettings.SmtpPassword
                )
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);

            await smtpClient.SendMailAsync(mailMessage);
            
            _logger.LogInformation("Email sent successfully to {Email}", toEmail);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
            return false;
        }
    }

    private string GetFrontendUrl()
    {
        // Lấy URL frontend từ environment variable hoặc dùng default
        var frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL");
        
        if (!string.IsNullOrEmpty(frontendUrl))
        {
            return frontendUrl;
        }

        // Default URLs theo môi trường
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        return environment switch
        {
            "Production" => "https://evrentalsystem.com",
            "Development" => "http://localhost:3000",
            _ => "http://localhost:3000"
        };
    }
}
