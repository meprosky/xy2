using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;


namespace xy2
{
    public partial class Form1
    {
        class XYval
        {
            public double x;
            public double y;

            public XYval() { this.x = 0; this.y = 0; }
            public XYval(double x, double y) { this.x = x; this.y = y; }
        }
        
        class GRAPHS
        {
            private List<Graph> xylist = new List<Graph>();

            public double x00;
            public double y00;

            public XYval min    = new XYval(),    max = new XYval();
            public XYval minabs = new XYval(), maxabs = new XYval();
            public XYval delta  = new XYval();

            public void Add(Graph xy)
            {
                if (xy.error != 0)
                    return;

                xylist.Add(xy);
                getGlobalMaxMinDelta();
                getCurrentKt();

            }

            public void findGraphsMinMaxDeltaAbs()
            {
                max.x = min.x = xylist[0].min.x;
                max.y = min.y = xylist[0].min.y;

                foreach (Graph arr in xylist)
                {
                    if (arr.min.x < min.x) min.x = arr.min.x;
                    if (arr.min.y < min.y) min.y = arr.min.y;

                    if (arr.max.x > max.x) max.x = arr.max.x;
                    if (arr.max.y > max.y) max.y = arr.max.y;
                }


                delta.x = max.x - min.x;
                delta.y = max.y - min.y;

                maxabs.x = (Math.Abs(max.x) > Math.Abs(min.x)) ? Math.Abs(max.x) : Math.Abs(min.x);
                maxabs.y = (Math.Abs(max.y) > Math.Abs(min.y)) ? Math.Abs(max.y) : Math.Abs(min.y);

                minabs.x = (Math.Abs(max.x) > Math.Abs(min.x)) ? Math.Abs(min.x) : Math.Abs(max.x);
                minabs.y = (Math.Abs(max.y) > Math.Abs(min.y)) ? Math.Abs(min.y) : Math.Abs(max.y);
            }

            public void updateGlobalMaxMinDeltaAbs()
            {
                globalMax.x = max.x;
                globalMin.x = min.x;
                globalMax.y = max.y;
                globalMin.y = min.y;

                deltaMinMax.x = delta.x;
                deltaMinMax.y = delta.y;

                globalMaxAbs.x = maxabs.x;
                globalMaxAbs.y = maxabs.y;

                globalMinAbs.x = minabs.x;
                globalMinAbs.y = minabs.y;
            }

            public void getGlobalMaxMinDelta()
            {
                findGraphsMinMaxDeltaAbs();
                updateGlobalMaxMinDeltaAbs();
                getDeltaAbsAndCheck();
            }

            public void getDeltaAbsAndCheck()
            {

                if (deltaMinMax.x == 0 || deltaMinMax.y == 0)
                {
                    NullMinMax();
                }


                if (!(globalMin.x <= 0 && globalMax.x >= 0)) //график не пересекает ось
                    if (globalMinAbs.x / deltaMinMax.x < ratioMinabsDelta)
                    //if (minabs.x / deltaMinMax.x < ratioMinabsDelta)
                    {
                        globalMinAbs.x = 0;
                        if (Math.Abs(globalMax.x) > Math.Abs(globalMin.x)) globalMin.x = 0;
                        else globalMax.x = 0;
                        deltaMinMax.x = globalMax.x - globalMin.x;
                    }

                if (!(globalMin.y <= 0 && globalMax.y >= 0)) //график пересекает ось
                    if (globalMinAbs.y / deltaMinMax.y < ratioMinabsDelta)
                    //if (minabs.y / deltaMinMax.y < ratioMinabsDelta)
                    {
                        globalMinAbs.y = 0;
                        if (Math.Abs(globalMax.y) > Math.Abs(globalMin.y)) globalMin.y = 0;
                        else globalMax.y = 0;
                        deltaMinMax.y = globalMax.y - globalMin.y;
                    }
            }

            private void getCurrentKt()
            {
                if (deltaMinMax.x == 0)
                {
                    double kp = 0.5 * uWidth;
                    double v0 = xylist[0].arrv[0].x;

                    Kt.x = (v0 != 0) ? kp / v0 : kp;
                }
                else
                {
                    Kt.x = (double)uWidth / deltaMinMax.x;
                }


                if (deltaMinMax.y == 0)
                {
                    Kt.y = 0.5 * uHeight / xylist[0].arrv[0].y;
                }
                else
                {
                    Kt.y = (double)uHeight / deltaMinMax.y;
                }
            }

