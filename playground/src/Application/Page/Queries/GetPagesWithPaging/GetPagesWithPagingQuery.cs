using ReadModelService.Application.Page.DTOs;

namespace ReadModelService.Application.Page.Queries.GetPagesWithPaging;

public record GetPagesWithPagingQuery(int page = 1, int pageSize = 10);