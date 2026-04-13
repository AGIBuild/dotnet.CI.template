using System;

namespace ChengYuan.ExceptionHandling;

public sealed class DefaultExceptionToErrorInfoConverter : IExceptionToErrorInfoConverter
{
    public ErrorInfo Convert(Exception exception, bool includeSensitiveDetails)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return exception switch
        {
            BusinessException businessException => ConvertBusinessException(businessException),
            _ => ConvertGenericException(exception, includeSensitiveDetails),
        };
    }

    private static ErrorInfo ConvertBusinessException(BusinessException exception)
    {
        return new ErrorInfo(
            code: exception.Code,
            message: exception.Message,
            details: exception.Details);
    }

    private static ErrorInfo ConvertGenericException(Exception exception, bool includeSensitiveDetails)
    {
        if (includeSensitiveDetails)
        {
            return new ErrorInfo(
                message: exception.Message,
                details: exception.ToString());
        }

        return new ErrorInfo(
            message: "An internal error occurred during your request.");
    }
}
