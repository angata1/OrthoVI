// File: DatabaseManager.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;

public class DatabaseManager
{
    public static string mainPath = "./SaveFiles/";

    public DatabaseManager() {
        if (Directory.Exists(mainPath))
        {

        }
        else {
        Directory.CreateDirectory(mainPath);
        }
    }

    public void CreateDatabase(string username, string password, string DoctorFirstName, string DoctorLastName, byte[] profilePicture)
    {
        string databaseFile = $"{mainPath}{username}.db";
        if (File.Exists(databaseFile))
        {
            Console.WriteLine("Database already exists.");
            return;
        }

        using (var context = new UserDbContext(databaseFile))
        {
            context.Database.EnsureCreated();
            var user = new User
            {
                Username = username,
                Password = password,
                DoctorInformation = new DoctorInformation
                {
                    Firstname = DoctorFirstName,
                    Lastname = DoctorLastName,
                    ProfilePicture = Convert.ToBase64String(profilePicture),
                    ProjectHistory = new List<Project>()
                }
            };
            context.Users.Add(user);
            context.SaveChanges();
        }
    }

    public void AddNewClient(string username, ClientInformation newClient)
    {
        string databaseFile = $"{mainPath}{username}.db";
        if (!File.Exists(databaseFile))
        {
            throw new FileNotFoundException("Database file not found.");
        }

        using (var context = new UserDbContext(databaseFile))
        {
            // Add the new client to the database
            context.ClientInformations.Add(newClient);
            context.SaveChanges();
            Console.WriteLine($"New client '{newClient.ClientFirstName} {newClient.ClientLastName}' added successfully.");
        }
    }


    public User ReadDatabase(string username, string password)
    {
        string databaseFile = $"{mainPath}{username}.db";
        if (!File.Exists(databaseFile))
        {
            throw new FileNotFoundException("Database file not found.");
        }

        using (var context = new UserDbContext(databaseFile))
        {
            var user = context.Users.Include(u => u.DoctorInformation)
                                     .ThenInclude(d => d.ProjectHistory)
                                     .ThenInclude(p => p.Client)
                                     .ThenInclude(c => c.Image)
                                     .ThenInclude(i => i.Annotation)
                                     .ThenInclude(a => a.Coordinates)
                                     .FirstOrDefault(u => u.Username == username && u.Password == password);

            if (user == null)
            {
                Console.WriteLine("Incorrect username or password");
                return null;
            }

            return user;
        }
    }


    public void UpdateDatabase(string username, User user)
    {
        string databaseFile = $"{mainPath}{username}.db";
        if (!File.Exists(databaseFile))
        {
            throw new FileNotFoundException("Database file not found.");
        }

        using (var context = new UserDbContext(databaseFile))
        {
            context.Users.Update(user);
            context.SaveChanges();
        }
    }

    public (string Username, string Password) GetUserCredentials(string username)
    {
        string databaseFile = $"{mainPath}{username}.db";
        if (!File.Exists(databaseFile))
        {
            throw new FileNotFoundException("Database file not found.");
        }

        using (var context = new UserDbContext(databaseFile))
        {
            var user = context.Users.FirstOrDefault();
            if (user == null) throw new Exception("User not found in the database.");

            return (user.Username, user.Password);
        }
    }
}

// DbContext and Entity Models
public class UserDbContext : DbContext
{
    private readonly string _databaseFile;

    public UserDbContext(string databaseFile)
    {
        _databaseFile = databaseFile;
    }

    public DbSet<User> Users { get; set; }
    public DbSet<DoctorInformation> DoctorInformations { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<ClientInformation> ClientInformations { get; set; }
    public DbSet<Image> Images { get; set; }
    public DbSet<ImageAnnotation> ImageAnnotations { get; set; }
    public DbSet<Coordinates> Coordinates { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={_databaseFile}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasKey(u => u.Username);
        modelBuilder.Entity<DoctorInformation>().HasKey(d => d.DoctorInformationId);
        modelBuilder.Entity<Project>().HasKey(p => p.ProjectId);
        modelBuilder.Entity<ClientInformation>().HasKey(c => c.ClientInformationId);
        modelBuilder.Entity<Image>().HasKey(i => i.ImageId);
        modelBuilder.Entity<ImageAnnotation>().HasKey(ia => ia.ImageAnnotationId);
        modelBuilder.Entity<Coordinates>().HasKey(c => c.CoordinatesId);
    }
}

// Entities
public class User
{
    public string Username { get; set; }
    public string Password { get; set; }
    public DoctorInformation DoctorInformation { get; set; }
}

public class DoctorInformation
{
    public int DoctorInformationId { get; set; }
    public string Firstname { get; set; }
    public string Lastname { get; set; }
    public string ProfilePicture { get; set; }
    public List<Project> ProjectHistory { get; set; }
}

public class Project
{
    public int ProjectId { get; set; }
    public int? ClientInformationId { get; set; }
    public ClientInformation Client { get; set; }
}

public class ClientInformation
{
    public int ClientInformationId { get; set; }
    public string ClientFirstName { get; set; }
    public string ClientLastName { get; set; }
    public string Gender { get; set; }
    public int ClientAge { get; set; }
    public List<Image> Image { get; set; }
}

public class Image
{
    public int ImageId { get; set; }
    public string ImageName { get; set; }
    public byte[] ImageContent { get; set; }
    public List<ImageAnnotation> Annotation { get; set; }
}

public class ImageAnnotation
{
    public int ImageAnnotationId { get; set; }
    public string LandmarkName { get; set; }
    public Coordinates Coordinates { get; set; }
}

public class Coordinates
{
    public int CoordinatesId { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
}
