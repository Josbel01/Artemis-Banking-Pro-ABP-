using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AutomaticOverduePaymentControlFunction;

public class FunctionAutomaticOverduePaymentControl
{
    private readonly ILogger _logger;
    public FunctionAutomaticOverduePaymentControl(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<FunctionAutomaticOverduePaymentControl>();
    }
    [Function("FunctionAutomaticOverduePaymentControl")]
    public void Run([TimerTrigger("%TimeTrigger%")] TimerInfo myTimer)
    {
        _logger.LogInformation("C# Timer trigger function executed at: {executionTime}", DateTime.Now);
        
        if (myTimer.ScheduleStatus is not null)
        {
            _logger.LogInformation("Next timer schedule at: {nextSchedule}", myTimer.ScheduleStatus.Next);
        }
    }
}