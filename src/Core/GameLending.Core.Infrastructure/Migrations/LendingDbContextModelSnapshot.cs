
using System;
using GameLending.Core.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GameLending.Core.Infrastructure.Migrations
{
    [DbContext(typeof(LendingDbContext))]
    partial class LendingDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "10.0.12")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("GameLending.Core.Domain.Friend", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");

                    b.Property<string>("Email")
                        .HasMaxLength(254)
                        .HasColumnType("character varying(254)")
                        .HasColumnName("email");

                    b.Property<bool>("IsActive")
                        .HasColumnType("boolean")
                        .HasColumnName("is_active");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(150)
                        .HasColumnType("character varying(150)")
                        .HasColumnName("name");

                    b.Property<DateTimeOffset>("UpdatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("updated_at");

                    b.HasKey("Id");

                    b.HasIndex("Name");

                    b.ToTable("friends", (string)null);
                });

            modelBuilder.Entity("GameLending.Core.Domain.Game", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");

                    b.Property<string>("Developer")
                        .HasColumnType("text")
                        .HasColumnName("developer");

                    b.PrimitiveCollection<string[]>("Genres")
                        .IsRequired()
                        .HasColumnType("text[]")
                        .HasColumnName("genres");

                    b.Property<bool>("IsActive")
                        .HasColumnType("boolean")
                        .HasColumnName("is_active");

                    b.PrimitiveCollection<string[]>("Platforms")
                        .IsRequired()
                        .HasColumnType("text[]")
                        .HasColumnName("platforms");

                    b.Property<decimal?>("Rating")
                        .HasPrecision(6, 2)
                        .HasColumnType("numeric(6,2)")
                        .HasColumnName("rating");

                    b.Property<DateOnly?>("ReleaseDate")
                        .HasColumnType("date")
                        .HasColumnName("release_date");

                    b.Property<decimal?>("SourcePrice")
                        .HasPrecision(12, 2)
                        .HasColumnType("numeric(12,2)")
                        .HasColumnName("source_price");

                    b.Property<string>("SourceUrl")
                        .HasColumnType("text")
                        .HasColumnName("source_url");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(300)
                        .HasColumnType("character varying(300)")
                        .HasColumnName("title");

                    b.Property<DateTimeOffset>("UpdatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("updated_at");

                    b.Property<long?>("Votes")
                        .HasColumnType("bigint")
                        .HasColumnName("votes");

                    b.HasKey("Id");

                    b.HasIndex("SourceUrl")
                        .IsUnique()
                        .HasFilter("source_url IS NOT NULL");

                    b.HasIndex("Title");

                    b.ToTable("games", (string)null);
                });

            modelBuilder.Entity("GameLending.Core.Domain.Loan", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<Guid>("FriendId")
                        .HasColumnType("uuid")
                        .HasColumnName("friend_id");

                    b.Property<Guid>("GameId")
                        .HasColumnType("uuid")
                        .HasColumnName("game_id");

                    b.Property<DateTimeOffset>("LoanedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("loaned_at");

                    b.Property<DateTimeOffset?>("ReturnedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("returned_at");

                    b.HasKey("Id");

                    b.HasIndex("GameId")
                        .IsUnique()
                        .HasDatabaseName("ux_loans_active_game")
                        .HasFilter("returned_at IS NULL");

                    b.HasIndex("LoanedAt");

                    b.HasIndex("FriendId", "LoanedAt");

                    b.ToTable("loans", null, t =>
                        {
                            t.HasCheckConstraint("ck_return_date", "returned_at IS NULL OR returned_at >= loaned_at");
                        });
                });

            modelBuilder.Entity("GameLending.Core.Infrastructure.ImportRun", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<int>("Analyzed")
                        .HasColumnType("integer")
                        .HasColumnName("analyzed");

                    b.Property<int>("Duplicates")
                        .HasColumnType("integer")
                        .HasColumnName("duplicates");

                    b.Property<string>("FileHash")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("file_hash");

                    b.Property<DateTimeOffset?>("FinishedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("finished_at");

                    b.Property<int>("Imported")
                        .HasColumnType("integer")
                        .HasColumnName("imported");

                    b.Property<string>("ParserVersion")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("parser_version");

                    b.Property<int>("Rejected")
                        .HasColumnType("integer")
                        .HasColumnName("rejected");

                    b.Property<DateTimeOffset>("StartedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("started_at");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("status");

                    b.Property<int>("Warnings")
                        .HasColumnType("integer")
                        .HasColumnName("warnings");

                    b.HasKey("Id");

                    b.HasIndex("FileHash", "ParserVersion")
                        .HasFilter("status = 'Completed'");

                    b.ToTable("catalog_import_runs", (string)null);
                });

            modelBuilder.Entity("GameLending.Core.Domain.Loan", b =>
                {
                    b.HasOne("GameLending.Core.Domain.Friend", null)
                        .WithMany()
                        .HasForeignKey("FriendId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("GameLending.Core.Domain.Game", null)
                        .WithMany()
                        .HasForeignKey("GameId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
