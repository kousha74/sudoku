using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sudoku.BusinessLogic
{
    class Clique
    {
        public readonly List<Condition> conditions = new List<Condition>();

        //cliques that have share than one condition with this one
        public readonly List<Clique> cliques = new List<Clique>();
        private readonly List<Clique> siblings = new List<Clique>();

        private readonly int size;
        private static int globalId = 0;
        public readonly int id;
        public bool updateNeeded = false;

        //once one condition of a clique is satisfied, we are done with it
        public bool active = true;

        private ushort undecidedConditions = 0;

        public Clique(int size)
        {
            this.size = size;
            id = globalId++;
            reset();
        }

        public void reset()
        {
            undecidedConditions = 0;
            for (int i = 0; i < size;i++)
            {
                undecidedConditions |= Bits.BITS[i];
            }
            updateNeeded = false;
            active = true;
        }

        public void addCondition(Condition condition)
        {
            conditions.Add(condition);
        }

        public void addClique(Clique clique)
        {
            cliques.Add(clique);
        }

        public void addSibling(Clique clique)
        {
            siblings.Add(clique);
        }

        public int getCommonConditionCount(Clique other)
        {
            int count = 0;

            foreach (Condition condition in conditions)
            {
                if (other.hasCondition(condition))
                {
                    count++;
                }
            }

            return count;
        }

        //checks of the other clique has all the UNDECIDED conditions
        public bool isSubsetOf(Clique other)
        {
            if ((!other.active) || (!active))
            {
                return false;
            }

            foreach (Condition condition in conditions)
            {
                if ((condition.GetStatus() == Condition.Status.UNDECIDED) && (!other.hasCondition(condition)))
                {
                    return false;
                }
            }

            return true;
        }

        public bool checkIfSuperSet(Clique other)
        {
            bool errorFound = false;

            //if this clique is a superet of another clique, then it can be removed
            if (other.isSubsetOf(this))
            {
                foreach (Condition condition in conditions)
                {
                    if (!other.hasCondition(condition))
                    {
                        if (!condition.setStatus(Condition.Status.NOT_SATISFIED))
                        {
                            errorFound = true;
                        }
                    }
                }
            }

            return !errorFound;
        }

        public bool hasCondition(Condition newCondition)
        {
            foreach(Condition condition in conditions)
            {
                if (condition.getId() == newCondition.getId())
                {
                    return true;
                }
            }

            return false;
        }

        public void setUpdateNeeded()
        {
            if (active)
            {
                updateNeeded = true;
            }
        }

        public void deactivate()
        {
            active = false;
        }

        public bool needsUpdate()
        {
            return updateNeeded;
        }

        public bool updateClique()
        {
            bool updated = false;
            if (active)
            {
                updateUndecidedConditions();
                //check for subsets
                foreach (Clique clique in cliques)
                {
                    clique.checkIfSuperSet(this); //tbd check return value
                }

                updated = checkHiddenSingles();
            }
            updateNeeded = false;
       
            return updated;
        }

        private void updateUndecidedConditions()
        {
            if (active) {
                undecidedConditions = 0;

                for (int i = 0; i < size; i++)
                {
                    if (conditions[i].GetStatus() == Condition.Status.UNDECIDED)
                    {
                        undecidedConditions |= Bits.BITS[i];
                    }
                }
            }
        }

        private bool checkHiddenSingles()
        {
            if (Bits.countSetBits(undecidedConditions) == 1)
            {
                active = false;
                int index = Bits.firstSetBits(undecidedConditions);
                conditions[index].setStatus(Condition.Status.SATISFIED);
                return true;
            }

            return false;
        }

        //2 cliques are siblings if the start cell and end cell are the same
        public bool isSibling(Clique other)
        {
            return
                (conditions.First().row == other.conditions.First().row) &&
                (conditions.First().col == other.conditions.First().col) &&
                (conditions.Last().row == other.conditions.Last().row) &&
                (conditions.Last().col == other.conditions.Last().col);
        }

        public override string ToString()
        {
            string str = "";
            if (conditions[0].number == conditions[1].number)
            {
                str += "Clique " + id.ToString() + " => Number = " + conditions[0].number.ToString();
                if (conditions.Last().row == conditions.First().row)
                {
                    str += ", ROW,";
                }
                else if (conditions.Last().col == conditions.First().col)
                {
                    str += ", COLUMN,";
                }
                else
                {
                    str += ", BOX,";
                }

                str += "(" + conditions.First().row.ToString() + conditions.First().col.ToString() + ")";
                str += "(" + conditions.Last().row.ToString() + conditions.Last().col.ToString() + ")";            
            }
            else //this is a cell clique
            {
                str += "Clique " + id.ToString() + " => Cell(" + conditions[0].row.ToString() + conditions[0].col.ToString() + ")";
            }
            
            return str;
        }

        public ushort getUndecidedConditions()
        {
            return undecidedConditions;
        }
    }
}
