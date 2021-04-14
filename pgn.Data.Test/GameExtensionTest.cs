using System;
using System.Collections.Generic;
using System.Linq;
using ilf.pgn.Data;
using Xunit;

namespace pgn.Data.Test
{
    public class GameExtensionTest
    {
        public const string TestGameString =
@"[Event ""Breslau""]
[Site ""Breslau""]
[Date ""1879.??.??""]
[Round ""?""]
[White ""Tarrasch, Siegbert""]
[Black ""Mendelsohn, J.""]
[FEN ""rnqkbbnr/pppppppp/8/8/8/8/PPPPPPPP/RNQKBBNR w KQkq - 0 1""]

1.d4 {...} e6 2.c4 Be7 3.e3 Nf6 4.Nc3 d5 5.cxd5 exd5 
6.f3 c5 7.dxc5 Bd7 8.Bd3 O-O 9.Nge2 Nc6 10.Bf2 
*";

        [Fact]
        public void GoToMove_return_correct_board()
        {
            // Read a PGN file.
            var reader = new ilf.pgn.PgnReader();
            var gameDb = reader.ReadFromString(TestGameString);

            // Get the first game from the file and print it to the console.
            var game = gameDb.Games.First();
            var pos = game.GoToMove();
            Assert.Equal(19, pos.HalfMove);
            var worth = pos.Board.CalcWorthInPawns();
            Assert.Equal(38, worth.White);
            Assert.Equal(37, worth.Black);
            Console.WriteLine(game);
        }
    }
}
