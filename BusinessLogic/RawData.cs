using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sudoku.BusinessLogic
{
    class RawData
    {
        public readonly List<Condition> conditions;
        public readonly string name;

        public RawData(List<Condition> conditions, string name)
        {
            this.conditions = conditions;
            this.name = name;
        }
    }
}
