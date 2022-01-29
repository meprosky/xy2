using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;

namespace xy2
{
    public partial class Form1
    {
        partial class Graph
        {
            //public int ptsize = 3;
            
            private void DrawDots(BufferedGraphics buf, int x, int y) //int x, int y)
            {
                Point[] triangle = { 
                    new Point(x-ptsize, y+ptsize),
                    new Point(x,        y-ptsize),
                    new Point(x+ptsize, y+ptsize),
                    new Point(x-ptsize, y+ptsize)
                    };

                Point[] romb = { 
                    new Point(x-ptsize, y),
                    new Point(x,        y+ptsize),
                    new Point(x+ptsize, y),
                    new Point(x,        y-ptsize),
                    new Point(x-ptsize, y)
                    };


                switch (typepoint) 
                {

                    case 0:
                        if(!isConnect)
                            buf.Graphics.FillRectangle(brushpoint, x, y, 1, 1);
                        break;
                    case 1:
                        buf.Graphics.DrawLine(penpoint, x - ptsize, y, x + ptsize, y);
                        buf.Graphics.DrawLine(penpoint, x, y - ptsize, x, y + ptsize);
                        break;
                    case 2:
                        buf.Graphics.DrawLine(penpoint, x - ptsize, y - ptsize, x + ptsize, y + ptsize);
                        buf.Graphics.DrawLine(penpoint, x - ptsize, y + ptsize, x + ptsize, y - ptsize);
                        break;
                    case 3:
                        buf.Graphics.DrawRectangle(penpoint, x - ptsize, y - ptsize, ptsize * 2, ptsize * 2);
                        break;
                    case 4:
                        buf.Graphics.FillRectangle(brushpoint, x - ptsize, y - ptsize, ptsize * 2, ptsize * 2);
                        break;
                    case 5:
                        buf.Graphics.DrawPolygon(penpoint, romb);
                        break;
                    case 6:
                        buf.Graphics.FillPolygon(brushpoint, romb);
                        break;
                    case 7:
                        buf.Graphics.DrawEllipse(penpoint, x - ptsize, y - ptsize, ptsize*2, ptsize*2);
                        break;
                    case 8:
                        buf.Graphics.FillEllipse(brushpoint, x - ptsize, y - ptsize, ptsize*2, ptsize*2);
                        break;
                    case 9:
                        buf.Graphics.DrawPolygon(penpoint, triangle);
                        break;
                    case 10:
                        buf.Graphics.FillPolygon(brushpoint, triangle);
                        break;
                    default:
                        break;
                }
            }



        }
    }
}
