using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.IO;
using System.Drawing.Imaging;

namespace Fractalv1._0
{
    public partial class Form1 : Form
    {


        public Form1()
        {
            InitializeComponent();

            init();
            start();

            //Added to reduce flickering
            this.DoubleBuffered = true;
        }

        private const int MAX = 256;      // max iterations
        private const double SX = -2.025; // start value real
        private const double SY = -1.125; // start value imaginary
        private const double EX = 0.6;    // end value real
        private const double EY = 1.125;  // end value imaginary
        private static int x1, y1, xs, ys, xe, ye;
        private static double xstart, ystart, xende, yende, xzoom, yzoom;
        private static bool action, rectangle, finished;

        private static float xy;
        //private Image picture;
        private Graphics g1;
        private Bitmap picture;
        Rectangle rec = new Rectangle(0, 0, 0, 0);
        public HSBColor HSBcol = new HSBColor();
        Pen p = new Pen(Color.Red, 2);
        private StatusBarPanel sbpMenu;
        private StatusBar statusBar;
        private StatusStrip statusStrip2;

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            //e.consume();
            if (action)
            {
                xs = e.X;
                ys = e.Y;
            }
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (action)
            {
                xe = e.X;
                ye = e.Y;
                rectangle = true;
            }
        }

        private void Form1_MouseHover(object sender, EventArgs e)
        {

        }


        private void colorRangeBox_ValueChanged(object sender, EventArgs e)
        {

        }

        private void restartToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        public struct HSBColor
        {
            float h;
            float s;
            float b;
            int a;

            public HSBColor(float h, float s, float b)
            {
                this.a = 0xff;
                this.h = Math.Min(Math.Max(h, 0), 255);
                this.s = Math.Min(Math.Max(s, 0), 255);
                this.b = Math.Min(Math.Max(b, 0), 255);
            }
            public HSBColor(int a, float h, float s, float b)
            {
                this.a = a;
                this.h = Math.Min(Math.Max(h, 0), 255);
                this.s = Math.Min(Math.Max(s, 0), 255);
                this.b = Math.Min(Math.Max(b, 0), 255);
            }


            public static Color FromHSB(HSBColor hsbColor)
            {
                float r = hsbColor.b;
                float g = hsbColor.b;
                float b = hsbColor.b;
                if (hsbColor.s != 0)
                {
                    float max = hsbColor.b;
                    float dif = hsbColor.b * hsbColor.s / 255f;
                    float min = hsbColor.b - dif;

                    float h = hsbColor.h * 360f / 255f;

                    if (h < 60f)
                    {
                        r = max;
                        g = h * dif / 60f + min;
                        b = min;
                    }
                    else if (h < 120f)
                    {
                        r = -(h - 120f) * dif / 60f + min;
                        g = max;
                        b = min;
                    }
                    else if (h < 180f)
                    {
                        r = min;
                        g = max;
                        b = (h - 120f) * dif / 60f + min;
                    }
                    else if (h < 240f)
                    {
                        r = min;
                        g = -(h - 240f) * dif / 60f + min;
                        b = max;
                    }
                    else if (h < 300f)
                    {
                        r = (h - 240f) * dif / 60f + min;
                        g = min;
                        b = max;
                    }
                    else if (h <= 360f)
                    {
                        r = max;
                        g = min;
                        b = -(h - 360f) * dif / 60 + min;
                    }
                    else
                    {
                        r = 0;
                        g = 0;
                        b = 0;
                    }
                }

                return Color.FromArgb
                    (
                        hsbColor.a,
                        (int)Math.Round(Math.Min(Math.Max(r, 0), 255)),
                        (int)Math.Round(Math.Min(Math.Max(g, 0), 255)),
                        (int)Math.Round(Math.Min(Math.Max(b, 0), 255))
                        );
            }

        }


