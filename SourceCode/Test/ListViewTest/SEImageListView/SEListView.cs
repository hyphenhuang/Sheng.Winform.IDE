﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Drawing;
using System.Diagnostics;

namespace ListViewTest
{
    public class SEListView : Control
    {
        #region 常量

        /// <summary>
        /// Creates a control with a border.
        /// </summary>
        private const int WS_BORDER = 0x00800000;
        /// <summary>
        /// Specifies that the control has a border with a sunken edge.
        /// </summary>
        private const int WS_EX_CLIENTEDGE = 0x00000200;

        #endregion

        #region 私有成员

        private bool _suspendLayout = false;
        public bool Suspend
        {
            get { return _suspendLayout; }
        }

        /// <summary>
        /// 是否需要在调用 ResumeLayout 时重绘
        /// </summary>
        private bool _needPaint = false;


        private SEListViewLayoutManager _layoutManager;


        //ToolTip _toolTip = new ToolTip();

        //private System.Timers.Timer lazyRefreshTimer;

        #endregion

        #region 公开属性

        /// <summary>
        /// Gets whether the shift key is down.
        /// </summary>
        internal bool ShiftKey { get; private set; }
        /// <summary>
        /// Gets whether the control key is down.
        /// </summary>
        internal bool ControlKey { get; private set; }

        /// <summary>
        /// 鼠标左键是否处于按下状态
        /// </summary>
        internal bool LeftButton { get; private set; }

        /// <summary>
        /// 鼠标右键是否处于按下状态
        /// </summary>
        internal bool RightButton { get; private set; }

        internal bool AnyMouseButton
        {
            get { return LeftButton || RightButton; }
        }

        private ListViewLayoutMode _layoutMode;
        public ListViewLayoutMode LayoutMode
        {
            get { return _layoutMode; }
            set
            {
                _layoutMode = value;
                SEListViewLayoutManager layoutManager;
                switch (_layoutMode)
                {
                    case ListViewLayoutMode.Standard:
                        layoutManager = new SEListViewStandardLayoutManager(this);
                        break;
                    case ListViewLayoutMode.Descriptive:
                        layoutManager = new SEListViewDescriptiveLayoutManager(this);
                        break;
                    default:
                        layoutManager = new SEListViewLayoutManager(this);
                        Debug.Assert(false, "没这ListViewRenderer");
                        break;
                }

                SetLayoutManager(layoutManager);
            }
        }

        //debug public
        /// <summary>
        /// 鼠标最后点击的位置
        /// </summary>
        public Point LastMouseDownLocation { get; private set; }

        private SEListViewItem _hoveredItem;
        /// <summary>
        /// 当前鼠标经过的项
        /// </summary>
        internal SEListViewItem HoveredItem
        {
            get { return _hoveredItem; }
            private set
            {
                SEListViewItem oldHoveredItem = _hoveredItem;
                SEListViewItem newHoveredItem = value;

                _hoveredItem = value;

                if (oldHoveredItem != null && oldHoveredItem != newHoveredItem)
                {
                    oldHoveredItem.Hovered = false;
                }

                if (newHoveredItem != null)
                    newHoveredItem.Hovered = true;

                if (oldHoveredItem != newHoveredItem)
                {
                    NeedPaint();
                }
            }
        }

        private BorderStyle _borderStyle = BorderStyle.Fixed3D;
        public BorderStyle BorderStyle
        {
            get { return _borderStyle; }
            set { _borderStyle = value; }
        }

        private SEListViewTheme _theme = new SEListViewTheme();
        /// <summary>
        /// 配色方案
        /// </summary>
        public SEListViewTheme Theme
        {
            get
            {
                return _theme;
            }
            set
            {
                _theme = value;
                Refresh();
            }
        }

        private bool _allowMultiSelection = false;
        public bool AllowMultiSelection
        {
            get { return _allowMultiSelection; }
            set { _allowMultiSelection = value; }
        }

