using System;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel.Activation;

namespace Microsoft.Toolkit.Uwp
{
    /// <summary>
    /// Provides assistance with parsing <see cref="ILaunchActivatedEventArgs"/> and its .Arguments property in to a key-value set and target path
    /// </summary>
    /// <example>
    /// in OnLaunched of App.xaml.cs:
    /// <code lang="c#">
    /// if (e.PrelaunchActivated == false)
    /// {
    ///     if (rootFrame.Content == null)
    ///     {
    ///         var parser = DeepLinkParser.Create(args);
    ///         if (parser["username"] == "John Doe")
    ///         {
    ///             // do work here
    ///         }
    ///         if (parser.Root == "Signup")
    ///         {
    ///             rootFrame.Navigate(typeof(Signup));
    ///         }
    /// </code>
    /// </example>
    public class DeepLinkParser : Dictionary<string, string>
    {
        /// <summary>
        /// Creates an instance of <see cref="DeepLinkParser"/> for the given <see cref="IActivatedEventArgs"/>
        /// </summary>
        /// <param name="args">The <see cref="IActivatedEventArgs"/> instance containing the launch Uri data.</param>
        /// <returns>An instance of <see cref="DeepLinkParser"/></returns>
        /// <remarks><paramref name="args"/> will be cast to <see cref="ILaunchActivatedEventArgs"/> </remarks>
        public static DeepLinkParser Create(IActivatedEventArgs args) => new DeepLinkParser(args);

        /// <summary>
        /// Validates the source URI.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <returns>
        ///   <paramref name="uri" /> as a <c>System.Uri</c> instance
        /// </returns>
        /// <exception cref="System.ArgumentNullException">thrown if <paramref name="uri"/> is null</exception>
        /// <exception cref="System.ArgumentException">Not a valid URI format</exception>
        protected static Uri ValidateSourceUri(string uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            Uri validatedUri;
            if (!Uri.TryCreate(uri, UriKind.RelativeOrAbsolute, out validatedUri)
                || !validatedUri.IsWellFormedOriginalString())
            {
                throw new ArgumentException("Not a valid URI format", nameof(uri));
            }

            return validatedUri;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeepLinkParser"/> class.
        /// </summary>
        protected DeepLinkParser()
        {
        }

        private DeepLinkParser(IActivatedEventArgs args)
        {
            var inputArgs = args as ILaunchActivatedEventArgs;

            if (inputArgs == null)
            {
                throw new ArgumentException("'args' is not a LaunchActivatedEventArgs instance", nameof(args));
            }

            ParseUriString(inputArgs.Arguments);
        }

        /// <summary>
        /// Parses the URI string in to components.
        /// </summary>
        /// <param name="uri">The URI.</param>
        protected virtual void ParseUriString(string uri)
        {
            Uri validatedUri = ValidateSourceUri(uri);

            string queryString = SetRootAndGetQueryString(validatedUri);
            if (!string.IsNullOrWhiteSpace(queryString))
            {
                foreach (var queryStringParam in queryString.Split('&')
                    .Select(param =>
                    {
                        var kvp = param.Split('=');
                        return new KeyValuePair<string, string>(kvp[0], kvp[1]);
                    }))
                {
                    try
                    {
                        Add(queryStringParam.Key, queryStringParam.Value);
                    }
                    catch (ArgumentException aex)
                    {
                        throw new ArgumentException("If you wish to use the same key name to add an array of values, try using CollectionFormingDeepLinkParser", aex);
                    }
                }
            }
        }

        /// <summary>
        /// Sets the root and gets the query string.
        /// </summary>
        /// <param name="validatedUri">The validated URI (from <see cref="ValidateSourceUri(string)" />).</param>
        /// <returns>The querystring specified on <paramref name="validatedUri"/>, <c>null</c> if one is not defined.</returns>
        protected virtual string SetRootAndGetQueryString(Uri validatedUri)
        {
            var origString = validatedUri.OriginalString;
            int queryStartPosition = origString.IndexOf('?');
            if (queryStartPosition == -1)
            { // No querystring on the URI
                this.Root = origString;
                return null;
            }
            else
            {
                this.Root = origString.Substring(0, queryStartPosition);
                return origString.Substring(queryStartPosition + 1);
            }
        }

        /// <summary>Gets or sets the root path of the Deep link URI</summary>
        /// <example>
        /// for "MainPage/Options?option1=value1"
        /// Root = "MainPage/Options"
        /// </example>
        public string Root { get; protected set; }
    }
}