namespace News
{
    public interface IUsers
    {
        static abstract Users Instance { get; }

        User? GetUserById(int id);
    }
}