        /// <summary>
        /// 是否没有任何项
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return Items.Count == 0;
            }
        }

        private SEListViewItemCollection _items = new SEListViewItemCollection();
        public SEListViewItemCollection Items
        {
            get { return _items; }
            set { _items = value; }
        }

        /// <summary>
        /// 获取当前具有输入焦点的项
        /// </summary>
        public SEListViewItem FocusedItem
        {
            get
            {
                foreach (var item in _items)
                {
                    if (item.Focused)
                        return item;
                }

                return null;
            }
        }


        #endregion

        #region 构造

        public SEListView()
        {
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.Selectable, true);

            this.Padding = new System.Windows.Forms.Padding(10);

            _items.Owner = this;

            //设置一个默认布局
            this.LayoutMode = ListViewLayoutMode.Standard;

            //_layoutManager = new ListViewLayoutManager(this);

            //lazyRefreshTimer = new System.Timers.Timer();
            //lazyRefreshTimer.Interval = 10;
            //lazyRefreshTimer.Enabled = false;
            //lazyRefreshTimer.Elapsed += lazyRefreshTimer_Tick;
            //lazyRefreshCallback = new RefreshDelegateInternal(Refresh);
        }

        //delegate void RefreshDelegateInternal();
        //private RefreshDelegateInternal lazyRefreshCallback;
        //void lazyRefreshTimer_Tick(object sender, EventArgs e)
        //{
        //    if (IsHandleCreated && IsDisposed == false)
        //        BeginInvoke(lazyRefreshCallback);
        //    lazyRefreshTimer.Stop();
        //}

        #endregion

        #region 公开方法

        #region internal

        /// <summary>
        /// 请求在下次调用 ResumeLayout 时重绘
        /// </summary>
        internal void NeedPaint()
        {
            _needPaint = true;
        }

        internal void RenderItem(SEListViewItem item)
        {
            if (Suspend == false)
            {
                _layoutManager.RenderItem(this.CreateGraphics(), item);
            }
        }

        //internal void Refresh(bool lazyRefresh)
        //{
        //    if (lazyRefresh)
        //    {
        //        lazyRefreshTimer.Stop();
        //        lazyRefreshTimer.Start();
        //    }
        //    else
        //    {
        //        Refresh();
        //    }
        //}

        #endregion

        public void SetRenderer(SEListViewRenderer renderer)
        {

        }

        /// <summary>
        /// 恢复正常的布局逻辑。
        /// </summary>
        public new void ResumeLayout()
        {
            _suspendLayout = false;

            if (_needPaint)
            {
                this.Refresh();
                _needPaint = false;
            }

            base.ResumeLayout();
        }

        public new void ResumeLayout(bool refreshNow)
        {
            _suspendLayout = false;

            if (refreshNow)
            {
                this.Refresh();
                _needPaint = false;
            }
            else
            {
                ResumeLayout();
            }

            base.ResumeLayout(refreshNow);
        }

        /// <summary>
        /// 临时挂起控件的布局逻辑。
        /// </summary>
        public new void SuspendLayout()
        {
            _suspendLayout = true;

            base.SuspendLayout();
        }

        public override void Refresh()
        {
            if (_suspendLayout)
                return;

            base.Refresh();
        }

        /// <summary>
        /// 获取当前选中的所有项
        /// </summary>
        /// <returns></returns>
        public List<SEListViewItem> GetSelectedItems()
        {
            List<SEListViewItem> items = new List<SEListViewItem>();

            foreach (var item in _items)
            {
                if (item.Selected)
                    items.Add(item);
            }

            return items;
        }

        /// <summary>
        /// 取消所有项的选择
        /// </summary>
        public void ClearSelect()
        {
            bool suspend = false;
            if (this.Suspend == false)
            {
                this.SuspendLayout();
                suspend = true;
            }

            foreach (var selectedItem in GetSelectedItems())
            {
                selectedItem.Selected = false;
            }

            if (suspend)
                this.ResumeLayout();
        }

        /// <summary>
        /// 更改了选择的项
        /// </summary>
        public void OnSelectedItemChanged()
        {
            if (SelectedItemChanaged != null)
            {
                SelectedItemChanaged(this, new EventArgs());
            }
        }

        /// <summary>
        /// 双击项
        /// </summary>
        /// <param name="item"></param>
        public void OnItemDoubleClick(SEListViewItem item)
        {
            if (ItemDoubleClick != null)
            {
                ItemDoubleClick(this, new SEListViewItemDoubleClickEventArgs(item));
            }
        }

        public void OnItemsRemoved(List<SEListViewItem> items)
        {
            _layoutManager.OnItemsRemoved(items);

            if (ItemsRemoved != null)
            {
                ItemsRemoved(this, new SEListViewItemsRemovedEventArgs(items));
            }
        }

        #endregion

        #region 私有方法

        private void Hover(Point location)
        {
            SEListViewHitInfo hitInfo = _layoutManager.HitTest(location);
            if (hitInfo.ItemHit)
            {
                HoveredItem = Items[hitInfo.ItemIndex];
            }
            else
            {
                HoveredItem = null;
            }
        }

        private void SetLayoutManager(SEListViewLayoutManager layoutManager)
        {
            if (_layoutManager == layoutManager)
                return;

            if (_layoutManager != null)
                _layoutManager.Dispose();

            _layoutManager = layoutManager;

            Refresh();
        }

        #endregion

        #region 重写的方法

        /// <summary>
        /// 获取创建控件句柄时所需要的创建参数
        /// </summary>
        protected override CreateParams CreateParams
        {
            get
            {
                //设置控件的边框样式
                CreateParams p = base.CreateParams;
                p.Style &= ~WS_BORDER;
                p.ExStyle &= ~WS_EX_CLIENTEDGE;
                if (_borderStyle == BorderStyle.Fixed3D)
                    p.ExStyle |= WS_EX_CLIENTEDGE;
                else if (_borderStyle == BorderStyle.FixedSingle)
                    p.Style |= WS_BORDER;
                return p;
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            //_layoutManager.Update();
        }

        #region Mouse

        protected override void OnMouseDown(MouseEventArgs e)
        {
            SuspendLayout();

            if (Focused == false)
                Focus();

            LeftButton = (e.Button & MouseButtons.Left) == MouseButtons.Left;
            RightButton = (e.Button & MouseButtons.Right) == MouseButtons.Right;

            LastMouseDownLocation = e.Location;

            _layoutManager.MouseDown(e);

            ResumeLayout();

            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
                LeftButton = false;
            if ((e.Button & MouseButtons.Right) == MouseButtons.Right)
                RightButton = false;

            SuspendLayout();

            _layoutManager.MouseUp(e);

            ResumeLayout();

            //显示上下文菜单
            bool rightButton = (e.Button & MouseButtons.Right) == MouseButtons.Right;
            if (rightButton && this.ContextMenuStrip != null)
            {
                this.ContextMenuStrip.Show(this.PointToScreen(e.Location));
            }

            base.OnMouseUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
          //  if (_toolTip.Active)
            //    _toolTip.Hide(this);

            SuspendLayout();

            //如果处于框选状态，不处理Hover
            if (_layoutManager.MouseSelecting == false)
            {
                Hover(e.Location);
            }

            _layoutManager.MouseMove(e);

            ResumeLayout();

            base.OnMouseMove(e);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            SuspendLayout();

            _layoutManager.OnMouseWheel(e);

            Hover(e.Location);

            NeedPaint();
            ResumeLayout();

            base.OnMouseWheel(e);
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            if (ItemDoubleClick != null)
            {
                SEListViewHitInfo hitInfo = _layoutManager.HitTest(e.Location);
                if (hitInfo.ItemHit)
                {
                    SEListViewItem  item = Items[hitInfo.ItemIndex];
                    OnItemDoubleClick(item);
                }
            }

            base.OnMouseDoubleClick(e);
        }

        protected override void OnMouseHover(EventArgs e)
        {
            //Point toolTipPoint = this.PointToClient(Cursor.Position);
            //_toolTip.Show("ff", this, toolTipPoint);

            base.OnMouseHover(e);
        }

        #endregion

        #region Key

        protected override bool IsInputKey(Keys keyData)
        {
            if ((keyData & Keys.Left) == Keys.Left ||
               (keyData & Keys.Right) == Keys.Right ||
               (keyData & Keys.Up) == Keys.Up ||
               (keyData & Keys.Down) == Keys.Down)
                return true;
            else
                return base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            ShiftKey = (e.Modifiers & Keys.Shift) == Keys.Shift;
            ControlKey = (e.Modifiers & Keys.Control) == Keys.Control;

            _layoutManager.OnKeyDown(e);

            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            ShiftKey = (e.Modifiers & Keys.Shift) == Keys.Shift;
            ControlKey = (e.Modifiers & Keys.Control) == Keys.Control;

            _layoutManager.OnKeyUp(e);

            base.OnKeyUp(e);
        }

        #endregion

        #region Focus

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            Refresh();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            Refresh();
        }

        #endregion

        #region Paint

        protected override void OnPaint(PaintEventArgs e)
        {
            if (_layoutManager != null)
            {
                _layoutManager.Render(e.Graphics);
            }

            //Size size1 = new Size(100, 100);
            //Size size2 = new Size(50, 50);
            //e.Graphics.DrawRectangle(Pens.Black, new Rectangle(new Point(10, 10), size1));
            //size1 = Size.Add(size1, size2);
            //e.Graphics.DrawRectangle(Pens.Red, new Rectangle(new Point(10, 10), size1));
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        #endregion

        #region 事件

        /// <summary>
        /// 更改了选择的项
        /// </summary>
        public event EventHandler SelectedItemChanaged;

        /// <summary>
        /// 双击项
        /// </summary>
        public event EventHandler<SEListViewItemDoubleClickEventArgs> ItemDoubleClick;

        /// <summary>
        /// 项被删除
        /// </summary>
        public event EventHandler<SEListViewItemsRemovedEventArgs> ItemsRemoved;

        #endregion
    }
}
