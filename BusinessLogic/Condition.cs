using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sudoku.BusinessLogic
{
    class Condition
    {
        public readonly int id;

        private static int globalId = 0;

        public Condition()
        {
            id = globalId++;
        }

        public int getId()
        {
            return id;
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return id == ((Condition)obj).id;
        }
    }
}
