using Company.TestService.Application.Item.DTOs;

namespace Company.TestService.Application.Item.Queries.GetItemsWithPaging;

public record GetItemsWithPagingQuery(int page = 1, int pageSize = 10);