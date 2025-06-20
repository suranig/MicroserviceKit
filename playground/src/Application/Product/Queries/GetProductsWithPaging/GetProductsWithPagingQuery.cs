using TestService.Application.Product.DTOs;

namespace TestService.Application.Product.Queries.GetProductsWithPaging;

public record GetProductsWithPagingQuery(int page = 1, int pageSize = 10);