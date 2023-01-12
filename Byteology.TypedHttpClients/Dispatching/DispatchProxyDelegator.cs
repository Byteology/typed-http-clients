using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Byteology.TypedHttpClients
{
	/// <summary>
	/// Used to transfer a <see cref="DispatchProxy"/> calls to an <see cref="IDispatchHandler"/>.
	/// </summary>
	[Browsable(false)] // This class shouldn't be used externally but needs to be seen by calling assemblies so we just hide it from intellisense.
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Meant for internal use only.")]
	public class DispatchProxyDelegator : DispatchProxy
	{
		private IDispatchHandler? _dispatcher;

		/// <summary>
		/// Creates an object instance that implements the provided interface type.
		/// </summary>
		/// <typeparam name="TInterface">The type of the interface to be implemented.</typeparam>
		/// <param name="handler">The object that will handle the <see cref="DispatchProxy"/> method calls.</param>
		[SuppressMessage("Major Code Smell",
						 "S3011:Reflection should not be used to increase accessibility of classes, methods, or fields")]
		public static TInterface Create<TInterface>(IDispatchHandler handler)
			where TInterface : class
		{
			TInterface proxy = DispatchProxy.Create<TInterface, DispatchProxyDelegator>();
			MethodInfo initMethod =
				typeof(DispatchProxyDelegator).GetMethod(nameof(initialize),
														 BindingFlags.NonPublic | BindingFlags.Instance)!;
			initMethod.Invoke(proxy, new object[] { handler });
			return proxy;
		}

		private void initialize(IDispatchHandler dispatcher)
		{
			_dispatcher = dispatcher;
		}

		/// <summary>
		/// Whenever any method on the generated proxy type is called, this method is invoked to dispatch control.
		/// </summary>
		/// <param name="targetMethod">The method the caller invoked.</param>
		/// <param name="args">The arguments the caller passed to the method.</param>
		/// <returns>The object to return to the caller, or <see langword="null" /> for void methods. </returns>
		protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
		{
			return _dispatcher!.Dispatch(targetMethod!, args);
		}
	}
}
