using System.Diagnostics.Metrics;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;
using Services.ModelServices;

namespace Services.StufferServices;

public sealed class StufferStepper : IStepperService<Stuffer>
{
    private readonly ILogger<StufferStepper> _logger;
    private readonly StufferRepository _stufferRepository;
    private readonly ModelState _modelState;
    
    public StufferStepper(
        ILogger<StufferStepper> logger,
        StufferRepository stufferRepository,
        ModelState modelState,
        Meter meter)
    {
        _logger = logger;
        _stufferRepository = stufferRepository;
        _modelState = modelState;
    }
    
    public Task DataCollectAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
    
    public Task StepAsync(Stuffer stuffer, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task StepAsync(CancellationToken cancellationToken)
    {
        var stuffers = _stufferRepository.Get()
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var stuffer in stuffers)
        {
            _logger.LogDebug("Handling Step \n({Step})\n for this Stuffer \n({@Stuffer})",
                _modelState.ModelTime,
                stuffer);
            
            await StepAsync(stuffer, cancellationToken);
            
            _logger.LogDebug("Completed handling Step \n({Step})\n for this Stuffer \n({@Stuffer})",
                _modelState.ModelTime,
                stuffer);
        }
    }
}