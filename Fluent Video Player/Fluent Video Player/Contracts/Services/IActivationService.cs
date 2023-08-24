namespace Fluent_Video_Player.Contracts.Services;

public interface IActivationService
{
    Task ActivateAsync(object activationArgs);
}
