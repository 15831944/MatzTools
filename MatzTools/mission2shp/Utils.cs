using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace mission2shp
{
    public static class Utils
    {
        public static bool ChkArgs(string[] args, ref string _in, ref OSGeo.OSR.SpatialReference _sr, ref string _out)
        {
            bool argOK = false;
            int check = 0;

            try
            {
                for (int i= 0; i < args.Length; i++)
            {
                    switch(args[i])
                    {
                        case "-i":
                            _in = args[i + 1];
                            check += 1;
                            break;

                        case "-cs":
                            int.TryParse(args[i + 1], out int srid);
                            _sr.ImportFromEPSG(srid);
                            check += 10;
                            break;

                        case "-o":
                            _out = args[i + 1];
                            if (Path.GetExtension(_out) != ".shp")
                                _out += ".shp";
                            check += 100;
                            break;
                    }
                    if (check == 111)
                        argOK = true;
                }
            }
            catch { }
            return argOK;
        }
    } 
}