            private void NullMinMax()
            {
                if (deltaMinMax.x == 0)
                {
                    if (globalMin.x == 0 && globalMax.x == 0)
                    {
                        globalMin.x = -2;
                        globalMax.x = 2;
                    }
                    else
                    {
                        globalMin.x = globalMin.x - 2 * globalMin.x;
                        globalMax.x = globalMax.x + 2 * globalMax.x;
                    }
                    deltaMinMax.x = globalMax.x - globalMin.x;
                    globalMaxAbs.x = Math.Abs(globalMax.x);
                    globalMinAbs.x = Math.Abs(globalMin.x);

                }

                if (deltaMinMax.y == 0)
                {
                    if (globalMin.y == 0 && globalMax.y == 0)
                    {
                        globalMin.y = -2;
                        globalMax.y = 2;
                    }
                    else
                    {
                        globalMin.y = globalMin.y - 2 * globalMin.y;
                        globalMax.y = globalMax.y + 2 * globalMax.y;
                    }
                    


                    deltaMinMax.y = globalMax.y - globalMin.y;
                    globalMaxAbs.y = Math.Abs(globalMax.y);
                    globalMinAbs.y = Math.Abs(globalMin.y);



                    /*
                    if (deltaMinMax.x < 0 || deltaMinMax.y < 0) //ошибка округления
                    {
                        int id = listfn.Count - 1;
                        string s = listfn[id];

                        listfn.RemoveAt(id);
                        xyGraphs.removeXYGgraph(id);

                        MessageBox.Show("Ошибка в блоке Min Max\n" +
                            "Данные - \"" + s + "\"\n", "Ошибка точности вычислений",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                    */
                    

                }

            }

            public void DrawAll(BufferedGraphics buf, bool isZoomMode)
            {
                if (xylist.Count == 0)
                {
                    buf.Graphics.Clear(backPanelColor);
                    buf.Render();
                    return;
                }

                if(!isZoomMode)
                    getGlobalMaxMinDelta();

                getCurrentKt();


                XYaxis axis = new XYaxis();

                if (axis.axerror != 0)
                {
                    this.DrawAll(buf, isZoomMode);
                    return;
                }

                x00 = axis.p00.x;
                y00 = axis.p00.y;


                foreach (Graph xy in xylist)
                {
                    int res = xy.Draw(buf);
                    
                    if (res != 0)
                    {
                        this.DrawAll(buf, false);
                        return;
                    }
                
                }

                axis.Draw(buf);

                //buf.Render();
            }

            public void RemoveAll()
            {
                xylist.Clear();
            }

            public void UnselectAll()
            {
                foreach (Graph xy in xylist)
                    xy.isSelected = false;
            }

            public void SelectGraph(int i)
            {
                if(xylist.Count > 0 && i < xylist.Count)
                    xylist[i].isSelected = true;
            }

            public void SelectOneGraph(int i)
            {
                if (xylist.Count > 0 && i < xylist.Count)
                {
                    UnselectAll();
                    xylist[i].isSelected = true;
                }
            }

            public int FindPoint(int i, ref Point p0)
            {
                if (xylist.Count == 0 || i > xylist.Count - 1 || i < 0)
                    return -1;

                Point[] pa = xylist[i].arrpt;

                int xm = offsetX + uWidth / 2;
                
                Point before = new Point(pa[0].X, pa[0].Y);
                Point after  = new Point(pa[0].X, pa[0].Y);
                
                int cx = 0;
                int cy = 0;
                int i_visible = -1;
                int i_a = -1, i_b = -1;

                for (int j = 0; j < pa.Length; j++)
                {
                    cx = pa[j].X;
                    cy = pa[j].Y;

                    
                    
                    if(cy >= offsetY && cy <=offsetY+uHeight && cx >= offsetX && cx <=offsetX+uWidth)
                    {
                        i_visible = j;
                    }
                    

                   if (cx <= xm && cx > before.X) {i_b = j;}
                   if (cx >= xm && cx < after.X)  {i_a = j;}



                }

                

                return 0;
            }

            public Graph getXYGgraph(int i)
            {
                if (xylist.Count > 0 && i < xylist.Count)
                    return xylist[i];
                else
                    return null;
            }

            public List<Graph> getListxy()
            {
                return xylist;
            }

            public int RemoveGgraph(int i)
            {
                if (xylist.Count == 0 || i > xylist.Count - 1 || i < 0)
                    return -1;

                xylist.RemoveAt(i);
                listfn.RemoveAt(i);

                updateGlobalMaxMinDeltaAbs();

                return 0;
            }
        
        }

        partial class Graph
        {
            public int error = 0;
            
            public bool isSelected = false;

            public int n;
            public string gfn;

