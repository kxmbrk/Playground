﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using Playground.WpfApp.Mvvm;

// ReSharper disable RedundantTypeArgumentsOfMethod
// ReSharper disable ConvertToLambdaExpression
// ReSharper disable BitwiseOperatorOnEnumWithoutFlags
// ReSharper disable InconsistentNaming

namespace Playground.WpfApp.Forms.TreeViewEx.TreeViewLib
{
    public class MetaAccountRootViewModel : BaseViewModel
    {
        //Fields
        protected readonly ObservableCollection<MetaAccountViewModel> _accountRootItems;
        protected readonly IList<MetaAccountViewModel> BackUpCountryRoots;
        private object _itemsLock = new object();

        //Constructor
        public MetaAccountRootViewModel()
        {
            _accountRootItems = new ObservableCollection<MetaAccountViewModel>();
            BindingOperations.EnableCollectionSynchronization(_accountRootItems, _itemsLock);

            BackUpCountryRoots = new List<MetaAccountViewModel>(400);

            ExpandCommand = new DelegateCommand<object>(param => OnExpandCommand(param));
        }

        //Properties
        public ObservableCollection<MetaAccountViewModel> AccountRootItems => _accountRootItems;

        public int BackUpCountryRootsCount => BackUpCountryRoots.Count;

        public ICommand ExpandCommand { get; }

        private void OnExpandCommand(object p)
        {
            var param = p as MetaAccountViewModel;
            if (param == null)
                return;
            param.LoadChildren();
        }

        //Methods

        public async Task LoadData()
        {
            var isoCountries = await Database.LoadData();

            foreach (var item in isoCountries)
            {
                lock (_itemsLock)
                {
                    var vmItem = MetaAccountViewModel.GetViewModelFromModel(item);

                    //RootsAdd(vmItem, true);
                    RootsAdd(vmItem, false); // Make all items initially visible
                }
            }
        }

        public async Task<int> DoSearchAsync(SearchParams searchParams, CancellationToken token)
        {
            return await Task.Run<int>(() =>
            {
                return DoSearch(searchParams, token);
            });
        }

        public int DoSearch(SearchParams searchParams, CancellationToken token)
        {
            IList<MetaAccountViewModel> backUpRoots = BackUpCountryRoots;
            ObservableCollection<MetaAccountViewModel> root = _accountRootItems;

            if (searchParams == null)
                searchParams = new SearchParams();

            searchParams.SearchStringTrim();
            searchParams.SearchStringToUpperCase();

            lock (_itemsLock)
            {
                root.Clear();
            }

            // Show all root items if string to search is empty
            if (searchParams.IsSearchStringEmpty ||
                searchParams.MinimalSearchStringLength >= searchParams.SearchString.Length)
            {
                foreach (var rootItem in backUpRoots)
                {
                    if (token.IsCancellationRequested)
                        return 0;

                    //PreOrderTraversal(rootItem);  !!! Lazy Loading Children !!!
                    rootItem.ChildrenClear(false);
                    rootItem.SetExpand(false);

                    lock (_itemsLock)
                    {
                        root.Add(rootItem);
                    }
                }

                return 0;
            }

            int imatchCount = 0;

            // Go through all root items and process their children
            foreach (var rootItem in backUpRoots)
            {
                if (token.IsCancellationRequested)
                    return 0;

                rootItem.SetMatch(MatchType.NoMatch);

                // Match children of this root item
                var nodeMatchCount = MatchNodes(rootItem, searchParams);

                imatchCount += nodeMatchCount;

                // Match this root item and find correct match type between
                // parent and children below
                int offset;
                if ((offset = searchParams.MatchSearchString(rootItem.Name)) >= 0)
                {
                    rootItem.SetMatch(MatchType.NodeMatch, offset, offset + searchParams.SearchString.Length);
                    imatchCount++;
                }

                if (nodeMatchCount > 0)
                {
                    if (rootItem.Match == MatchType.NodeMatch)
                        rootItem.SetMatch(MatchType.Node_AND_SubNodeMatch, offset, offset + searchParams.SearchString.Length);
                    else
                        rootItem.SetMatch(MatchType.SubNodeMatch);
                }

                // Determine wether root item should be visible and expanded or not
                if (rootItem.Match != MatchType.NoMatch)
                {
                    if ((rootItem.Match & (MatchType.SubNodeMatch | MatchType.Node_AND_SubNodeMatch)) != 0)
                        rootItem.SetExpand(true);
                    else
                        rootItem.SetExpand(false);

                    //Console.WriteLine("node: {0} match count: {1}", rootItem.LocalName, nodeMatchCount);
                    lock (_itemsLock)
                    {
                        root.Add(rootItem);
                    }

                }
            }

            return imatchCount;
        }

        protected void RootsAdd(MetaAccountViewModel vmItem, bool addBackupItemOnly)
        {
            BackUpCountryRoots.Add(vmItem);

            if (addBackupItemOnly == false)
                _accountRootItems.Add(vmItem);
        }

        private Task<int> MatchNodesAsync(MetaAccountViewModel root, SearchParams searchParams)
        {
            return Task.Run<int>(() => { return MatchNodes(root, searchParams); });
        }

        private int MatchNodes(MetaAccountViewModel root, SearchParams searchParams)
        {
            var toVisit = new Stack<MetaAccountViewModel>();
            var visitedAncestors = new Stack<MetaAccountViewModel>();
            int matchCount = 0;

            toVisit.Push(root);
            while (toVisit.Count > 0)
            {
                var node = toVisit.Peek();
                if (node.ChildrenCount > 0)
                {
                    if (PeekOrDefault(visitedAncestors) != node)
                    {
                        visitedAncestors.Push(node);
                        PushReverse(toVisit, node.BackUpNodes);
                        continue;
                    }

                    visitedAncestors.Pop();
                }

                // Process Node and count matches (if any)
                var match = node.ProcessNodeMatch(searchParams, out int matchStart);
                node.SetMatch(match, matchStart, matchStart + searchParams.SearchString.Length);

                if (node.Match == MatchType.NodeMatch)
                    matchCount++;

                if (node.Match == MatchType.SubNodeMatch ||
                    node.Match == MatchType.Node_AND_SubNodeMatch)
                {
                    node.SetExpand(true);
                }
                else
                    node.SetExpand(false);

                toVisit.Pop();
            }

            return matchCount;
        }

        private MetaAccountViewModel PeekOrDefault(Stack<MetaAccountViewModel> s)
        {
            return s.Count == 0 ? null : s.Peek();
        }

        private void PushReverse(Stack<MetaAccountViewModel> s, IEnumerable<MetaAccountViewModel> list)
        {
            foreach (var l in list.ToArray().Reverse())
            {
                s.Push(l);
            }
        }
    }
}
