using System;

namespace Bookmarks.Data
{
    public interface IUser
    {
        string Email { get; set; }
        string Id { get; set; }
        string Name { get; set; }
        string PasswordHash { get; set; }
    }
}
