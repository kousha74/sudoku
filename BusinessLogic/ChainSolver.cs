using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sudoku.BusinessLogic
{
    class ChainSolver
    {
        private readonly AbstractSudoku abstractSudoku;
        private readonly List<Clique> cliques;

        public ChainSolver(AbstractSudoku abstractSudoku, List<Clique> cliques)
        {
            this.abstractSudoku = abstractSudoku;
            this.cliques = cliques;
        }

        public Outcome solveChains()
        {
            Outcome outcome = Outcome.NO_CHANGE;

            foreach(Clique clique in cliques)
            {
                List<ConditionInfo> undecidedConditions = clique.getUndecidedConditions();

                if (undecidedConditions.Count == 2)
                {
                    switch (buildChain(new List<ConditionInfo>() { undecidedConditions[0], undecidedConditions[1] }))
                    {
                        case Outcome.FAILED:
                            return Outcome.FAILED;

                        case Outcome.UPDATED:
                            return Outcome.UPDATED;

                        case Outcome.NO_CHANGE:
                            break;
                    }

                    switch (buildChain(new List<ConditionInfo>() { undecidedConditions[1], undecidedConditions[0] }))
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

        //Chain starts with a RED link
        public void logChain(List<ConditionInfo> chain)
        {
            string str = "CHAIN: ";

            foreach(ConditionInfo info in chain)
            {
                str += info.condition.name + ",";
            }

            Logger.Instance.WriteLine(Logger.LogLevel.ERROR, str);
            Logger.Instance.flush();
        }
        public Outcome buildChain(List<ConditionInfo> chain)
        {
            if (chain.Count < 2)
            {
                return Outcome.NO_CHANGE;
            }

            if (chain.Count % 2 == 0)
            {
                //apply chain first
                switch (applyChain(chain))
                {
                    case Outcome.FAILED:
                        return Outcome.FAILED;

                    case Outcome.UPDATED:
                        return Outcome.UPDATED;

                    case Outcome.NO_CHANGE:
                        break;
                }

                //look for blue links
                foreach (ConditionInfo info in chain.Last().getBlueNeighbors())
                {
                    if (!containsConditionInfo(chain, info))
                    {
                        chain.Add(info);

                        switch (buildChain(chain))
                        {
                            case Outcome.FAILED:
                                return Outcome.FAILED;

                            case Outcome.UPDATED:
                                return Outcome.UPDATED;

                            case Outcome.NO_CHANGE:
                                chain.RemoveAt(chain.Count - 1);
                                break;
                        }
                    }
                }
            }
            else
            {
                //look for red links
                foreach (ConditionInfo info in chain.Last().getRedNeighbors())
                {
                    if (!containsConditionInfo(chain, info))
                    {
                        chain.Add(info);

                        switch (buildChain(chain))
                        {
                            case Outcome.FAILED:
                                return Outcome.FAILED;

                            case Outcome.UPDATED:
                                return Outcome.UPDATED;

                            case Outcome.NO_CHANGE:
                                chain.RemoveAt(chain.Count - 1);
                                break;
                        }
                    }
                }
            }

            return Outcome.NO_CHANGE;
        }

        public bool containsConditionInfo(List<ConditionInfo> chain, ConditionInfo newInfo)
        {
            foreach (ConditionInfo info in chain)
            {
                if (info.id == newInfo.id)
                {
                    return true;
                }
            }

            return false;
        }

        public Outcome applyChain(List<ConditionInfo> chain)
        {
            //chain with odd number of nodes
            if ((chain.Count > 2) && (chain.Count % 2 == 1))
            {
                foreach (Clique clique in abstractSudoku.getCommonCliques(chain.First(), chain.Last()))
                {
                    switch (clique.removeAllBut(chain.First(), chain.Last()))
                    {
                        case Outcome.FAILED:
                            return Outcome.FAILED;

                        case Outcome.UPDATED:
                            abstractSudoku.onPairFound(chain);
                            logChain(chain);
                            return Outcome.UPDATED;

                        case Outcome.NO_CHANGE:
                            break;
                    }
                }
            }

            return Outcome.NO_CHANGE;
        }

    }
}
