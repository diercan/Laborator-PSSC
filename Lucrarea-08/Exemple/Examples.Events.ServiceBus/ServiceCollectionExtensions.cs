using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Examples.Events.ServiceBus;

/// <summary>Înregistrarea în containerul de dependențe a trimiterii și ascultării evenimentelor peste Azure Service Bus.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Înregistrează <see cref="IEventSender"/> peste Azure Service Bus. Necesită un <c>ServiceBusClient</c> deja înregistrat (de exemplu prin <c>AddAzureClients</c>).</summary>
    public static IServiceCollection AddServiceBusEventSender(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<ServiceBusEventsOptions>().Bind(configuration.GetSection(ServiceBusEventsOptions.SectionName));
        services.AddSingleton(TimeProvider.System);
        services.TryAddSingleton<IEventSender, ServiceBusTopicEventSender>();
        return services;
    }

    /// <summary>Înregistrează <see cref="IEventListener"/> peste Azure Service Bus. Continuați cu <see cref="EventListenerBuilder.AddHandler{TEvent, THandler}"/> pentru fiecare tip de eveniment.</summary>
    public static EventListenerBuilder AddServiceBusEventListener(this IServiceCollection services)
    {
        services.TryAddSingleton<IEventListener, ServiceBusTopicEventListener>();
        return new EventListenerBuilder(services);
    }
}

/// <summary>Permite înregistrarea, pe rând, a fiecărui handler de eveniment pe care ascultătorul trebuie să-l cunoască.</summary>
public sealed class EventListenerBuilder(IServiceCollection services)
{
    public EventListenerBuilder AddHandler<TEvent, THandler>()
        where TEvent : IIntegrationEvent
        where THandler : class, IEventHandler<TEvent>
    {
        services.AddScoped<IEventHandler<TEvent>, THandler>();
        services.AddSingleton<IEventDispatcher, EventDispatcher<TEvent>>();
        return this;
    }
}
