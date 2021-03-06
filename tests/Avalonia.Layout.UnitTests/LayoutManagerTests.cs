// Copyright (c) The Avalonia Project. All rights reserved.
// Licensed under the MIT license. See licence.md file in the project root for full license information.

using Avalonia.Controls;
using Avalonia.UnitTests;
using System;
using Xunit;
using System.Collections.Generic;

namespace Avalonia.Layout.UnitTests
{
    public class LayoutManagerTests
    {
        [Fact]
        public void Measures_And_Arranges_InvalidateMeasured_Control()
        {
            var control = new LayoutTestControl();
            var root = new LayoutTestRoot { Child = control };

            root.LayoutManager.ExecuteInitialLayoutPass(root);
            control.Measured = control.Arranged = false;

            control.InvalidateMeasure();
            root.LayoutManager.ExecuteLayoutPass();

            Assert.True(control.Measured);
            Assert.True(control.Arranged);
        }

        [Fact]
        public void Arranges_InvalidateArranged_Control()
        {
            var control = new LayoutTestControl();
            var root = new LayoutTestRoot { Child = control };

            root.LayoutManager.ExecuteInitialLayoutPass(root);
            control.Measured = control.Arranged = false;

            control.InvalidateArrange();
            root.LayoutManager.ExecuteLayoutPass();

            Assert.False(control.Measured);
            Assert.True(control.Arranged);
        }

        [Fact]
        public void Measures_Parent_Of_Newly_Added_Control()
        {
            var control = new LayoutTestControl();
            var root = new LayoutTestRoot();

            root.LayoutManager.ExecuteInitialLayoutPass(root);
            root.Child = control;
            root.Measured = root.Arranged = false;

            root.LayoutManager.ExecuteLayoutPass();

            Assert.True(root.Measured);
            Assert.True(root.Arranged);
            Assert.True(control.Measured);
            Assert.True(control.Arranged);
        }

        [Fact]
        public void Measures_In_Correct_Order()
        {
            LayoutTestControl control1;
            LayoutTestControl control2;
            var root = new LayoutTestRoot
            {
                Child = control1 = new LayoutTestControl
                {
                    Child = control2 = new LayoutTestControl(),
                }
            };


            var order = new List<ILayoutable>();
            Size MeasureOverride(ILayoutable control, Size size)
            {
                order.Add(control);
                return new Size(10, 10);
            }

            root.DoMeasureOverride = MeasureOverride;
            control1.DoMeasureOverride = MeasureOverride;
            control2.DoMeasureOverride = MeasureOverride;
            root.LayoutManager.ExecuteInitialLayoutPass(root);

            control2.InvalidateMeasure();
            control1.InvalidateMeasure();
            root.InvalidateMeasure();

            order.Clear();
            root.LayoutManager.ExecuteLayoutPass();

            Assert.Equal(new ILayoutable[] { root, control1, control2 }, order);
        }

        [Fact]
        public void Measures_Root_And_Grandparent_In_Correct_Order()
        {
            LayoutTestControl control1;
            LayoutTestControl control2;
            var root = new LayoutTestRoot
            {
                Child = control1 = new LayoutTestControl
                {
                    Child = control2 = new LayoutTestControl(),
                }
            };


            var order = new List<ILayoutable>();
            Size MeasureOverride(ILayoutable control, Size size)
            {
                order.Add(control);
                return new Size(10, 10);
            }

            root.DoMeasureOverride = MeasureOverride;
            control1.DoMeasureOverride = MeasureOverride;
            control2.DoMeasureOverride = MeasureOverride;
            root.LayoutManager.ExecuteInitialLayoutPass(root);

            control2.InvalidateMeasure();
            root.InvalidateMeasure();

            order.Clear();
            root.LayoutManager.ExecuteLayoutPass();

            Assert.Equal(new ILayoutable[] { root, control2 }, order);
        }

        [Fact]
        public void Doesnt_Measure_Non_Invalidated_Root()
        {
            var control = new LayoutTestControl();
            var root = new LayoutTestRoot { Child = control };

            root.LayoutManager.ExecuteInitialLayoutPass(root);
            root.Measured = root.Arranged = false;
            control.Measured = control.Arranged = false;

            control.InvalidateMeasure();
            root.LayoutManager.ExecuteLayoutPass();

            Assert.False(root.Measured);
            Assert.False(root.Arranged);
            Assert.True(control.Measured);
            Assert.True(control.Arranged);
        }

        [Fact]
        public void Doesnt_Measure_Removed_Control()
        {
            var control = new LayoutTestControl();
            var root = new LayoutTestRoot { Child = control };

            root.LayoutManager.ExecuteInitialLayoutPass(root);
            control.Measured = control.Arranged = false;

            control.InvalidateMeasure();
            root.Child = null;
            root.LayoutManager.ExecuteLayoutPass();

            Assert.False(control.Measured);
            Assert.False(control.Arranged);
        }

        [Fact]
        public void Measures_Root_With_Infinity()
        {
            var root = new LayoutTestRoot();
            var availableSize = default(Size);

            // Should not measure with this size.
            root.MaxClientSize = new Size(123, 456);

            root.DoMeasureOverride = (_, s) =>
            {
                availableSize = s;
                return new Size(100, 100);
            };

            root.LayoutManager.ExecuteInitialLayoutPass(root);

            Assert.Equal(Size.Infinity, availableSize);
        }

        [Fact]
        public void Arranges_Root_With_DesiredSize()
        {
            var root = new LayoutTestRoot
            {
                Width = 100,
                Height = 100,
            };
 
            var arrangeSize = default(Size);
 
            root.DoArrangeOverride = (_, s) =>
            {
                arrangeSize = s;
                return s;
            };

            root.LayoutManager.ExecuteInitialLayoutPass(root);
            Assert.Equal(new Size(100, 100), arrangeSize);
 
            root.Width = 120;

            root.LayoutManager.ExecuteLayoutPass();
            Assert.Equal(new Size(120, 100), arrangeSize);
        }

        [Fact]
        public void Invalidating_Child_Remeasures_Parent()
        {
            Border border;
            StackPanel panel;

            var root = new LayoutTestRoot
            {
                Child = panel = new StackPanel
                {
                    Children =
                    {
                        (border = new Border())
                    }
                }
            };

            root.LayoutManager.ExecuteInitialLayoutPass(root);
            Assert.Equal(new Size(0, 0), root.DesiredSize);

            border.Width = 100;
            border.Height = 100;

            root.LayoutManager.ExecuteLayoutPass();
            Assert.Equal(new Size(100, 100), panel.DesiredSize);             
        }
    }
}
