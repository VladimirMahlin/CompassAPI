using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Server.API.Controllers;
using Server.API.DTOs;
using Server.Core.Entities;
using Server.Infrastructure.Data;
using Xunit;

namespace Server.Tests.Unit.API
{
    public class BookControllerTests
    {
        private readonly DbContextOptions<AppDbContext> _options;

        public BookControllerTests()
        {
            _options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            using var context = new AppDbContext(_options);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            context.Authors.Add(new Author { AuthorId = 1, Name = "Test Author", Biography = "Test Biography" });
            context.Books.Add(new Book
            {
                BookId = 1,
                Title = "Test Book",
                ISBN = "1234567890",
                PublicationYear = 2024,
                Publisher = "Test Publisher",
                CoverImageUrl = "test.jpg",
                Description = "Test Description"
            });
            context.SaveChanges();
        }

        private AppDbContext CreateContext() => new(_options);

        [Fact]
        public async Task GetBooks_ReturnsOkResultWithBooks()
        {
            await using var context = CreateContext();
            var loggerMock = new Mock<ILogger<BookController>>();
            var controller = new BookController(context, loggerMock.Object);

            var result = await controller.GetBooks();

            Assert.IsType<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            Assert.IsType<List<BookDto>>(okResult.Value);
        }

        [Fact]
        public async Task GetBook_ReturnsOkResultWithBookDetail_WhenBookExists()
        {
            await using var context = CreateContext();
            var loggerMock = new Mock<ILogger<BookController>>();
            var controller = new BookController(context, loggerMock.Object);

            var result = await controller.GetBook(1);

            Assert.IsType<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            Assert.IsType<BookDetailDto>(okResult.Value);
        }

        [Fact]
        public async Task GetBook_ReturnsNotFound_WhenBookDoesNotExist()
        {
            await using var context = CreateContext();
            var loggerMock = new Mock<ILogger<BookController>>();
            var controller = new BookController(context, loggerMock.Object);

            var result = await controller.GetBook(999);

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task CreateBook_ReturnsCreatedAtAction_WithCorrectValues()
        {
            await using var context = CreateContext();
            var loggerMock = new Mock<ILogger<BookController>>();
            var controller = new BookController(context, loggerMock.Object);

            var createDto = new CreateBookDto
            {
                Title = "New Book",
                ISBN = "111222333",
                PublicationYear = 2025,
                Publisher = "New Publisher",
                CoverImageUrl = "new.jpg",
                Description = "New Description",
                AuthorIds = new List<int> { 1 }
            };

            var result = await controller.CreateBook(createDto);

            Assert.IsType<CreatedAtActionResult>(result.Result);
            var createdAtResult = result.Result as CreatedAtActionResult;
            Assert.IsType<BookDto>(createdAtResult.Value);
            Assert.Equal("GetBook", createdAtResult.ActionName);
        }

        [Fact]
        public async Task UpdateBook_ReturnsNoContent_WhenUpdateIsSuccessful()
        {
            await using var context = CreateContext();
            var loggerMock = new Mock<ILogger<BookController>>();
            var controller = new BookController(context, loggerMock.Object);

            var updateDto = new UpdateBookDto
            {
                Title = "Updated Book",
                ISBN = "99999",
                PublicationYear = 2026,
                Publisher = "Updated Publisher",
                CoverImageUrl = "updated.jpg",
                Description = "Updated Description",
                AuthorIds = new List<int> { 1 }
            };

            var result = await controller.UpdateBook(1, updateDto);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task UpdateBook_ReturnsNotFound_WhenBookDoesNotExist()
        {
            await using var context = CreateContext();
            var loggerMock = new Mock<ILogger<BookController>>();
            var controller = new BookController(context, loggerMock.Object);

            var updateDto = new UpdateBookDto
            {
                Title = "Updated Book",
                ISBN = "99999",
                PublicationYear = 2026,
                Publisher = "Updated Publisher",
                CoverImageUrl = "updated.jpg",
                Description = "Updated Description",
                AuthorIds = new List<int> { 1 }
            };

            var result = await controller.UpdateBook(999, updateDto);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task DeleteBook_ReturnsNoContent_WhenDeletionIsSuccessful()
        {
            await using var context = CreateContext();
            var loggerMock = new Mock<ILogger<BookController>>();
            var controller = new BookController(context, loggerMock.Object);

            var result = await controller.DeleteBook(1);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteBook_ReturnsNotFound_WhenBookDoesNotExist()
        {
            await using var context = CreateContext();
            var loggerMock = new Mock<ILogger<BookController>>();
            var controller = new BookController(context, loggerMock.Object);

            var result = await controller.DeleteBook(999);

            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}