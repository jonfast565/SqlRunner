using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlRunner.Messages.Abstract
{
    abstract class AbstractScriptMessage
    {
        public int ScriptId { get; set; }
    }
}
