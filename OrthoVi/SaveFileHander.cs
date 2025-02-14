using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using static OrthoVi.MainWindow;

public class DatabaseManager
{
    public static string mainPath = "./SaveFiles/";

    public DatabaseManager()
    {
        if (!Directory.Exists(mainPath))
        {
            Directory.CreateDirectory(mainPath);
        }
    }

    public void CreateDatabase(string username, string password, string doctorFirstName, string doctorLastName, byte[] profilePicture)
    {
        string databaseFile = $"{mainPath}{username}.db";
        if (File.Exists(databaseFile))
        {
            Console.WriteLine("Database already exists.");
            return;
        }

        using (var context = new UserDbContext(databaseFile))
        {
            // This will create the database and tables.
            context.Database.EnsureCreated();

            // Create a new user with its related doctor information.
            var user = new User
            {
                Username = username,
                Password = password,
                DoctorInformation = new DoctorInformation
                {
                    Firstname = doctorFirstName,
                    Lastname = doctorLastName,
                    ProfilePicture = Convert.ToBase64String(profilePicture),
                    // Set the foreign key to link this doctor record with the user.
                    UserUsername = username,
                    Clients = new List<ClientInformation>()
                }
            };

            context.Users.Add(user);
            context.SaveChanges();
        }
    }

