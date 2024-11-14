using Microsoft.AspNetCore.Mvc;
using SampleSecureWeb.Models;
using SampleSecureWeb.Data;
using System.Threading.Tasks;

public class UsersController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly EncryptionService _encryptionService;

    public UsersController(ApplicationDbContext context, EncryptionService encryptionService)
    {
        _context = context;
        _encryptionService = encryptionService;
    }

}
