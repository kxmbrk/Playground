﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;
// ReSharper disable InconsistentlySynchronizedField
// ReSharper disable PossibleNullReferenceException
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Playground.WpfApp.Forms.TreeViewEx.TreeViewLib
{
    public class MetaAccountViewModel : BaseViewModel, IHasDummyChild
    {
        //Fields
        private static readonly MetaAccountViewModel DummyChild = new MetaAccountViewModel();
        private bool _isItemExpanded;
        private List<MetaAccountViewModel> _backUpNodes;
        private readonly ObservableCollection<MetaAccountViewModel> _children;
        private MatchType _match;
        private string _name;
        private ISelectionRange _range;
        private object _itemsLock = new object();

        public MetaAccountViewModel(MetaAccountModel model, MetaAccountViewModel parent)
            : this()
        {
            Parent = parent;

            _name = model.Name;
            Id = model.Id;
            ParentId = model.ParentId;
            TypeOfAccount = model.Type;

            ChildrenClear(false);  // Lazy Load Children !!!
        }

        //Constructors
        protected MetaAccountViewModel()
        {
            _isItemExpanded = false;

            _children = new ObservableCollection<MetaAccountViewModel>();
            BindingOperations.EnableCollectionSynchronization(_children, _itemsLock);

            _backUpNodes = new List<MetaAccountViewModel>();

            _match = MatchType.NoMatch;
            Parent = null;
        }

        //Properties
        public MetaAccountViewModel Parent { get; private set; }

        public MatchType Match
        {
            get => _match;

            private set
            {
                if (_match != value)
                {
                    _match = value;
                    NotifyPropertyChanged(() => Match);
                }
            }
        }

        public ISelectionRange Range
        {
            get => _range;

            private set
            {
                if (_range != null && value != null)
                {
                    // Nothing changed - so we change nothing here :-)
                    if (_range.Start == value.Start &&
                        _range.End == value.End &&
                        _range.SelectionBackground == value.SelectionBackground &&
                        _range.SelectionBackground == value.SelectionBackground)
                        return;

                    _range = (ISelectionRange)value.Clone();
                    NotifyPropertyChanged(() => Range);
                }

                if (_range == null && value != null ||
                    _range != null && value == null)
                {
                    _range = (ISelectionRange)value.Clone();
                    NotifyPropertyChanged(() => Range);
                }
            }
        }

        public bool IsItemExpanded
        {
            get => _isItemExpanded;
            set
            {
                if (_isItemExpanded != value)
                {
                    _isItemExpanded = value;
                    NotifyPropertyChanged(() => IsItemExpanded);
                }
            }
        }

        public string Name
        {
            get => _name;

            private set
            {
                if (_name != value)
                {
                    _name = value;
                    NotifyPropertyChanged(() => Name);
                }
            }
        }

        public int Id { get; }

        public int? ParentId { get; }

        public AcctType TypeOfAccount { get; }

        public IEnumerable<MetaAccountViewModel> BackUpNodes => _backUpNodes;

        public IEnumerable<MetaAccountViewModel> Children => _children;

        public int ChildrenCount => _backUpNodes.Count;

        public virtual bool HasDummyChild
        {
            get
            {
                if (Children != null)
                {
                    if (_children.Count == 1)
                    {
                        if (_children[0] == DummyChild)
                            return true;
                    }
                }

                return false;
            }
        }

        //Methods
        public string GetStackPath(MetaAccountViewModel current = null)
        {
            if (current == null)
                current = this;

            string result = string.Empty;

            // Traverse the list of parents backwards and
            // add each child to the path
            while (current != null)
            {
                result = "/" + Name + result;

                current = current.Parent;
            }

            return result;
        }

        internal static MetaAccountViewModel GetViewModelFromModel(MetaAccountModel srcRoot)
        {
            if (srcRoot == null)
                return null;

            var srcItems = TreeLib.BreadthFirst.Traverse.LevelOrder(srcRoot, i => i.Children);
            var dstIdItems = new Dictionary<int, MetaAccountViewModel>();
            MetaAccountViewModel dstRoot = null;

            foreach (var node in srcItems.Select(i => i.Node))
            {
                if (node.Parent == null)
                {
                    dstRoot = new MetaAccountViewModel(node, null);
                    dstIdItems.Add(dstRoot.Id, dstRoot);
                }
                else
                {
                    try
                    {
                        MetaAccountViewModel vmParentItem;     // Find parent ViewModel for Model
                        dstIdItems.TryGetValue(node.Parent.Id, out vmParentItem);
                        var dstNode = new MetaAccountViewModel(node, vmParentItem);
                        vmParentItem.ChildrenAdd(dstNode);     // Insert converted ViewModel below ViewModel parent
                        Console.WriteLine(dstNode.Id + " - " + dstNode.Name + " - " + dstNode.ParentId);
                        dstIdItems.Add(dstNode.Id, dstNode);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }

            dstIdItems.Clear(); // Destroy temp ID look-up structure

            return dstRoot;
        }

        internal int LoadChildren()
        {
            ChildrenClear(false, false); // Clear collection of children

            if (_backUpNodes.Any())
            {
                foreach (var item in _backUpNodes)
                    ChildrenAdd(item, false);
            }

            return _children.Count;
        }

        internal void ChildrenClear(bool bClearBackup = true, bool bAddDummyChild = true)
        {
            try
            {
                lock (_itemsLock)
                {
                    _children.Clear();

                    // Accounts do not have children so we need no dummy child here
                    if (bAddDummyChild && TypeOfAccount != AcctType.Account)
                        _children.Add(DummyChild);
                }

                if (bClearBackup)
                    _backUpNodes.Clear();
            }
            catch
            {
                // ignored
            }
        }

        internal MatchType ProcessNodeMatch(SearchParams searchParams, out int matchStart)
        {
            matchStart = -1;
            MatchType matchThisNode = MatchType.NoMatch;

            // Determine whether this node is a match or not
            if ((matchStart = searchParams.MatchSearchString(Name)) >= 0)
                matchThisNode = MatchType.NodeMatch;

            ChildrenClear(false);

            if (ChildrenCount > 0)
            {
                // Evaluate children by adding only those children that contain no 'NoMatch'
                MatchType maxChildMatch = MatchType.NoMatch;
                foreach (var item in BackUpNodes)
                {
                    if (item.Match != MatchType.NoMatch)
                    {
                        // Expand this item if it (or one of its children) contains a match
                        if (item.Match == MatchType.SubNodeMatch ||
                            item.Match == MatchType.Node_AND_SubNodeMatch)
                        {
                            item.SetExpand(true);
                        }
                        else
                            item.SetExpand(false);

                        if (maxChildMatch < item.Match)
                            maxChildMatch = item.Match;

                        ChildrenAdd(item, false);
                    }
                }

                if (matchThisNode == MatchType.NoMatch && maxChildMatch != MatchType.NoMatch)
                    matchThisNode = MatchType.SubNodeMatch;

                if (matchThisNode == MatchType.NodeMatch && maxChildMatch != MatchType.NoMatch)
                    matchThisNode = MatchType.Node_AND_SubNodeMatch;
            }

            return matchThisNode;
        }

        internal void SetExpand(bool isExpanded)
        {
            IsItemExpanded = isExpanded;
        }

        internal MatchType SetMatch(MatchType match, int matchStart = -1, int matchEnd = -1)
        {
            Match = match;
            Range = new SelectionRange(matchStart, matchEnd);

            return match;
        }

        private void ChildrenAdd(MetaAccountViewModel child, bool bAddBackup = true)
        {
            try
            {
                if (HasDummyChild)
                {
                    lock (_itemsLock)
                    {
                        _children.Clear();
                    }
                }

                lock (_itemsLock)
                {
                    _children.Add(child);
                }

                if (bAddBackup)
                    _backUpNodes.Add(child);
            }
            catch
            {
                // ignored
            }
        }

        private void ChildrenAddBackupNodes(MetaAccountViewModel child)
        {
            _backUpNodes.Add(child);
        }

        private void ChildrenRemove(MetaAccountViewModel child, bool bRemoveBackup = true)
        {
            lock (_itemsLock)
            {
                _children.Remove(child);
            }

            if (bRemoveBackup)
                _backUpNodes.Remove(child);
        }
    }
}
