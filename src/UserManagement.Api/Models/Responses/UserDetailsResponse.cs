using System;

namespace UserManagement.Api.Models.Responses
{
    public class UserDetailsResponse
    {
        public UserDetailsResponse(Guid id, string name, DateTimeOffset createdOn)
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