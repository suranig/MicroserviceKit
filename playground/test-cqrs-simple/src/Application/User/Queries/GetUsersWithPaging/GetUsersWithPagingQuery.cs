using SimpleService.Application.User.DTOs;

namespace SimpleService.Application.User.Queries.GetUsersWithPaging;

public record GetUsersWithPagingQuery(int Page = 1, int PageSize = 10);