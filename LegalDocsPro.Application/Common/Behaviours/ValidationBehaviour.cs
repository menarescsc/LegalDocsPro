using FluentValidation;
using MediatR;

namespace LegalDocsPro.Application.Common.Behaviours
{
    // Este código intercepta CUALQUIER comando antes de que llegue a su Handler
    public class ValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehaviour(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (_validators.Any())
            {
                var context = new ValidationContext<TRequest>(request);

                // Ejecutamos todos los validadores que existan para este comando
                var validationResults = await Task.WhenAll(
                    _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

                // Recopilamos los errores
                var failures = validationResults
                    .SelectMany(r => r.Errors)
                    .Where(f => f != null)
                    .ToList();

                if (failures.Count != 0)
                {
                    // Si hay errores, lanzamos una excepción de FluentValidation
                    throw new ValidationException(failures);
                }
            }

            // Si todo está bien, le decimos a MediatR que continúe hacia el Handler
            return await next();
        }
    }
}