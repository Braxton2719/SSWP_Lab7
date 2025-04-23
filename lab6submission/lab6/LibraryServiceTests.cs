using Microsoft.VisualStudio.TestTools.UnitTesting;
using lab5.Components.Models;
using lab5.Components.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace lab6
{
    [TestClass]
    public class LibraryServiceTests
    {
        private string booksTestFile;
        private string usersTestFile;
        private LibraryService service;

        [TestInitialize]
        public async Task Setup()
        {
            booksTestFile = Path.GetTempFileName();
            usersTestFile = Path.GetTempFileName();

            await File.WriteAllLinesAsync(booksTestFile, new[]
            {
                "1,Original Book,Author A,1111",
                "2,Another Book,Author B,2222"
            });

            await File.WriteAllLinesAsync(usersTestFile, new[]
            {
                "1,Alice,alice@example.com",
                "2,Bob,bob@example.com"
            });

            service = new LibraryService(booksTestFile, usersTestFile);
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (File.Exists(booksTestFile)) File.Delete(booksTestFile);
            if (File.Exists(usersTestFile)) File.Delete(usersTestFile);
        }

        // ------------------------ BOOK TESTS ------------------------

        [DataTestMethod]
        [DataRow("New Book", "Author B", "2222")]
        public async Task TestAddBookSuccess(string title, string author, string isbn)
        {
            // Arrange
            var book = new Book { Title = title, Author = author, ISBN = isbn };

            // Act
            var result = await service.AddBookAsync(book);

            // Assert
            Assert.IsTrue(result.Any(b => b.Title == title));
        }

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public async Task TestAddBookFail()
        {
            // Arrange
            var service = new LibraryService(booksTestFile, usersTestFile);
            Book invalidBook = null;

            // Act
            await service.AddBookAsync(invalidBook);

            // Assert is handled by ExpectedException
        }


        [TestMethod]
        public async Task TestReadBooksSuccess()
        {
            var result = await service.ReadBooksAsync();
            Assert.IsTrue(result.Count > 0);
        }

        [TestMethod]
        public async Task TestReadBooksEmptyFile()
        {
            await File.WriteAllTextAsync(booksTestFile, "");
            var result = await service.ReadBooksAsync();
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task TestEditBookSuccess()
        {
            // Arrange
            var service = new LibraryService(booksTestFile, usersTestFile);
            await service.ReadBooksAsync(); 

            var updatedBook = new Book
            {
                Id = 1, 
                Title = "Updated Title",
                Author = "Updated Author",
                ISBN = "1111111111"
            };

            // Act
            List<Book> result = await service.EditBookAsync(updatedBook);
            Book editedBook = result.FirstOrDefault(b => b.Id == 1);

            // Assert
            Assert.IsNotNull(editedBook);
            Assert.AreEqual("Updated Title", editedBook.Title);
            Assert.AreEqual("Updated Author", editedBook.Author);
            Assert.AreEqual("1111111111", editedBook.ISBN);
        }


        [TestMethod]
        public async Task TestEditBookFail()
        {
            // Arrange
            var book = new Book { Id = 999, Title = "Ghost", Author = "Nobody", ISBN = "0000" };

            // Act
            var result = await service.EditBookAsync(book);

            // Assert
            Assert.IsFalse(result.Any(b => b.Id == 999));
        }

        [TestMethod]
        public async Task TestDeleteBookSuccess()
        {
            // Arrange
            var service = new LibraryService(booksTestFile, usersTestFile);
            await service.ReadBooksAsync();
            var book = new Book { Id = 2 };

            // Act
            var result = await service.DeleteBookAsync(book);

            // Assert
            Assert.IsFalse(result.Any(b => b.Id == 2));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task TestDeleteBookFail()
        {
            // Arrange
            var service = new LibraryService(booksTestFile, usersTestFile);
            await service.ReadBooksAsync();

            var nonExistentBook = new Book
            {
                Id = 9999, // ID that doesn't exist
                Title = "Ghost Book",
                Author = "Unknown",
                ISBN = "0000000000"
            };

            // Act - trying to delete a non-existent book
            await service.DeleteBookAsync(nonExistentBook);
        }

        // ------------------------ USER TESTS ------------------------

        [DataTestMethod]
        [DataRow("Bob", "bob@example.com")]
        public async Task TestAddUserSuccess(string name, string email)
        {
            // Arrange
            var user = new User { Name = name, Email = email };

            // Act
            var result = await service.AddUsersAsync(user);

            // Assert
            Assert.IsTrue(result.Any(u => u.Name == name));
        }

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public async Task TestAddUserFail()
        {
            // Arrange
            var service = new LibraryService(booksTestFile, usersTestFile);
            await service.ReadBooksAsync();

            // Act
            await service.AddUsersAsync(null);
        }

        [TestMethod]
        public async Task TestReadUsersSuccess()
        {
            var result = await service.ReadUsersAsync();
            Assert.IsTrue(result.Count > 0);
        }

        [TestMethod]
        public async Task TestReadUsersEmpty()
        {
            await File.WriteAllTextAsync(usersTestFile, "");
            var result = await service.ReadUsersAsync();
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task TestEditUserSuccess()
        {
            // Arrange
            var service = new LibraryService(booksTestFile, usersTestFile);
            await service.ReadUsersAsync();

            var updatedUser = new User
            {
                Id = 2,
                Name = "Updated2",
                Email = "updatedemail@example.com"
            };

            // Act
            var result = await service.EditUserAsync(updatedUser);
            var editedUser = result.FirstOrDefault(u => u.Id == 2);

            // Assert
            Assert.IsNotNull(editedUser);
            Assert.AreEqual("Updated2", editedUser.Name);
            Assert.AreEqual("updatedemail@example.com", updatedUser.Email);
        }

        [TestMethod]
        public async Task TestEditUserFail()
        {
            // Arrange
            var user = new User { Id = 999, Name = "Ghost", Email = "ghost@example.com" };

            // Act
            var result = await service.EditUserAsync(user);

            // Assert
            Assert.IsFalse(result.Any(u => u.Id == 999));
        }

        [TestMethod]
        public async Task TestDeleteUserSuccess()
        {
            // Arrange
            var service = new LibraryService(booksTestFile, usersTestFile);
            await service.ReadUsersAsync();
            var user = new User { Id = 1 };

            // Act
            var result = await service.DeleteUserAsync(user);

            // Assert
            Assert.IsFalse(result.Any(u => u.Id == 1));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task TestDeleteUserFail()
        {
            // Arrange
            var service = new LibraryService(booksTestFile, usersTestFile);
            await service.ReadUsersAsync();
            var user = new User { Id = 999 };

            // Act
            var result = await service.DeleteUserAsync(user);
        }

        // ------------------------ BORROW & RETURN ------------------------

        [TestMethod]
        public async Task TestBorrowBookSuccess()
        {
            // Arrange
            var service = new LibraryService(booksTestFile, usersTestFile);
            await service.ReadUsersAsync();
            await service.ReadBooksAsync();

            var user = new User { Id = 1 };
            var book = new Book { Id = 1 };

            // Act
            var result = await service.BorrowBookAsync(user, book);

            var borrowedEntry = result.FirstOrDefault(kvp => kvp.Key.Id == user.Id);


            Assert.IsNotNull(borrowedEntry);

        }

        [TestMethod]
        public async Task TestBorrowBookFail()
        {

            // Arrange
            var user = new User { Id = 999 };
            var book = new Book { Id = 999 };

            // Act
            var result = await service.BorrowBookAsync(user, book);

            // Assert
            Assert.IsFalse(result.ContainsKey(user));
        }

        [TestMethod]
        public async Task TestReturnBookSuccess()
        {
            // Arrange
            var service = new LibraryService(booksTestFile, usersTestFile);
            await service.ReadUsersAsync();
            await service.ReadBooksAsync();

            var user = new User { Id = 1 };
            var book = new Book { Id = 1 };

            // Act
            var borrowedEntry = await service.BorrowBookAsync(user, book);
            var result = await service.ReturnBookAsync(user, 1);

            var entry = result.FirstOrDefault(kvp => kvp.Key.Id == user.Id);

            // Assert
            Assert.IsNotNull(entry);
        }

        [TestMethod]
        public async Task TestReturnBookFail()
        {
            // Arrange
            var user = new User { Id = 1 };

            // Act
            var result = await service.ReturnBookAsync(user, 1);

            // Assert
            Assert.IsFalse(result.ContainsKey(user));
        }
    }
}
