using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;
using System.Windows;

namespace xy2
{
    public partial class Form1 : Form
    {
        public Form1(string[] args)
        {
            this.args = args;
            InitializeComponent();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (PanelGraphicArea.Width < 3 * borderW || PanelGraphicArea.Height < 3 * borderH)
                return;

            gWidth = PanelGraphicArea.Width;
            gHeight = PanelGraphicArea.Height;

            uWidth = PanelGraphicArea.Width - 2 * borderW;
            uHeight = PanelGraphicArea.Height - 2 * borderH;

            PanelGraphicArea.Invalidate();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            context = BufferedGraphicsManager.Current;
            context.MaximumBuffer = new Size(PanelGraphicArea.Width + 1, PanelGraphicArea.Height + 1);
            buf = context.Allocate(PanelGraphicArea.CreateGraphics(),
                new Rectangle(0, 0, PanelGraphicArea.Width, PanelGraphicArea.Height));

            buf.Graphics.SmoothingMode = SmoothingMode.None;
            
            InitColors();

            brushDefault = new SolidBrush(PanelGraphicArea.BackColor);
            
            cbox_linecolor.DataSource = xycolors;
            cbox_ptcolor.DataSource = xycolors.Clone();
            cbox_typept.DataSource = arr;

            cbox_typept.DropDownWidth = 5;
            

            gWidth = PanelGraphicArea.Width;
            gHeight = PanelGraphicArea.Height;
            
            uWidth = PanelGraphicArea.Width - 2 * borderW;
            uHeight = PanelGraphicArea.Height - 2 * borderH;

            offsetX = borderW;
            offsetY = borderH;

            if (args.Length > 0)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    xyGraphs.Add(new Graph(args[i]));
                }
            }
            else 
                xyGraphs.Add(new Graph(10000));
           



            //XYval[] a1 = new XYval[]{ new XYval(-30, -1000), new XYval(30, -1001)};
            //xyGraphs.Add(new Graph("3.1"));
            //xyGraphs.Add(new Graph("3"));
            //xyGraphs.Add(new Graph(100));
            //xyGraphs.Add(new Graph("3"));
            //xyGraphs.Add(new Graph("3"));
            //xyGraphs.Add(new Graph(a1));
            //this.Size = new Size(1024, 768);

            cbox_files.DataSource = listfn;

            cbox_files.SelectedIndex = -1;

