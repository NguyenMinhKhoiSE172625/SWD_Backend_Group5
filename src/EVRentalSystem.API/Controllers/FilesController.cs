using EVRentalSystem.API.Filters;
using EVRentalSystem.Application.DTOs.Common;
using EVRentalSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EVRentalSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[ValidateModel]
public class FilesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<FilesController> _logger;

    public FilesController(
        ApplicationDbContext context,
        IWebHostEnvironment environment,
        ILogger<FilesController> logger)
    {
        _context = context;
        _environment = environment;
        _logger = logger;
    }

    /// <summary>
    /// Upload file (ảnh giấy phép lái xe, CMND/CCCD, ảnh xe)
    /// </summary>
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [Produces("application/json")]
    public async Task<IActionResult> UploadFile([FromForm] FileUploadRequest request)
    {
        if (request.File == null || request.File.Length == 0)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Không có file được upload"));
        }

        // Validate file type
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
        var fileExtension = Path.GetExtension(request.File.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(fileExtension))
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Định dạng file không hợp lệ. Chỉ chấp nhận: jpg, jpeg, png, pdf"));
        }

        // Validate file size (max 5MB)
        if (request.File.Length > 5 * 1024 * 1024)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("File quá lớn. Kích thước tối đa: 5MB"));
        }

        try
        {
            // Create uploads directory if not exists
            var uploadsPath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, "uploads", request.Type);
            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }

            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadsPath, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await request.File.CopyToAsync(stream);
            }

            // Generate URL (relative path)
            var fileUrl = $"/uploads/{request.Type}/{fileName}";

            return Ok(ApiResponse<object>.SuccessResponse(new
            {
                FileUrl = fileUrl,
                FileName = fileName,
                FileSize = request.File.Length,
                ContentType = request.File.ContentType
            }, "Upload file thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file");
            return BadRequest(ApiResponse<object>.ErrorResponse("Lỗi khi upload file"));
        }
    }

    /// <summary>
    /// Upload giấy phép lái xe và CMND/CCCD (Renter)
    /// </summary>
    [HttpPost("upload-documents")]
    [Authorize(Roles = "Renter")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [Produces("application/json")]
    public async Task<IActionResult> UploadDocuments([FromForm] DocumentsUploadRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse("Không tìm thấy người dùng"));
        }

        var uploadedFiles = new List<string>();

        try
        {
            // Upload driver license
            if (request.DriverLicense != null && request.DriverLicense.Length > 0)
            {
                var driverLicenseUrl = await SaveFile(request.DriverLicense, "documents", "driver-license");
                user.DriverLicenseImageUrl = driverLicenseUrl;
                uploadedFiles.Add("Giấy phép lái xe");
            }

            // Upload ID card
            if (request.IdCard != null && request.IdCard.Length > 0)
            {
                var idCardUrl = await SaveFile(request.IdCard, "documents", "id-card");
                user.IdCardImageUrl = idCardUrl;
                uploadedFiles.Add("CMND/CCCD");
            }

            if (uploadedFiles.Count == 0)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Không có file nào được upload"));
            }

            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.SuccessResponse(new
            {
                DriverLicenseImageUrl = user.DriverLicenseImageUrl,
                IdCardImageUrl = user.IdCardImageUrl
            }, $"Upload thành công: {string.Join(", ", uploadedFiles)}"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading documents");
            return BadRequest(ApiResponse<object>.ErrorResponse("Lỗi khi upload giấy tờ"));
        }
    }

    private async Task<string> SaveFile(IFormFile file, string folder, string prefix)
    {
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(fileExtension))
        {
            throw new Exception("Định dạng file không hợp lệ");
        }

        if (file.Length > 5 * 1024 * 1024)
        {
            throw new Exception("File quá lớn");
        }

        var uploadsPath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, "uploads", folder);
        if (!Directory.Exists(uploadsPath))
        {
            Directory.CreateDirectory(uploadsPath);
        }

        var fileName = $"{prefix}_{Guid.NewGuid()}{fileExtension}";
        var filePath = Path.Combine(uploadsPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return $"/uploads/{folder}/{fileName}";
    }
}

