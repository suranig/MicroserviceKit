using OrderService.Application.Order.DTOs;

namespace OrderService.Application.Order.Queries.GetOrdersWithPaging;

public record GetOrdersWithPagingQuery(int Page = 1, int PageSize = 10);