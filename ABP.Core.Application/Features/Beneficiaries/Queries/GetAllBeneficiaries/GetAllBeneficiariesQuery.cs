using ABP.Core.Domain.Interfaces;
using ABP.Core.Application.Dtos.Beneficiaries;
using AutoMapper;
using MediatR;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ABP.Core.Application.Features.Beneficiaries.Queries.GetAllBeneficiaries
{
    /// <summary>
    /// Parameters required to get all beneficiaries
    /// </summary>
    public class GetAllBeneficiariesQuery : IRequest<IEnumerable<BeneficiaryDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
[SwaggerParameter(Description = "Optional client ID filter")]
        public string? ClientId { get; set; }
    }

    public class GetAllBeneficiariesQueryHandler : IRequestHandler<GetAllBeneficiariesQuery, IEnumerable<BeneficiaryDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
private readonly IBeneficiaryRepository _beneficiaryRepository;
        private readonly IMapper _mapper;

        public GetAllBeneficiariesQueryHandler(IBeneficiaryRepository beneficiaryRepository, IMapper mapper)
        {
            _beneficiaryRepository = beneficiaryRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<BeneficiaryDto>> Handle(GetAllBeneficiariesQuery request, CancellationToken cancellationToken)
        {
            var beneficiaries = string.IsNullOrEmpty(request.ClientId) 
                ? await _beneficiaryRepository.GetAllListAsync()
                : await _beneficiaryRepository.GetAllByClientIdAsync(request.ClientId);
            var beneficiariesDto = _mapper.Map<IEnumerable<BeneficiaryDto>>(beneficiaries);

            return beneficiariesDto;
        }
    }
}
