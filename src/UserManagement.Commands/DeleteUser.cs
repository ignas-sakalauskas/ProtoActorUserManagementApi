using System;

namespace UserManagement.Commands
{
    public sealed class DeleteUser : UserMessage
    {
        public DeleteUser(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; }
    }
}