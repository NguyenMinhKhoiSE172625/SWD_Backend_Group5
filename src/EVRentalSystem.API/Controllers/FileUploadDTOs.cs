namespace EVRentalSystem.API.Controllers;

public class FileUploadRequest
{
    public IFormFile File { get; set; } = null!;
    public string Type { get; set; } = "general";
}

public class DocumentsUploadRequest
{
    public IFormFile? DriverLicense { get; set; }
    public IFormFile? IdCard { get; set; }
}

