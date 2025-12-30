using System;
using System.Collections.Generic;
using System.Text;

namespace UmamusumeDeserializeDB5.Generator
{
    internal class WinSaddle : GeneratorBase
    {
        public void Generate()
        {
            Save("wins_saddle", Data.JP.SingleModeWinsSaddleTable.Where(x => x.win_saddle_type == 3).Select(x => x.id));
        }
    }
}
