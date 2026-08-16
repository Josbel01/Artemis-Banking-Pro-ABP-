using ABP.Core.Application.Features.HermesPay.Commands.ProcessPayment;
using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Artemis_Banking_Pro_WebApi.Controllers.v1
{
    [ApiVersion("1.0")]
    public class PayController : BaseApiController
    {
        [HttpPost("process-payment/{commerceId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ProcessPayment(int commerceId, [FromBody] ProcessPaymentCommand command)
        {
            if (commerceId != command.CommerceId)
            {
                command.CommerceId = commerceId;
            }

            var transactionId = await Mediator.Send(command);
            return Ok(transactionId);
        }
    }
}
