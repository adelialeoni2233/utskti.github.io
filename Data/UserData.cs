using System;
using System.Text.RegularExpressions;
using SampleSecureWeb.Models;

namespace SampleSecureWeb.Data;

public class UserData : IUser
{
    private readonly ApplicationDbContext _db;
    public UserData(ApplicationDbContext db)
    {
        _db = db;
    }
    public User Login(User user)
    {
        var _user = _db.Users.FirstOrDefault(u => u.Username == user.Username);
        if (_user == null)
        {
            throw new Exception("User not found");
        }
        if (!BCrypt.Net.BCrypt.Verify(user.Password, _user.Password))
        {
            throw new Exception("Password is incorrect");
        }
        return _user;
    }

    public User Registration(User user)
    {
        try
        {
            if (!IsValidPassword(user.Password))
            {
                throw new Exception("Password must contain at least one lowercase letter, one uppercase letter, and one number.");
            }
            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            _db.Users.Add(user);
            _db.SaveChanges();
            return user;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    public User GetUserByUsername(string username)
    {
        return _db.Users.FirstOrDefault(u => u.Username == username);
    }

    public void UpdateUserPassword(User user)
    {
        var existingUser = _db.Users.FirstOrDefault(u => u.Username == user.Username);
        if (existingUser != null)
        {
            existingUser.Password = user.Password;
            _db.SaveChanges();
        }

        else
        {
            throw new Exception("User not found");
        }
    }

    public void ChangePassword(string username, string newPassword)
    {
        var _user = _db.Users.FirstOrDefault(u => u.Username == username);
        if (_user == null)
        {
            throw new Exception("User not found");
        }

        if (!IsValidPassword(newPassword))
        {
            throw new Exception("Password must contain at least one lowercase letter, one uppercase letter, and one number.");
        }

        _user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
        _db.Users.Update(_user);
        _db.SaveChanges();

    }
    private bool IsValidPassword(string password)
    {
        return Regex.IsMatch(password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$");
    }

    public string GenerateOtp(string username)
    {
        throw new NotImplementedException();
    }

    public void SendOtp(string username, string otp)
    {
        throw new NotImplementedException();
    }

    public bool ValidateOtp(string username, string otp)
    {
        throw new NotImplementedException();
    }
}
