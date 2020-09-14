using System.Collections.Generic;

namespace TaTooIne.Demo.Models
{
    public class CalculatedState
    {
        public string FuncDescription { get; set; }

        public IReadOnlyCollection<string> InputNames { get; set; }

        public IReadOnlyCollection<string> OutputNames { get; set; }

        public IReadOnlyCollection<IReadOnlyCollection<Line>> InputValues { get; set; }

        public IReadOnlyCollection<IReadOnlyCollection<Line>> OutputValues { get; set; }
    }
}