
using System.Runtime.InteropServices;

NativeLibrary.Load("nvapi64.dll");
using var game = new multi_pass_stress_test.Game1();
game.Run();
