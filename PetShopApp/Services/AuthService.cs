using PetShopApp.Data;
using PetShopApp.Models;
using Microsoft.EntityFrameworkCore;

namespace PetShopApp.Services;

public class AuthService
{
    private readonly PetShopContext _context;

    public static AppUser? CurrentUser { get; private set; }

    public AuthService()
    {
        _context = new PetShopContext();
    }

    public AppUser? Login(string login, string password)
    {
        var user = _context.AppUsers.Include(u => u.Role)
            .FirstOrDefault(u => u.UserLogin == login && u.UserPassword == password);

        if (user != null)
        {
            CurrentUser = user;
        }
        return user;
    }

    public bool Register(string surname, string name, string? patronymic, string login, string password)
    {
        if (_context.AppUsers.Any(u => u.UserLogin == login))
        {
            return false;
        }

        // Default role: User
        var userRole = _context.Roles.FirstOrDefault(r => r.RoleName == "Пользователь");
        if (userRole == null)
        {
            // Fallback or error if DB not seeded
            return false;
        }

        var newUser = new AppUser
        {
            UserSurname = surname,
            UserName = name,
            UserPatronymic = patronymic,
            UserLogin = login,
            UserPassword = password,
            RoleID = userRole.RoleID
        };

        _context.AppUsers.Add(newUser);
        _context.SaveChanges();
        return true;
    }

    public static void Logout()
    {
        CurrentUser = null;
    }
}
