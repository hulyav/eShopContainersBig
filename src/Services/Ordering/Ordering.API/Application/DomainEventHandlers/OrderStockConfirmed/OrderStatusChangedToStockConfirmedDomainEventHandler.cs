namespace Ordering.API.Application.DomainEventHandlers.OrderStockConfirmed
{
    using MediatR;
    using Microsoft.eShopOnContainers.Services.Ordering.Domain.AggregatesModel.OrderAggregate;
    using Microsoft.Extensions.Logging;
    using Domain.Events;
    using System;
    using System.Threading.Tasks;
    using Ordering.API.Application.IntegrationEvents;
    using Ordering.API.Application.IntegrationEvents.Events;
    using System.Threading;
    using Microsoft.eShopOnContainers.BuildingBlocks.EventBus.Events;
    using Microsoft.Extensions.Options;
    using Microsoft.eShopOnContainers.Services.Ordering.API;

    public class OrderStatusChangedToStockConfirmedDomainEventHandler
                   : INotificationHandler<OrderStatusChangedToStockConfirmedDomainEvent>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILoggerFactory _logger;
        private readonly IOrderingIntegrationEventService _orderingIntegrationEventService;
        private readonly OrderingSettings _settings;

        public OrderStatusChangedToStockConfirmedDomainEventHandler(
            IOrderRepository orderRepository, ILoggerFactory logger,
            IOrderingIntegrationEventService orderingIntegrationEventService,
            IOptionsSnapshot<OrderingSettings> settings)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _orderingIntegrationEventService = orderingIntegrationEventService;
            _settings = settings.Value;
        }

        public async Task Handle(OrderStatusChangedToStockConfirmedDomainEvent orderStatusChangedToStockConfirmedDomainEvent, CancellationToken cancellationToken)
        {
            _logger.CreateLogger(nameof(OrderStatusChangedToStockConfirmedDomainEventHandler))
                .LogTrace($"Order with Id: {orderStatusChangedToStockConfirmedDomainEvent.OrderId} has been successfully updated with " +
                          $"a status order id: {OrderStatus.StockConfirmed.Id}");

            #region
            IntegrationEvent orderPaymentIntegrationEvent;

            //Business feature comment:
            // When OrderStatusChangedToStockConfirmed Integration Event is handled.
            // Here we're simulating that we'd be performing the payment against any payment gateway
            // Instead of a real payment we just take the env. var to simulate the payment 
            // The payment can be successful or it can fail

            if (_settings.PaymentSucceded)
            {
                var orderToUpdate = await _orderRepository.GetAsync(orderStatusChangedToStockConfirmedDomainEvent.OrderId);

                orderToUpdate.SetPaidStatus();

                await _orderRepository.UnitOfWork.SaveEntitiesAsync();

                //orderPaymentIntegrationEvent = new OrderPaymentSuccededIntegrationEvent(orderStatusChangedToStockConfirmedDomainEvent.OrderId);
            }
            else
            {
                var orderToUpdate = await _orderRepository.GetAsync(orderStatusChangedToStockConfirmedDomainEvent.OrderId);

                orderToUpdate.SetCancelledStatus();

                await _orderRepository.UnitOfWork.SaveEntitiesAsync();
                //orderPaymentIntegrationEvent = new OrderPaymentFailedIntegrationEvent(orderStatusChangedToStockConfirmedDomainEvent.OrderId);
            }

            //await _orderingIntegrationEventService.PublishThroughEventBusAsync(orderPaymentIntegrationEvent);
            //await Task.CompletedTask;
            #endregion
        }
    }  
}