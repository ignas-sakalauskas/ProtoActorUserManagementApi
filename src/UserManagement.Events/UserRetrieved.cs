using System;

namespace UserManagement.Events
{
    public sealed class UserRetrieved : UserEvent
    {
        public UserRetrieved(Guid id, string name, DateTimeOffset createdOn)
        {
            Id = id;
            Name = name;
            CreatedOn = createdOn;
        }

        public Guid Id { get; }
        public string Name { get; }
        public DateTimeOffset CreatedOn { get; }
    }
}