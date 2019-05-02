using System.Collections.Generic;

namespace UserManagement.Events
{
    public sealed class UsersRetrieved : UserEvent
    {
        public UsersRetrieved(long totalCount, IReadOnlyList<UserRetrieved> users)
        {
            TotalCount = totalCount;
            Users = users ?? new List<UserRetrieved>();
        }

        public long TotalCount { get; }
        public IReadOnlyList<UserRetrieved> Users { get; }
    }
}