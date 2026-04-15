using System;
using Microsoft.AspNetCore.Mvc;

namespace ChengYuan.ExceptionHandling;

public interface IExceptionToProblemDetailsMapper
{
    ProblemDetails Map(Exception exception, bool includeSensitiveDetails);
}
