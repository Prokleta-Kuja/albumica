using albumica.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace albumica.Extensions;

public class ExceptionFilter : IActionFilter, IOrderedFilter
{
    public int Order => int.MaxValue - 10;

    public void OnActionExecuting(ActionExecutingContext context) { }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Exception != null)
        {
            var message = C.IsDebug ? context.Exception.InnerException != null ? context.Exception.InnerException.Message : context.Exception.Message : "Something went wrong";
            context.Result = new ObjectResult(new PlainError(message)) { StatusCode = StatusCodes.Status500InternalServerError };
            context.ExceptionHandled = true;
        }
    }
}