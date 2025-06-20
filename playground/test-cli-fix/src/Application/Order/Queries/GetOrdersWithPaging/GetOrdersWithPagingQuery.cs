using TestService.Application.Order.DTOs;

namespace TestService.Application.Order.Queries.GetOrdersWithPaging;

public record GetOrdersWithPagingQuery(int Page = 1, int PageSize = 10);