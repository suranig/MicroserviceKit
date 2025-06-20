using TestService.Application.Test.DTOs;

namespace TestService.Application.Test.Queries.GetTestsWithPaging;

public record GetTestsWithPagingQuery(int Page = 1, int PageSize = 10);