            //Rectangle monitorScreenSize = Screen.PrimaryScreen.Bounds;
            //this.Size = new Size(monitorScreenSize.Width / 2, monitorScreenSize.Width / 3 );
            //this.Location = new Point(monitorScreenSize.Width / 2 - this.Size.Width / 2, 0); //monitorScreenSize.Height);
        }

        private void PanelGraphicArea_Paint(object sender, PaintEventArgs e)
        {
            context = BufferedGraphicsManager.Current;
            
            context.MaximumBuffer = new Size(PanelGraphicArea.Width + 1, PanelGraphicArea.Height + 1);
            buf = context.Allocate(PanelGraphicArea.CreateGraphics(),
                new Rectangle(0, 0, PanelGraphicArea.Width, PanelGraphicArea.Height));

            buf.Graphics.Clear(backPanelColor);

            RedrawAll();

        }
        
        private void PanelGraphicArea_MouseDown(object sender, MouseEventArgs e)
        {

            if (e.Button != MouseButtons.Left)
                return;

            int x = e.X;
            int y = e.Y;

            xyGraphs.UnselectAll();

            if (isIdentifyMode)
            {
                int i = FindLineObject(x, y);
                if (i >= 0)
                {
                    cbox_files.SelectedIndex = i;
                    cbox_files_DropDownClosed(sender, e);
                    xyGraphs.SelectOneGraph(i);
                }
            }

            if (x < offsetX) x = offsetX;
            if (x > offsetX + uWidth) x = offsetX + uWidth;
            if (y < offsetY) y = offsetY;
            if (y > offsetY + uHeight) y = offsetY + uHeight;

            mouse_down_X = x;
            mouse_down_Y = y;

            RedrawAll();

        }

        private void PanelGraphicArea_MouseUp(object sender, MouseEventArgs e)
        {

            if (!isMagni) return;

            if (zoomRect.Width < 5 && zoomRect.Height < 5) return;
            //if (getZoomMaxMinDeltaAbsKt(zoomRect) != 0) return;

            if (isZoomProporc)
            {
                int new_a = zoomRect.Width * uHeight / uWidth;

                if (new_a < zoomRect.Height)
                {
                    new_a = uWidth * zoomRect.Height / uHeight;
                    zoomRect.X = zoomRect.X - (new_a - zoomRect.Width) / 2;
                    zoomRect.Width = new_a;
                }
                else
                {
                    zoomRect.Y = zoomRect.Y - (new_a - zoomRect.Height) / 2;
                    zoomRect.Height = new_a;
                }
            }

            if (getZoomMaxMinDeltaAbsKt(zoomRect) != 0) return;

            buf.Graphics.Clear(backPanelColor);
            xyGraphs.DrawAll(buf, isZoomed);
            buf.Render();

            isMagni = false;
            button_magn.BackColor = SystemColors.Control;
        }

        private void PanelGraphicArea_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
            {
                string[] fns = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string fn in fns)
                {
                    xyGraphs.Add(new Graph(fn));
                }

                RefreshDataSource();


                isMagni = false;
                isZoomed = false;
                //isDrawGlobRect = false;
                //isCrossXY = false;
                //isIdentifyMode = false;

                //button_cross.BackColor = SystemColors.Control;
                //button_globe_rect.BackColor = SystemColors.Control;
                //button_identify.BackColor = SystemColors.Control;
                button_magn.BackColor = SystemColors.Control;

                RedrawAll();
            }
        }

        private void PanelGraphicArea_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Move | DragDropEffects.Copy | DragDropEffects.Scroll;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private bool isZoomProporc = false;
        
        private void PanelGraphicArea_MouseMove(object sender, MouseEventArgs e)
        {

            if (!isCrossXY && !isMagni)
            {
                return;
            }

            int x = e.X;
            int y = e.Y;

            if (x < offsetX) x = offsetX;
            if (x > offsetX + uWidth) x = offsetX + uWidth;
            if (y < offsetY) y = offsetY;
            if (y > offsetY + uHeight) y = offsetY + uHeight;

            RedrawAll();

            buf.Graphics.DrawLine(new Pen(Color.Gray), offsetX, y, offsetX + uWidth, y);
            buf.Graphics.DrawLine(new Pen(Color.Gray), x, offsetY, x, offsetY + uHeight);

            double x0 = xyGraphs.x00;
            double y0 = xyGraphs.y00;

            //double xv = ((double)e.X - x0) / Kt.x;
            //double yv = ((double)gHeight - ((double)e.Y + y0)) / Kt.y;

            double xv = ((double)x - x0) / Kt.x;
            double yv = ((double)gHeight - ((double)y + y0)) / Kt.y;


            string sxv = "x= " + xv.ToString(), syv = "y= " + yv.ToString();

            /*
            if (e.X < offsetX || e.X > offsetX + uWidth) 
                sxv = "x= NA";
            if (e.Y < offsetY || e.Y > offsetY + uHeight) 
                syv = "y= NA";
            */

            float h = buf.Graphics.MeasureString(sxv + "\n" + syv, new Font("Verdana", 8)).Height;
            float w = buf.Graphics.MeasureString(sxv + "\n" + syv, new Font("Verdana", 8)).Width;

            
            if(e.X > gWidth - (int)(w * 1.2) )
                buf.Graphics.DrawString(sxv + "\n" + syv, new Font("Verdana", 8), Brushes.Black, x - w, y);
            else
                buf.Graphics.DrawString(sxv + "\n" + syv, new Font("Verdana", 8), Brushes.Black, x, y - h);


            if (e.Button == MouseButtons.Left && isMagni)
            {
                zoomRect.X = (mouse_down_X > x) ? x : mouse_down_X;
                zoomRect.Y = (mouse_down_Y > y) ? y : mouse_down_Y;

                zoomRect.Width = Math.Abs(x - mouse_down_X);
                zoomRect.Height = Math.Abs(y - mouse_down_Y);

                buf.Graphics.DrawRectangle(new Pen(Color.Blue), zoomRect);
            }

            buf.Render();
        }

        private void button_clipboard_Click(object sender, EventArgs e)
        {
            Bitmap b = new Bitmap(PanelGraphicArea.Width, PanelGraphicArea.Height);
            Graphics gr = Graphics.FromImage(b);
            buf.Render(gr);
            Clipboard.SetImage(b);
        }
        
        private void button_removeAll_Click(object sender, EventArgs e)
        {
            listfn.Clear();
            xyGraphs.RemoveAll();
            isMagni = false;
            isZoomed = false;
            isDrawGlobRect = false;
            isCrossXY = false;
            isIdentifyMode = false;

            button_cross.BackColor = SystemColors.Control;
            button_globe_rect.BackColor = SystemColors.Control;
            button_identify.BackColor = SystemColors.Control;
            button_magn.BackColor = SystemColors.Control;

            RefreshDataSource();
            PanelGraphicArea.Invalidate();
        }
        
        private void button_remove_Click(object sender, EventArgs e)
        {
            int selidx = cbox_files.SelectedIndex;

            if (selidx >= 0)
            {
                xyGraphs.RemoveGgraph(selidx);
                cbox_files.SelectedIndex = -1;
            }

            //isIdentifyMode = false;
            //button_identify.BackColor = SystemColors.Control;
            
            if (listfn.Count == 0)
            {
                isMagni = false;
                isZoomed = false;
                isDrawGlobRect = false;
                isCrossXY = false;

                button_cross.BackColor = SystemColors.Control;
                button_globe_rect.BackColor = SystemColors.Control;
               
                button_magn.BackColor = SystemColors.Control;
            }

            RefreshDataSource();
            PanelGraphicArea.Invalidate();
        }

        private void button_cross_Click(object sender, EventArgs e)
        {
            
            if((xyGraphs.getListxy()).Count == 0) return;
            isCrossXY = !isCrossXY;

            button_cross.ForeColor = (isCrossXY) ? Color.Red : Color.Black;

            if (!isCrossXY)
                RedrawAll();
        }

        private void button_magn_Click(object sender, EventArgs e)
        {

            if((xyGraphs.getListxy()).Count == 0) return;
            isMagni = !isMagni;

            if (isMagni)
            {
                isZoomed = true;
                button_magn.BackColor = Color.LightCyan;
                if (isIdentifyMode)
                    button_identify_Click(sender, e);

            }
            else
                button_magn.BackColor = SystemColors.Control;

            RedrawAll();

        }

        private void button_magn_propor_Click(object sender, EventArgs e)
        {
            isZoomProporc = !isZoomProporc;
            button_magn_propor.BackColor = (isZoomProporc) ? Color.LightCyan : SystemColors.Control;
        }


        private void button_showall_Click(object sender, EventArgs e)
        {
            isMagni = false;
            isZoomed = false;
            RedrawAll();
        }

        private void button_reset_Click(object sender, EventArgs e)
        {
            if ((xyGraphs.getListxy()).Count == 0) return;
            
            List<Graph> xylist = xyGraphs.getListxy();

            int sel = -1;

            for (int i = 0; i < xylist.Count; i++)
            {
                int col = 15;

                if (i == 0) col = 15;
                else if (i == 5) col = 17;
                else if (i == 13) col = 8;
                else col = i;

                xylist[i].colorline = col;
                xylist[i].colorpoint = col;

                xylist[i].lineth = 1;
                xylist[i].typepoint = 0;
                xylist[i].ptsize = 2;

                if (xylist[i].isSelected)
                    sel = i;

            }

            cbox_files.SelectedIndex = sel;
            cbox_files_DropDownClosed(sender, e);

            RedrawAll();

        }

        private void button_identify_Click(object sender, EventArgs e)
        {
            if ((xyGraphs.getListxy()).Count == 0) return;
            isIdentifyMode = !isIdentifyMode;

            if (isIdentifyMode)
            {
                button_identify.BackColor = Color.LightCyan;
                if (isMagni)
                    button_magn_Click(sender, e);
            }
            else
            {
                button_identify.BackColor = SystemColors.Control;
            }

            RedrawAll();

        }

        private void button_globe_rect_Click(object sender, EventArgs e)
        {
            if ((xyGraphs.getListxy()).Count == 0) return;
            
            isDrawGlobRect = !isDrawGlobRect;

            if (isDrawGlobRect)
            { 
                button_globe_rect.BackColor = Color.LightCyan;
            }
            else
                button_globe_rect.BackColor = SystemColors.Control;
            
            RedrawAll();
        }

        private void cbox_files_DropDownClosed(object sender, EventArgs e)
        {
            int i = cbox_files.SelectedIndex;
            
            if (i < 0 || i > listfn.Count - 1)
            {
                PanelGraphicArea.Focus();
                return;
            }

            Graph g = xyGraphs.getXYGgraph(i);
            xyGraphs.SelectOneGraph(i);

            cbox_linecolor.SelectedIndex = g.colorline;
            cbox_ptcolor.SelectedIndex = g.colorpoint;
            cbox_typept.SelectedIndex = g.typepoint;

            numUpDown_ptsize.Value = g.ptsize;

            numUpDown_lineth.Value = g.lineth;

            label1.Text = g.comments[0];
            label2.Text = g.comments[1];
            label3.Text = g.comments[2];
            label4.Text = g.comments[3];

            RedrawAll();

            PanelGraphicArea.Focus();
        }

        private void cbox_files_SelectedIndexChanged(object sender, EventArgs e)
        {
            

            int selidx = cbox_files.SelectedIndex;
            if (selidx >= 0)
            {
                Graph xy = xyGraphs.getXYGgraph(selidx);

                label1.Text = xy.comments[0];
                label2.Text = xy.comments[1];
                label3.Text = xy.comments[2];
                label4.Text = xy.comments[3];

                label5.Text = "N= " + xy.n.ToString() + " " + "(xmin, xmax= " + xy.min.x.ToString() + ", " + xy.max.x.ToString() +
                    ") (ymin, ymax= " + xy.min.y.ToString() + ", " + xy.max.y.ToString() +
                    ") (delta x,y= " + (xy.max.x - xy.min.x).ToString() + ", " + (xy.max.y - xy.min.y).ToString() + ")";

                label6.Text = "Файл= " + listfn[selidx];
            }
            else
            {
                label1.Text = "";
                label2.Text = "";
                label3.Text = "";
                label4.Text = "";

                label5.Text = "";
                label6.Text = "";
            }

            RedrawAll();

        }

        private void cbox_colors_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();

            int idx = e.Index;

            Color color = Color.FromKnownColor((KnownColor)xycolors[idx]);

            Rectangle rectangle = new Rectangle(20, e.Bounds.Top+1,
                e.Bounds.Width, e.Bounds.Height-2);
           
            e.Graphics.FillRectangle(new SolidBrush(color), rectangle);
            e.Graphics.DrawString(idx.ToString(), fontmy, Brushes.Gray, 1, e.Bounds.Y);

            e.DrawFocusRectangle();
        }

        private void cbox_colors_DropDownClosed(object sender, EventArgs e)
        {

            int i = cbox_files.SelectedIndex;
            if (i < 0)
            {
                PanelGraphicArea.Focus();
                return;
            }
            Graph g = xyGraphs.getXYGgraph(i);

            g.colorline = this.cbox_linecolor.SelectedIndex;
            g.colorpoint = this.cbox_ptcolor.SelectedIndex;
            g.typepoint = this.cbox_typept.SelectedIndex;
            
            
            //g.ptsize = (int)this.numUpDown_ptsize.Value;
            //g.lineth = (int)this.numUpDown_lineth.Value;

            PanelGraphicArea.Focus();

            RedrawAll();
            
            
        }

        private void cbox_typept_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();

            int i = e.Index;

            e.Graphics.DrawString(arr[i], fontmy, Brushes.Gray,
                new Point(2, e.Bounds.Y));

            e.Graphics.DrawImage(imageArr[i], new Point(e.Bounds.X+18, e.Bounds.Y));

            if ((e.State & DrawItemState.Focus) == 0)
            {
                //this code keeps the last item drawn from having a Bisque background. 

               //e.Graphics.FillRectangle(Brushes.White, e.Bounds);
                e.Graphics.DrawString(arr[i], fontmy, Brushes.Gray,
                new Point(2, e.Bounds.Y));

                e.Graphics.DrawImage(imageArr[i], new Point(e.Bounds.X + 18, e.Bounds.Y));
            }

            e.DrawFocusRectangle();

        }

        private void numUpDown_lineth_ValueChanged(object sender, EventArgs e)
        {
            int i = cbox_files.SelectedIndex;

            if (i < 0)
            {
                PanelGraphicArea.Focus();
                return;
            }

            Graph g = xyGraphs.getXYGgraph(i);
            g.lineth = (int)this.numUpDown_lineth.Value;
            PanelGraphicArea.Focus();
            RedrawAll();
        }

        private void numUpDown_ptsize_ValueChanged(object sender, EventArgs e)
        {
            int i = cbox_files.SelectedIndex;

            if (i < 0)
            {
                PanelGraphicArea.Focus();
                return;
            }

            Graph g = xyGraphs.getXYGgraph(i);
            g.ptsize = (int)this.numUpDown_ptsize.Value;
            PanelGraphicArea.Focus();
            RedrawAll();
        }
        
        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form help = new Form1_help();
            help.Show();
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
            //Application.Exit();
        }

        private void button_addfiles_Click(object sender, EventArgs e)
        {
            string[] ofd_fn = OpenFromFile();

            if (ofd_fn != null && ofd_fn.Length > 0)
            {
                foreach (string str in ofd_fn)
                {
                    xyGraphs.Add(new Graph(str));
                }

                RefreshDataSource();
                RedrawAll();
            }
        }

    }
}

