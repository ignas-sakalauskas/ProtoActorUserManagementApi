using System;

namespace UserManagement.Commands
{
    public sealed class CreateUser : UserMessage
    {
        public CreateUser(Guid id, string name)
        {
            Id = id;
            Name = name;
        }

        public Guid Id { get; }
        public string Name { get; set; }
    }
}