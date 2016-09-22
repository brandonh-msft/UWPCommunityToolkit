﻿using System;
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
        /// Creates an instance of <see cref="DeepLinkParser"/> for the given <see cref="Uri"/>
        /// </summary>
        /// <param name="uri">The URI to parse.</param>
        /// <returns>An instance of <see cref="DeepLinkParser"/></returns>
        /// <remarks><paramref name="uri"/> will be tested for null</remarks>
        public static DeepLinkParser Create(Uri uri) => new DeepLinkParser(uri?.OriginalString);

        /// <summary>
        /// Creates an instance of <see cref="DeepLinkParser"/> for the given <see cref="Uri"/>
        /// </summary>
        /// <param name="uri">The URI to parse.</param>
        /// <returns>An instance of <see cref="DeepLinkParser"/></returns>
        /// <remarks><paramref name="uri"/> will be tested for null</remarks>
        public static DeepLinkParser Create(string uri) => new DeepLinkParser(uri);

        /// <summary>
        /// Validates the source URI.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <returns><paramref name="uri"/> as a <c>System.Uri</c> instance</returns>
        /// <exception cref="System.ArgumentException">Not a valid URI format</exception>
        protected static Uri ValidateSourceUri(string uri)
        {
            Uri validatedUri;
            if (!Uri.TryCreate(uri, UriKind.RelativeOrAbsolute, out validatedUri)
                || !validatedUri.IsWellFormedOriginalString())
            {
                throw new ArgumentException("Not a valid URI format", nameof(uri));
            }

            return validatedUri;
        }

        private readonly ILaunchActivatedEventArgs inputArgs;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeepLinkParser"/> class.
        /// </summary>
        protected DeepLinkParser()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeepLinkParser" /> class.
        /// </summary>
        /// <param name="args">The <see cref="IActivatedEventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.ArgumentException">'args' is not a LaunchActivatedEventArgs instance</exception>
        protected DeepLinkParser(IActivatedEventArgs args)
        {
            inputArgs = args as ILaunchActivatedEventArgs;

            if (inputArgs == null)
            {
                throw new ArgumentException("'args' is not a LaunchActivatedEventArgs instance", nameof(args));
            }

            ParseUriString(inputArgs.Arguments);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeepLinkParser" /> class.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="uri"/> is null</exception>
        protected DeepLinkParser(Uri uri)
            : this(uri?.OriginalString)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeepLinkParser" /> class.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="uri"/> is null, empty, or consists only of whitespace characters</exception>
        protected DeepLinkParser(string uri)
        {
            if (string.IsNullOrWhiteSpace(uri))
            {
                throw new ArgumentNullException(nameof(uri));
            }

            ParseUriString(uri);
        }

        /// <summary>
        /// Parses the URI string in to components.
        /// </summary>
        /// <param name="uri">The URI.</param>
        protected virtual void ParseUriString(string uri)
        {
            Uri validatedUri = ValidateSourceUri(uri);
            string queryString;
            SetRoot(validatedUri, out queryString);
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
        /// Sets <see cref="Root" /> on this <see cref="DeepLinkParser" /> instance and computes the query string position
        /// </summary>
        /// <param name="validatedUri">The validated URI (from <see cref="ValidateSourceUri(string)" />).</param>
        /// <param name="queryString">The query string computed as part of determining where the Root starts and ends.</param>
        protected void SetRoot(Uri validatedUri, out string queryString)
        {
            var origString = validatedUri.OriginalString;
            var startIndex = origString.IndexOf("://");
            if (startIndex != -1)
            {
                origString = origString.Substring(startIndex + 3);
            }

            int queryStartPosition = origString.IndexOf('?');
            queryString = null;
            if (queryStartPosition == -1)
            { // No querystring on the URI
                this.Root = origString;
            }
            else
            {
                this.Root = origString.Substring(0, queryStartPosition);
                if (queryStartPosition != -1)
                { // No querystring on the URI
                    queryString = origString.Substring(queryStartPosition + 1);
                }
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