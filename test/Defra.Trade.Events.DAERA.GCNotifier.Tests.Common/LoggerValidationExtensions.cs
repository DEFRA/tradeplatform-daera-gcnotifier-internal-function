// Copyright DEFRA (c). All rights reserved.
// Licensed under the Open Government License v3.0.

using Microsoft.Extensions.Logging;
using Moq;

namespace Defra.Trade.Events.DAERA.GCNotifier.Tests.Common;

public static class LoggerValidationExtensions
{
    public static void VerifyLoggedWarning<T>(this Mock<ILogger<T>> logger, string messageContained)
    {
        logger.VerifyLogged(messageContained, LogLevel.Warning);
    }

    public static void VerifyLogged<T>(this Mock<ILogger<T>> logger, string messageContained, LogLevel logLevel)
    {
        logger.Verify(
            l => l.Log(
                It.Is<LogLevel>(level => level == logLevel),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((@object, type) => @object.ToString()!.Contains(messageContained)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    public static void VerifyLoggedWithException<T>(this Mock<ILogger<T>> logger, string messageContained, LogLevel logLevel, Exception exception)
    {
        logger.Verify(
            l => l.Log(
                It.Is<LogLevel>(level => level == logLevel),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((@object, type) => @object.ToString()!.Contains(messageContained)),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    public static void VerifyLoggedWithExceptionMessage<T>(this Mock<ILogger<T>> logger, string messageContained, LogLevel logLevel, string exceptionMessage)
    {
        logger.Verify(
            l => l.Log(
                It.Is<LogLevel>(level => level == logLevel),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((@object, type) => @object.ToString()!.Contains(messageContained)),
                It.Is<Exception>(e => e.Message == exceptionMessage),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    public static void VerifyNoWarningsLogged<T>(this Mock<ILogger<T>> logger)
    {
        logger.Verify(
            l => l.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Warning),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    public static void VerifyNoCriticalsLogged<T>(this Mock<ILogger<T>> logger)
    {
        logger.Verify(
            l => l.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Critical),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    public static void VerifyNothingLogged<T>(this Mock<ILogger<T>> logger)
    {
        logger.Verify(
            l => l.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }
}
