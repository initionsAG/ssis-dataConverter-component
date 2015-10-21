using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace DataConverter.Framework {
    /// <summary>
    /// Makes the BindingList sortable
    /// </summary>
    /// <typeparam name="T">type of list elements</typeparam>
    public class SortableBindingList<T>: BindingList<T> where T: class {
        /// <summary>
        /// is the bindglist already sorted?
        /// </summary>
        private bool _isSorted;

        /// <summary>
        /// sort direction
        /// </summary>
        private ListSortDirection _sortDirection = ListSortDirection.Ascending;

        /// <summary>
        /// the property that is used for sorting
        /// </summary>
        private PropertyDescriptor _sortProperty;

        /// <summary>
        /// constructor
        /// </summary>
        public SortableBindingList()
        {
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="list">a list that fills the bindinglist</param>
        public SortableBindingList(IList<T> list)
            : base(list)
        {
        }

        /// <summary>
        /// Sorting is always supported
        /// </summary>
        protected override bool SupportsSortingCore
        {
            get { return true; }
        }

        /// <summary>
        /// Is the list sorted?
        /// </summary>
        protected override bool IsSortedCore
        {
            get { return _isSorted; }
        }

        /// <summary>
        ///Returns sorting direction
        /// </summary>
        protected override ListSortDirection SortDirectionCore
        {
            get { return _sortDirection; }
        }

        /// <summary>
        /// Returns the property that is used for sorting
        /// </summary>
        protected override PropertyDescriptor SortPropertyCore
        {
            get { return _sortProperty; }
        }

        /// <summary>
        /// Removes sort of this list
        /// </summary>
        protected override void RemoveSortCore()
        {
            _isSorted = false;

            _sortDirection = ListSortDirection.Ascending;
            _sortProperty = null;
        }

        /// <summary>
        /// Sorts the items
        /// </summary>
        /// <param name="sortProperty">the property that is used for sorting</param>
        /// <param name="sortOrder">sort order</param>
        protected override void ApplySortCore(PropertyDescriptor sortProperty, ListSortDirection sortOrder)
        {
            _sortProperty = sortProperty;
            _sortDirection = sortOrder;

            List<T> list = (List<T>) this.Items;// as List<T>;

            if (list != null)
            {
                list.Sort(Compare);

                _isSorted = true;

                OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
            }
        }

        /// <summary>
        /// compares 2 values
        /// </summary>
        /// <param name="left">left value</param>
        /// <param name="right">right value</param>
        /// <returns>-1,0 or 1</returns>
        private int Compare(T left, T right)
        {
            object leftValue = left == null ? null : _sortProperty.GetValue(left);
            object rightValue = right == null ? null : _sortProperty.GetValue(right);

            int compareResult;


            if (leftValue == null)
            {
                compareResult = (rightValue == null) ? 0 : -1; //treat nulls as equal
            }
            if (rightValue == null)
            {
                compareResult = 1;
            }
            if (leftValue is IComparable)
            {
                compareResult = ((IComparable) leftValue).CompareTo(rightValue);
            }
            if (leftValue.Equals(rightValue))
            {
                compareResult = 0;
            }
            else
            {
                //not IComparable -> compare strings
                compareResult = leftValue.ToString().CompareTo(rightValue.ToString());
            }

            //invert compareResult if sort order is descending
            if (_sortDirection == ListSortDirection.Descending)
                compareResult = -compareResult;

            return compareResult;
        }
    }

}
