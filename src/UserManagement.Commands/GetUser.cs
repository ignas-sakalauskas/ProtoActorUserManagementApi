using System;

namespace UserManagement.Commands
{
    public sealed class GetUser : UserMessage
    {
        public GetUser(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; }
    }
}