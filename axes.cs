using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace xy2
{
    public partial class Form1
    {
        
        static public int brokenAxisGap = 10;                //если оси разорваны это промежуток между началом оси и нулем
        static public int ppdx = 100, ppdy = 70;             //число пикселей на интервал между шрихами
        static public int stickLenght = 6;                   //длина шриха в пикселях
        static public int stickLenght2 = stickLenght / 3;    //длина маленького шриха в пикселях
        static public int stickTextOffset = stickLenght + 2; //смещение цифр отн. шриха
        static public int stickSmallPerInterval = 5;         //число маленьких шрихов между основными шрихами

        class XYaxis
        {
            //private int ngraph;
            public int axerror;

            private bool isXbroken_left = false;
            private bool isXbroken_right = false;
            private bool isYbroken_top = false;
            private bool isYbroken_bottom = false;

            Pen penAxis = new Pen(Color.Gray, 1);   //цвет осей
            Font fontAxis = new Font("Verdana", 8); //шрифт осей
            Font fontAxis10 = new Font("Verdana", 7); //шрифт степени 10

            SolidBrush fontAxisBrush = 
                new SolidBrush(Color.Black);        //цвет шрифта
            
            public XYval p00 = new XYval();                //координаты нуля
            XYval norm_min = new XYval(), norm_max = new XYval(), norm_deltaMinMax = new XYval(); //нормрованные значения (т.е. в интервале 0..1)
            XYval interval = new XYval();           //значение интервала между шрихами
            XYval first_stick_norm = new XYval();   //значение первого шриха нормированный (0..1) если оси разорваны
            Point first_stick_pixel = new Point();  //значение первого в пикселях если оси разорваны

            Point ab0, ab1, ab_1;       //ось абсцисс(X) 0, +x, -x
            Point or0, or1, or_1;       //ось ординат(Y) 0, +y, -y
            
            int n_exp10_x, n_exp10_y;   //порядок экспоненты осей

            double dx, dy;              //интервал нормированный (0.1,0.2..)между шрихами
            int pixeldx, pixeldy;       //интервал в пикселях между шрихами

            public XYaxis()
            {
                axerror = 0;

                p00 = new XYval();

                getAxes();
                getAxisDivisions();
            }

            private void getAxisDivisions()
            {
                n_exp10_x = (int)Math.Ceiling(Math.Log10(globalMaxAbs.x));
                n_exp10_y = (int)Math.Ceiling(Math.Log10(globalMaxAbs.y));

                double exp10x = Math.Pow(10, n_exp10_x);
                double exp10y = Math.Pow(10, n_exp10_y);
                
                if (Math.Abs(n_exp10_x) < 4) //если данные в диапазоне до 1000 то не нормируем оси
                {
                    n_exp10_x = 0;
                    exp10x = 1;
                }


                if (Math.Abs(n_exp10_y) < 4) //если данные в диапазоне до 1000 то не нормируем оси
                {
                    n_exp10_y = 0;
                    exp10y = 1;
                }
                

                norm_min.x = globalMin.x / exp10x;
                norm_min.y = globalMin.y / exp10y;

                norm_max.x = globalMax.x / exp10x;
                norm_max.y = globalMax.y / exp10y;

                norm_deltaMinMax.x = deltaMinMax.x / exp10x;
                norm_deltaMinMax.y = deltaMinMax.y / exp10y;

                dx = getSticks(ppdx, uWidth, norm_deltaMinMax.x, exp10x, ref interval.x);
                dy = getSticks(ppdy, uHeight, norm_deltaMinMax.y, exp10y, ref interval.y);

                pixeldx = Convert.ToInt32(dx * Kt.x);
                pixeldy = Convert.ToInt32(dy * Kt.y);

                first_stick_norm.x = getFirstStick(norm_min.x, norm_max.x, interval.x);
                first_stick_norm.y = getFirstStick(norm_min.y, norm_max.y, interval.y);

                first_stick_pixel.X = Convert.ToInt32((first_stick_norm.x * exp10x - globalMin.x) * Kt.x);
                first_stick_pixel.Y = Convert.ToInt32((first_stick_norm.y * exp10y - globalMin.y) * Kt.y);
            }

            private double getSticks(int ppd, int lenght, double norm_interval, double exp10xy, ref double l)
            {
                double nd = (double)lenght / ppd;
             
                double DI = norm_interval / nd;

                int exp = (int)Math.Ceiling( Math.Log10(DI) ); // получаем порядок интервала
                
                double exp10 = Math.Pow(10, exp); //получаем 10 в степени exp
                
                double DC = 1.0;

                DI = DI / exp10; //ненормированный интервад без 10 в степени

                //нормированный интервал
                if (DI < 0.12) DC = 0.1;
                else if (DI < 0.3) DC = 0.2;
                else if (DI < 0.4) DC = 0.25;
                else if (DI < 0.7) DC = 0.5;

                l = DC * exp10;

                return DC * exp10 * exp10xy;
            }

            private double getFirstStick(double norm_min_val, double norm_max_val, double norm_interval_val)
            {
                double abs_val = (Math.Abs(norm_min_val) > Math.Abs(norm_max_val)) ? norm_max_val : norm_min_val;

                int sign = (Math.Log10(norm_interval_val) < 0) ? -1 : 1;
                int n = (int)Math.Ceiling(Math.Abs(Math.Log10(norm_interval_val)));

                double d = 0;
                try
                {
                    d = Math.Round(abs_val, n);
                }
                catch
                {
                    axerror = -1;
                    int id = listfn.Count - 1;
                    string s = listfn[id];

                    xyGraphs.RemoveGgraph(id);

                    MessageBox.Show("Ошибка при построении осей X и Y\n" +
                    "Данные - \"" + s + "\"\n", "Ошибка точности вычислений",
                                 MessageBoxButtons.OK,
                                 MessageBoxIcon.Error);
                    return 0;
                }


                if (d < norm_min_val)
                    d += norm_interval_val * Math.Pow(10, sign * n - 1);
                
                /*
                if (d < norm_min_val)
                    d += Math.Pow(10, sign * n);
                */



                return d;
            }

            //вычисление нуля координат и координат осей в экранных координатах для построения осей
            private void getAxes()  
            {
                //xyVal p00 = new xyVal();
                p00.x = -globalMin.x * Kt.x + offsetX;
                p00.y = -globalMin.y * Kt.y + offsetY;

                if (p00.x < borderW / 2) 
                {
                    ab0.X = borderW;
                    ab1.X = gWidth;
                    ab_1.X = -1;
                    or0.X = or1.X = or_1.X = borderW - brokenAxisGap;
                    isXbroken_left = true;
                }
                else if (p00.x > (borderW + uWidth + borderW / 2))
                {
                    ab0.X = borderW + uWidth;
                    ab1.X = 0;
                    ab_1.X = -2;
                    or0.X = or1.X = or_1.X = borderW + uWidth + brokenAxisGap;
                    isXbroken_right = true;
                }
                else
                { 
                    ab0.X = Convert.ToInt32(p00.x);
                    ab1.X = gWidth;
                    ab_1.X = 0;

                    or0.X = ab0.X;
                    or1.X = ab0.X;
                    or_1.X = ab0.X;
                    isXbroken_left = false;
                    isXbroken_right = false;
                }

                if (p00.y < borderH / 2) 
                {
                    or0.Y = borderH;
                    or1.Y = gHeight;
                    or_1.Y = -1;
                    ab0.Y = ab1.Y = ab_1.Y = borderH - brokenAxisGap;
                    isYbroken_bottom = true;
                }
                else if (p00.y > (borderH + uHeight + borderH / 2))
                {
                    or0.Y = borderH + uHeight;
                    or1.Y = 0;
                    or_1.Y = -2;
                    ab0.Y = ab1.Y = ab_1.Y = borderH + uHeight + brokenAxisGap;
                    isYbroken_top = true;
                }
                else
                {
                    or0.Y = Convert.ToInt32(p00.y);
                    or1.Y = gHeight;
                    or_1.Y = 0;

                    ab0.Y = or0.Y;
                    ab1.Y = or0.Y;
                    ab_1.Y = or0.Y;
                    isYbroken_bottom = false;
                    isYbroken_top = false;
                }
            }

            public void Draw(BufferedGraphics buf)
            {
                if (isXbroken_left)  buf.Graphics.FillRectangle(brushDefault, 0,                                0, offsetX - brokenAxisGap, gHeight);
                if (isXbroken_right) buf.Graphics.FillRectangle(brushDefault, gWidth - offsetX + brokenAxisGap, 0, offsetX - brokenAxisGap, gHeight);

                if (isYbroken_top)    buf.Graphics.FillRectangle(brushDefault, 0, 0,                                 gWidth, offsetY - brokenAxisGap);
                if (isYbroken_bottom) buf.Graphics.FillRectangle(brushDefault, 0, gHeight - offsetY + brokenAxisGap, gWidth, offsetY - brokenAxisGap);
                
                DrawArrow(buf);
                
                //ориентация штрихов (вверх, вних, влево, вправо) в зависимости от положения осей
                int stickOrientation;


                stickOrientation = (ab0.Y < gHeight * 2/3) ? 1 : -1;

                //Строим ось X
                if (ab_1.X >= 0)  //если ноль лежит в переделах области построения
                {
                    buf.Graphics.DrawLine(penAxis, ab0.X, gHeight - ab0.Y, ab1.X, gHeight - ab1.Y);     //ось +Х
                    buf.Graphics.DrawLine(penAxis, ab0.X, gHeight - ab0.Y, ab_1.X, gHeight - ab_1.Y);   //ось -Х

                    DrawStickX(buf, ab0.X + pixeldx, gWidth, pixeldx, stickOrientation,  interval.x,  interval.x);     //штрихи по оси +X
                    DrawStickX(buf, ab0.X - pixeldx, 0,     -pixeldx, stickOrientation, -interval.x, -interval.x);     //штрихи по оси -X
                }
                else //если оси разорваны
                {
                    //голая ось
                    buf.Graphics.DrawLine(penAxis, ab0.X, gHeight - ab0.Y, ab1.X, gHeight - ab1.Y);

                    //маленький штрих если ось разорвана
                   
                    /*
                    buf.Graphics.DrawLine(penAxis, 
                        ab0.X, gHeight - ab0.Y, 
                        ab0.X, gHeight - (ab0.Y - stickLenght * stickOrientation)); 
                    */

                    //если оси разорваны (в прямом и обратном направлении)
                    if(ab0.X < ab1.X)
                        DrawStickX(buf,
                            first_stick_pixel.X + offsetX, 
                            gWidth, pixeldx,
                            stickOrientation,
                            first_stick_norm.x, interval.x);
                    else
                        DrawStickX(buf,
                            first_stick_pixel.X + offsetX, 
                            0, -pixeldx,
                            stickOrientation,
                            first_stick_norm.x, -interval.x); 

                }


                stickOrientation = (or0.X > gWidth * 2/3) ? 1 : -1;

                //Строим ось Y
                if (or_1.Y >= 0) //если ноль лежит в переделах области построения
                {
                    buf.Graphics.DrawLine(penAxis, or0.X, gHeight - or0.Y, or1.X,  gHeight - or1.Y);
                    buf.Graphics.DrawLine(penAxis, or0.X, gHeight - or0.Y, or_1.X, gHeight - or_1.Y);

                    DrawStickY(buf, or0.Y + pixeldy, gHeight, pixeldy, stickOrientation, interval.y, interval.y);
                    DrawStickY(buf, or0.Y - pixeldy, 0, -pixeldy, stickOrientation, -interval.y, -interval.y);
                }
                else //если оси разорваны
                {
                    buf.Graphics.DrawLine(penAxis, or0.X, gHeight - or0.Y, or1.X, gHeight - or1.Y);
                    
                    //buf.Graphics.DrawLine(penAxis, or0.X, gHeight - or0.Y, or0.X + stickLenght * stickOrientation, gHeight - or0.Y);

                    if (or0.Y < or1.Y)
                        DrawStickY(buf,
                            first_stick_pixel.Y + offsetY,
                            gHeight, pixeldy,
                            stickOrientation,
                            first_stick_norm.y, interval.y);
                    else
                        DrawStickY(buf,
                            first_stick_pixel.Y + offsetY,
                            0, -pixeldy,
                            stickOrientation,
                            first_stick_norm.y, -interval.y);
                }
                
                if(isDrawGlobRect)
                    buf.Graphics.DrawRectangle(Pens.Gray, offsetX, offsetY, uWidth, uHeight);
            }

            private void DrawStickX(BufferedGraphics buf, int p0, int p1, int dp, int direction, double s, double ds)
            {
                string str;
                int x, l, h;

                for (x = p0; Math.Sign(dp) * ( p1 - x) > 0; x += dp, s += ds)
                {

                    if (dp > 0)
                    {
                        if (x < offsetX / 2) continue;
                        if (x > gWidth - offsetX / 2) break;
                        if (isXbroken_left && x < offsetX) continue;
                        if (isXbroken_right && x > gWidth - offsetX) break;
                    }

                     if (dp < 0)
                    {
                        if (x < offsetX / 2) break;
                        if (x > gWidth - offsetX / 2) continue;
                        if (isXbroken_left && x < offsetX) break;
                        if (isXbroken_right && x > gWidth - offsetX) continue;
                    }


                    buf.Graphics.DrawLine(penAxis, x, gHeight - ab0.Y, x, gHeight - (ab0.Y - stickLenght * direction));

                    str = s.ToString();

                    l = (int)(buf.Graphics.MeasureString(str, fontAxis).Width / 2);
                    h = (direction == 1) ? 0 : (int)(buf.Graphics.MeasureString(str, fontAxis).Height);

                    buf.Graphics.DrawString(str, fontAxis, fontAxisBrush, x - l, gHeight - (ab0.Y - (stickTextOffset + h) * direction));

                    DrawSmallStick_X(buf, x-dp, x, direction);
                }

                DrawSmallStick_X(buf, x-dp, x, direction);
            }

            private void DrawStickY(BufferedGraphics buf, int p0, int p1, int dp, int direction, double s, double ds)
            {
                string str;
                int y, l, w;

                for (y = p0; Math.Sign(dp) * (p1 - y) > 0; y += dp, s += ds)
                {

                    if (dp > 0)
                    {
                        if (y < offsetY / 2) continue;
                        if (y > gHeight - offsetY / 2) break;
                        if (isYbroken_bottom && y < offsetY) continue;
                        if (isYbroken_top && y > gHeight - offsetY) break;
                    }

                    if (dp < 0)
                    {
                        if (y < offsetY / 2) break;
                        if (y > gHeight - offsetY / 2) continue;
                        if (isYbroken_bottom && y < offsetY) break;
                        if (isYbroken_top && y > gHeight - offsetY) continue;
                    }


                    buf.Graphics.DrawLine(penAxis, or0.X, gHeight - y, or0.X + stickLenght * direction, gHeight - y);

                    str = s.ToString();
                    l = (int)(buf.Graphics.MeasureString(str, fontAxis).Height / 2);
                    w = (direction == 1) ? 0 : (int)(buf.Graphics.MeasureString(str, fontAxis).Width);

                    buf.Graphics.DrawString(str, fontAxis, fontAxisBrush, or0.X + (direction * (w + stickTextOffset)), gHeight - (y + l));

                    DrawSmallStick_Y(buf, y-dp, y, direction);
                }
                DrawSmallStick_Y(buf, y-dp, y, direction);
            }

            private void DrawSmallStick_X(BufferedGraphics buf, int p0, int p1, int direction)
            {
                                
                int lenght = (int)Math.Round((double) (p1 - p0) / stickSmallPerInterval);
                int x;
                
                for (int i = 1; i < stickSmallPerInterval; i++)
                {
                    x = p0 + i * lenght;
                    
                    if (isXbroken_left  && x < offsetX)              continue;
                    if (isXbroken_right && x > gWidth - offsetX)     continue;
                    if (x < offsetX / 2 || x > gWidth - offsetX / 2) continue;

                    buf.Graphics.DrawLine(penAxis, x, gHeight - ab0.Y, x, gHeight - (ab0.Y - stickLenght2 * direction));
                }

            }

            private void DrawSmallStick_Y(BufferedGraphics buf, int p0, int p1, int direction) //промежуточные шрихи между большими
            {
                int lenght = (int)Math.Round((double)(p1 - p0) / stickSmallPerInterval);
                int y;

                for (int i = 1; i < stickSmallPerInterval; i++)
                {
                    y = p0 + i * lenght;

                    if (isYbroken_bottom && y < offsetY) continue;
                    if (isYbroken_top && y > gHeight - offsetY) continue;
                    if (y < offsetY / 2 || y > gHeight - offsetY / 2) continue;

                    buf.Graphics.DrawLine(penAxis, or0.X, gHeight - y, or0.X + (stickLenght2 * direction), gHeight - y);
                }

            }

            private void DrawArrow(BufferedGraphics buf)
            {
                
                Point p0 = new Point(or0.X, ab0.Y);
                Point p1 = new Point(or0.X, ab0.Y);

                if(ab_1.X == -1) p1.X = 0;
                if(ab_1.X == -2) p1.X = gWidth;
                if(or_1.Y == -1) p1.Y = 0;
                if(or_1.Y == -2) p1.Y = gHeight;


                Point[] arrowX = { 
                                     new Point(gWidth-5, gHeight - (p0.Y+3)),
                                     new Point(gWidth,    gHeight - p0.Y),
                                     new Point(gWidth-5, gHeight - (p0.Y-3))
                                 };

                Point[] arrowY = { 
                                     new Point(p0.X-3, gHeight - (gHeight-5)),
                                     new Point(p0.X,   gHeight -  gHeight),
                                     new Point(p0.X+3, gHeight - (gHeight-5))
                                 };

               
                int h = (int)Math.Round(buf.Graphics.MeasureString("10", fontAxis).Height);
                int w = (int)Math.Round(buf.Graphics.MeasureString("10", fontAxis).Width);

                int h10, w10, wa;

                if (n_exp10_x != 0)
                {
                    h10 = (int)Math.Round(buf.Graphics.MeasureString(n_exp10_x.ToString(), fontAxis10).Height);
                    w10 = (int)Math.Round(buf.Graphics.MeasureString(n_exp10_x.ToString(), fontAxis10).Width);

                    wa = w + w10;

                    buf.Graphics.DrawString("10", fontAxis, fontAxisBrush, gWidth - wa, gHeight - (p0.Y + h + 1));
                    buf.Graphics.DrawString(n_exp10_x.ToString(), fontAxis10, fontAxisBrush, gWidth - w10 - 4, gHeight - (p0.Y + h / 2 + 1 + 12));
                }


                if (n_exp10_y != 0)
                {
                    h10 = (int)Math.Round(buf.Graphics.MeasureString(n_exp10_y.ToString(), fontAxis10).Height);
                    w10 = (int)Math.Round(buf.Graphics.MeasureString(n_exp10_y.ToString(), fontAxis10).Width);

                    wa = w + w10;

                    buf.Graphics.DrawString("10", fontAxis, fontAxisBrush, p0.X + 3, gHeight - (gHeight - h10 + 5));
                    buf.Graphics.DrawString(n_exp10_y.ToString(), fontAxis10, fontAxisBrush, p0.X + w, gHeight - (gHeight - 1));
                }
                
                buf.Graphics.DrawLines(penAxis, arrowX);
                buf.Graphics.DrawLines(penAxis, arrowY);

                buf.Graphics.DrawLine(penAxis, p0.X, gHeight - p0.Y, p0.X, gHeight - p1.Y);
                buf.Graphics.DrawLine(penAxis, p0.X, gHeight - p0.Y, p1.X, gHeight - p0.Y);



            }

        }

    }


}
