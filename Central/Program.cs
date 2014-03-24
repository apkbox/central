namespace Central
{
    class Program
    {
        /// <summary>
        /// Main entry point.
        /// </summary>
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
