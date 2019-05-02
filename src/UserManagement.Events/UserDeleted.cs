using System;

namespace UserManagement.Events
{
    public sealed class UserDeleted : UserEvent
    {
        public UserDeleted(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; }
    }
}