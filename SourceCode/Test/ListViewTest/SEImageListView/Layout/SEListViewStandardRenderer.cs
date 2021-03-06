﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace ListViewTest
{
    /// <summary>
    /// 默认渲染器，以类似微软ListView的方式按行绘制项，只绘制简单的文本
    /// </summary>
    class SEListViewStandardRenderer : SEListViewRenderer
    {
        #region 私有成员

        /// <summary>
        /// 字的高度是否已初始化
        /// 在第一次绘制时，测量文本的高度
        /// </summary>
        bool _headerHeightInited = false;

        int _headerHeight;
        Size _itemPadding = new Size(8, 4);
        Rectangle _headerBounds;
        StringFormat _itemHeaderStringFormat = new StringFormat();

        #endregion

        #region 构造

        public SEListViewStandardRenderer(SEListViewLayoutManager layoutManager)
            : base(layoutManager)
        {
            layoutManager.ItemSize = 24;

            //_itemHeaderStringFormat.Alignment = StringAlignment.Center;
            _itemHeaderStringFormat.FormatFlags = StringFormatFlags.LineLimit| StringFormatFlags.NoWrap;
        }

        #endregion

        #region 受保护的方法

        internal override void DrawForeground(Graphics g)
        {

        }

        internal override void DrawItemContent(Graphics g, Rectangle bounds, SEListViewItem item)
        {
            if (_headerHeightInited == false)
            {
                SizeF headerSize = g.MeasureString(item.Header, Theme.ItemHeaderFont);
                _headerHeight = (int)Math.Ceiling(headerSize.Height);

                _headerHeightInited = true;
            }

            #region 绘制文本

            _headerBounds = new Rectangle();
            _headerBounds.X = _itemPadding.Width;
            _headerBounds.Y = _itemPadding.Height;
            _headerBounds.Width = bounds.Width;
            _headerBounds.Height = _headerHeight;
            _headerBounds.Offset(bounds.Location);

            if (String.IsNullOrEmpty(item.Header) == false)
            {
                using (SolidBrush brush = new SolidBrush(Theme.ItemHeaderColor))
                {
                    g.DrawString(item.Header, Theme.ItemHeaderFont, brush, _headerBounds, _itemHeaderStringFormat);
                }
            }

            #endregion
        }

        #endregion
    }
}
