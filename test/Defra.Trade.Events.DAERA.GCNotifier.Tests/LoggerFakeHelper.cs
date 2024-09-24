// Copyright DEFRA (c). All rights reserved.
// Licensed under the Open Government License v3.0.

using System.Linq.Expressions;
using System.Reflection;
using FakeItEasy;
using FakeItEasy.Configuration;
using Microsoft.Extensions.Logging;

namespace Defra.Trade.Events.DAERA.GCNotifier;

internal static class LoggerFakeHelper
{
    private static readonly PropertyInfo _aStateThat;
    private static readonly MethodInfo _aStateThatMatches;
    private static readonly MethodInfo _loggerLogMethod;
    private static readonly Delegate _messageFormatter;
    private static readonly MethodInfo _sequenceEqual;
    private static readonly MemberExpression _stateOriginalMessage;
    private static readonly ParameterExpression _stateParam;
    private static readonly MemberExpression _stateValues;

#pragma warning disable S3963 // "static" fields should be initialized inline

    static LoggerFakeHelper()
#pragma warning restore S3963 // "static" fields should be initialized inline
    {
        Expression<Action<ILogger>> logExpr = l => l.LogInformation(null as string, null!);
        var tLoggerExtensions = ((MethodCallExpression)logExpr.Body).Method.DeclaringType!;
        _messageFormatter = (Delegate)tLoggerExtensions.GetField("_messageFormatter", BindingFlags.Static | BindingFlags.NonPublic)!.GetValue(null)!;
        var tFormattedLogValues = _messageFormatter.GetType().GetGenericArguments()[0]!;
        _loggerLogMethod = typeof(ILogger).GetMethod(nameof(ILogger.Log))!.MakeGenericMethod(tFormattedLogValues);
        _stateParam = Expression.Parameter(tFormattedLogValues, "p");
        _stateOriginalMessage = Expression.Field(_stateParam, tFormattedLogValues.GetField("_originalMessage", BindingFlags.NonPublic | BindingFlags.Instance)!);
        _stateValues = Expression.Field(_stateParam, tFormattedLogValues.GetField("_values", BindingFlags.NonPublic | BindingFlags.Instance)!);
        _sequenceEqual = typeof(Enumerable).GetMethods().Single(m => m.Name == nameof(Enumerable.SequenceEqual) && m.GetParameters().Length == 2).MakeGenericMethod(typeof(object));
        _aStateThat = typeof(A<>).MakeGenericType(tFormattedLogValues).GetProperty(nameof(A<object>.That))!;
        _aStateThatMatches = typeof(ArgumentConstraintManagerExtensions).GetMethods().Single(m => m.Name == nameof(ArgumentConstraintManagerExtensions.Matches) && m.GetParameters().Length == 2)!.MakeGenericMethod(tFormattedLogValues);
    }

    public static IVoidArgumentValidationConfiguration LoggerCall(ILogger logger, LogLevel logLevel, EventId eventId, Exception? exception, string message, Expression<Func<object?[]>>? messageArgs = null)
    {
        return A.CallTo(Expression.Lambda<Action>(
            Expression.Call(
                Expression.Constant(logger),
                _loggerLogMethod,
                    Expression.Constant(logLevel),
                    Expression.Constant(eventId),
                    Expression.Call(
                        _aStateThatMatches,
                            Expression.Property(null, _aStateThat),
                            Expression.Lambda(
                                Expression.And(
                                    Expression.Equal(_stateOriginalMessage, Expression.Constant(message)),
                                    Expression.Call(_sequenceEqual, _stateValues, messageArgs?.Body ?? Expression.Constant(Array.Empty<object>()))
                                ),
                                _stateParam
                            )
                    ),
                    Expression.Constant(exception, typeof(Exception)),
                    Expression.Constant(_messageFormatter)
            )
        ));
    }
}
