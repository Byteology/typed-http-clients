using Byteology.GuardClauses;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;

namespace Byteology.TypedHttpClients
{
	/// <summary>
	/// Contains extensions methods for injecting <see cref="TypedHttpClient{TServiceContract}"/> instances.
	/// </summary>
	public static class ServiceCollectionExtensions
	{
		/// <inheritdoc cref="AddTypedHttpClient{TServiceContract, TClient}(IServiceCollection, System.Action{System.Net.Http.HttpClient})"/>
		public static IServiceCollection AddTypedHttpClient<TServiceContract, TClient>(this IServiceCollection services)
			where TClient : TypedHttpClient<TServiceContract>
			where TServiceContract : class
		{
			return AddTypedHttpClient<TServiceContract, TClient>(services, _ => { });
		}

		/// <summary>
		/// Configures a binding between <typeparamref name="TServiceContract"/> and <typeparamref name="TClient"/> using an
		/// underlying named <see cref="HttpClient"/>. The <see cref="HttpClient"/> name will be set to the
		/// type name of <typeparamref name="TServiceContract"/>.
		/// </summary>
		/// <typeparam name="TServiceContract"><inheritdoc cref="TypedHttpClient{TServiceContract}" path="/typeparam"/></typeparam>
		/// <typeparam name="TClient">The type of the <see cref="TypedHttpClient{TServiceContract}"/> to use.</typeparam>
		/// <param name="services">The <see cref="IServiceCollection"/>.</param>
		/// <param name="configureClient">A delegate that is used to configure an <see cref="HttpClient"/>.</param>
		/// <returns>A reference to this instance after the operation has completed.</returns>
		public static IServiceCollection AddTypedHttpClient<TServiceContract, TClient>(
			this IServiceCollection services,
			Action<HttpClient> configureClient)
			where TClient : TypedHttpClient<TServiceContract>
			where TServiceContract : class
		{
			Guard.Argument(configureClient, nameof(configureClient)).NotNull();

			if (typeof(TClient).IsAbstract)
				throw new ArgumentException(typeof(TClient).Name + " must be a concrete implementation.");

			services.AddHttpClient<TClient>(typeof(TServiceContract).FullName, configureClient);
			services.AddScoped<TServiceContract>(sp => sp.GetRequiredService<TClient>().Endpoints);

			return services;
		}
	}
}
