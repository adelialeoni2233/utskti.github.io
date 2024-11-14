using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using SampleSecureWeb.Data;
using SampleSecureWeb.Services;

var builder = WebApplication.CreateBuilder(args);

// Memuat konfigurasi dari appsettings.json
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

// Menambahkan layanan MVC
builder.Services.AddControllersWithViews();

// Konfigurasi koneksi database menggunakan SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Konfigurasi layanan autentikasi menggunakan cookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Mengarahkan ke halaman login jika belum autentikasi
        options.AccessDeniedPath = "/Account/AccessDenied"; // Mengarahkan jika akses ditolak
        options.ExpireTimeSpan = TimeSpan.FromMinutes(5); // Batas waktu cookie
        options.SlidingExpiration = false; // Menghindari perpanjangan sesi otomatis
        options.Cookie.MaxAge = options.ExpireTimeSpan;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Mengharuskan HTTPS untuk cookie
        options.Cookie.HttpOnly = true; // Mencegah JavaScript mengakses cookie
    });

// Menambahkan layanan khusus ke DI (Dependency Injection)
builder.Services.AddTransient<SmtpEmailService>(); // Layanan email
builder.Services.AddScoped<IUser, UserData>(); // Layanan data pengguna

// Konfigurasi sesi
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Batas waktu sesi
    options.Cookie.HttpOnly = true; // Mencegah JavaScript mengakses sesi
    options.Cookie.IsEssential = true; // Memastikan sesi selalu tersedia
});

var app = builder.Build();

// Middleware untuk menonaktifkan cache
app.Use(async (context, next) =>
{
    context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
    context.Response.Headers["Pragma"] = "no-cache";
    context.Response.Headers["Expires"] = "0";
    await next();
});

// Middleware untuk menangani pengecualian dan keamanan
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error"); // Menangani kesalahan di produksi
    app.UseHsts(); // Menambahkan header HSTS untuk keamanan HTTPS
}

app.UseHttpsRedirection(); // Pengalihan ke HTTPS
app.UseStaticFiles(); // Melayani file statis seperti CSS, JS, dan gambar

app.UseRouting();
app.UseSession(); // Middleware untuk mendukung sesi

app.UseAuthentication(); // Middleware untuk autentikasi
app.UseAuthorization(); // Middleware untuk otorisasi

// Konfigurasi rute default aplikasi
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
