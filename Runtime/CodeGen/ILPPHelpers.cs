using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Unity.CompilationPipeline.Common.ILPostProcessing;

namespace BsiGame.CodeGen
{
	public static class ILPPHelpers
	{
		// INTERFACES

		public static AssemblyDefinition OpenCompiledAssembly(ICompiledAssembly assembly, out PostProcessorAssemblyResolver assemblyResolver)
		{
			assemblyResolver = new PostProcessorAssemblyResolver(assembly);
			var readerParameters = new ReaderParameters
			{
				SymbolStream               = new MemoryStream(assembly.InMemoryAssembly.PdbData),
				SymbolReaderProvider       = new PortablePdbReaderProvider(),
				AssemblyResolver           = assemblyResolver,
				ReflectionImporterProvider = new PostProcessorReflectionImporterProvider(),
				ReadingMode                = ReadingMode.Immediate
			};

			var assemblyDefinition = AssemblyDefinition.ReadAssembly(new MemoryStream(assembly.InMemoryAssembly.PeData), readerParameters);

			assemblyResolver.AddAssemblyDefinitionBeingOperatedOn(assemblyDefinition);

			return assemblyDefinition;
		}

		public static ILPostProcessResult WriteAssembly(AssemblyDefinition assembly)
		{
			var pe  = new MemoryStream();
			var pdb = new MemoryStream();

			var writerParameters = new WriterParameters
			{
				SymbolWriterProvider = new PortablePdbWriterProvider(),
				SymbolStream         = pdb,
				WriteSymbols         = true
			};

			assembly.Write(pe, writerParameters);

			return new ILPostProcessResult(new InMemoryAssembly(pe.ToArray(), pdb.ToArray()));
		}

		public static TypeDefinition GetTypeDefinition(this ModuleDefinition module, string name)
		{
			return module.Types.FirstOrDefault(t => t.Name == name);
		}
	}
}