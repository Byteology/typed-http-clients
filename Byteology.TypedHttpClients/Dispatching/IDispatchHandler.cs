using System;
using System.ComponentModel;
using System.Reflection;

namespace Byteology.TypedHttpClients
{
	/// <summary>
	/// Provides a functionality for handling method calls.
	/// </summary>
	[Browsable(false)] // This interface shouldn't be used externally but needs to be seen by calling assemblies so we just hide it from intellisense.
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Meant for internal use only.")]
	public interface IDispatchHandler
	{
		/// <summary>
		/// Dispatches a specified method call.
		/// </summary>
		/// <param name="targetMethod">The invoked method.</param>
		/// <param name="args">The arguments with which the method was invoked.</param>
		/// <returns>The result of the method execution.</returns>
		object? Dispatch(MethodInfo targetMethod, object?[]? args);
	}
}
