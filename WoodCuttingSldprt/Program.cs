using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Diagnostics;

if (!File.Exists("C:\\Program Files\\SOLIDWORKS Corp\\SOLIDWORKS\\SolidWorks.Interop.sldworks.dll"))
{
    Console.WriteLine("SolidWorks.Interop.sldworks.dll not found.");
    return;
}

if (!File.Exists("C:\\Program Files\\SOLIDWORKS Corp\\SOLIDWORKS\\SolidWorks.Interop.swconst.dll"))
{
    Console.WriteLine("SolidWorks.Interop.swconst.dll not found.");
    return;
}

SldWorks swApp = new SldWorks();
ModelDoc2 model = swApp.OpenDoc6(@"C:\Users\User\Desktop\workbench\workbench.SLDPRT",
    (int)swDocumentTypes_e.swDocPART,
    (int)swOpenDocOptions_e.swOpenDocOptions_Silent, "", 0, 0);

PartDoc part = (PartDoc)model;
object[] bodies = (object[])part.GetBodies2((int)swBodyType_e.swSolidBody, true);

List<double> lengths = new List<double>();

foreach (Body2 body in bodies)
{
    double[] box = (double[])body.GetBodyBox(); // {xmin, ymin, zmin, xmax, ymax, zmax}

    double lx = (box[3] - box[0]) * 100.0; // cm
    double ly = (box[4] - box[1]) * 100.0; // cm
    double lz = (box[5] - box[2]) * 100.0; // cm

    double longest = Math.Max(lx, Math.Max(ly, lz));

    Console.WriteLine($"Body name: [{body.Name}] X:[{lx:F2} cm] Y:[{ly:F2} cm] Z:[{lz:F2} cm] Longest: {Math.Round(longest, 3)} cm");

    lengths.Add(longest);
}

OptimizeCuts(lengths, 244, 0.2);
static void OptimizeCuts(List<double> lengths, double stockLength, double kerf)
{
    lengths.Sort((a, b) => b.CompareTo(a)); // largest to smallest
    List<List<double>> boards = new List<List<double>>();

    foreach (var piece in lengths)
    {
        int bestIndex = -1;
        double minWaste = double.MaxValue;

        for (int i = 0; i < boards.Count; i++)
        {
            double used = boards[i].Sum();
            int cuts = boards[i].Count; // every additional piece adds one cut
            double totalUsed = used + cuts * kerf;
            double remaining = stockLength - totalUsed;

            if (piece <= remaining && remaining - piece < minWaste)
            {
                bestIndex = i;
                minWaste = remaining - piece;
            }
        }

        if (bestIndex == -1)
        {
            boards.Add(new List<double> { piece }); // new board
        }
        else
        {
            boards[bestIndex].Add(piece);
        }
    }

    // Print result
    int n = 1;
    foreach (var board in boards)
    {
        double used = board.Sum();
        int cuts = board.Count - 1;
        double totalUsed = used + cuts * kerf;
        Console.WriteLine(
            $"Board {n++}: {string.Join(", ", board.Select(p => p.ToString("F1")))} " +
            $"(pieces {board.Count}, kerf {cuts * kerf:F1}, used {totalUsed:F1} / {stockLength} cm, waste {stockLength - totalUsed:F1} cm)"
        );
    }
}