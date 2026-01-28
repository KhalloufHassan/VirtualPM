namespace VirtualPM.Services;

public interface IMessageGenerator
{
    Task<string> GenerateHumorousMessageAsync();
}
