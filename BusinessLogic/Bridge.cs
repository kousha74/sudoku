using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sudoku.BusinessLogic
{
    class Bridge
    {
        public readonly Clique clique;
        public readonly ConditionInfo start;
        public readonly ConditionInfo end;
        public readonly ConditionInfo extension;

        public Bridge(Clique clique, ConditionInfo start, ConditionInfo end, ConditionInfo extension)
        {
            this.clique = clique;
            this.start = start;
            this.end = end;
            this.extension = extension;
        }
    }
}
