﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Internal.PropertyInspector;
using Autodesk.AutoCAD.Runtime;
using eZcad.AddinManager;
using eZcad.Addins;
using eZcad.Addins.Entities;
using eZcad.Debug;
using eZcad.Utility;

[assembly: CommandClass(typeof(ExtensionGetter))]

namespace eZcad.Addins.Entities
{
    /// <summary> <seealso cref="CommandDescription"/> </summary>
    [EcDescription(CommandDescription)]
    public class ExtensionGetter : ICADExCommand
    {
        #region --- 命令设计

        /// <summary> 命令行命令名称，同时亦作为命令语句所对应的C#代码中的函数的名称 </summary>
        public const string CommandName = @"GetExtension";

        private const string CommandText = @"图元几何范围";
        private const string CommandDescription = @"提取元素的 Extend3d 信息";

        /// <summary> <seealso cref="CommandDescription"/> </summary>
        [CommandMethod(eZConstants.eZGroupCommnad, CommandName, CommandFlags.Interruptible | CommandFlags.UsePickSet)
        , DisplayName(CommandText), Description(CommandDescription)
        , RibbonItem(CommandText, CommandDescription, eZConstants.ImageDirectory + "HighFill_32.png")]
        public void GetExtension()
        {
            DocumentModifier.ExecuteCommand(GetExtension);
        }

        public ExternalCommandResult Execute(SelectionSet impliedSelection, ref string errorMessage,
            ref IList<ObjectId> elementSet)
        {
            var s = new ExtensionGetter();
            return eZcadAddinManagerDebuger.DebugInAddinManager(s.GetExtension,
                impliedSelection, ref errorMessage, ref elementSet);
        }

        #endregion

        private DocumentModifier _docMdf;

        /// <summary> <seealso cref="CommandDescription"/> </summary>
        public ExternalCmdResult GetExtension(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            _docMdf = docMdf;
            //
            var entis = AddinManagerDebuger.PickEntities<Entity>(_docMdf.acEditor, "选择多个对象，提取几何定位");
            if (entis == null || entis.Length == 0) return ExternalCmdResult.Cancel;
            //
            var sb = new StringBuilder();
            sb.AppendLine("Min;Max;Center;Width;Height;Depth;");
            foreach (var ent in entis)
            {
                AppendDescription(ent.GeometricExtents, ref sb);
                sb.AppendLine();
            }
            docMdf.WriteLineIntoDebuger("选择的元素个数：", entis.Length);
            _docMdf.WriteLineIntoDebuger(sb.ToString());
            //
            return ExternalCmdResult.Commit;
        }
        
        /// <summary> 获取 Extents3d 的几何描述信息，并附加到 <seealso cref="StringBuilder"/> 中 </summary>
        private void AppendDescription(Extents3d ext, ref StringBuilder description)
        {
            const string sep = ";";
            var adExt = new AdvancedExtents3d(ext);
            description.Append(ext.MinPoint + sep);
            description.Append(ext.MaxPoint + sep);
            description.Append((adExt.GetAnchor(AdvancedExtents3d.Anchor.GeometryCenter)) + sep);
            description.Append(adExt.GetWidth() + sep);
            description.Append(adExt.GetHeight() + sep);
            description.Append(adExt.GetDepth() + sep);
        }

        #region ---   界面操作

        #endregion
    }
}