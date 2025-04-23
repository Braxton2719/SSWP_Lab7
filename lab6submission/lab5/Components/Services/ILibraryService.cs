using lab5.Components.Models;

namespace lab5.Components.Services
{
    public interface ILibraryService
    {
        //Book methods
        Task<List<Book>> ReadBooksAsync();
        Task<List<Book>> AddBookAsync(Book book);
        Task<List<Book>> EditBookAsync(Book book);
        Task<List<Book>> DeleteBookAsync(Book book);

        //User Methods
        Task<List<User>> ReadUsersAsync();
        Task<List<User>> AddUsersAsync(User user);
        Task<List<User>> EditUserAsync(User user);
        Task<List<User>> DeleteUserAsync(User user);

        //Borrow + Return Methods
        Task<Dictionary<User, List<Book>>> BorrowBookAsync(User user, Book book);
        Task<Dictionary<User, List<Book>>> ReturnBookAsync(User user, int bookNumber);
    }
}
