using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Qmmands;

namespace Pepper.Structures.Commands
{
    public class PrefixCheckAttribute : CheckAttribute
    {
        public override ValueTask<CheckResult> CheckAsync(Qmmands.CommandContext context)
        {
            var extendedContext = (CommandContext) context;
            var command = extendedContext.Command;
            var prefix = extendedContext.Prefix;
            var category = command.Module.Attributes.OfType<PrefixCategoryAttribute>().FirstOrDefault();
            if (category != null)
            {
                extendedContext.CommandService.CategoriesByAllowedPrefixes.TryGetValue(prefix, out var allowedCategories);
                if (allowedCategories!.Count != 0 && !allowedCategories.Contains(category.PrefixCategory))
                    return Failure(
                        $"Prefix \"{prefix}\" is not allowed to invoke command with prefix category \"{category.PrefixCategory}\"");
            }

            return Success();
        }
    }
}