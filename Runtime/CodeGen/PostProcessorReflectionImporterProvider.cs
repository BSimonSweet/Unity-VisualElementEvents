using Mono.Cecil;

namespace BsiGame.CodeGen
{
    /**
     * Source : from Unity's Netcode for GameObject package
     */
    public class PostProcessorReflectionImporterProvider : IReflectionImporterProvider
    {
        public IReflectionImporter GetReflectionImporter(ModuleDefinition moduleDefinition)
        {
            return new PostProcessorReflectionImporter(moduleDefinition);
        }
    }
}
