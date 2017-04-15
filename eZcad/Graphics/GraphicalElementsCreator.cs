﻿using System;
using AutoCADDev.Graphics;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Exception = System.Exception;

// This line is not mandatory, but improves loading performances

[assembly: CommandClass(typeof(GraphicalElementsCreator))]

namespace AutoCADDev.Graphics
{
    /// <summary> 创建各种新的图元对象 </summary>
    internal static class GraphicalElementsCreator
    {
        /// <summary> 向文档中添加多段线 </summary>
        /// <param name="srcX">用来创建二维多段线的 X 集合</param>
        /// <param name="srcY">用来创建二维多段线的 Y 集合，其元素个数必须与 X 集合的元素个数相同 </param>
        /// <param name="close">是否要将整个多段线进行闭合</param>
        [CommandMethod("AddPolyline")]
        public static void AddPolyline2D(double[] srcX, double[] srcY, bool close)
        {
            // 获得当前文档和数据库   Get the current document and database
            Document acDoc = Application.DocumentManager.MdiActiveDocument;

            using (var docLocker = acDoc.LockDocument())
            {
                Database acCurDb = acDoc.Database;

                // 启动一个事务  Start a transaction
                using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
                {
                    try
                    {
                        // 以只读方式打开块表   Open the Block table for read
                        var acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                        // 以写方式打开模型空间块表记录   Open the Block table record Model space for write
                        var acBlkTblRec =
                            acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as
                                BlockTableRecord;

                        // 创建一条有两段的多段线   Create a polyline with two segments (3 points)
                        Polyline acPoly = new Polyline();
                        acPoly.SetDatabaseDefaults();
                        for (int i = 0; i < srcX.Length; i++)
                        {
                            acPoly.AddVertexAt(i, new Point2d(srcX[i], srcY[i]), 0, startWidth: 0, endWidth: 0);
                        }
                        acPoly.Closed = close;

                        // 添加新对象到块表记录和事务中   Add the new object to the block table record and the transaction
                        acBlkTblRec.AppendEntity(acPoly);
                        acTrans.AddNewlyCreatedDBObject(acPoly, true);

                        // 保存新对象到数据库中   Save the new object to the database
                        acTrans.Commit();
                    }
                    catch (Exception ex)
                    {
                        acTrans.Abort(); // Abort the transaction and rollback to the previous state
                        throw new InvalidOperationException("向文档中添加多段线时出错", ex);
                    }
                } // Dispose 事务
            } // 解锁文档
        }

        /// <summary> 向文档中添加多段线 </summary>
        /// <param name="srcX">用来创建二维多段线的 X 集合</param>
        /// <param name="srcY">用来创建二维多段线的 Y 集合，其元素个数必须与 X 集合的元素个数相同 </param>
        /// <param name="close">是否要将整个多段线进行闭合</param>
        [CommandMethod("AddPolyline")]
        public static void AddText(double[] srcX, double[] srcY, bool close)
        {
            // 获得当前文档和数据库   Get the current document and database
            Document acDoc = Application.DocumentManager.MdiActiveDocument;

            using (var docLocker = acDoc.LockDocument())
            {
                Database acCurDb = acDoc.Database;

                // 启动一个事务  Start a transaction
                using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
                {
                    try
                    {
                        // 以只读方式打开块表   Open the Block table for read
                        var acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                        // 以写方式打开模型空间块表记录   Open the Block table record Model space for write
                        var acBlkTblRec =
                            acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as
                                BlockTableRecord;

                        // 创建一条有两段的多段线   Create a polyline with two segments (3 points)
                        Polyline acPoly = new Polyline();
                        acPoly.SetDatabaseDefaults();
                        for (int i = 0; i < srcX.Length; i++)
                        {
                            acPoly.AddVertexAt(i, new Point2d(srcX[i], srcY[i]), 0, startWidth: 0, endWidth: 0);
                        }
                        acPoly.Closed = close;

                        // 添加新对象到块表记录和事务中   Add the new object to the block table record and the transaction
                        acBlkTblRec.AppendEntity(acPoly);

                        acTrans.AddNewlyCreatedDBObject(acPoly, true);

                        // 保存新对象到数据库中   Save the new object to the database
                        acTrans.Commit();
                    }
                    catch (Exception ex)
                    {
                        acTrans.Abort(); // Abort the transaction and rollback to the previous state
                        throw new InvalidOperationException("向文档中添加多段线时出错", ex);
                    }
                } // Dispose 事务
            } // 解锁文档
        }

    }
}