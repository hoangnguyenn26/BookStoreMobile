﻿
using Bookstore.Mobile.Models;
using Refit;

namespace Bookstore.Mobile.Interfaces.Apis
{
    public interface IBooksApi
    {
        [Get("/v1/books")]
        Task<ApiResponse<IEnumerable<BookDto>>> GetBooks(
            [Query] Guid? categoryId,
            [Query] Guid? authorId,
            [Query] string? search,
            [Query] int page = 1,
            [Query] int pageSize = 10);

        [Get("/v1/books/{id}")]
        Task<ApiResponse<BookDto>> GetBookById(Guid id);

        // Admin endpoints
        [Post("/v1/books")]
        Task<ApiResponse<BookDto>> CreateBook([Body] CreateBookDto dto);

        [Put("/v1/books/{id}")]
        Task<ApiResponse<object>> UpdateBook(Guid id, [Body] UpdateBookDto dto);

        [Delete("/v1/books/{id}")]
        Task<ApiResponse<object>> DeleteBook(Guid id);

        [Multipart]
        [Post("/v1/books/{id}/image")]
        Task<ApiResponse<object>> UploadBookCoverImage(Guid id, [AliasAs("imageFile")] StreamPart imageFile);

    }
}