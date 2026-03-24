using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;

namespace WinXound_Net
{
    [ToolboxBitmap(typeof(wxDraggableTabControl))]
    /// <summary>
    /// Summary description for DraggableTabPage.
    /// </summary>
    public class wxDraggableTabControl : System.Windows.Forms.TabControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public wxDraggableTabControl()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            // TODO: Add any initialization after the InitForm call
            AllowDrop = true;
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            //this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);

        }



        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // wxDraggableTabControl
            // 
            this.DragOver += new System.Windows.Forms.DragEventHandler(this.wxDraggableTabControl_DragOver);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.wxDraggableTabControl_MouseMove);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.wxDraggableTabControl_DragDrop);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.wxDraggableTabControl_MouseDown);
            this.ResumeLayout(false);

        }
        #endregion




        private void wxDraggableTabControl_MouseDown(object sender, MouseEventArgs e)
        {
            //NOTHING TO DO
        }

        private void wxDraggableTabControl_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.Button == MouseButtons.Left)
                {
                    Point pt = new Point(e.X, e.Y);
                    TabPage tp = GetTabPageByTab(pt);

                    if (tp != null)
                    {
                        DoDragDrop(tp, DragDropEffects.Move);
                    }
                }
            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message,
                    "Form DraggableTabControl - wxDraggableTabControl_MouseMove");
            }
        }

        private void wxDraggableTabControl_DragOver(object sender, DragEventArgs e)
        {
            try
            {
                Point pt = new Point(e.X, e.Y);
                pt = PointToClient(pt);

                //Get the tab we are hovering over.
                TabPage hover_tab = GetTabPageByTab(pt);

                //Make sure we are on a tab.
                if (hover_tab != null)
                {
                    //Make sure there is a TabPage being dragged.
                    if (e.Data.GetDataPresent(typeof(TabPage)))
                    {
                        e.Effect = DragDropEffects.Move;
                    }
                }
                else
                {
                    e.Effect = DragDropEffects.None;
                }
            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message,
                    "Form DraggableTabControl - wxDraggableTabControl_DragOver");
            }
        }

        private void wxDraggableTabControl_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                Point pt = new Point(e.X, e.Y);
                pt = PointToClient(pt);          //NB!!!

                //Get the tab we are hovering over.
                TabPage hover_tab = GetTabPageByTab(pt);

                //Make sure we are on a tab.
                if (hover_tab != null)
                {
                    //Make sure there is a TabPage being dragged.
                    if (e.Data.GetDataPresent(typeof(TabPage)))
                    {
                        e.Effect = DragDropEffects.Move;
                        TabPage drag_tab = (TabPage)e.Data.GetData(typeof(TabPage));

                        int item_drag_index = FindIndex(drag_tab);
                        int drop_location_index = FindIndex(hover_tab);

                        //Don't do anything if we are hovering over ourself.
                        if (item_drag_index != drop_location_index)
                        {
                            ArrayList pages = new ArrayList();

                            //Put all tab pages into an array.
                            for (int i = 0; i < TabPages.Count; i++)
                            {
                                //Except the one we are dragging.
                                if (i != item_drag_index)
                                    pages.Add(TabPages[i]);
                            }


                            ////Now put the one we are dragging it at the proper location.
                            //pages.Insert(drop_location_index, drag_tab);
                            ////Make them all go away for a nanosec.
                            //TabPages.Clear();
                            ////Add them all back in.
                            //TabPages.AddRange((TabPage[])pages.ToArray(typeof(TabPage)));

                            //MODDED
                            TabPages.Remove(drag_tab);
                            TabPages.Insert(drop_location_index, drag_tab);

                            //Make sure the drag tab is selected.
                            SelectedTab = drag_tab;

                        }
                    }
                }
                else
                {
                    e.Effect = DragDropEffects.None;
                }
            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message,
                    "Form DraggableTabControl - wxDraggableTabControl_DragDrop");
            }
        }







        //protected override void OnDragOver(System.Windows.Forms.DragEventArgs e)
        //{
        //    base.OnDragOver(e);

        //    Point pt = new Point(e.X, e.Y);
        //    //We need client coordinates.
        //    pt = PointToClient(pt);

        //    //Get the tab we are hovering over.
        //    TabPage hover_tab = GetTabPageByTab(pt);

        //    //Make sure we are on a tab.
        //    if (hover_tab != null)
        //    {
        //        //Make sure there is a TabPage being dragged.
        //        if (e.Data.GetDataPresent(typeof(TabPage)))
        //        {
        //            e.Effect = DragDropEffects.Move;
        //            TabPage drag_tab = (TabPage)e.Data.GetData(typeof(TabPage));

        //            int item_drag_index = FindIndex(drag_tab);
        //            int drop_location_index = FindIndex(hover_tab);

        //            //Don't do anything if we are hovering over ourself.
        //            if (item_drag_index != drop_location_index)
        //            {
        //                ArrayList pages = new ArrayList();

        //                //Put all tab pages into an array.
        //                for (int i = 0; i < TabPages.Count; i++)
        //                {
        //                    //Except the one we are dragging.
        //                    if (i != item_drag_index)
        //                        pages.Add(TabPages[i]);
        //                }


        //                //Now put the one we are dragging it at the proper location.
        //                pages.Insert(drop_location_index, drag_tab);

        //                //Make them all go away for a nanosec.
        //                TabPages.Clear();

        //                //Add them all back in.
        //                TabPages.AddRange((TabPage[])pages.ToArray(typeof(TabPage)));

        //                ////PROVA
        //                //TabPages.Remove(drag_tab);
        //                //TabPages.Insert(drop_location_index, drag_tab);

        //                //Make sure the drag tab is selected.
        //                SelectedTab = drag_tab;


        //            }
        //        }
        //    }
        //    else
        //    {
        //        e.Effect = DragDropEffects.None;
        //    }
        //}

        //protected override void OnMouseDown(MouseEventArgs e)
        //{
        //    base.OnMouseDown(e);

        //    Point pt = new Point(e.X, e.Y);
        //    TabPage tp = GetTabPageByTab(pt);

        //    if (tp != null)
        //    {
        //        DoDragDrop(tp, DragDropEffects.All);
        //    }
        //}



        /// <summary>
        /// Finds the TabPage whose tab is contains the given point.
        /// </summary>
        /// <param name="pt">The point (given in client coordinates) to look for a TabPage.</param>
        /// <returns>The TabPage whose tab is at the given point (null if there isn't one).</returns>
        private TabPage GetTabPageByTab(Point pt)
        {
            TabPage tp = null;

            try
            {
                for (int i = 0; i < TabPages.Count; i++)
                {
                    if (GetTabRect(i).Contains(pt))
                    {
                        tp = TabPages[i];
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message,
                    "Form DraggableTabControl - wxDraggableTabControl_DragDrop");
            }

            return tp;
        }

        /// <summary>
        /// Loops over all the TabPages to find the index of the given TabPage.
        /// </summary>
        /// <param name="page">The TabPage we want the index for.</param>
        /// <returns>The index of the given TabPage(-1 if it isn't found.)</returns>
        private int FindIndex(TabPage page)
        {
            for (int i = 0; i < TabPages.Count; i++)
            {
                if (TabPages[i] == page)
                    return i;
            }

            return -1;
        }






    }
}


