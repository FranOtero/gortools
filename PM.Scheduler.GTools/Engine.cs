using Google.OrTools.Sat;
using PM.Scheduler.GTools.Models;

namespace PM.Scheduler.GTools
{
    public class Engine
    {
        List<Operation> _operations = new();
        List<GSchedule> _processes = new();
        public Dictionary<Resource, List<ProcessSchedule>> Scheduling { get; set; } = new();
        public void AddOperations(params Operation[] operations)
        {
            _operations.AddRange(operations);
        }

        public bool Calculate()
        {
            #region CrearModelo
            int minTime = 0;
            int maxTime = WorseMakeSpan();

            CpModel model = new CpModel();
            Dictionary<Resource, List<IntervalVar>> resourceIntervals = new();
            List<IntVar> ends = new();
            GSchedule previous = null;
            foreach (var operation in _operations)
            {
                foreach (var process in operation.Processes)
                {
                    int minD = process.AllowedResources.Min(a => a.Duration);
                    int maxD = process.AllowedResources.Max(a => a.Duration);
                    GSchedule main = GSchedule.New(model, minTime, maxTime, minD, maxD, $"{process.Code}");
                    main.Process = process;
                    ends.Add(main.End);
                    foreach (AllowedResource allowed in process.AllowedResources)
                    {
                        GSchedule sc = GSchedule.NewOptional(model,
                             main,
                             minTime,
                             maxTime,
                             allowed.Duration,
                             $"{process.Code}@{allowed.Resource.Code}"
                             );
                        sc.Process = process;
                        sc.AllowedResource = allowed;
                        _processes.Add(sc);

                        if (!resourceIntervals.ContainsKey(allowed.Resource))
                            resourceIntervals.Add(allowed.Resource, new());
                        resourceIntervals[allowed.Resource].Add(sc.Interval);
                    }
                    model.AddExactlyOne(_processes.Where(p => p.Process == process).Select(s => s.Presence));
                    if (previous != null)
                    {
                        model.Add(previous.End <= main.Start);
                        if (previous.Process.MaxWaitTime.HasValue)
                        {
                            model.Add(main.Start - previous.End <= previous.Process.MaxWaitTime.Value);
                        }
                    }
                    previous = main;
                }
                previous = null;
            }
            //que cada máquina no solape trabajos:
            foreach (List<IntervalVar> intervals in resourceIntervals.Values)
            {
                model.AddNoOverlap(intervals);
            }

            #endregion

            #region Execute

            IntVar objVar = model.NewIntVar(0, maxTime, "TTotal");
            model.AddMaxEquality(objVar, ends);
            model.Minimize(objVar);
            CpSolver solver = new CpSolver();
            CpSolverStatus status = solver.Solve(model);
            Console.WriteLine($"Solve status: {status} - {solver.SolutionInfo()}");
            bool result = status == CpSolverStatus.Feasible || status == CpSolverStatus.Optimal;
            #endregion

            if (!result) return false;

            #region Solve
            Scheduling.Clear();
            foreach (var option in _processes)
            {
                if (solver.BooleanValue(option.Presence))
                {
                    ProcessSchedule sch = new();
                    sch.Start = (int)solver.Value(option.Start);
                    sch.End = (int)solver.Value(option.End);
                    sch.Process = option.Process;
                    Resource r = option.AllowedResource.Resource;
                    Scheduling.TryAdd(r, new());
                    Scheduling[r].Add(sch);
                }
            }
            foreach (Resource key in Scheduling.Keys)
            {
                Scheduling[key].Sort();
            }
            #endregion

            return true;
        }

        private int WorseMakeSpan()
        {
            int span = 0;
            foreach (var operation in _operations)
            {
                foreach (var process in operation.Processes)
                {
                    span += process.AllowedResources.Max(r => r.Duration);
                }
            }
            return span;
        }

    }
}
