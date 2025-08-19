using BankAPI.Shared;
using BankAPI.Shared.Models;
using BankAPI.Shared.Models.Enums;
using Microsoft.EntityFrameworkCore;

public class OutboxDispatcherService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<OutboxDispatcherService> _logger;

    public OutboxDispatcherService(IServiceProvider services, ILogger<OutboxDispatcherService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<BankDbContext>();
                var rabbitMq = scope.ServiceProvider.GetRequiredService<RabbitMqService>();

                var messages = await db.OutboxMessages
                    .Where(m => m.Status == OutboxMessageStatus.Pending)
                    .OrderBy(m => m.CreatedAt)
                    .Take(100)
                    .ToListAsync(stoppingToken);

                foreach (var message in messages)
                {
                    try
                    {
                        rabbitMq.Publish(
                            message: message.EventData,
                            routingKey: $"account.{message.EventType.ToLower()}",
                            correlationId: message.Id);

                        message.Status = OutboxMessageStatus.Processed;
                        await db.SaveChangesAsync(stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to publish message {MessageId}", message.Id);
                        message.Status = OutboxMessageStatus.Failed;
                        await db.SaveChangesAsync(stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in outbox dispatcher");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken); // Проверяем каждые 5 сек
        }
    }
}