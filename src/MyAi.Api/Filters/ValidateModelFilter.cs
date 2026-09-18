using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MyAi.Api.Filters;

/// <summary>Returns a structured 400 response when ModelState is invalid.</summary>
public class ValidateModelFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ModelState.IsValid)
        {
            return;
        }

        var errors = context.ModelState.Values
            .SelectMany(v => v.Errors)
            .Where(e => !string.IsNullOrWhiteSpace(e.ErrorMessage))
            .Select(e => e.ErrorMessage)
            .Distinct()
            .ToList();

        context.Result = new BadRequestObjectResult(new
        {
            error = true,
            message = errors.Count > 0 ? string.Join("; ", errors) : "Invalid request.",
            errors
        });
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // No post-action behaviour.
    }
}
