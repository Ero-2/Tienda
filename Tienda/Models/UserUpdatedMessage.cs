// Models/Messages.cs
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Tienda.Models;

public class UserUpdatedMessage : ValueChangedMessage<string>
{
    public UserUpdatedMessage(string value) : base(value) { }
}