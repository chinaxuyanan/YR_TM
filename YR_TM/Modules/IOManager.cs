using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YR_TM
{
    public class IOFilePath
    {
        public string MAGFilePath = "IOConfig\\MAG-IO.xlsx";
        public string TAMFilePath = "IOConfig\\TAM-IO.xlsx";
    }

    public class IOPoint
    {
        public string Address {  get; set; }
        public string Name { get; set; }
        public string Description {  get; set; }
        public bool State {  get; set; } = false;
    }

    public class IOConfig
    {
        public List<IOPoint> Inputs { get; set; } = new List<IOPoint>();
        public List<IOPoint> Outputs { get; set; } = new List<IOPoint>();
    }
}
