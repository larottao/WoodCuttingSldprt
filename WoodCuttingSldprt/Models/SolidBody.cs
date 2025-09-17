using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoodCuttingSldprt.Models
{
    public class SolidBody
    {
        public SolidBody(Body2 argBody)
        {
            body = argBody;
            box = (double[])body.GetBodyBox();

            xCm = (box[3] - box[0]) * 100.0;
            yCm = (box[4] - box[1]) * 100.0;
            zCm = (box[5] - box[2]) * 100.0;
            longestAxis = Math.Max(xCm, Math.Max(yCm, zCm));
        }

        public Body2 body { get; set; }

        public string Name
        { get { return body.Name; } }

        public double[] box { get; set; }
        public double xCm { get; set; }
        public double yCm { get; set; }
        public double zCm { get; set; }
        public double longestAxis { get; set; }
    }
}