using System;
using Pepper.Commons.Structures;
using Qmmands;
using Qmmands.Text;

namespace Pepper.Test.Commands.Mock
{
    public class TestCommandModule : TextModuleBase
    {
        public class ExecResult<T> : IResult
        {
            public bool IsSuccessful => true;
            public string? FailureReason => null;
            public T[] Arguments { get; init; } = Array.Empty<T>();
        }

        [TextCommand(nameof(Exec1))]
        public IResult Exec1(string a1, string a2) => new ExecResult<string> { Arguments = new[] { a1, a2 } };

        [TextCommand(nameof(Exec2))]
        public IResult Exec2([Flag("/f=")] int a1, int a2) => new ExecResult<int> { Arguments = new[] { a1, a2 } };
    }
}