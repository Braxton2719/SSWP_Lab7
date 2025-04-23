using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Linq;
using lab5.Components.Models;
using lab5.Components.Pages;

// 1. Lab 7 workflow trigger to main branch changes
namespace lab5.Components.Services
{
    public class LibraryService : ILibraryService
    {
        private readonly string booksFile = "Components/Data/Books.csv";
        private readonly string usersFile = "Components/Data/Users.csv";

        public LibraryService(string booksFilePath, string usersFilePath)
        {
            booksFile = booksFilePath;
            usersFile = usersFilePath;
        }


        private List<Book> books = new();
        private List<User> users = new();
        private Dictionary<User, List<Book>> borrowedBooks = new Dictionary<User, List<Book>>();




        //-----------------------------------------Book Methods---------------------------------

        public async Task<List<Book>> ReadBooksAsync()
        {
            var lines = await File.ReadAllLinesAsync(booksFile);
            books = new List<Book>();

            foreach (var line in lines)
            {
                var fields = line.Split(',');
                if (fields.Length >= 4)
                {
                    var book = new Book
                    {
                        Id = int.Parse(fields[0].Trim()),
                        Title = fields[1].Trim(),
                        Author = fields[2].Trim(),
                        ISBN = fields[3].Trim()
                    };
                    books.Add(book);
                }
            }
            return books;
        }

        public async Task<List<Book>> AddBookAsync(Book bookToBeAdded)
        {
            int newId = books.Any() ? books.Max(b => b.Id) + 1 : 1;

            var newBook = new Book
            {
                Id = newId,
                Title = bookToBeAdded.Title.Trim(),
                Author = bookToBeAdded.Author.Trim(),
                ISBN = bookToBeAdded.ISBN.Trim()
            };
            books.Add(newBook);

            var lines = books.Select(b => $"{b.Id},{b.Title},{b.Author},{b.ISBN}");
            await File.WriteAllLinesAsync(booksFile, lines);

            return books;
        }

        public async Task<List<Book>> EditBookAsync(Book bookToBeEdited) 
        {
            var book = books.FirstOrDefault(b => b.Id == bookToBeEdited.Id);
            if (book != null)
            {
                book.Title = bookToBeEdited.Title.Trim();
                book.Author = bookToBeEdited.Author.Trim();
                book.ISBN = bookToBeEdited.ISBN.Trim();
            }

            var lines = books.Select(b => $"{b.Id},{b.Title},{b.Author},{b.ISBN}");
            await File.WriteAllLinesAsync(booksFile, lines);

            return books;
        }

        public async Task<List<Book>> DeleteBookAsync(Book bookToBeDeleted) 
        {
            var book = books.FirstOrDefault(b => b.Id == bookToBeDeleted.Id);
            if (book == null)
            { 
                throw new ArgumentException("Book not found");
            }

            books.Remove(book);

            var lines = books.Select(b => $"{b.Id},{b.Title},{b.Author},{b.ISBN}");
            await File.WriteAllLinesAsync(booksFile, lines);

            return books;
        }



        //---------------------------------------User Methods------------------------------------------

        public async Task<List<User>> ReadUsersAsync()
        {
            var lines = await File.ReadAllLinesAsync(usersFile);
            users = new List<User>();

            foreach (var line in lines)
            {
                var fields = line.Split(',');
                if (fields.Length >= 3)
                {
                    var user = new User
                    {
                        Id = int.Parse(fields[0].Trim()),
                        Name = fields[1].Trim(),
                        Email = fields[2].Trim()
                    };
                    users.Add(user);
                }
            }
            return users;
        }

        public async Task<List<User>> AddUsersAsync(User user)
        {
            int newId = users.Any() ? users.Max(u => u.Id) + 1 : 1;

            var newUser = new User
            {
                Id = newId,
                Name = user.Name.Trim(),
                Email = user.Email.Trim()
            };

            users.Add(newUser);

            var lines = users.Select(u => $"{u.Id},{u.Name},{u.Email}");
            await File.WriteAllLinesAsync(usersFile, lines);

            return users;
        }


        public async Task<List<User>> EditUserAsync(User updatedUser) 
        {
            var user = users.FirstOrDefault(u => u.Id == updatedUser.Id);
            if (user != null)
            {
                user.Name = updatedUser.Name;
                user.Email = updatedUser.Email;
            }

            var lines = users.Select(u => $"{u.Id},{u.Name},{u.Email}");
            await File.WriteAllLinesAsync(usersFile, lines);

            return users;
        }

        public async Task<List<User>> DeleteUserAsync(User userToBeDeleted) 
        {
            var user = users.FirstOrDefault(u => u.Id == userToBeDeleted.Id);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            users.Remove(user);

            var lines = users.Select(u => $"{u.Id},{u.Name},{u.Email}");
            await File.WriteAllLinesAsync(usersFile, lines);

            return users;
        }

        //----------------------Borrow & Return--------------------------------------

        

        public async Task<Dictionary<User, List<Book>>> BorrowBookAsync(User userToBorrow, Book bookToBorrow)
        {
            Book book = books.FirstOrDefault(b => b.Id == bookToBorrow.Id);
            if (book != null)
            {
                User user = users.FirstOrDefault(u => u.Id == userToBorrow.Id);
                if (user != null) 
                {
                    if (!borrowedBooks.ContainsKey(user))
                    {
                        borrowedBooks[user] = new List<Book>();
                    }
                    borrowedBooks[user].Add(book);
                    books.Remove(book);

                    var lines = books.Select(b => $"{b.Id},{b.Title},{b.Author},{b.ISBN}");
                    await File.WriteAllLinesAsync(booksFile, lines);
                }
            }
            return borrowedBooks;
        }

        public async Task<Dictionary<User, List<Book>>> ReturnBookAsync(User userToReturn, int bookNumber)
        {
            var user = users.FirstOrDefault(u => u.Id == userToReturn.Id);
            if (user != null && borrowedBooks.ContainsKey(user) && borrowedBooks[user].Count > 0)
            {
                if (bookNumber != null && bookNumber >= 1 && bookNumber <= borrowedBooks[user].Count)
                {
                    Book bookToReturn = borrowedBooks[user][bookNumber - 1];

                    borrowedBooks[user].RemoveAt(bookNumber - 1);
                    books.Add(bookToReturn);

                    var lines = books.Select(b => $"{b.Id},{b.Title},{b.Author},{b.ISBN}");
                    await File.WriteAllLinesAsync(booksFile, lines);
                }
            }
            return borrowedBooks;
        }
    }
}
