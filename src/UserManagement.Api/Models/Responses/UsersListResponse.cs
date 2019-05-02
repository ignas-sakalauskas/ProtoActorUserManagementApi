using System.Collections.Generic;

namespace UserManagement.Api.Models.Responses
{
    public class UsersListResponse
    {
        public UsersListResponse(long totalCount, IEnumerable<UserDetailsResponse> users)
        {
            TotalCount = totalCount;
            Users = users ?? new List<UserDetailsResponse>();
        }

        public long TotalCount { get; }
        public IEnumerable<UserDetailsResponse> Users { get; }
    }
}