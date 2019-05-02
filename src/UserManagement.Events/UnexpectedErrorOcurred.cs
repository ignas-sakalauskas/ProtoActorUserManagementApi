using System;

namespace UserManagement.Events
{
    public sealed class UnexpectedErrorOcurred : UserEvent
    {
        public UnexpectedErrorOcurred(Guid userId)
        {
            UserId = userId;
        }

        public Guid UserId { get; }
    }
}