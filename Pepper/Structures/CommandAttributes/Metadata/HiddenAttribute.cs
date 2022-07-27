using System;

namespace Pepper.Structures.CommandAttributes.Metadata
{
    /// <summary>
    /// Specify this on modules/commands to hide them from everyone except owners in <see cref="Pepper.Commands.General.Help"/> results.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class HiddenAttribute : Attribute { }
}