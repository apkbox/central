namespace Central
{
    class Program
    {
        /// <summary>
        /// Main entry point.
        /// </summary>
        /// <remarks>
        /// <para>
        ///  -build directory         Source and symbol directory root.
        ///  -sym file                PDB or EXE file. This option may be specified more than once.
        ///                           This option may be specified more than once as soon as
        ///                           all directories have non-empty common path prefix.
        ///  -sym dir                 Directory containing PDB or EXE.
        ///                           This option may be specified more than once as soon as
        ///                           all directories have non-empty common path prefix.
        ///  -sym @file_list          List of files.
        ///  -src sources_dir         Directory that designates source files to be collected.
        ///                           Utility will not collect referenced files outside this location.
        ///                           May be specified more than once.
        ///  -store store_dir         Symbol and source store directory.
        ///                           Binaries are stored in src subdirectory, while
        ///                           sources are stored in sym subdirectory.
        ///  -symstore sym_store_dir  Symbol store directory.
        ///  -srcstore src_store_dir  Source code store directory.
        ///  -product                 Product name
        ///  -version                 Version
        ///  -comment                 Comment
        ///  -exclude pattern         Exclude files matching the specified pattern.
        ///                           This option can be specified more than once.
        ///                           The pattern matching applies to both source and
        ///                           PDB files.
        ///  -exclude @pattern_file
        /// </para>
        /// </remarks>
        /// <param name="args"></param>
        private static void Main(string[] args)
        {
            var parameters = new Parameters();
            if (!parameters.ParseCommandLine(args))
            {
                return;
            }

            if (!parameters.Validate())
            {
                return;
            }

            var engine = new Engine(parameters);
            engine.Run();

            return;
        }
    }
}
