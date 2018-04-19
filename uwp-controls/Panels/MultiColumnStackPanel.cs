using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace uwp_controls.Panels
{
    public class MultiColumnStackPanel : Panel
    {
        public static readonly DependencyProperty MaximumColumnWidthProperty =
            DependencyProperty.Register("MaximumColumnWidth", typeof(double), typeof(MultiColumnStackPanel), null);

        public static readonly DependencyProperty MaximumColumnCountProperty =
           DependencyProperty.Register("MaximumColumnCount", typeof(int), typeof(MultiColumnStackPanel), null);

        public static readonly DependencyProperty HorizontalItemSpacingProperty =
           DependencyProperty.Register("HorizontalItemSpacing", typeof(double), typeof(MultiColumnStackPanel), null);

        
        public static readonly DependencyProperty VerticalItemSpacingProperty =
           DependencyProperty.Register("VerticalItemSpacing", typeof(double), typeof(MultiColumnStackPanel), null);


        /// <summary>
        /// How much space to put between columns.  This also applies to outside edges.
        /// </summary>
        public double HorizontalItemSpacing
        {
            get
            {

                return (double)GetValue(HorizontalItemSpacingProperty);
            }
            set
            {
                SetValue(HorizontalItemSpacingProperty, value);
            }
        }

        /// <summary>
        /// How much space to put between items in the same column.  This also applies to the first items.
        /// </summary>
        public double VerticalItemSpacing
        {
            get
            {
                return (double)GetValue(VerticalItemSpacingProperty);
            }
            set
            {
                SetValue(VerticalItemSpacingProperty, value);
            }
        }


        /// <summary>
        /// Set a maximum width for columns.  This determines column break points.
        /// </summary>
        public double MaximumColumnWidth
        {
            get
            {
                return (double)GetValue(MaximumColumnWidthProperty);
            }
            set
            {
                SetValue(MaximumColumnWidthProperty, value);
                columnBreaks = getColumnBreaks();
            }
        }


        /// <summary>
        /// Maximum number of columns.  After this, you just get white space
        /// </summary>
        public int MaximumColumnCount
        {
            get
            {
                return (int)GetValue(MaximumColumnCountProperty);
            }
            set
            {
                SetValue(MaximumColumnCountProperty, value);
                columnBreaks = getColumnBreaks();
            }
        }



        protected override Size MeasureOverride(Size availableSize)
        {
            var columnCount = determineColumnCount(availableSize);
            var columnWidth = getActualColumnWidth(columnCount, availableSize.Width);

            initializeColumnHeights(columnCount);

            for (var i = 0; i < Children.Count; i++)
            {
                Children[i].Measure(new Size(columnWidth, double.PositiveInfinity));
                addHeightToColumn(Children[i].DesiredSize.Height, shortestColumnIndex);
            }


            return new Size(availableSize.Width, heighestColumn);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {

            var columnCount = determineColumnCount(finalSize);
            var columnWidth = getActualColumnWidth(columnCount, finalSize.Width);
            initializeColumnHeights(columnCount);

            double x, y;
            for (var i = 0; i < Children.Count; i++)
            {

                x = columnOffsets[shortestColumnIndex];
                y = columnHeights[shortestColumnIndex];
                Point anchorPoint = new Point(x, y);
                addHeightToColumn(Children[i].DesiredSize.Height, shortestColumnIndex);
                Children[i].Arrange(new Rect(anchorPoint, Children[i].DesiredSize));
            }


            return finalSize;
        }


        // Column break points should be at intervals of the maximum column width
        private double[] getColumnBreaks()
        {
            var breakCount = this.MaximumColumnCount;
            var result = new double[breakCount];
            for (var i = 0; i < breakCount; i++)
            {
                result[i] = i * this.MaximumColumnWidth;
            }

            return result;
        }

        // Calculate how many columns we should have currently
        private int determineColumnCount(Size availableSize)
        {
            var breaks = getColumnBreaks();

            for (var i = 0; i < breaks.Length; i++)
            {
                if (availableSize.Width < breaks[i])
                {
                    if (i >= 0)
                    {
                        return i;
                    }
                    else
                    {
                        return 1;
                    }
                }
            }

            return MaximumColumnCount;

        }


        //  An array of widths at which the number of columns changes
        private double[] columnBreaks;

        //  An array storing the total height of each column
        private double[] columnHeights;

        //  An array storing the X Offsets for each column
        private double[] columnOffsets;

        // The current column with the shortest height
        private int shortestColumnIndex = 0;

        // The current height of the tallest column
        private double heighestColumn = 0;


        //  Initialize all column heights to Vertical Item Spacing
        private void initializeColumnHeights(int columnCount)
        {
            heighestColumn = 0;
            shortestColumnIndex = 0;
            if (columnHeights == null || columnHeights.Length != columnCount)
            {
                columnHeights = new double[columnCount];
            }
            for (var i = 0; i < columnCount; i++)
            {
                columnHeights[i] = VerticalItemSpacing;
            }
        }

        //  Calculate the correct column width accounting for Horizontal Item Spacing
        //  Also calculates the column offset values
        private double getActualColumnWidth(int columnCount, double availableWidth)
        {
            double gutterWidth = (columnCount + 1) * HorizontalItemSpacing;
            availableWidth = availableWidth - gutterWidth;

            var columnWidth = limitColumnWidth(new Size(availableWidth, double.PositiveInfinity), columnCount);

            columnOffsets = new double[columnCount];
            for (var i = 0; i < columnCount; i++)
            {
                columnOffsets[i] = (HorizontalItemSpacing * (i + 1)) + (columnWidth * i);
            }

            return columnWidth;
        }


        //  Add a height value to a column.
        //  Todo:  If a column value is NOT 0, don't let it be the same as any
        //         other column.  If it is, we can get strange resizing behavior.
        private void addHeightToColumn(double height, int columnIndex)
        {
            columnHeights[columnIndex] += height + VerticalItemSpacing;
            if (columnHeights[columnIndex] > heighestColumn)
            {
                heighestColumn = columnHeights[columnIndex];
            }
            double lowerLimit = columnHeights[0];
            shortestColumnIndex = 0;
            for (var i = 1; i < columnHeights.Length; i++)
            {
                if (columnHeights[i] < lowerLimit)
                {
                    shortestColumnIndex = i;
                    lowerLimit = columnHeights[i];
                }
            }
        }

        //  Force column widht to never exceed maximum width
        private double limitColumnWidth(Size availableSize, int columnCount)
        {

            double defaultColumnWidth = availableSize.Width / (double)columnCount;

            if (defaultColumnWidth > MaximumColumnWidth)
            {
                return MaximumColumnWidth;
            }
            return defaultColumnWidth;
        }
    }
}
}
