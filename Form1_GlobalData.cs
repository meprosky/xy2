using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace xy2
{
    public partial class Form1
    {
        static int borderH = 60, borderW = 80;
        static double ratioMinabsDelta = 2;
        static int gWidth, gHeight, uWidth, uHeight, offsetX, offsetY;

        static private GRAPHS xyGraphs = new GRAPHS();

        static public List<string> listfn = new List<string>();

        private String[] arr =
            new String[11] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" };

        private Image[] imageArr = new Image[11]{
            new Bitmap(xy2.Properties.Resources._void),
            new Bitmap(xy2.Properties.Resources.cross),
            new Bitmap(xy2.Properties.Resources.cross2),
            new Bitmap(xy2.Properties.Resources.rect),
            new Bitmap(xy2.Properties.Resources.rect2),
            new Bitmap(xy2.Properties.Resources.romb),
            new Bitmap(xy2.Properties.Resources.romb2),
            new Bitmap(xy2.Properties.Resources.circl),
            new Bitmap(xy2.Properties.Resources.circl2),
            new Bitmap(xy2.Properties.Resources.triangle),
            new Bitmap(xy2.Properties.Resources.triangle2)};

        static public KnownColor[] xycolors = new KnownColor[32];

        private Font fontmy = new Font("Verdana", 8);
        static private Brush brushDefault;
        static public Color backPanelColor = Color.White;


        static XYval globalMax = new XYval(), globalMin = new XYval();
        static XYval deltaMinMax = new XYval();
        static XYval globalMaxAbs = new XYval(), globalMinAbs = new XYval();
        static XYval Kt = new XYval();

        public BufferedGraphicsContext context;
        public BufferedGraphics buf;

        private string[] args;

        Rectangle zoomRect = new Rectangle();

        static private 
            bool isDrawGlobRect = false;
        bool isMagni = false;
        bool isZoomed = false;
        bool isIdentifyMode = false;
        bool isCrossXY = false;
        
        int mouse_down_X = 0;
        int mouse_down_Y = 0;

        //int id_x = 0, id_y = 0;

        private void InitColors()
        {
            xycolors[0] = KnownColor.White;

            xycolors[1] = KnownColor.Red;
            xycolors[2] = KnownColor.Orange;
            xycolors[3] = KnownColor.Blue;
            xycolors[4] = KnownColor.Green;
            xycolors[5] = KnownColor.Yellow;
            xycolors[6] = KnownColor.DarkBlue;
            xycolors[7] = KnownColor.Violet;

            xycolors[8] = KnownColor.Gray;
            xycolors[9] = KnownColor.Brown;
            xycolors[10] = KnownColor.Cyan;
            xycolors[11] = KnownColor.Magenta;
            xycolors[12] = KnownColor.CornflowerBlue;
            xycolors[13] = KnownColor.Cornsilk;
            xycolors[14] = KnownColor.DarkCyan;

            xycolors[15] = KnownColor.Black;

            xycolors[16] = KnownColor.DarkOrange;
            xycolors[17] = KnownColor.Gold;
            xycolors[18] = KnownColor.DarkGreen;
            xycolors[19] = KnownColor.LightBlue;
            xycolors[20] = KnownColor.DeepSkyBlue;
            xycolors[21] = KnownColor.DarkRed;
            xycolors[22] = KnownColor.DeepPink;
            xycolors[23] = KnownColor.LightCoral;
            xycolors[24] = KnownColor.Olive;
            xycolors[25] = KnownColor.OliveDrab;
            xycolors[26] = KnownColor.YellowGreen;
            xycolors[27] = KnownColor.DimGray;
            xycolors[28] = KnownColor.DarkSlateGray;
            xycolors[29] = KnownColor.SkyBlue;
            xycolors[30] = KnownColor.DarkGoldenrod;
            xycolors[31] = KnownColor.Coral;
        }

        private int getZoomMaxMinDeltaAbsKt(Rectangle zoomRectangle)
        {
            double x0 = xyGraphs.x00;
            double y0 = xyGraphs.y00;

            XYval zMin = new XYval(), zMax = new XYval(), zdeltaMinMax = new XYval(), zMaxAbs = new XYval(), zMinAbs = new XYval(), zKt = new XYval();

            zMin.x = ((double)zoomRectangle.X - x0) / Kt.x;
            zMin.y = ((double)gHeight - ((double)zoomRectangle.Y + y0)) / Kt.y;

            zMax.x = ((double)(zoomRectangle.X + zoomRectangle.Width) - x0) / Kt.x;
            zMax.y = ((double)gHeight - ((double)(zoomRectangle.Y + zoomRectangle.Height) + y0)) / Kt.y;

            if (zMax.x < zMin.x)
            {
                double d = zMax.x;
                zMax.x = zMin.x;
                zMin.x = d;
            }

            if (zMax.y < zMin.y)
            {
                double d = zMax.y;
                zMax.y = zMin.y;
                zMin.y = d;
            }

            zdeltaMinMax.x = zMax.x - zMin.x;
            zdeltaMinMax.y = zMax.y - zMin.y;

            zMaxAbs.x = (Math.Abs(zMax.x) > Math.Abs(zMin.x)) ? Math.Abs(zMax.x) : Math.Abs(zMin.x);
            zMaxAbs.y = (Math.Abs(zMax.y) > Math.Abs(zMin.y)) ? Math.Abs(zMax.y) : Math.Abs(zMin.y);

            zMinAbs.x = (Math.Abs(zMax.x) > Math.Abs(zMin.x)) ? Math.Abs(zMin.x) : Math.Abs(zMax.x);
            zMinAbs.y = (Math.Abs(zMax.y) > Math.Abs(zMin.y)) ? Math.Abs(zMin.y) : Math.Abs(zMax.y);

            zKt.x = (double)uWidth / zdeltaMinMax.x;
            zKt.y = (double)uHeight / zdeltaMinMax.y;

            globalMax.x = zMax.x;
            globalMax.y = zMax.y;

            globalMin.x = zMin.x;
            globalMin.y = zMin.y;

            deltaMinMax.x = zdeltaMinMax.x;
            deltaMinMax.y = zdeltaMinMax.y;

            globalMaxAbs.x = zMaxAbs.x;
            globalMaxAbs.y = zMaxAbs.y;

            globalMinAbs.x = zMinAbs.x;
            globalMinAbs.y = zMinAbs.y;

            //Kt.x = zKt.x;
            //Kt.y = zKt.y;

            return 0;
        }

        public int FindLineObject(int x, int y)
        {
            List<Graph> xylist = xyGraphs.getListxy();

            //GraphicsPath gprect = new GraphicsPath();
            //gprect.AddRectangle(new Rectangle(0, 0, gWidth, gHeight));

            for (int i = 0; i < xylist.Count; i++)
            {
                GraphicsPath gp = new GraphicsPath();
                
                /*
                // старый поиск
                gp.AddLines(xylist[i].arrpt);
                if (gp.IsOutlineVisible(x, y, new Pen(Brushes.Blue, 7f)))
                {
                    return i;
                }
                */

                // новый поиск гораздо быстрее при увеличении участка графика с большим кол-вом точек
                for (int j = 0; j < xylist[i].n - 1; j++)
                {
                    int x11 = xylist[i].arrpt[j].X;
                    int y11 = xylist[i].arrpt[j].Y;
                    int x12 = xylist[i].arrpt[j+1].X;
                    int y12 = xylist[i].arrpt[j+1].Y;

                    bool b5 = (x11 > 0 && x11 < gWidth) && (y11 > 0 && y11 < gHeight);
                    bool b6 = (x12 > 0 && x12 < gWidth) && (y12 > 0 && y12 < gHeight);

                    if (b5 || b6)                                                                 gp.AddLine(x11, y11, x12, y12);
                    else if (IsLinesCross2(x11, y11, x12, y12, 0,      0,       0,      gHeight)) gp.AddLine(x11, y11, x12, y12);
                    else if (IsLinesCross2(x11, y11, x12, y12, 0,      0,       gWidth, 0))       gp.AddLine(x11, y11, x12, y12);
                    else if (IsLinesCross2(x11, y11, x12, y12, gWidth, gHeight, gWidth, 0))       gp.AddLine(x11, y11, x12, y12);
                    else if (IsLinesCross2(x11, y11, x12, y12, gWidth, gHeight, 0,      gHeight)) gp.AddLine(x11, y11, x12, y12);
                }

                if (gp.IsOutlineVisible(x, y, new Pen(Brushes.Blue, 7f)))
                {
                    return i;
                }
            }
            return -1;
        }

        private void RefreshDataSource()
        {
            int saved_selected = cbox_files.SelectedIndex;
            cbox_files.DataSource = null;
            cbox_files.DataSource = listfn;

            if (cbox_files.Items.Count > 0)
            {
                if (cbox_files.Items.Count > saved_selected) cbox_files.SelectedIndex = saved_selected;
                else cbox_files.SelectedIndex = 0;
            }
            
            

        }

        private string[] OpenFromFile()
        {

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "All files (*.*)|*.*"; //"sgs files (*.sgs)|*.sgs";         //|All files (*.*)|*.*";
            ofd.RestoreDirectory = true;
            ofd.Multiselect = true;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string[] fileNames = ofd.FileNames;
                return fileNames;
            }

            return null;
        }

        private bool IsLinesCross(
            double x11, double y11, double x12, double y12, double x21, double y21, double x22, double y22) 
        {
            double maxx1 = Math.Max(x11, x12), maxy1 = Math.Max(y11, y12);
            double minx1 = Math.Min(x11, x12), miny1 = Math.Min(y11, y12);
            double maxx2 = Math.Max(x21, x22), maxy2 = Math.Max(y21, y22);
            double minx2 = Math.Min(x21, x22), miny2 = Math.Min(y21, y22);
 
            if (minx1 > maxx2 || maxx1 < minx2 || miny1 > maxy2 || maxy1 < miny2) {
                return false; // Момент, када линии имеют одну общую вершину...
            }
 
            double dx1 = x12 - x11, dy1 = y12 - y11; // Длина проекций первой линии на ось x и y
            double dx2 = x22 - x21, dy2 = y22 - y21; // Длина проекций второй линии на ось x и y
            double dxx = x11 - x21, dyy = y11 - y21;
            double div, mul;
 
            if (Math.Abs((div = (dy2*dx1 - dx2*dy1)) - 0) < double.Epsilon) {
                return false; // Линии параллельны...
            }
            if (div > 0) {
                if ((mul = (dx1*dyy - dy1*dxx)) < 0 || mul > div) {
                    return false; // Первый отрезок пересекается за своими границами...
                }
                if ((mul = (dx2*dyy - dy2*dxx)) < 0 || mul > div) {
                    return false; // Второй отрезок пересекается за своими границами...
                }
            }
 
            if ((mul = -(dx1*dyy - dy1*dxx)) < 0 || mul > -div) {
                return false; // Первый отрезок пересекается за своими границами...
            }
            if ((mul = -(dx2*dyy - dy2*dxx)) < 0 || mul > -div) {
                return false; // Второй отрезок пересекается за своими границами...
            }
 
            return true;
        }

        private bool IsLinesCross2(
            int x11, int y11, int x12, int y12, int x21, int y21, int x22, int y22)
        {
            
            int maxx1 = Math.Max(x11, x12), maxy1 = Math.Max(y11, y12);
            int minx1 = Math.Min(x11, x12), miny1 = Math.Min(y11, y12);
            int maxx2 = Math.Max(x21, x22), maxy2 = Math.Max(y21, y22);
            int minx2 = Math.Min(x21, x22), miny2 = Math.Min(y21, y22);
            
            if (minx1 > maxx2 || maxx1 < minx2 || miny1 > maxy2 || maxy1 < miny2)
            {
                return false; // Момент, када линии имеют одну общую вершину...
            }

            int dx1 = x12 - x11, dy1 = y12 - y11; // Длина проекций первой линии на ось x и y
            int dx2 = x22 - x21, dy2 = y22 - y21; // Длина проекций второй линии на ось x и y
            int dxx = x11 - x21, dyy = y11 - y21;
            int div, mul;

            if (Math.Abs((div = (dy2 * dx1 - dx2 * dy1)) - 0) == 0)
            {
                return false; // Линии параллельны...
            }

            if (div > 0)
            {
                if ((mul = (dx1 * dyy - dy1 * dxx)) < 0 || mul > div)
                {
                    return false; // Первый отрезок пересекается за своими границами...
                }
                if ((mul = (dx2 * dyy - dy2 * dxx)) < 0 || mul > div)
                {
                    return false; // Второй отрезок пересекается за своими границами...
                }
            }

            if ((mul = -(dx1 * dyy - dy1 * dxx)) < 0 || mul > -div)
            {
                return false; // Первый отрезок пересекается за своими границами...
            }
            if ((mul = -(dx2 * dyy - dy2 * dxx)) < 0 || mul > -div)
            {
                return false; // Второй отрезок пересекается за своими границами...
            }

            return true;
        }


        private void DrawSelected(int sel)
        {
            xyGraphs.UnselectAll();
            
            if (sel < 0)
            {
                return;
            }
            
            xyGraphs.SelectGraph(sel);
        }
        
        private void RedrawAll()
        {
            buf.Graphics.Clear(backPanelColor);
            
            xyGraphs.DrawAll(buf, isZoomed);

            buf.Render();

            toolStripStatusLabel1.Text = "Global. " + "(xmin,xmax= " + xyGraphs.min.x.ToString() + ", " + xyGraphs.max.x.ToString() +
                ") (ymin,ymax= " + xyGraphs.min.y.ToString() + ", " + xyGraphs.max.y.ToString() +
                ") (delta x,y= " + xyGraphs.delta.x.ToString() + ", " + xyGraphs.delta.y.ToString() + ")";

        }

    }
}
