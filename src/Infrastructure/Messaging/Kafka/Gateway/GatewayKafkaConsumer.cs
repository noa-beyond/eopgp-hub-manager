using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects.ChDM;
using Domain.ValueObjects.Gateway;
using Infrastructure.Messaging.Interfaces;
using Infrastructure.Messaging.Kafka.Common;
using Infrastructure.Messaging.Kafka.Config;
using Infrastructure.Percistance.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Messaging.Kafka.Gateway
{
    public class GatewayKafkaConsumer(IOrderDbRepository orderDbRepository,
                                      IProductDbRepository productDbRepository,
                                      IOptionsSnapshot<KafkaConfiguration> infrastructureConfiguration,
                                      IKafkaProducer<ChDetectionMappingProducerMessage> chDetectionProducer,
                                      ILogger<GatewayKafkaConsumer> logger)
        : BaseKafkaConsumer<GatewayMessage>(infrastructureConfiguration.Value.GatewayKafkaConfiguration, logger)
    {
        public override async Task ProcessMessage(GatewayMessage message)
        {
            ArgumentNullException.ThrowIfNull(message);

            OrderType orderType = await orderDbRepository.GetOrderTypeAsync(message.Id);

            await orderDbRepository.UpdateOrderAsync(message.Id, OrderStatus.Processing);

            switch (orderType)
            {
                case OrderType.ChDetectionMapping:
                    {
                        List<Product> productsBeforeEvent = await productDbRepository.GetProductsByOrderIdAndGroupIdAsync(message.Id, GroupType.Before);
                        List<Product> productsAfterEvent = await productDbRepository.GetProductsByOrderIdAndGroupIdAsync(message.Id, GroupType.After);
                        GeometryData orderDetails = await orderDbRepository.GetOrderRequestDetails(message.Id);

                        //var kafkaMessage = new ChDetectionMappingProducerMessage
                        //{
                        //    OrderId = message.Id,
                        //    Geometry = orderDetails,
                        //    InitialSelectionProductPaths = [.. productsBeforeEvent.Select(x => x.Path)],
                        //    FinalSelectionProductPaths = [.. productsAfterEvent.Select(x => x.Path)]
                        //};

                        //await chDetectionProducer.ProduceAsync(kafkaMessage);
                        break;
                    }
                default:
                    {
                        throw new InvalidOperationException();
                    }
            }


        }
    }
}