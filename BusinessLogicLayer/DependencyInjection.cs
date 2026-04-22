using BusinessLogicLayer.Mappers;
using BusinessLogicLayer.Policies;
using BusinessLogicLayer.RabbitMQ;
using BusinessLogicLayer.ServiceContracts;
using BusinessLogicLayer.Services;
using BusinessLogicLayer.Validators;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BusinessLogicLayer;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessLogicLayer(this IServiceCollection services, IConfiguration configuration)
    {
        //Add services related to business logic layer here

        services.AddValidatorsFromAssemblyContaining<AddOrderItemRequestValidator>();
        services.AddAutoMapper(config =>
        {
            config.AddProfile<OrderAddRequestToOrderMappingProfile>();
            config.AddProfile<OrderItemAddRequestToOrderItemMappingProfile>();
            config.AddProfile<OrderItemToOrderItemResponseMappingProfile>();
            config.AddProfile<OrderItemUpdateRequestToOrderItemMappingProfile>();
            config.AddProfile<OrderToOrderResponseMappingProfile>();
            config.AddProfile<OrderUpdateRequestToOrderMappingProfile>();
            config.AddProfile<ProductDTOToOrderItemResponseMappingProfile>();
            config.AddProfile<UserDTOToOrderResponseMappingProfile>();
        });

        services.AddScoped<IOrdersService, OrdersService>();
        services.AddSingleton<IPollyPolicies, PollyPolicies>();
        services.AddTransient<IRabbitMQProductNameUpdateConsumer, RabbitMQProductNameUpdateConsumer>();
        services.AddTransient<IRabbitMQProductDeletionConsumer, RabbitMQProductDeletionConsumer>();
        services.AddHostedService<RabbitMQProductNameUpdateHostedService>();
        services.AddHostedService<RabbitMQProductDeletionHostedService>();
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = $"{configuration["REDIS_HOST"]}:{configuration["REDIS_PORT"]}";
        });

        return services;
    }
}
