using ECommerce.OrderService.Application.Customer.DTOs;

namespace ECommerce.OrderService.Application.Customer.Queries.GetCustomersWithPaging;

public record GetCustomersWithPagingQuery(int page = 1, int pageSize = 10);