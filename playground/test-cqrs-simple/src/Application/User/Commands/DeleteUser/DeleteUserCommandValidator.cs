using FluentValidation;

namespace SimpleService.Application.User.Commands.DeleteUser;

public class DeleteUserCommandValidator : AbstractValidator<DeleteUserCommand>
{
    public DeleteUserCommandValidator()
    {
        // Add validation rules as needed
    }
}