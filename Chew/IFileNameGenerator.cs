using JetBrains.Annotations;

namespace Chew {
    public interface IFileNameGenerator {
        [NotNull]
        string GenerateName([NotNull] string content);
    }
}