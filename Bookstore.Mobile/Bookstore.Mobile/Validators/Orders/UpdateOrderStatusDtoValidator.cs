using Bookstore.Mobile.Models;
using FluentValidation;

namespace Bookstore.Mobile.Validators.Orders
{
    public class UpdateOrderStatusDtoValidator : AbstractValidator<UpdateOrderStatusDto>
    {
        public UpdateOrderStatusDtoValidator()
        {
            RuleFor(x => x.NewStatus)
                .IsInEnum().WithMessage("Invalid order status provided.");
        }
    }
}