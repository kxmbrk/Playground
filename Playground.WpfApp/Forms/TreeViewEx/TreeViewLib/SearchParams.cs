﻿using System;

namespace Playground.WpfApp.Forms.TreeViewEx.TreeViewLib
{
    public class SearchParams
    {
        #region constructors
        /// <summary>
        /// Class constructor
        /// </summary>
        public SearchParams(
              string searchString
            , SearchMatch match)
            : this()
        {
            OriginalSearchString = SearchString = (searchString == null ? string.Empty : searchString);
            Match = match;
        }

        /// <summary>
        /// Class constructor
        /// </summary>
        public SearchParams()
        {
            SearchString = string.Empty;
            Match = SearchMatch.StringIsContained;
            MinimalSearchStringLength = 1;
        }
        #endregion constructors

        #region properties
        /// <summary>
        /// Gets the plain text string being searched or filtered.
        /// </summary>
        public string SearchString { get; private set; }

        /// <summary>
        /// Gets the plain text ORIGINAL string being searched or filtered.
        /// This string is the string that was set by the constructor and
        /// CANNOT be changed later on.
        /// </summary>
        public string OriginalSearchString { get; private set; }

        /// <summary>
        /// Gets the string being searched or filtered.
        /// </summary>
        public SearchMatch Match { get; private set; }

        /// <summary>
        /// Gets whether search string contains actual content or not.
        /// </summary>
        public bool IsSearchStringEmpty => string.IsNullOrEmpty(SearchString);

        /// <summary>
        /// Gets the minimal search string length required.
        /// Any string shorter than this will not be searched at all.
        /// </summary>
        public int MinimalSearchStringLength { get; set; }
        #endregion properties

        #region methods
        /// <summary>
        /// Determines if a given string is considered a match in comparison
        /// to the search string and its options or not.
        /// </summary>
        /// <param name="stringToFind"></param>
        /// <returns>true if <paramref name="stringToFind"/>is a match, otherwise false</returns>
        public int MatchSearchString(string stringToFind)
        {
            stringToFind = (stringToFind == null ? string.Empty : stringToFind);

            stringToFind = stringToFind.ToUpper();

            switch (Match)
            {
                case SearchMatch.StringIsContained:
                    return stringToFind.IndexOf(SearchString, StringComparison.Ordinal);

                case SearchMatch.StringIsMatched:
                    if (SearchString == stringToFind)
                        return 0;
                    break;

                default:
                    throw new ArgumentOutOfRangeException($"Internal Error: Search option '{Match}' not implemented.");
            }

            return -1;
        }

        /// <summary>
        /// Can be called to trim the search string before matching takes place.
        /// </summary>
        public void SearchStringTrim()
        {
            SearchString = SearchString.Trim();
        }

        /// <summary>
        /// Can be called to convert the search string
        /// to upper case before matching takes place.
        /// </summary>
        public void SearchStringToUpperCase()
        {
            SearchString = SearchString.ToUpper();
        }
        #endregion methods
    }
}
