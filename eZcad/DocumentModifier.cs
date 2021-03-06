﻿using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace eZcad
{

    public enum ExternalCmdResult
    {
        Commit,
        Cancel,
    }

    /// <param name="docMdf"></param>
    /// <param name="impliedSelection"></param>
    /// <returns>如果要取消操作（即将事务 Abort 掉），则返回 false，如果要提交事务，则返回 true </returns>
    public delegate ExternalCmdResult ExternalCommand(DocumentModifier docMdf, SelectionSet impliedSelection);

    /// <summary> 对文档进行配置，以启动文档的改写模式 </summary>
    public class DocumentModifier : IDisposable
    {
        #region ---   执行外部命令

        /// <summary> 执行外部命令，并且在执行命令之前，自动将 事务打开</summary>
        /// <param name="cmd">要执行的命令</param>
        public static void ExecuteCommand(ExternalCommand cmd)
        {
            using (DocumentModifier docMdf = new DocumentModifier(openDebugerText: true))
            {
                try
                {
                    var impliedSelection = docMdf.acEditor.SelectImplied().Value;
                    var res = cmd(docMdf, impliedSelection);
                    //
                    switch (res)
                    {
                        case ExternalCmdResult.Commit:
                            docMdf.acTransaction.Commit();
                            break;
                        case ExternalCmdResult.Cancel:
                            docMdf.acTransaction.Abort();
                            break;
                        default:
                            docMdf.acTransaction.Abort();
                            break;
                    }
                }
                catch (System.Exception ex)
                {
                    docMdf.acTransaction.Abort(); // Abort the transaction and rollback to the previous state
                    string errorMessage = ex.Message + "\r\n\r\n" + ex.StackTrace;
                    MessageBox.Show(errorMessage, @"出错", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        #endregion

        #region ---   fields

        /// <summary> 启动命令时的最被的那个事务，区别于在命令执行过程中新开启的事务 </summary>
        private readonly Transaction _originalTransaction;
        public Transaction acTransaction { get; private set; }

        /// <summary> 当前活动的AutoCAD文档 </summary>
        public readonly Document acActiveDocument;

        /// <summary> 当前活动的AutoCAD文档中的数据库 </summary>
        public readonly Database acDataBase;

        /// <summary>  </summary>
        public readonly Editor acEditor;

        private readonly DocumentLock acLock;

        //
        private readonly bool _openDebugerText;
        private readonly StringBuilder _debugerSb;

        #endregion

        #region ---   构造函数
        /// <summary> 对文档进行配置，以启动文档的改写模式 </summary>
        /// <param name="openDebugerText">是否要打开一个文本调试器</param>
        public DocumentModifier(bool openDebugerText)
        {
            _openDebugerText = openDebugerText;

            // 获得当前文档和数据库   Get the current document and database
            acActiveDocument = Application.DocumentManager.MdiActiveDocument;
            acDataBase = acActiveDocument.Database;
            acEditor = acActiveDocument.Editor;

            //
            acLock = acActiveDocument.LockDocument();
            acTransaction = acDataBase.TransactionManager.StartTransaction();
            _originalTransaction = acTransaction;

            if (openDebugerText)
            {
                _debugerSb = new StringBuilder();
            }
        }
        #endregion

        #region --- Transaction 操作

        /// <summary> 重启一个新的事务 </summary>
        /// <param name="commitCancel">true 表示将当前事务提交后再重启，false 表示将当前事务回滚后再重启。 </param>
        public void RestartTransaction(bool commitCancel = true)
        {
            var tm = acTransaction.TransactionManager;
            if (commitCancel)
            {
                acTransaction.Commit();
            }
            else
            {
                acTransaction.Abort();
            }
            acTransaction.Dispose();
            acTransaction = tm.StartTransaction();
        }
        #endregion

        #region IDisposable Support

        private bool valuesDisposed = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!valuesDisposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    acTransaction.Dispose();
                    acLock.Dispose();

                    // 写入调试信息 并 关闭文本调试器
                    if (_openDebugerText && _debugerSb.Length > 0)
                    {
                        ShowDebugerInfo();
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                valuesDisposed = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~DocumentModifier()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion

        #region --- Debuger Info

        /// <summary> 向文本调试器中写入数据 </summary>
        /// <param name="value"></param>
        public void WriteLineIntoDebuger(params object[] value)
        {
            if (_openDebugerText)
            {
                _debugerSb.Append(value[0]);
                for (int i = 1; i < value.Length; i++)
                {
                    _debugerSb.Append($", {value[i]}");
                }

                _debugerSb.AppendLine();
            }
        }

        /// <summary> 向文本调试器中写入多行数据 </summary>
        /// <param name="lines"></param>
        public void WriteLinesIntoDebuger(params object[] lines)
        {
            if (_openDebugerText)
            {
                foreach (var s in lines)
                {
                    if (s != null)
                    {
                        _debugerSb.AppendLine(s.ToString());

                    }
                    else
                    {
                        _debugerSb.AppendLine("eZNull");
                    }
                }
            }
        }

        /// <summary> 实时显示调试信息在同一行中 </summary>
        /// <param name="value">集合中的所有数据写在一行，并以“,”分隔</param>
        public void WriteNow(params object[] value)
        {
            if (value.Length==0)
            {
                acEditor.WriteMessage("\n");
                return;
            }
            var sb = new StringBuilder();
            sb.Append(value[0]);
            for (int i = 1; i < value.Length; i++)
            {
                sb.Append($", {value[i]}");
            }
            sb.AppendLine();
            acEditor.WriteMessage(sb.ToString());
        }

        /// <summary> 实时显示调试信息在不同行中 </summary>
        /// <param name="value">集合中的所有数据分别写在不同行</param>
        public void WriteLinesNow(params object[] value)
        {
            if (value.Length==0)
                {
                    acEditor.WriteMessage("\n");
                return;
            }
            var sb = new StringBuilder();
            sb.Append(value[0]);
            for (int i = 1; i < value.Length; i++)
            {
                sb.Append($"\n {value[i]}");
            }
            sb.AppendLine();
            acEditor.WriteMessage(sb.ToString());
        }

        private void ShowDebugerInfo()
        {
            if (_openDebugerText)
            {
                acEditor.WriteMessage("\n------------------------- AddinManager 调试信息 ---------------------\n");
                acEditor.WriteMessage(_debugerSb.ToString());
                //
                _debugerSb.Clear();
            }
        }

        #endregion

        #region --- Other Utilities

        /// <summary> 将 各种无关提示进行换行 ，使命令行中显示更清爽 </summary>
        public static void LineFeedInCommandLine()
        {
            // 将加载命令后的各种无关提示进行换行
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            ed.WriteMessage("\n \n");
        }
        #endregion
    }
}