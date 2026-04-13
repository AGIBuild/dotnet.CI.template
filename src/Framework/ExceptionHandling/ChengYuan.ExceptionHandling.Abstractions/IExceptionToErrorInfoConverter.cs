using System;

namespace ChengYuan.ExceptionHandling;

public interface IExceptionToErrorInfoConverter
{
    ErrorInfo Convert(Exception exception, bool includeSensitiveDetails);
}