            public XYval[] arrv;
            public Point[] arrpt;
            public XYval max = new XYval(), min = new XYval();

            public Pen penpoint = new Pen(Color.Black, 1);
            public Pen penline = new Pen(Color.Black, 1);
            public SolidBrush brushpoint = new SolidBrush(Color.Black);
            public SolidBrush brushline  = new SolidBrush(Color.Black);
            
            private int _colorpoint = 15;
            private int _colorline  = 15;
            public int   typepoint  = 0;
            public bool   isConnect = true;
            public int    ptsize = 2;
            public int    _lineth = 1;

            public List<string> comments = new List<string>();

            public int colorpoint
            {
                get { return _colorpoint; }
                set { 
                    _colorpoint = value;
                    penpoint =   new Pen       (Color.FromKnownColor(xycolors[_colorpoint]), 1);
                    brushpoint = new SolidBrush(Color.FromKnownColor(xycolors[_colorpoint]));
                }
            }

            public int colorline
            {
                get { return _colorline; }
                set
                {
                    _colorline = value;
                    penline = new Pen(Color.FromKnownColor(xycolors[_colorline]), _lineth);
                    brushline = new SolidBrush(Color.FromKnownColor(xycolors[_colorline]));
                }
            }

            public int lineth
            {
                get { return _lineth; }
                set
                {
                    _lineth = value;
                    if (_lineth < 1)
                        isConnect = false;
                    else
                        isConnect = true;
                    
                    penline = new Pen(Color.FromKnownColor(xycolors[_colorline]), _lineth);
                }
            }

            public Graph(int na)//xyVal[] inarr)
            {
                n = na;
                arrv =  new XYval[n];
                arrpt = new Point[n];

                for (int i = 0; i < n; i++)
                {
                    //arrv[i] = new XYval(-1000 + -i * 0.002, -1000 + -i * i * i * 0.000002);
                    //arrv[i] = new XYval(-1000e-180 + -i * 0.2e-105, -1000e+301 + -i * i * i * 0.000002e+301); 
                    
                    //arrv[i] = new XYval(0, i+100);
                    
                    //arrv[i] = new XYval(1000 + i * 0.002, 1000 + i * i * i * 0.000002);
                    //arrv[i] = new XYval(i*1e-15,  1 + i * 0.0000000000012 + 0.0000001345); 
                    //arrv[i] = new XYval(i, i * 1e-23);
                    
                    //arrv[i] = new XYval(i-50, -1e20 + -i * i * i * 2e-5); //здесь получается ошибка точности вычислений
                    //arrv[i] = new XYval(i - 50, -1e5 + -i * i * i * 2e-5); //ПРОВЕРОЧНЫЙ
                    //arrv[i] = new XYval(i - 10, i -90);
                    //arrv[i] = new XYval(-i*0.000001, -i*70);
                    //arrv[i] = new XYval(i - 50, i -50);


                    
                    double a = 10;                                          // график "Жезл"
                    double f = (double)(i + 1) / 10;                        // график "Жезл"
                    double v = a / Math.Sqrt(f);                            // график "Жезл"
                    arrv[i] = new XYval(v * Math.Cos(f), v * Math.Sin(f));  // график "Жезл"
                    



                }
                

                FindMinMax();

                gfn = "generate " + n.ToString() + "pt";
                listfn.Add(gfn);

                comments.Add("No comments 1");
                comments.Add("No comments 2");
                comments.Add("No comments 3");
                comments.Add("No comments 4");

                this.colorline = 3;
                this.colorpoint = 19;
                this.typepoint = 3;
                this.ptsize = 3;

            }

            public Graph(XYval[] arr)
            {
                arrv = arr;

                n = arr.Length;
                arrpt = new Point[n];

                FindMinMax();

                gfn = "generate " + n.ToString() + "pt";
                listfn.Add(gfn);

                comments.Add("No comments 1");
                comments.Add("No comments 2");
                comments.Add("No comments 3");
                comments.Add("No comments 4");

                this.colorline = 15;
                this.colorpoint = 15;
                this.typepoint = 0;
                this.ptsize = 2;

            }

