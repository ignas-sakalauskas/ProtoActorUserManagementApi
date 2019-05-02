using System;

namespace UserManagement.Events
{
    public sealed class UserNotFound : UserEvent
    {
        public UserNotFound(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; }
    }
}