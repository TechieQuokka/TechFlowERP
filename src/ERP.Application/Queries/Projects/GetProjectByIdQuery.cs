using ERP.Application.DTOs;
using ERP.SharedKernel.Utilities;

using MediatR;

namespace ERP.Application.Queries.Projects
{
    public class GetProjectByIdQuery : IRequest<Result<ProjectDto>>
    {
        public Guid Id { get; set; }

        public GetProjectByIdQuery(Guid id)
        {
            Id = id;
        }
    }
}