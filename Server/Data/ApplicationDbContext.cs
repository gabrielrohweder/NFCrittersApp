using Microsoft.EntityFrameworkCore;
using AnimalCollector.Shared.Models;

namespace AnimalCollector.Server.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Animal> Animals { get; set; }
    public DbSet<UserAnimal> UserAnimals { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity to match existing schema
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Username).HasColumnName("username");
            entity.Property(e => e.Password).HasColumnName("password");
            entity.HasIndex(e => e.Username).IsUnique();
        });

        // Configure Animal entity
        modelBuilder.Entity<Animal>(entity =>
        {
            entity.ToTable("animals");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Species).HasColumnName("species");
            entity.Property(e => e.Habitat).HasColumnName("habitat");
            entity.Property(e => e.Rarity).HasColumnName("rarity");
            entity.Property(e => e.ImageUrl).HasColumnName("image_url");
            entity.Property(e => e.Facts).HasColumnName("facts");
            entity.Property(e => e.Token).HasColumnName("token");
            entity.HasIndex(e => e.Token).IsUnique();
        });

        // Configure UserAnimal entity
        modelBuilder.Entity<UserAnimal>(entity =>
        {
            entity.ToTable("user_animals");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.AnimalId).HasColumnName("animal_id");
            entity.Property(e => e.CollectedAt).HasColumnName("collected_at");

            entity.HasOne(e => e.User)
                .WithMany(u => u.UserAnimals)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Animal)
                .WithMany(a => a.UserAnimals)
                .HasForeignKey(e => e.AnimalId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Seed animal data
        modelBuilder.Entity<Animal>().HasData(
            new Animal
            {
                Id = "lion-001",
                Name = "Savanna Lion",
                Species = "Panthera leo",
                Habitat = "African Grasslands",
                Rarity = "rare",
                ImageUrl = "/images/lion.png",
                Facts = "[\"Lions are the only cats that live in groups called prides\",\"A lion's roar can be heard up to 5 miles away\",\"Lions sleep 16-20 hours per day\",\"Male lions can weigh up to 420 pounds\"]",
                Token = "LN001"
            },
            new Animal
            {
                Id = "elephant-002",
                Name = "African Elephant",
                Species = "Loxodonta africana",
                Habitat = "African Savannas and Forests",
                Rarity = "legendary",
                ImageUrl = "/images/elephant.png",
                Facts = "[\"Elephants can weigh up to 13,000 pounds\",\"They have excellent memories and can remember other elephants for decades\",\"Elephants use their trunks like we use our hands\",\"Baby elephants are called calves and stay close to their mothers\"]",
                Token = "EL002"
            },
            new Animal
            {
                Id = "penguin-003",
                Name = "Emperor Penguin",
                Species = "Aptenodytes forsteri",
                Habitat = "Antarctic Ice Sheets",
                Rarity = "common",
                ImageUrl = "/images/penguin.png",
                Facts = "[\"Emperor penguins can dive up to 500 meters deep\",\"Males incubate eggs on their feet for 64 days\",\"They can hold their breath for up to 22 minutes\",\"Penguins huddle together to stay warm in temperatures below -40°C\"]",
                Token = "PG003"
            },
            new Animal
            {
                Id = "panda-004",
                Name = "Red Panda",
                Species = "Ailurus fulgens",
                Habitat = "Himalayan Mountain Forests",
                Rarity = "rare",
                ImageUrl = "/images/panda.png",
                Facts = "[\"Red pandas are also called firefox\",\"They are excellent climbers and spend most of their time in trees\",\"Red pandas have a false thumb to help them grasp bamboo\",\"They use their bushy tails as blankets in cold weather\"]",
                Token = "PD004"
            },
            new Animal
            {
                Id = "owl-005",
                Name = "Great Horned Owl",
                Species = "Bubo virginianus",
                Habitat = "North American Forests",
                Rarity = "common",
                ImageUrl = "/images/owl.png",
                Facts = "[\"Great horned owls can rotate their heads 270 degrees\",\"They have excellent night vision and hearing\",\"Their feathers allow them to fly completely silently\",\"They are fierce hunters and can catch prey much larger than themselves\"]",
                Token = "OW005"
            },
            new Animal
            {
                Id = "dolphin-006",
                Name = "Bottlenose Dolphin",
                Species = "Tursiops truncatus",
                Habitat = "Ocean Waters Worldwide",
                Rarity = "rare",
                ImageUrl = "/images/dolphin.png",
                Facts = "[\"Dolphins are highly intelligent and can recognize themselves in mirrors\",\"They communicate using clicks, whistles, and body language\",\"Dolphins can jump up to 20 feet out of the water\",\"They live in groups called pods and work together to hunt\"]",
                Token = "DL006"
            }
        );
    }
}
