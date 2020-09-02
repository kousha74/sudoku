using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sudoku.BusinessLogic
{
    class Clique
    {
        public readonly HashSet<ConditionInfo> conditions = new HashSet<ConditionInfo>();

        private readonly int size;
        private static int globalId = 0;
        public readonly int id;
        public bool updateNeeded = false;

        //once one condition of a clique is satisfied, we are done with it
        public bool active = true;
        public readonly string name;
        private readonly AbstractSudoku abstractSudoku;
        List<ConditionInfo> undecidedConditions = new List<ConditionInfo>();

        public Clique(int size, string name, AbstractSudoku abstractSudoku)
        {
            this.size = size;
            this.name = name;
            this.abstractSudoku = abstractSudoku;
            id = globalId++;
            Logger.Instance.WriteLine(Logger.LogLevel.ERROR, "Id = " + id.ToString() + ", Clique Created: " + name);
            reset();
        }

        public void reset()
        {
            updateNeeded = false;
            active = true;
            buildUndecidedConditions();
        }

        public void addCondition(ConditionInfo condition)
        {
            conditions.Add(condition);
        }

        public int getCommonConditionCount(Clique other)
        {
            int count = 0;

            foreach (ConditionInfo conditionInfo in conditions)
            {
                if (other.hasCondition(conditionInfo))
                {
                    count++;
                }
            }

            return count;
        }

        //checks of the other clique has all the UNDECIDED conditions
        public bool isSubsetOf(Clique other)
        {
          /*  if ((!other.active) || (!active))
            {
                return false;
            }

            foreach (Condition condition in conditions)
            {
                if ((condition.GetStatus() == Condition.Status.UNDECIDED) && (!other.hasCondition(condition)))
                {
                    return false;
                }
            }*/

            return true;
        }

        public bool hasCondition(ConditionInfo otherCondition)
        {
            foreach(ConditionInfo conditionInfo in conditions)
            {
                if (conditionInfo.condition.getId() == otherCondition.condition.getId())
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
            undecidedConditions.Clear();
        }

        public bool needsUpdate()
        {
            return updateNeeded;
        }

        public Outcome updateClique()
        {
            Outcome outcome = Outcome.NO_CHANGE;
            if (active)
            {
                buildUndecidedConditions();

                if (undecidedConditions.Count == 0)
                {
                    return Outcome.FAILED;
                }

                outcome = checkHiddenSingles();

                if (outcome == Outcome.FAILED)
                {
                    return outcome;
                }

                //check for subsets
                foreach(Clique clique in abstractSudoku.getCommonCliques(undecidedConditions))
                {
                    //if it's not this clique
                    if (clique.id != id)
                    {
                        switch (clique.removeAllBut(undecidedConditions))
                        {
                            case Outcome.FAILED:
                                return Outcome.FAILED;

                            case Outcome.UPDATED:
                                outcome = Outcome.UPDATED;
                                break;

                            case Outcome.NO_CHANGE:
                                break;
                        }

                    }
                }

                switch (checkForPairs())
                {
                    case Outcome.FAILED:
                        return Outcome.FAILED;

                    case Outcome.UPDATED:
                        outcome = Outcome.UPDATED;
                        break;

                    case Outcome.NO_CHANGE:
                        break;
                }
            }

            updateNeeded = false;
       
            return outcome;
        }

        private Outcome checkHiddenSingles()
        {
            if (undecidedConditions.Count == 1)
            {
                active = false;
                return undecidedConditions[0].setStatus(Status.SATISFIED);
            }

            return Outcome.NO_CHANGE;
        }

        private Outcome checkForPairs()
        {
            Outcome outcome = Outcome.NO_CHANGE;

            if (active && undecidedConditions.Count == 2 )
            {
                ConditionInfo A = undecidedConditions[0];
                ConditionInfo B = undecidedConditions[1];

                List<Bridge> bridges = abstractSudoku.findBridges(A);

                foreach(Bridge bridge1 in abstractSudoku.findBridges(A))
                {
                    foreach (Bridge bridge2 in abstractSudoku.findBridges(bridge1.extension))
                    {
                        if (bridge2.extension.condition.id == A.condition.id)
                        {
                            switch (abstractSudoku.applyBridge(bridge1))
                            {
                                case Outcome.FAILED:
                                    return Outcome.FAILED;

                                case Outcome.UPDATED:
                                    outcome = Outcome.UPDATED;
                                    break;

                                case Outcome.NO_CHANGE:
                                    break;
                            }

                            switch (abstractSudoku.applyBridge(bridge2))
                            {
                                case Outcome.FAILED:
                                    return Outcome.FAILED;

                                case Outcome.UPDATED:
                                    outcome = Outcome.UPDATED;
                                    break;

                                case Outcome.NO_CHANGE:
                                    break;
                            }
                        }
                    }
                }
            }

            return outcome;
        }

        public Outcome removeAllBut(List<ConditionInfo> excludedconditions)
        {
            Outcome outcome = Outcome.NO_CHANGE;

            foreach(ConditionInfo info in conditions)
            {
                bool isExcluded = false;
                foreach(ConditionInfo excludedcondition in excludedconditions)
                {
                    if(info.condition.id == excludedcondition.condition.id)
                    {
                        isExcluded = true;
                        break;
                    }
                }

                if (!isExcluded)
                {
                    switch (info.setStatus(Status.NOT_SATISFIED))
                    {
                        case Outcome.FAILED:
                            return Outcome.FAILED;

                        case Outcome.UPDATED:
                            outcome = Outcome.UPDATED;
                            break;

                        case Outcome.NO_CHANGE:
                            break;
                    }
                }
            }

            return outcome;
        }

        private void buildUndecidedConditions()
        {
            undecidedConditions.Clear();

            foreach(ConditionInfo condition in conditions)
            {
                if (condition.getStatus() == Status.UNDECIDED)
                {
                    undecidedConditions.Add(condition);
                }
            }
        }

        public List<ConditionInfo> getUndecidedConditions()
        {
            return undecidedConditions;
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return id == ((Clique)obj).id;
        }


    }
}
