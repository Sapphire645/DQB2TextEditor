using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DQB2TextEditor.Windows
{
    public interface ObtainData
    {
        public (List<List<string>>, List<bool>) GetNPCFiles();
    }
}
