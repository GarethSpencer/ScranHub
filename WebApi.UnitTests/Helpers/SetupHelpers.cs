using FluentValidation;
using FluentValidation.Results;
using Moq;

namespace WebApi.UnitTests.Helpers;

public class SetupHelpers
{
    public static void SetupValidatorPass<T>(Mock<IValidator<T>> validatorMock)
    {
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<T>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
    }

    public static void SetupValidatorFail<T>(Mock<IValidator<T>> validatorMock, string propertyName = "Test Property", string errorMessage = "The value is invalid.")
    {
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<T>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([
                new ValidationFailure(propertyName, errorMessage)
            ]));
    }
}
