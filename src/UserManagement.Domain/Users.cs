using System;
using System.Collections.Concurrent;
using System.Linq;
using UserManagement.Domain.ValueObjects;
using UserManagement.Events;

namespace UserManagement.Domain
{
    public sealed class Users
    {
        public Users(ConcurrentDictionary<Guid, User> state = null)
        {
            State = state ?? new ConcurrentDictionary<Guid, User>();
        }

        public ConcurrentDictionary<Guid, User> State { get; }

        public UserEvent GetUserById(Guid id)
        {
            var user = GetUser(id);
            if (user != null)
            {
                return new UserRetrieved(user.Id, user.Name, user.CreatedOn);
            }

            return new UserNotFound(id);
        }

        public UserEvent CreateUser(Guid id, string name, DateTimeOffset? createdOn = null)
        {
            var user = GetUser(id);
            if (user != null)
            {
                return new UserRetrieved(user.Id, user.Name, user.CreatedOn);
            }

            var newUser = new User(id, name, createdOn ?? DateTimeOffset.UtcNow);
            if (State.TryAdd(id, newUser))
            {
                return new UserCreated(newUser.Id, newUser.Name, newUser.CreatedOn);
            }

            return new UnexpectedErrorOcurred(id);
        }

        public UserEvent DeleteUser(Guid id)
        {
            if (!State.ContainsKey(id))
            {
                return new UserNotFound(id);
            }

            if (State.TryRemove(id, out _))
            {
                return new UserDeleted(id);
            }

            return new UnexpectedErrorOcurred(id);
        }

        public UserEvent GetAllUsers(int limit, int skip)
        {
            var users = State
                .OrderByDescending(u => u.Value.CreatedOn)
                .Skip(skip)
                .Take(limit)
                .Select(user => new UserRetrieved(user.Value.Id, user.Value.Name, user.Value.CreatedOn))
                .ToList();

            return new UsersRetrieved(State.Count, users);
        }

        private User GetUser(Guid id)
        {
            State.TryGetValue(id, out var user);
            return user;
        }
    }
}