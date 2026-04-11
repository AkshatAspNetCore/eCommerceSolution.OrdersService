using BusinessLogicLayer.Mappers;
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
        return services;
    }
}
