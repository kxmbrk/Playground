﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Playground.WpfApp.Forms.TreeViewEx.TreeViewLib
{

    public class MetaAccountModel
    {
        private readonly ObservableCollection<MetaAccountModel> _children;

        public MetaAccountModel(MetaAccountModel parent, int id, string name, int parentId, AcctType type)
            : this()
        {
            Parent = parent;
            Id = id;
            Name = name;
            ParentId = parentId;
            Type = type;
        }

        public MetaAccountModel()
        {
            _children = new ObservableCollection<MetaAccountModel>();
        }

        public MetaAccountModel Parent { get; private set; }

        public int Id { get; }
        public string Name { get; }
        public int ParentId { get; }
        public AcctType Type { get; }

        public IEnumerable<MetaAccountModel> Children => _children;

        public int ChildrenCount => _children.Count;

        public void ChildrenAdd(MetaAccountModel child)
        {
            _children.Add(child);
        }

        public void ChildrenRemove(MetaAccountModel child)
        {
            _children.Remove(child);
        }

        public void ChildrenClear()
        {
            _children.Clear();
        }

        public void SetParent(MetaAccountModel parent)
        {
            Parent = parent;
        }
    }
    public enum AcctType
    {
        Category = 0,
        Account = 1
    }

    public enum MatchType
    {
        /// <summary>
        /// The node (and its children) contains no match.
        /// </summary>
        NoMatch = 0,

        /// <summary>
        /// The node was macth against the search parameters.
        /// </summary>
        NodeMatch = 1,

        /// <summary>
        /// A child node or children nodes of this node contain a match
        /// BUT this node does not contain a match.
        /// </summary>
        SubNodeMatch = 2,

        /// <summary>
        /// A child node or children nodes of this node contain a match
        /// AND this node does also contain a match.
        /// </summary>
        Node_AND_SubNodeMatch = 4
    }

    public enum SearchMatch
    {
        /// <summary>
        /// The string searched is contained somewhere in within a nodes string.
        /// </summary>
        StringIsContained = 0,

        /// <summary>
        /// The string searched is an exact match within a nodes string
        /// (length of strings is identical and ALL letters are present
        /// in the given order - but case folding may still be applied).
        /// </summary>
        StringIsMatched = 1
    }

    public interface ISelectionRange : ICloneable
    {
        /// <summary>
        /// Gets the start of the indicated range.
        /// </summary>
        int Start { get; }

        /// <summary>
        /// Gets the end of the indicated range.
        /// </summary>
        int End { get; }

        /// <summary>
        /// Gets a bool value to determine whether DarkSkin default
        /// value for <see cref="SelectionBackground"/> property should
        /// be applied or not.
        /// </summary>
        bool DarkSkin { get; }

        /// <summary>
        /// Gets the background color that is applied to the background brush,
        /// which should be applied when no match is indicated
        /// (this can be default(Color) in which case standard selection Brush
        /// is applied).
        /// 
        /// Note:
        /// Standard selection background color on light skin: 208, 247, 255
        /// Standard selection background color on dark  skin: 254, 252, 200
        /// </summary>
        Color SelectionBackground { get; }

        /// <summary>
        /// Gets the background color that is applied to the background brush.
        /// which should be applied when no match is indicated
        /// (this can be default(Color) in which case Transparent is applied).
        /// </summary>
        Color NormalBackground { get; }
    }

    public interface IHasDummyChild
    {
        bool HasDummyChild { get; }
    }
}
