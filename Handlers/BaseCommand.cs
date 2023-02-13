using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YSGM_GUI.Handlers
{
    public interface BaseCommand
    {
        public string Execute(string[] args);
    }
}
