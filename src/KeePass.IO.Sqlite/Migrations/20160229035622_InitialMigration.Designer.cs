using System;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations;
using KeePass.IO.Sqlite;

namespace KeePass.IO.Sqlite.Migrations
{
    [DbContext(typeof(KeePassSqliteContext))]
    [Migration("20160229035622_InitialMigration")]
    partial class InitialMigration
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
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
