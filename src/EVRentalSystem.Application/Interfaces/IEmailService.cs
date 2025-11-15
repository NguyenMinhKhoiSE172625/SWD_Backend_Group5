namespace EVRentalSystem.Application.Interfaces;

public interface IEmailService
{
    Task<bool> SendPasswordResetEmailAsync(string toEmail, string resetToken, string userName);
    Task<bool> SendWelcomeEmailAsync(string toEmail, string userName);
    Task<bool> SendBookingConfirmationEmailAsync(string toEmail, string userName, string bookingCode);
}
