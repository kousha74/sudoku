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

        private readonly int size;
        private static int globalId = 0;
        public readonly int id;
        public bool updateNeeded = false;

        //once one condition of a clique is satisfied, we are done with it
        public bool active = true;

        public Clique(int size)
        {
            this.size = size;
            id = globalId++;
            reset();
        }

        public void reset()
        {
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
            foreach (Condition condition in conditions)
            {
                if ((condition.GetStatus() == Condition.Status.UNDECIDED) && (!other.hasCondition(condition)))
                {
                    return false;
                }
            }

            return true;
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

        public bool needsUpdate()
        {
            return updateNeeded;
        }

        public bool updateClique()
        {
            bool updated = false;
            if (active)
            {
                int undecidedConditions = 0;
                Condition undecidedCondition = null;
                //check if there's only one undecided condition
                foreach(Condition condition in conditions)
                {
                    if (condition.GetStatus() == Condition.Status.UNDECIDED)
                    {
                        undecidedConditions++;
                        undecidedCondition = condition;
                    }

                    if (undecidedConditions > 1)
                    {
                        break;
                    }
                }

                if (undecidedConditions ==1)
                {
                    active = false;
                    undecidedCondition.setStatus(Condition.Status.SATISFIED);
                    updated = true;
                }
            }
            updateNeeded = false;

            return updated;
        }
    }
}
