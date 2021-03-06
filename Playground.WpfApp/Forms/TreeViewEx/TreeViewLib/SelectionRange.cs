﻿using System.Windows.Media;

namespace Playground.WpfApp.Forms.TreeViewEx.TreeViewLib
{
    public class SelectionRange : BaseViewModel, ISelectionRange
    {
        #region fields
        private int _start;
        private int _end;
        #endregion fields

        #region ctors
        /// <summary>
        /// Parameterized class constructor.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public SelectionRange(int start, int end)
            : this()
        {
            Start = start;
            End = end;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="copyThis"></param>
        public SelectionRange(SelectionRange copyThis)
            : this()
        {
            if (copyThis == null)
                return;

            Start = copyThis.Start;
            End = copyThis.End;
            SelectionBackground = copyThis.SelectionBackground;
            NormalBackground = copyThis.NormalBackground;
            DarkSkin = copyThis.DarkSkin;
        }

        /// <summary>
        /// Class constructor.
        /// </summary>
        public SelectionRange()
        {
            _start = _end = 1;
            SelectionBackground = default(Color);
            NormalBackground = default(Color);
            DarkSkin = false;
        }
        #endregion ctors

        #region properties
        /// <summary>
        /// Gets the start of the indicated range.
        /// </summary>
        public int Start
        {
            get => _start;

            private set
            {
                if (_start != value)
                {
                    _start = value;
                    NotifyPropertyChanged(() => Start);
                }
            }
        }

        /// <summary>
        /// Gets the end of the indicated range.
        /// </summary>
        public int End
        {
            get => _end;

            private set
            {
                if (_end != value)
                {
                    _end = value;
                    NotifyPropertyChanged(() => End);
                }
            }
        }

        /// <summary>
        /// Gets a bool value to determine whether DarkSkin default
        /// value for <see cref="SelectionBackground"/> property should
        /// be applied or not.
        /// </summary>
        public bool DarkSkin { get; private set; }

        /// <summary>
        /// Gets the background color that is applied to the background brush,
        /// which should be applied when no match is indicated
        /// (this can be default(Color) in which case standard selection Brush
        /// is applied).
        /// </summary>
        public Color SelectionBackground { get; set; }

        /// <summary>
        /// Gets the background color that is applied to the background brush.
        /// which should be applied when no match is indicated
        /// (this can be default(Color) in which case Transparent is applied).
        /// </summary>
        public Color NormalBackground { get; set; }

        /// <summary>
        /// Gets a copy of this object.
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return new SelectionRange(this);
        }
        #endregion properties
    }
}
