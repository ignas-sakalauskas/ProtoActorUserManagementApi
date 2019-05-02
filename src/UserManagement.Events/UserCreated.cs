using System;

namespace UserManagement.Events
{
    public sealed class UserCreated : UserEvent
    {
        public UserCreated(Guid userId, string name, DateTimeOffset createdOn)
        {
            Id = userId;
            Name = name;
            CreatedOn = createdOn;
        }

        public Guid Id { get; }
        public string Name { get; }
        public DateTimeOffset CreatedOn { get; }
    }
}