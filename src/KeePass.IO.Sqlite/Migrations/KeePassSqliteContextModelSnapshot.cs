using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Infrastructure;

namespace KeePass
{
    [DbContext(typeof(KeePassSqliteContext))]
    partial class KeePassSqliteContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.0-rc1-16348");

            modelBuilder.Entity("KeePass.IO.Sqlite.DatabaseEntry", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("KeePassId");

                    b.Property<int?>("KeyId");

                    b.HasKey("Id");
                });

            modelBuilder.Entity("KeePass.IO.Sqlite.File", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AccessToken");

                    b.Property<string>("Path")
                        .IsRequired();

                    b.HasKey("Id");
                });

            modelBuilder.Entity("KeePass.IO.Sqlite.DatabaseEntry", b =>
                {
                    b.HasOne("KeePass.IO.Sqlite.File")
                        .WithMany()
                        .HasForeignKey("KeePassId");

                    b.HasOne("KeePass.IO.Sqlite.File")
                        .WithMany()
                        .HasForeignKey("KeyId");
                });
        }
    }
}
