namespace SqlRunner.Utilities
{
    internal static class Header
    {
        private const string HeaderString = @"
 _____       _______                            
/  ___|     | | ___ \                           
\ `--.  __ _| | |_/ /   _ _ __  _ __   ___ _ __ 
 `--. \/ _` | |    / | | | '_ \| '_ \ / _ \ '__|
/\__/ / (_| | | |\ \ |_| | | | | | | |  __/ |   
\____/ \__, |_\_| \_\__,_|_| |_|_| |_|\___|_|   
          | |                                   
          |_|                                   

------------------------------------------------
Author: Jon Fast
Last Modified: 12/16/2016
------------------------------------------------
";

        public static string GetHeader()
        {
            return HeaderString;
        }
    }
}