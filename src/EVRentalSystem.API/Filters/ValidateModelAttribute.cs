using EVRentalSystem.Application.DTOs.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EVRentalSystem.API.Filters;

public class ValidateModelAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors.Select(e => 
                    string.IsNullOrEmpty(e.ErrorMessage) 
                        ? e.Exception?.Message ?? "Lỗi validation" 
                        : e.ErrorMessage))
                .ToList();

            var response = ApiResponse<object>.ErrorResponse(
                "Dữ liệu không hợp lệ",
                errors
            );

            context.Result = new BadRequestObjectResult(response);
        }
    }
}

