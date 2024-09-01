using AutoMapper;
using FluentValidation;
using MediatR;
using ProjectName.Application.Common.BaseRequests;
using ProjectName.Application.Common.Interfaces;

namespace ProjectName.Application.Services.FeatureName;

public record ServiceMethodCommand : BaseRequest<ServiceMethodResponse>
{

}

public class ServiceMethodResponse : BaseResponse
{
	
}

public class ServiceMethodCommandValidator : AbstractValidator<ServiceMethodCommand>
{
    public ServiceMethodCommandValidator()
    {
        
    }
}

public class ServiceMethodCommandHandler : IRequestHandler<ServiceMethodCommand, ServiceMethodResponse>
{
    private readonly IAppDbContext _context;
    private readonly IMapper _mapper;

    public ServiceMethodCommandHandler(IAppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ServiceMethodResponse> Handle(ServiceMethodCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
