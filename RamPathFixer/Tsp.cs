using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RamPathFixer
{
    public class Tsp
    {
        public static double[,] do2opt(double[,] xy)
        {
            // 
            // Voor 2opt                 na 2opt
            // Y   Z                    Y   Z
            // O   O----->              O-->O---->
            // / \  ^                     \
            // /   \ |                      \
            // /     \|                       \
            // ->O       O              ->O------>O
            // C       X                C       X
            // 
            // We bepalen waar - bij wisselen van 2opt - het grootste verschil zit van alle lijnstukken
            // en deze wordt dan doorgevoerd
            // daarna dit opnieuw doen tot er geen verschil meer is
            // hiervoor worden eerst de lengtes berekend van c-y, x-z (voor 2 opt) als ook c-x, y-z (na 2 opt)
            // lengte tussen 2 punten = sqrt((x1 - x2)² + (y1 - y2)²)
            // 
            // als het verschil van het eerste - voor 2opt - (cy+xz) en het 2de - na 2opt - (cx + yz) groter is dan de al berekende
            // dan houden we deze nieuwe bij. daar enkel X en Y van plaats moeten wisslen, enkel X en Y bijhouden
            // na alles met alles vergeleken te hebben weten we welke 2 lijnstukken we gaan verwisselen
            // en voeren dit ook door. 
            // nadien de routine opnieuw doen tot er geen verbetering meer is.
            // 
            // Bij het doorgeven van de tabel xy(,) moet deze 1 element meer bevatten dan het aantal punten
            // bv. bij 5 punten moet deze 6 elementen bevatten (0 -> 4 en 5) 
            // deze laatste wordt gebruikt om het eindpunt te zetten op het beginpunt
            // dus dim xy(1,5) zelf opvullen element 0->4, 5 wordt in deze functie gedaan
            // 
            int c = 0, y = 0, x = 0, z = 0, bx = 0, by = 0;
            double best, gain;
            double cy, xz, cx, yz;
            int nrofpnt;


            // nrofpnt = (xy.Length / (xy.Rank + 1)) - 1
            nrofpnt = xy.GetUpperBound(xy.Rank - 1);

            do
            {
                best = 0;
                for (c = 0; c <= nrofpnt - 1; c++)
                {
                    for (x = 0; x <= nrofpnt - 1; x++)
                    {
                        y = c + 1;
                        z = x + 1;
                        if (!(x == c | x == y))
                        {
                            cy = Math.Sqrt((xy[0, c] - xy[0, y]) * (xy[0, c] - xy[0, y]) + (xy[1, c] - xy[1, y]) * (xy[1, c] - xy[1, y]));
                            xz = Math.Sqrt((xy[0, x] - xy[0, z]) * (xy[0, x] - xy[0, z]) + (xy[1, x] - xy[1, z]) * (xy[1, x] - xy[1, z]));
                            cx = Math.Sqrt((xy[0, c] - xy[0, x]) * (xy[0, c] - xy[0, x]) + (xy[1, c] - xy[1, x]) * (xy[1, c] - xy[1, x]));
                            yz = Math.Sqrt((xy[0, y] - xy[0, z]) * (xy[0, y] - xy[0, z]) + (xy[1, y] - xy[1, z]) * (xy[1, y] - xy[1, z]));
                            gain = ((cy + xz) - (cx + yz));
                            if (gain > best)
                            {
                                bx = x;
                                by = y;
                                best = gain;
                            }
                        }
                    }
                }

                if (best > 0)
                {
                    // swap best mogelijke swap (bx en by)
                    cx = xy[0, bx];
                    cy = xy[1, bx];
                    xy[0, bx] = xy[0, by];
                    xy[1, bx] = xy[1, by];
                    xy[0, by] = cx;
                    xy[1, by] = cy;
                }
            }
            while (best != 0);

            return xy;
        }
    }
}
