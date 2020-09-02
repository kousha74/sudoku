using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sudoku.BusinessLogic
{
    abstract class AbstractSudoku
    {
        private Dictionary<Condition, ConditionInfo> conditions = new Dictionary<Condition, ConditionInfo>();
        private List<Clique> cliques = new List<Clique>();

        public readonly int size;
        public readonly int[] bitCounter;

        public AbstractSudoku(int size)
        {
            int allCombinations = 2 ^ size;
            bitCounter = new int[allCombinations];
            for (ushort i = 0; i <allCombinations; i++)
            {
                bitCounter[i] = Bits.countSetBits(i);
            }

            this.size = size;
        }

        public abstract void onConditionSatidfied(Condition condition);

        public bool init(List<RawData> data)
        {
            if (cliques.Count != 0)
            {
                return false;
            }

            //logging all the cliques
            foreach (RawData rawData in data)
            {
                Clique clique = new Clique(size, rawData.name, this);
                cliques.Add(clique);
                
                List<ConditionInfo> conditionInfos = new List<ConditionInfo>();

                foreach (Condition condition in rawData.conditions)
                {
                    ConditionInfo conditionInfo = null;
                    if (conditions.ContainsKey(condition))
                    {
                        conditionInfo = conditions[condition];
                    }
                    else
                    {
                        conditionInfo = new ConditionInfo(condition, this);
                        conditions.Add(condition, conditionInfo);
                    }

                    conditionInfo.addClique(clique);
                    clique.addCondition(conditionInfo);

                    foreach (ConditionInfo info in conditionInfos)
                    {
                        info.addNeighbor(conditionInfo);
                        conditionInfo.addNeighbor(info);
                    }

                    conditionInfos.Add(conditionInfo);
                }
            }

            return true;
        }

        public virtual void Reset()
        {
            foreach (KeyValuePair<Condition, ConditionInfo> entry in conditions)
            {
                entry.Value.reset();
            }

            if (cliques != null)
            {
                foreach (Clique clique in cliques)
                {
                    clique.reset();
                }
            }
        }

        //returns false if nothing happened
        public Outcome solveStep()
        {
            Outcome outcome = Outcome.NO_CHANGE;

            foreach(Clique clique in cliques)
            {
                if (clique.needsUpdate())
                {
                    switch (clique.updateClique())
                    {
                        case Outcome.FAILED:
                            return Outcome.FAILED;

                        case Outcome.UPDATED:
                            return Outcome.UPDATED;

                        case Outcome.NO_CHANGE:
                            break;
                    }
                }
            }

            return outcome;
        }

        public Outcome setStatus(Condition condition, Status status)
        {
            if (conditions.ContainsKey(condition))
            {
                return conditions[condition].setStatus(status);
            }

            return Outcome.FAILED;
        }

        public Status getStatus(Condition condition)
        {
            if (conditions.ContainsKey(condition))
            {
                return conditions[condition].getStatus();
            }

            return Status.UNDECIDED;
        }

        public List<Clique> getCommonCliques(ConditionInfo info1, ConditionInfo info2)
        {
            return getCommonCliques(new List<ConditionInfo>() { info1, info2 });
        }

        public List<Clique> getCommonCliques(List<ConditionInfo> conditionInfos)
        {
            List<Clique> cliques = new List<Clique>();

            foreach(ConditionInfo info in conditionInfos)
            {
                if (cliques.Count == 0)
                {
                    //copy the cliques
                    foreach(Clique clique in info.cliques)
                    {
                        cliques.Add(clique);
                    }
                }
                else
                {
                    cliques = cliques.Intersect(info.cliques).ToList();
                }
            }

            return cliques;
        }

        public List<Clique> getCliques(int undecidedConditions)
        {
            List<Clique> desiredCliques = new List<Clique>();

            foreach(Clique clique in cliques)
            {
                if (clique.getUndecidedConditions().Count == undecidedConditions)
                {
                    desiredCliques.Add(clique);
                }
            }

            return desiredCliques;
        }

        public List<Bridge> findBridges(ConditionInfo info)
        {
            List<Bridge> bridges = new List<Bridge>();

            foreach(Clique clique in cliques)
            {
                if (!clique.hasCondition(info)) 
                {
                    List<ConditionInfo> undecidedConditions = clique.getUndecidedConditions();

                    if (undecidedConditions.Count == 2)
                    {
                        foreach(Clique commonClique in getCommonCliques(info, undecidedConditions[0]))
                        {
                            bridges.Add(new Bridge(commonClique, info, undecidedConditions[0], undecidedConditions[1]));
                        }

                        foreach(Clique commonClique in getCommonCliques(info, undecidedConditions[1]))
                        {
                            bridges.Add(new Bridge(commonClique, info, undecidedConditions[1], undecidedConditions[0]));
                        }
                    }
                }
            }

            return bridges;
        }

        public Outcome applyBridge(Bridge bridge)
        {
            return bridge.clique.removeAllBut(new List<ConditionInfo>() { bridge.start, bridge.end });
        }

    }
}
