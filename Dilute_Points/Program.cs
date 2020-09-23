using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Xml;
using System.Windows.Forms;

namespace Dilute_Points
{
    class Program
    {
        static void Main(string[] args)
        {

            PreplotSet preplotSet = new PreplotSet();

            //初始化一个xml实例
            XmlDocument xml = new XmlDocument();
            xml.Load("..\\..\\test_data\\CSSE4D_streamer_simple.klmd");
            XmlNodeList preplotsNodeList = xml.SelectNodes("/KLMarineData/KLMDPreplotSet/KLMDPreplot");
            foreach (XmlNode preplotNode in preplotsNodeList)
            {
                Preplot preplot = new Preplot();
                foreach (XmlNode node in preplotNode.ChildNodes)
                {
                    XmlElement n = (XmlElement)node;
                    if (n.Name == "id")
                    {
                        preplot.id = int.Parse(n.InnerText);
                    }
                }

                XmlNodeList inflextionNodeList = preplotNode.SelectNodes("Inflextions/Inflextion");
                foreach (XmlNode node in inflextionNodeList)
                {
                    Inflextion inflextion = new Inflextion();
                    foreach (XmlNode attrNode in node.ChildNodes)
                    {
                        XmlElement n = (XmlElement)attrNode;
                        if (n.Name == "id")
                        {
                            inflextion.id = int.Parse(n.InnerText);
                        }
                        else if (n.Name == "name")
                        {
                            inflextion.name = n.InnerText;
                        }
                        else if (n.Name == "inflextionType")
                        {
                            inflextion.inflextionType = n.InnerText;
                        }
                        else if (n.Name == "x")
                        {
                            inflextion.x = double.Parse(n.InnerText);
                        }
                        else if (n.Name == "y")
                        {
                            inflextion.y = double.Parse(n.InnerText);
                        }
                        else if (n.Name == "oX")
                        {
                            inflextion.x = double.Parse(n.InnerText);
                        }
                        else if (n.Name == "oY")
                        {
                            inflextion.y = double.Parse(n.InnerText);
                        }
                    }
                    preplot.inflextions.Add(inflextion);
                }

                preplotSet.preplots.Add(preplot);
            }


            // 抽稀
            double DEGREE = 10.0;
            Dictionary<int, List<Inflextion>> sparsedPreplots = new Dictionary<int, List<Inflextion>>();
            foreach (Preplot preplot in preplotSet.preplots)
            {
                // 添加第一个点，第一个点必须在集合中
                List<Inflextion> sparsedInfList = new List<Inflextion>();
                sparsedInfList.Add(preplot.inflextions[0]);
                // 遍历后续点进行抽稀
                for (int i = 1; i < preplot.inflextions.Count - 1; i++)
                {
                    Inflextion inflextion1 = sparsedInfList[sparsedInfList.Count - 1]; // 最新抽稀的点
                    Inflextion inflextion2 = preplot.inflextions[i];
                    Inflextion inflextion3 = preplot.inflextions[i + 1];
                    Vector v1 = new Vector(inflextion1.x, inflextion1.y, inflextion2.x, inflextion2.y);
                    Vector v2 = new Vector(inflextion2.x, inflextion2.y, inflextion3.x, inflextion3.y);
                    if (Vector.IsSameStraight(v1, v2, DEGREE) == false)
                    {
                        sparsedInfList.Add(inflextion2);
                    }
                }
                // 添加最后一个点
                sparsedInfList.Add(preplot.inflextions[preplot.inflextions.Count - 1]);
                sparsedPreplots.Add(preplot.id, sparsedInfList);
            }

            // 剪裁xml
            foreach (XmlNode preplotNode in preplotsNodeList)
            {
                string preplotId = "-999";
                foreach (XmlNode node in preplotNode.ChildNodes)
                {
                    XmlElement n = (XmlElement)node;
                    if (n.Name == "id")
                    {
                        preplotId = n.InnerText;
                    }
                }

                XmlNodeList inflextionNodeList = preplotNode.SelectNodes("Inflextions/Inflextion");
                XmlElement newNode = xml.CreateElement("Inflextions");
                foreach (XmlNode node in inflextionNodeList)
                {
                    string infId = "-99";
                    foreach (XmlNode attrNode in node.ChildNodes)
                    {
                        XmlElement n = (XmlElement)attrNode;
                        if (n.Name == "id")
                        {
                            infId = n.InnerText;
                        }
                    }

                    // 在集合中则保留，否则删除
                    Inflextion inf = getInflextionFromSparsed(preplotId, infId, sparsedPreplots);
                    if (inf != null)
                    {
                        inf.inflextionType = "0";
                        newNode.AppendChild(createInflextionNode(xml, inf));
                    }
                }
                preplotNode.RemoveChild(preplotNode.SelectSingleNode("Inflextions"));
                preplotNode.AppendChild(newNode);
            }

            xml.Save("..\\..\\test_data\\CSSE4D_streamer_sparsed_DEGREE" + DEGREE + ".klmd");

            // 画图
            List<Point> points = new List<Point>();
            foreach (int preplotId in sparsedPreplots.Keys)
            {
                foreach (Inflextion inf in sparsedPreplots[preplotId])
                {
                    points.Add(new Point(inf.x, inf.y));
                }
            }
            drawInflextions(points, "..\\..\\test_data\\out-points-sparsed.bmp");
            drawInflextions(sparsedPreplots, "..\\..\\test_data\\out-lines-sparsed.bmp");

            Console.WriteLine("Dilute " + preplotSet.preplots.Count + " preplots");

        }