    public void ReadDatabase(string username, string password)
    {
        string databaseFile = $"{mainPath}{username}.db";
        try
        {


            using (var context = new UserDbContext(databaseFile))
            {
                var user = context.Users
                                  .Include(u => u.DoctorInformation)
                                    .ThenInclude(d => d.Clients)
                                      .ThenInclude(c => c.Images)
                                        .ThenInclude(i => i.Annotation)
                                          .ThenInclude(a => a.Coordinates)
                                  .FirstOrDefault(u => u.Username == username && u.Password == password);

                if (user == null)
                {

                    SessionManager.LoggedInUser = null;
                }
                else
                {
                    SessionManager.LoggedInUser = user;
                }

            }
        }
        catch (Exception)
        {

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

    public void UpdateProfilePicture(string username, byte[] newProfilePicture)
    {
        string databaseFile = $"{mainPath}{username}.db";
        if (!File.Exists(databaseFile))
        {
            throw new FileNotFoundException("Database file not found.");
        }

        using (var context = new UserDbContext(databaseFile))
        {
            // Find the user's doctor information
            var doctorInfo = context.DoctorInformations.FirstOrDefault(d => d.UserUsername == username);

            // Convert and update the profile picture
            doctorInfo.ProfilePicture = Convert.ToBase64String(newProfilePicture);

            // Save changes to the database
            context.SaveChanges();
        }
    }


    public void DeleteClient(string username, string password, int clientId)
    {
        string databaseFile = $"{mainPath}{username}.db";
        if (!File.Exists(databaseFile))
        {
            Console.WriteLine("Database file not found.");
            return;
        }

        using (var context = new UserDbContext(databaseFile))
        {
            var user = context.Users
                              .Include(u => u.DoctorInformation)
                                .ThenInclude(d => d.Clients)
                              .FirstOrDefault(u => u.Username == username && u.Password == password);

            if (user == null || user.DoctorInformation == null)
            {
                Console.WriteLine("User or doctor information not found.");
                return;
            }

            var clientToRemove = user.DoctorInformation.Clients.FirstOrDefault(c => c.ClientInformationId == clientId);
            if (clientToRemove != null)
            {
                context.ClientInformations.Remove(clientToRemove);
                context.SaveChanges();
                Console.WriteLine($"Client {clientId} deleted successfully.");
            }
            else
            {
                Console.WriteLine("Client not found.");
            }
        }
    }

    public void DeleteDatabase(string username)
    {
        string directoryPath = Path.Combine(mainPath);

        if (Directory.Exists(directoryPath))
        {
            string[] files = Directory.GetFiles(directoryPath);
            bool deleted = false;

            foreach (string file in files)
            {
                if (Path.GetFileName(file).Contains(username))
                {
                    try
                    {
                        // Clear SQLite connection pools to release file locks
                        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();

                        File.Delete(file);
                        Console.WriteLine($"File '{file}' deleted successfully.");
                        deleted = true;
                    }
                    catch (IOException ex)
                    {
                        Console.WriteLine($"Error deleting file '{file}': {ex.Message}");
                    }
                }
            }

            if (!deleted)
            {
                Console.WriteLine("No database files found containing the username.");
            }
        }
        else
        {
            Console.WriteLine("The specified directory does not exist.");
        }
    }

}

public class UserDbContext : DbContext
{
    private readonly string _databaseFile;

    public UserDbContext(string databaseFile)
    {
        _databaseFile = databaseFile;
    }

    public DbSet<User> Users { get; set; }
    public DbSet<DoctorInformation> DoctorInformations { get; set; }
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
        // Set primary keys.
        modelBuilder.Entity<User>().HasKey(u => u.Username);
        modelBuilder.Entity<DoctorInformation>().HasKey(d => d.DoctorInformationId);
        modelBuilder.Entity<ClientInformation>().HasKey(c => c.ClientInformationId);
        modelBuilder.Entity<Image>().HasKey(i => i.ImageId);
        modelBuilder.Entity<ImageAnnotation>().HasKey(ia => ia.ImageAnnotationId);
        modelBuilder.Entity<Coordinates>().HasKey(c => c.CoordinatesId);

        // Configure one-to-one: User <--> DoctorInformation.
        modelBuilder.Entity<User>()
            .HasOne(u => u.DoctorInformation)
            .WithOne(d => d.User)
            .HasForeignKey<DoctorInformation>(d => d.UserUsername)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure one-to-many: DoctorInformation --> ClientInformation.
        modelBuilder.Entity<DoctorInformation>()
            .HasMany(d => d.Clients)
            .WithOne(c => c.DoctorInformation)
            .HasForeignKey(c => c.DoctorInformationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure one-to-many: ClientInformation --> Image.
        modelBuilder.Entity<ClientInformation>()
            .HasMany(c => c.Images)
            .WithOne(i => i.ClientInformation)
            .HasForeignKey(i => i.ClientInformationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure one-to-many: Image --> ImageAnnotation.
        modelBuilder.Entity<Image>()
            .HasMany(i => i.Annotation)
            .WithOne(a => a.Image)
            .HasForeignKey(a => a.ImageId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure one-to-one: ImageAnnotation --> Coordinates.
        modelBuilder.Entity<ImageAnnotation>()
            .HasOne(a => a.Coordinates)
            .WithOne()
            .HasForeignKey<ImageAnnotation>(a => a.CoordinatesId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

// -------------------------
// Entity Classes
// -------------------------

public class User
{
    public string Username { get; set; }
    public string Password { get; set; }
    // Navigation property for one-to-one relationship.
    public DoctorInformation DoctorInformation { get; set; }
}

public class DoctorInformation
{
    public int DoctorInformationId { get; set; }
    public string Firstname { get; set; }
    public string Lastname { get; set; }
    public string ProfilePicture { get; set; }

    // Foreign key property to link to User.
    public string UserUsername { get; set; }
    // Navigation back to User.
    public User User { get; set; }

    // One-to-many: a doctor can have many clients.
    public List<ClientInformation> Clients { get; set; }
}

public class ClientInformation
{
    public int ClientInformationId { get; set; }
    public string ClientFirstName { get; set; }
    public string ClientMiddleName { get; set; }
    public string ClientLastName { get; set; }
    public string Gender { get; set; }
    public int ClientAge { get; set; }

    // Foreign key property to link to DoctorInformation.
    public int DoctorInformationId { get; set; }
    // Navigation back to DoctorInformation.
    public DoctorInformation DoctorInformation { get; set; }

    // One-to-many: a client can have many images.
    public List<Image> Images { get; set; }
}

public class Image
{
    public int ImageId { get; set; }
    public string ImageName { get; set; }
    public byte[] ImageContent { get; set; }

    // Foreign key property to link to ClientInformation.
    public int ClientInformationId { get; set; }
    // Navigation back to ClientInformation.
    public ClientInformation ClientInformation { get; set; }

    // One-to-many: an image can have many annotations.
    public List<ImageAnnotation> Annotation { get; set; }
}

public class ImageAnnotation
{
    public int ImageAnnotationId { get; set; }
    public string LandmarkName { get; set; }

    // Foreign key property to link back to Image.
    public int ImageId { get; set; }
    public Image Image { get; set; }

    // Foreign key property to link to Coordinates.
    public int CoordinatesId { get; set; }
    public Coordinates Coordinates { get; set; }
}

public class Coordinates
{
    public int CoordinatesId { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
}
