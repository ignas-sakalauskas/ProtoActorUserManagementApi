namespace UserManagement.Commands
{
    public sealed class GetUsers : UserMessage
    {
        public GetUsers(int limit, int skip)
        {
            Limit = limit;
            Skip = skip;
        }

        public int Skip { get; }

        public int Limit { get; }
    }
}
