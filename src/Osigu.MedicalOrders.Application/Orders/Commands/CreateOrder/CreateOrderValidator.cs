using Osigu.MedicalOrders.Domain.Enums;

namespace Osigu.MedicalOrders.Application.Orders.Commands.CreateOrder;

public static class CreateOrderValidator
{
    public static IReadOnlyList<string> Validate(CreateOrderCommand command)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(command.PatientId))
        {
            errors.Add("PatientId is required.");
        }

        if (string.IsNullOrWhiteSpace(command.ServiceCode))
        {
            errors.Add("ServiceCode is required.");
        }

        if (!Enum.IsDefined(command.Priority))
        {
            errors.Add("Priority must be Normal or Urgent.");
        }

        return errors;
    }
}
