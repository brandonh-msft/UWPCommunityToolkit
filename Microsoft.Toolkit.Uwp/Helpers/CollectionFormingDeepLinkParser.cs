﻿using System.Linq;

namespace Microsoft.Toolkit.Uwp
{
    /// <summary>
    /// A version of <see cref="DeepLinkParser" /> which generates a comma-separated-list as the value for any option that is repeated in the query string
    /// </summary>
    /// <example>
    /// in OnLaunched of App.xaml.cs:
    /// <code lang="c#">
    /// if (e.PrelaunchActivated == false)
    /// {
    ///     if (rootFrame.Content == null)
    ///     {
    ///         var parser = CollectionFormingDeepLinkParser.Create(args);
    ///         if (parser["username"] == "John Doe")
    ///         {
    ///             // do work here
    ///         }
    ///         if (parser.Root == "Signup")
    ///         {
    ///             var preferences = parser["pref"].Split(',');    // now a string[] of all 'pref' querystring values passed in URI
    ///             rootFrame.Navigate(typeof(Signup));
    ///         }
    /// </code>
    /// </example>
    public class CollectionFormingDeepLinkParser : DeepLinkParser
    {
        /// <inheritdoc/>
        protected override void ParseUriString(string uri)
        {
            var validatedUri = ValidateSourceUri(uri);

            SetRoot(validatedUri);
            var queryParams = new Helpers.QueryParameterCollection(validatedUri);
            var grouped = queryParams.GroupBy(pair => pair.Key);
            foreach (var group in grouped)
            { // adds the group to the base with ',' separating each item within a group
                Add(group.Key, string.Join(",", group.Select(item => item.Value)));
            }
        }
    }
}
