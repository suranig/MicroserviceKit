using ECommerce.OrderService.Application.Order.DTOs;

namespace ECommerce.OrderService.Application.Order.Queries.GetOrdersWithPaging;

public record GetOrdersWithPagingQuery(int page = 1, int pageSize = 10);