        private void initvalues() // reset start values
        {
            xstart = SX;
            ystart = SY;
            xende = EX;
            yende = EY;
            if ((float)((xende - xstart) / (yende - ystart)) != xy)
                xstart = xende - (yende - ystart) * (double)xy;

        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            int z, w;
            rectangle = false;

            if (action)
            {
                xe = e.X;
                ye = e.Y;
                if (xs > xe)
                {
                    z = xs;
                    xs = xe;
                    xe = z;
                }
                if (ys > ye)
                {
                    z = ys;
                    ys = ye;
                    ye = z;
                }
                w = (xe - xs);
                z = (ye - ys);
                if ((w < 2) && (z < 2)) initvalues();
                else
                {
                    if (((float)w > (float)z * xy)) ye = (int)((float)ys + (float)w / xy);
                    else xe = (int)((float)xs + (float)z * xy);
                    xende = xstart + xzoom * (double)xe;
                    yende = ystart + yzoom * (double)ye;
                    xstart += xzoom * (double)xs;
                    ystart += yzoom * (double)ys;
                }
                xzoom = (xende - xstart) / (double)x1;
                yzoom = (yende - ystart) / (double)y1;
                mandelbrot();
                rectangle = false;
                Refresh();
            }
        }

        private float pointcolour(double xwert, double ywert) // color value from 0.0 to 1.0 by iterations
        {
            double r = 0.0, i = 0.0, m = 0.0;
            int j = 0;

            while ((j < MAX) && (m < 4.0))
            {
                j++;
                m = r * r - i * i;
                i = 2.0 * r * i + ywert;
                r = m + xwert;
            }
            return (float)j / (float)MAX;
        }
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            //put the bitmap on the window
            Graphics windowG = e.Graphics;
            windowG.DrawImageUnscaled(picture, 0, 0);
            Pen p1 = new Pen(Color.Black, 2);
            if (rectangle == true)
            {
                e.Graphics.DrawRectangle(p1, rec);
            }
            if (rectangle == false)
            {
                Invalidate();
            }

        }


        public void init() // all instances will be prepared
        {
            finished = false;
            x1 = 640;
            y1 = 480;
            xy = (float)x1 / (float)y1;
            picture = new Bitmap(x1, y1);
            g1 = Graphics.FromImage(picture);

            Pen myPen = new Pen(System.Drawing.Color.White, 3);
            g1.DrawRectangle(myPen, 0, 0, 0, 0);
            if (rectangle)
            {

                if (xs < xe)
                {
                    if (ys < ye) g1.DrawRectangle(myPen, xs, ys, (xe - xs), (ye - ys));
                    else g1.DrawRectangle(myPen, xs, ye, (xe - xs), (ys - ye));
                }
                else
                {
                    if (ys < ye) g1.DrawRectangle(myPen, xe, ys, (xs - xe), (ye - ys));
                    else g1.DrawRectangle(myPen, xe, ye, (xs - xe), (ys - ye));
                }
                Rectangle rectangleObj = new Rectangle(10, 10, 200, 200);
                g1.DrawRectangle(myPen, rectangleObj);
                finished = true;
                g1.Dispose();
            }

        }
        private void Form1_Load(object sender, EventArgs e)
        {
            start();
            init();
            mandelbrot();

        }

        public void start()
        {
            action = false;
            rectangle = false;
            initvalues();
            xzoom = (xende - xstart) / (double)x1;
            yzoom = (yende - ystart) / (double)y1;
            Pen p = new Pen(Color.Red, 2);
            mandelbrot();
        }

        public void stop()
        {
        }

        public void paint(Graphics g)
        {
            update(g);
        }

        public void update(Graphics g)
        {

        }

        private void mandelbrot()
        {
            int x, y;
            float h, b, alt = 0.0f;
            Pen p = new Pen(Color.Red, 2); //changed
            action = false;
            this.Cursor = Cursors.WaitCursor;

            this.statusStrip2 = new System.Windows.Forms.StatusStrip();
            this.statusStrip2.Text = "statusStrip2";

            for (x = 0; x < x1; x += 2)
                for (y = 0; y < y1; y++)
                {
                    h = pointcolour(xstart + xzoom * (double)x, ystart + yzoom * (double)y); // color value
                    if (h != alt)
                    {
                        alt = h;
                    }
                    b = 1.0f - h * h; // brightness

                    Color col = HSBColor.FromHSB(new HSBColor(h * 255, 0.8f * 255, b * 255));
                    p = new Pen(col);
                    g1.DrawLine(p, x, y, x + 1, y);
                }
            this.Cursor = Cursors.Cross;
            action = true;
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }




    }

}