            public Graph(string fn)
            {
                string[] fstr = new string[1];

                try
                {
                    fstr = File.ReadAllLines(fn, Encoding.Default);
                }
                catch
                {
                    MessageBox.Show("Ошибка чтения файла - " +  fn);
                    error = 101;
                    return;
                    //System.Environment.Exit(101);
                }

                List<XYval> xyv = new List<XYval>();

                //4 строки комментариев
                for (int i = 0; i < 4; i++)
                    comments.Add(fstr[i]);



                string regstr = @"(?<!//.*)(?<=\s|=|\A)[+-]?(\d+)?\.?\d+(?:[eE][+-][0-9]{1,3})?(?=\s|,|;|\Z)";

                string[] param = regex_strings(fstr[4], regstr);

                if (param.Length < 5)
                {
                    MessageBox.Show("Ошибка чтения файла - " + fn);
                    error = 101;
                    return;
                    //System.Environment.Exit(101);
                }

                int npoints = Convert.ToInt32(param[0]);

                
                if (npoints != -1 && npoints > fstr.Length - 5)
                {
                    MessageBox.Show("Количество точек в заголовке превышет количество строк в файле\n" + fn + "\n" +
                        "Будет предпринята попытка прочитать подходящие строки");
                    npoints = -1;
                }
                
                
                colorpoint =  Convert.ToInt32(param[1]);
                colorline =   Convert.ToInt32(param[2]);
                isConnect =  (Convert.ToInt32(param[3]) == 0) ? false : true;
                typepoint =   Convert.ToInt32(param[4]);


                if (colorpoint < 0 || colorpoint > 31) colorpoint = 15;
                if (colorline  < 0 || colorline  > 31) colorline  = 15;

                if (param.Length > 5)
                {
                    ptsize = Convert.ToInt32(param[5]);
                    if (ptsize < 1 || ptsize > 100)
                        ptsize = 2;
                }


                int nstr = (npoints == -1) ? fstr.Length : npoints + 5;
                
                for (int i = 5; i < nstr; i++)
                {
                    string[] s = regex_strings(fstr[i], regstr);

                    if (s.Length == 2) //т.е. есть в строке 2 числа (x и y)
                    {
                        xyv.Add( new XYval(
                            Convert.ToDouble(s[0]), 
                            Convert.ToDouble(s[1]) ));
                    }
                }

                n = xyv.Count;

                if (n < 2)
                {
                    MessageBox.Show("Число прочитанных точек меньше двух.\nГрафик построен не будет\n" + fn);
                    error = -1;
                    return;
                }
                
                if (n != npoints && npoints != -1)
                {
                    MessageBox.Show("Число точек для чтения в заголовке= " + npoints.ToString() + " не совпадает= " + n.ToString() +
                        " с прочитанными из файла\n" + fn + "\nграфик будет построени по прочитанным точкам");
                }

                arrv = xyv.ToArray();
                arrpt = new Point[n];

                gfn = fn;

                FindMinMax();
                
                listfn.Add(gfn);
            
            }

            public void FindMinMax()
            {
                
                max.x = min.x = arrv[0].x;
                max.y = min.y = arrv[0].y;

                for (int i = 0; i < n; i++) //ищем мин и макс значения координат
                {
                    if (arrv[i].x < min.x) min.x = arrv[i].x;
                    if (arrv[i].y < min.y) min.y = arrv[i].y;
                    
                    if (arrv[i].x > max.x) max.x = arrv[i].x;
                    if (arrv[i].y > max.y) max.y = arrv[i].y;
                }


                if (max.x - min.x < 0 || max.y - min.y < 0)
                {
                    error = -1;

                    MessageBox.Show("Ошибка в блоке Min Max\n" +
                        "Данные - \"" + gfn + "\"\n", "Ошибка точности вычислений",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }


            }

            public int Draw(BufferedGraphics buf)
            {
                try
                {
                    for (int i = 0; i < n; i++)
                    {
                        arrpt[i].X = Convert.ToInt32((arrv[i].x - globalMin.x) * Kt.x + offsetX);
                        arrpt[i].Y = Convert.ToInt32(gHeight - ((arrv[i].y - globalMin.y) * Kt.y + offsetY));
                    }
                    
                    if (isSelected)
                    {
                        buf.Graphics.DrawLines(new Pen(Color.Blue, lineth + lineth / 2 + 2), arrpt);
                    }
                    
                    if (isConnect)
                    {
                        buf.Graphics.DrawLines(penline, arrpt);
                    }
                    
                    

                }
                catch
                {
                    MessageBox.Show("Ошибка переполнения");
                    return -1;
                }


                for (int i = 0; i < n; i++)
                    DrawDots(buf, arrpt[i].X, arrpt[i].Y);

                return 0;
            }
        }

        public static string[] regex_strings(string s, string pattern)
        {
            Regex regex = new Regex(pattern);
            Match match = regex.Match(s);

            List<string> strlist = new List<string>();

            while (match.Success)
            {

                strlist.Add(match.Value);

                match = match.NextMatch();

            }
            return strlist.GetRange(0, strlist.Count).ToArray();
        }


    }
}
