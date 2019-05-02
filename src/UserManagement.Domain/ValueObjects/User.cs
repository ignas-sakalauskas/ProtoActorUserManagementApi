using System;

namespace UserManagement.Domain.ValueObjects
{
    public class User : ValueObject<User>
    {
        public User(Guid id, string name, DateTimeOffset createdOn)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("User ID must not be empty");
            Id = id;

            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("User name must not be empty");
            Name = name;

            if (createdOn == DateTimeOffset.MinValue)
                throw new ArgumentException("Created On date must not be empty");
            CreatedOn = createdOn;
        }

        public Guid Id { get; }
        public string Name { get; }
        public DateTimeOffset CreatedOn { get; }

        protected override bool EqualsCore(User other)
        {
            return Id == other.Id &&
                    Name == other.Name &&
                    CreatedOn == other.CreatedOn;
        }

        protected override int GetHashCodeCore()
        {
            unchecked
            {
                var hashCode = Id.GetHashCode();
                hashCode = (hashCode * 397) ^ Name.GetHashCode();
                hashCode = (hashCode * 397) ^ CreatedOn.GetHashCode();
                return hashCode;
            }
        }
    }
}