        static Inflextion getInflextionFromSparsed(string preplotId, string infId, Dictionary<int, List<Inflextion>> sparsedPreplots)
        {
            int pid = int.Parse(preplotId);
            int iid = int.Parse(infId);

            if (sparsedPreplots[pid] == null)
                return null;

            foreach (Inflextion inf in sparsedPreplots[pid])
            {
                if (inf.id == iid)
                    return inf;
            }

            return null;
        }


        static XmlElement createInflextionNode(XmlDocument xml, Inflextion inf)
        {
            XmlElement newNode = xml.CreateElement("Inflextion");
            XmlElement idNode = xml.CreateElement("id");
            idNode.InnerText = inf.id.ToString();
            newNode.AppendChild(idNode);
            XmlElement nameNode = xml.CreateElement("name");
            nameNode.InnerText = inf.name;
            newNode.AppendChild(nameNode);
            XmlElement inflextionTypeNode = xml.CreateElement("inflextionType");
            inflextionTypeNode.InnerText = inf.inflextionType;
            newNode.AppendChild(inflextionTypeNode);
            XmlElement xNode = xml.CreateElement("x");
            xNode.InnerText = inf.x.ToString();
            newNode.AppendChild(xNode);
            XmlElement yNode = xml.CreateElement("y");
            yNode.InnerText = inf.y.ToString();
            newNode.AppendChild(yNode);
            return newNode;
        }


        static void drawInflextions(List<Point> allPoints, string outpath)
        {
            Bitmap bmp = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            Graphics g = Graphics.FromImage(bmp);
            g.FillRectangle(new SolidBrush(Color.White), new Rectangle(0, 0, bmp.Width, bmp.Height));
            double minx = 999999999;
            double maxx = -999999999;
            double miny = 999999999;
            double maxy = -999999999;
            foreach (Point pt in allPoints)
            {
                if (pt.x < minx) minx = pt.x;
                if (pt.y < miny) miny = pt.y;
                if (pt.x > maxx) maxx = pt.x;
                if (pt.y > maxy) maxy = pt.y;
            }
            double scalex = bmp.Width / Math.Abs(maxx - minx);
            double scaley = -bmp.Height / Math.Abs(maxy - miny);


            foreach (Point pt in allPoints)
            {
                Point p = pt;
                g.FillRectangle(new SolidBrush(Color.Green), new Rectangle((int)((p.x - minx) * scalex), (int)((p.y - miny) * scaley + bmp.Height), 2, 2));
            }

            bmp.Save(outpath);
        }


