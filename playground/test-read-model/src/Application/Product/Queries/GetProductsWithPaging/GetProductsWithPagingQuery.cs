using ReadModelService.Application.Product.DTOs;

namespace ReadModelService.Application.Product.Queries.GetProductsWithPaging;

public record GetProductsWithPagingQuery(int Page = 1, int PageSize = 10);