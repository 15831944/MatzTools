using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Globalization;
using OSGeo.OGR;

namespace mission2shp
{
    class Program
    {
        static string _in = String.Empty;
        static string _out = String.Empty;

        static void Main(string[] args)
        {
            //Ausgabe abschalten
            Console.SetOut(new StringWriter());
            GdalConfiguration.ConfigureGdal();
            GdalConfiguration.ConfigureOgr();

            //Ausgabe einschalten
            var stdOut = new StreamWriter(Console.OpenStandardOutput())
                        { AutoFlush = true };
            
            Console.SetOut(stdOut);

            //check Arguments
            OSGeo.OSR.SpatialReference _sr = new OSGeo.OSR.SpatialReference("");

            if (!Utils.ChkArgs(args, ref _in, ref _sr, ref _out))
            {
                Console.WriteLine("****    mission2shp    ****");
                Console.WriteLine("Befehlszeilenargumente:");
                Console.WriteLine("   -i: Input File, Mission Planner Polygon File (usually *.poly)");
                Console.WriteLine("  -cs: EPSG code of coordinate system");
                Console.WriteLine("        4326 WGS84");
                Console.WriteLine("       31255 MGI central Austria");
                Console.WriteLine("   -o: Output File, shp File to be generated");
                Console.WriteLine();
            }
            else
            { 

            OSGeo.OGR.Driver driver = Ogr.GetDriverByName("ESRI Shapefile");

            _in = Path.Combine(Environment.CurrentDirectory, _in);

                //Konsole
                Console.WriteLine(" -i: " + _in);
                string wkt;
                _sr.ExportToPrettyWkt(out wkt, 0);
                Console.WriteLine("-cs: " + wkt);
                Console.WriteLine(" -o:" + _out);

            //Output
            DataSource ds = driver.CreateDataSource(_out, new string[] { });
            Layer ly = ds.CreateLayer("Test", _sr, wkbGeometryType.wkbPolygon, new string[] { });

            //Felder
            OSGeo.OGR.FieldDefn fDefn = new FieldDefn("Block", FieldType.OFTString);
            fDefn.SetWidth(12);
            ly.CreateField(fDefn, 1);

            //Feature
            OSGeo.OGR.FeatureDefn featureDefn = new FeatureDefn("");
            featureDefn.AddFieldDefn(fDefn);

            try
            {
                string path = Path.GetDirectoryName(_in);
                string datei = Path.GetFileName(_in);
                string[] files = Directory.GetFiles(path, datei);

                foreach (string file in files)
                {
                    Geometry geoPL = new Geometry(wkbGeometryType.wkbCurvePolygon);
                    string Block = Path.GetFileNameWithoutExtension(file);
                    geoPL = ReadFile(file);

                    if (!geoPL.IsEmpty())
                    {
                        //Ausgabe shp File

                        OSGeo.OGR.Feature feature = new Feature(featureDefn);
                        feature.SetGeometryDirectly(geoPL);

                        feature.SetField("Block", Block);

                        ly.CreateFeature(feature);
                    }
                }
            }
            catch { Console.WriteLine("no Input"); }
            ds.Dispose();
        }

        Geometry ReadFile(string input)
            {
                Geometry geoPL = new Geometry(wkbGeometryType.wkbPolygon);
                string Text = String.Empty;
                try
                {
                    StreamReader sr = new StreamReader(input);
                    Text = sr.ReadToEnd();
                    sr.Close();
                }
                catch { }

                if (Text != String.Empty)
                {
                    string[] rows = Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                    Geometry geoRing = new Geometry(wkbGeometryType.wkbLinearRing);

                    foreach (string row in rows)
                    {
                        try
                        {
                            string[] pt = row.Split(new[] { ' ' });
                            double x = double.Parse(pt[1], CultureInfo.InvariantCulture);
                            double y = double.Parse(pt[0], CultureInfo.InvariantCulture);

                            geoRing.AddPoint_2D(x, y);
                        }
                        catch { }
                    }

                    geoPL.AddGeometry(geoRing);
                }
                return geoPL;
            }
        }
    }
}