        static void drawInflextions(Dictionary<int, List<Inflextion>> sparsedPreplots, string outpath)
        {
            List<Point> allPoints = new List<Point>();
            foreach (int preplotId in sparsedPreplots.Keys)
            {
                foreach (Inflextion inf in sparsedPreplots[preplotId])
                {
                    allPoints.Add(new Point(inf.x, inf.y));
                }
            }

            Bitmap bmp = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            Graphics g = Graphics.FromImage(bmp);
            g.FillRectangle(new SolidBrush(Color.White), new Rectangle(0, 0, bmp.Width, bmp.Height));
            double minx = 999999999;
            double maxx = -999999999;
            double miny = 999999999;
            double maxy = -999999999;
            foreach (Point pt in allPoints)
            {
                if (pt.x < minx) minx = pt.x;
                if (pt.y < miny) miny = pt.y;
                if (pt.x > maxx) maxx = pt.x;
                if (pt.y > maxy) maxy = pt.y;
            }
            double scalex = bmp.Width / Math.Abs(maxx - minx);
            double scaley = -bmp.Height / Math.Abs(maxy - miny);


            System.Drawing.PointF[] thePoints = new System.Drawing.PointF[allPoints.Count];
            foreach (int preplotId in sparsedPreplots.Keys)
            {
                for (int i = 0; i < sparsedPreplots[preplotId].Count - 1; i++)
                {
                    float x1, y1, x2, y2;
                    {
                        Inflextion inf = sparsedPreplots[preplotId][i];
                        float x = (float)((inf.x - minx) * scalex);
                        float y = (float)((inf.y - miny) * scaley + bmp.Height);
                        x1 = x;
                        y1 = y;
                    }
                    {
                        Inflextion inf = sparsedPreplots[preplotId][i + 1];
                        float x = (float)((inf.x - minx) * scalex);
                        float y = (float)((inf.y - miny) * scaley + bmp.Height);
                        x2 = x;
                        y2 = y;
                    }
                    g.DrawLine(new Pen(new SolidBrush(Color.Black)), x1, y1, x2, y2);
                }
            }

            foreach (Point pt in allPoints)
            {
                Point p = pt;
                g.FillRectangle(new SolidBrush(Color.Green), new Rectangle((int)((p.x - minx) * scalex), (int)((p.y - miny) * scaley + bmp.Height), 2, 2));
            }

            bmp.Save(outpath);
        }

    }



    public class Point
    {
        public int rowNo;
        public int lineNo;
        public int ptNo;
        public double x;
        public double y;

        public double distance(Point that)
        {
            return Math.Sqrt((x - that.x) * (x - that.x) + (y - that.y) * (y - that.y));
        }


        public double X
        {
            get
            {
                return x;
            }
            set
            {
                x = value;
            }
        }

        public double Y
        {
            get
            {
                return y;
            }
            set
            {
                y = value;
            }
        }

        public Point()
        {

        }

        public Point(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public Point(Point p)
        {
            this.x = p.x;
            this.y = p.y;
        }

        public static Point operator +(Point p, double n)
        {
            p.x += n;
            p.y += n;
            return p;
        }

        public static Point operator +(Point p, Point pp)
        {
            p.x += pp.x;
            p.y += pp.y;
            return p;
        }

        public static Point operator +(Point p, Vector v)
        {
            p.x += v.X;
            p.y += v.Y;
            return p;
        }

        // P指向Q的向量
        public static Vector operator -(Point p, Point q)
        {
            Vector v = new Vector(q.x - p.x, q.y - p.y);
            return v;
        }

        public static Point operator -(Point p, Vector v)
        {
            p.x -= v.X;
            p.y -= v.Y;
            return p;
        }

        public static Point operator *(Point p, double a)
        {
            p.x *= a;
            p.y *= a;
            return p;
        }

        public static Point operator /(Point p, double a)
        {
            p.x /= a;
            p.y /= a;
            return p;
        }

        public Point Clone()
        {
            Point p = new Point();
            p.x = this.x;
            p.y = this.y;
            return p;
        }

        public System.Drawing.Point toSysPoint()
        {
            return new System.Drawing.Point((int)x, (int)y);
        }
    }

