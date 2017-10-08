using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Nightwatch.Core.Resources;

namespace Nightwatch.Samples.CSharp
{
    public class SampleResource
    {
        public static ResourceFactory Factory { get; } =
            new ResourceFactory("sample", CreateChecker);

        private static Func<Task<bool>> CreateChecker(IDictionary<string, string> param) =>
            () => Task.FromResult(param["a"] == param["b"]);
    }
}
