﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using WebAPI_tutorial_recursos.Models;

namespace WebAPI_tutorial_recursos.Context
{
    public class ContextDB : IdentityDbContext
    {
        public ContextDB(DbContextOptions<ContextDB> options) : base(options)
        {
        }

        #region DB Tables

        public DbSet<Author> Author { get; set; }
        public DbSet<Book> Book { get; set; }
        public DbSet<Review> Review { get; set; }
        public DbSet<AuthorBook> AuthorBook { get; set; }

        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Author>()
                .HasData(
                new Author()
                {
                    Id = 1,
                    Name = "Gonzalo"
                },
                new Author()
                {
                    Id = 2,
                    Name = "Ramiro"
                },
                new Author()
                {
                    Id = 3,
                    Name = "Daniel"
                },
                new Author()
                {
                    Id = 4,
                    Name = "Gastón"
                },
                new Author()
                {
                    Id = 5,
                    Name = "Martín"
                }
                );

            modelBuilder.Entity<Book>()
               .HasData(
               new Book()
               {
                   Id = 1,
                   Title = "El libro de la selva"
               },
               new Book()
               {
                   Id = 2,
                   Title = "La vida de Steve Jobs"
               }
               );

            modelBuilder.Entity<AuthorBook>()
                 .HasKey(v => new { v.AuthorId, v.BookId }); // navegación n..n
        }

        internal async Task<List<Author>> GetAuthors()
        {
            return await Author.ToListAsync();
        }

        internal async Task<Author> GetAuthor(int id)
        {
            return await Author.AsNoTracking().FirstAsync(x => x.Id == id);
        }

        internal async Task<Author> CreateAuthors(Author autor)
        {
            EntityEntry<Author> response = await Author.AddAsync(autor);
            await SaveChangesAsync();
            return await GetAuthor(response.Entity.Id);
        }

    }
}