    public class Rect
    {
        public double x;
        public double y;
        public double width;
        public double height;

        public bool containsPoint(Point pt)
        {
            return x <= pt.x && pt.x <= (x + width) && y <= pt.y && pt.y <= (y + height);
        }
    }

    class PreplotSet
    {
        public List<Preplot> preplots;
        public PreplotSet()
        {
            preplots = new List<Preplot>();
        }
    }

    class Preplot
    {
        public int id;
        public List<Inflextion> inflextions;
        public Preplot()
        {
            inflextions = new List<Inflextion>();
        }
    }

    class Inflextion
    {
        public int id;
        public string name = "";
        public string inflextionType = "0";
        public double x;
        public double y;
    }

    /// <summary>
    /// 向量，不可变类
    /// </summary>
    [Serializable]
    public class Vector
    {
        private double length = Double.NaN;
        private double x;
        private double y;
        private Vector norm;
        private Vector perpendicular; // 转90度的向量,延正向

        public Vector(Point start, Point end)
        {
            this.x = end.x - start.x;
            this.y = end.y - start.y;
        }

        public Vector(double x1, double y1, double x2, double y2)
        {
            this.x = x2 - x1;
            this.y = y2 - y1;
        }

        public Vector(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public Vector(Vector v)
        {
            this.x = v.x;
            this.y = v.y;
        }

        public static Vector operator +(Vector lhs, Vector rhs)
        {
            Vector result = new Vector(lhs);
            result.x += rhs.x;
            result.y += rhs.y;
            return result;
        }

        public static Vector operator -(Vector lhs, Vector rhs)
        {
            Vector result = new Vector(lhs);
            result.x -= rhs.x;
            result.y -= rhs.y;
            return result;
        }

        public static Vector operator ^(Vector lhs, double a)
        {
            Vector result = new Vector(lhs);
            result.x *= a;
            result.y *= a;
            return result;
        }

        public static double operator *(Vector lhs, Vector rhs)
        {
            double mul = lhs.x * rhs.x + lhs.y * rhs.y;
            return mul;
        }


        public double X
        {
            get { return x; }
            set { throw new Exception("不能赋值"); }
        }
        public double Y
        {
            get { return y; }
            set { throw new Exception("不能赋值"); }
        }

        public double Length
        {
            get
            {
                if (Double.IsNaN(length))
                {
                    length = Math.Sqrt(this.x * this.x + this.y * this.y);
                    return length;
                }
                else
                {
                    return length;
                }
            }
            set { throw new Exception("不能赋值"); }
        }

        public Vector Norm
        {
            get
            {
                if (norm == null)
                {
                    norm = this ^ (1 / this.Length);
                    return norm;
                }
                else
                {
                    return norm;
                }
            }
            set { throw new Exception("不能赋值"); }
        }

        public Vector Perpendicular
        {
            get
            {
                if (perpendicular == null)
                {
                    perpendicular = new Vector(this.y * -1, this.x);
                    return perpendicular;
                }
                else
                {
                    return perpendicular;
                }
            }
            set { throw new Exception("不能赋值"); }
        }

        public bool isNonZero()
        {
            return X != 0 || Y != 0;
        }

        public static double CosToVector(Vector v, Vector w)
        {
            double cos = v * w / v.Length / w.Length;
            return cos;
        }


        public static bool IsSameSide(Vector v, Vector w)
        {
            double cos = Vector.CosToVector(v, w);
            return 0 <= cos && cos <= 1.1; //允许误差
        }


        public static bool IsSameStraight(Vector v, Vector w, double degree)
        {
            double cos = Vector.CosToVector(v, w);
            double limit = Math.Cos(degree * Math.PI / 180);
            return limit <= cos && cos <= 1.1;
        }


        public static Vector Project(Vector v, Vector toVector)
        {
            Vector proj = toVector ^ (v * toVector / (toVector * toVector));
            return proj;
        }

        public Vector rotate(double radian)
        {
            Vector v = new Vector(Math.Cos(radian) * x - Math.Sin(radian) * y,
                                    Math.Sin(radian) * x + Math.Cos(radian) * y);
            return v;
        }
    }
}
