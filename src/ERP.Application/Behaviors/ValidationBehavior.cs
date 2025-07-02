using ERP.SharedKernel.Utilities;

using FluentValidation;

using MediatR;

namespace ERP.Application.Behaviors
{
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : class
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (!_validators.Any())
            {
                return await next();
            }

            var context = new ValidationContext<TRequest>(request);
            var validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));
            var failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Any())
            {
                var errors = failures.Select(f => f.ErrorMessage).ToList();

                // Result<T> 타입인 경우 실패 결과 반환
                if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
                {
                    var resultType = typeof(TResponse).GetGenericArguments()[0];

                    // 정확한 제네릭 메서드 타입을 직접 지정
                    var failureMethod = typeof(Result)
                        .GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                        .Where(m => m.Name == "Failure" &&
                                   m.IsGenericMethod &&
                                   m.GetGenericArguments().Length == 1)
                        .FirstOrDefault(m => m.GetParameters().Length == 1 &&
                                           m.GetParameters()[0].ParameterType == typeof(IEnumerable<string>));

                    if (failureMethod != null)
                    {
                        var genericMethod = failureMethod.MakeGenericMethod(resultType);
                        return (TResponse)genericMethod.Invoke(null, new object[] { errors })!;
                    }

                    // 대안: 단일 에러 메시지로 처리
                    var singleFailureMethod = typeof(Result)
                        .GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                        .Where(m => m.Name == "Failure" &&
                                   m.IsGenericMethod &&
                                   m.GetGenericArguments().Length == 1)
                        .FirstOrDefault(m => m.GetParameters().Length == 1 &&
                                           m.GetParameters()[0].ParameterType == typeof(string));

                    if (singleFailureMethod != null)
                    {
                        var genericMethod = singleFailureMethod.MakeGenericMethod(resultType);
                        var errorMessage = string.Join("; ", errors);
                        return (TResponse)genericMethod.Invoke(null, new object[] { errorMessage })!;
                    }
                }

                throw new ValidationException(failures);
            }

            return await next();
        }
    }
}