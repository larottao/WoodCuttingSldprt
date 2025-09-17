using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using WoodCuttingSldprt.Models;

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

List<SolidBody> bodiesList = ((object[])part.GetBodies2((int)swBodyType_e.swSolidBody, true))
    .Cast<Body2>()
    .Select(b => new SolidBody(b))
    .ToList();

bodiesList = bodiesList.OrderByDescending(b => b.longestAxis).ToList();

foreach (SolidBody body in bodiesList)
{
    Console.WriteLine($"Body name: [{body.Name}] X:[{body.xCm:F2} cm] Y:[{body.yCm:F2} cm] Z:[{body.zCm:F2} cm] Longest: {body.longestAxis:F3} cm");
}

OptimizeCuts(bodiesList, 244, 0.2);
static void OptimizeCuts(List<SolidBody> bodiesList, double stockLength, double kerf)
{
    bodiesList = bodiesList.OrderByDescending(b => b.longestAxis).ToList();

    List<List<double>> boards = new List<List<double>>();

    foreach (SolidBody body in bodiesList)
    {
        int bestIndex = -1;
        double minWaste = double.MaxValue;

        for (int i = 0; i < boards.Count; i++)
        {
            double used = boards[i].Sum();
            int cuts = boards[i].Count; // every additional piece adds one cut
            double totalUsed = used + cuts * kerf;
            double remaining = stockLength - totalUsed;

            if (body.longestAxis <= remaining && remaining - body.longestAxis < minWaste)
            {
                bestIndex = i;
                minWaste = remaining - body.longestAxis;
            }
        }

        if (bestIndex == -1)
        {
            boards.Add(new List<double> { body.longestAxis }); // new board
        }
        else
        {
            boards[bestIndex].Add(body.longestAxis);
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