using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.BoundaryRepresentation;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Colors;
using System;
using System.Collections.Generic;
using System.Collections;

namespace AcadRoundVertices
{
  public class RoundVertices
  {
    [CommandMethod("ConnectLines")]
    static public void ReversePolylineDirection()
    {
      var doc = Application.DocumentManager.MdiActiveDocument;
      var ed = doc.Editor;

      // Ask for how many decimal places
      int precision_value;
      while (true)
      {
        PromptIntegerOptions precision = new PromptIntegerOptions("Округление координат вершин полилиний");
        precision.Message = "Введите количество десятичных знаков:";
        precision.AllowNone = false;
        PromptIntegerResult precision_result = ed.GetInteger(precision);
        precision_value = precision_result.Value;

        if (precision_result.Status != PromptStatus.OK)
        {
            return;
        }

        if ((precision_value < 0) | (precision_value > 15))
        {
            ed.WriteMessage("Необходимо ввести целое число от 0 до 15\n");
        }
        else
        {
            break;
        }
      }

      // Create filter for selection
      TypedValue[] filterList = new TypedValue[1];
      filterList[0] = new TypedValue(0, "LWPOLYLINE");
      SelectionFilter filter = new SelectionFilter(filterList);

      // Select all Polylines
      PromptSelectionResult psr = ed.SelectAll(filter);
      if (psr.Status != PromptStatus.OK)
      {
          ed.WriteMessage("Не найдено ни одной полилинии\n");
          return;
      }

      var tr = doc.TransactionManager.StartTransaction();
      using (tr)
      {
        List<Point2d> pts = new List<Point2d>();
        foreach (SelectedObject so in psr.Value)
        {
            Entity ent =
                (Entity)tr.GetObject(
                so.ObjectId,
                OpenMode.ForRead
                );

            var obj = tr.GetObject(ent.ObjectId, OpenMode.ForRead);
            Polyline pl = (Polyline)obj;
    
            if (pl != null)
            {
                // Collect vertices
                pts = new List<Point2d>();
                for (int i = 0; i < pl.NumberOfVertices; i++)
                {
                    pts.Add(pl.GetPoint2dAt(i));
                }
        
                // Open Polyline for edit
                pl.UpgradeOpen();
        
                // Write rounded coordinates back to Polyline
                for (int i = 0; i < pl.NumberOfVertices; i++)
                {
                    pl.SetPointAt(i, new Point2d(
                        Math.Round(pts[i].X, precision_value, MidpointRounding.AwayFromZero),
                        Math.Round(pts[i].Y, precision_value, MidpointRounding.AwayFromZero)
                        ));
                }
            }
        }
        tr.Commit();
        ed.WriteMessage(
            String.Format(
                "Координаты вершин Полилиний окрулены до {0} знаков после запятой\n", precision_value
                ));
      }
    }
  }
}
