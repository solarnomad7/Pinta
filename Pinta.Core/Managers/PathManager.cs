// 
// PathManager.cs
//  
// Author:
//       dufoli <${AuthorEmail}>
// 
// Copyright (c) 2010 dufoli
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using Cairo;

namespace Pinta.Core
{


	public class PathManager
	{

		public PathManager ()
		{
		}
		
		public unsafe static Point[][] PolygonSetFromStencil(IBitVector2D stencil, Rectangle bounds, int translateX, int translateY)
        {
            List<Point[]> polygons = new List<Point[]>();

            if (!stencil.IsEmpty)
            {
                Point start = bounds.Location ();
                List<Point> pts = new List<Point>();
                int count = 0;

                // find all islands
                while (true) 
                {
                    bool startFound = false;

                    while (true)
                    {
                        if (stencil[start])
                        {
                            startFound = true;
                            break;
                        }

                        ++start.X;

                        if (start.X >= bounds.GetRight ())
                        {
                            ++start.Y;
                            start.X = (int)bounds.X;

                            if (start.Y >= bounds.GetBottom ())
                            {
                                break;
                            }
                        }
                    }
            
                    if (!startFound)
                    {
                        break;
                    }

                    pts.Clear();
                    Point last = new Point(start.X, start.Y + 1);
                    Point curr = new Point(start.X, start.Y);
                    Point next = curr;
                    Point left = new Point ();
                    Point right = new Point ();
            
                    // trace island outline
                    while (true)
                    {
                        left.X = ((curr.X - last.X) + (curr.Y - last.Y) + 2) / 2 + curr.X - 1;
                        left.Y = ((curr.Y - last.Y) - (curr.X - last.X) + 2) / 2 + curr.Y - 1;

                        right.X = ((curr.X - last.X) - (curr.Y - last.Y) + 2) / 2 + curr.X - 1;
                        right.Y = ((curr.Y - last.Y) + (curr.X - last.X) + 2) / 2 + curr.Y - 1;

                        if (bounds.ContainsPoint(left.X, left.Y) && stencil[left])
                        {
                            // go left
                            next.X += curr.Y - last.Y;
                            next.Y -= curr.X - last.X;
                        }
                        else if (bounds.ContainsPoint(right.X, right.Y) && stencil[right])
                        {
                            // go straight
                            next.X += curr.X - last.X;
                            next.Y += curr.Y - last.Y;
                        }
                        else
                        {
                            // turn right
                            next.X -= curr.Y - last.Y;
                            next.Y += curr.X - last.X;
                        }

                        if (Math.Sign(next.X - curr.X) != Math.Sign(curr.X - last.X) ||
                            Math.Sign(next.Y - curr.Y) != Math.Sign(curr.Y - last.Y))
                        {
                            pts.Add(curr);
                            ++count;
                        }

                        last = curr;
                        curr = next;

                        if (next.X == start.X && next.Y == start.Y)
                        {
                            break;
                        }
                    }

                    Point[] points = pts.ToArray();
                    Scanline[] scans = points.GetScans ();

                    foreach (Scanline scan in scans)
                    {
                        stencil.Invert(scan);
                    }

                    points.TranslatePointsInPlace (translateX, translateY);
                    polygons.Add(points);
                }
            }

            Point[][] returnVal = polygons.ToArray();
            return returnVal;
        }
	}
}
