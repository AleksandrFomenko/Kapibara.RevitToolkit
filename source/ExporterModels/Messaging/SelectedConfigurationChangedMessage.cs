using CommunityToolkit.Mvvm.Messaging.Messages;

namespace ExporterModels.Messaging;

public sealed class SelectedConfigurationChangedMessage : ValueChangedMessage<string>
{
    public SelectedConfigurationChangedMessage(string newPath) : base(newPath)
    {
    }
}