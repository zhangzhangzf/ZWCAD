using ZwSoft.ZwCAD.Runtime;
using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.EditorInput;
using ZwSoft.ZwCAD.ApplicationServices;
using ZWCAD.ShipBracket.ViewModels;

namespace ZWCAD.Publish
{
    public class Commands
    {
        [CommandMethod("Hello")]
        public void Hello()
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            ed.WriteMessage("欢迎进入.NET开发中望CAD的世界！");
        }
        
        [CommandMethod("Bracket")]
        public void Bracket()
        {
            Document document = Application.DocumentManager.MdiActiveDocument;
            Editor ed = document.Editor;
            BracketViewModel viewModel = new BracketViewModel(document);
            viewModel.Run();
        }
    }
}
