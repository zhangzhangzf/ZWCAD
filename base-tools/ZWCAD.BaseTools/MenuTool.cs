//using ZwSoft.ZwCAD.ApplicationServices;
//using ZwSoft.ZwCAD.DatabaseServices;
//using ZwSoft.ZwCAD.EditorInput;
//using ZwSoft.ZwCAD.Runtime;


//namespace ZWCAD.BaseTools
//{
//    /// <summary>
//    /// 
//    /// </summary>
//    public class MenuTool
//    {



//        #region Private Variables

//        Document m_document;


//        #endregion



//        #region Default Constructor


//        /// <summary>
//        /// 构造函数
//        /// </summary>
//        public MenuTool()
//        {
//        }




//        [CommandMethod("MyMenu")]
//        public static void CreateMenu()
//        {
//            // Get the current document and database
//            Document doc = Application.DocumentManager.MdiActiveDocument;
//            Database db = doc.Database;

//            // Create a new menu
//            string menuName = "MyMenu";
//            ObjectId menuId = CreateMenu(menuName);

//            // Add menu items
//            AddMenuItem(menuId, "Command1", "MyCommand1");
//            AddMenuItem(menuId, "Command2", "MyCommand2");

//            // Add the menu to the application menu
//            AddMenuToApplicationMenu(menuName, menuId);
//        }

//        [CommandMethod("MyCommand1")]
//        public static void MyCommand1()
//        {
//            // Command1 code goes here
//        }

//        [CommandMethod("MyCommand2")]
//        public static void MyCommand2()
//        {
//            // Command2 code goes here
//        }

//        private static ObjectId CreateMenu(string menuName)
//        {
//            // Get the current document and database
//            Document doc = Application.DocumentManager.MdiActiveDocument;
//            Database db = doc.Database;

//            // Create a new menu
//            ObjectId menuId = ObjectId.Null;
//            using (Transaction tr = db.TransactionManager.StartTransaction())
//            {
//                // Get the menu group dictionary
//                DBDictionary menuGroupDict = tr.GetObject(db.MenuGroupDictionaryId, OpenMode.ForRead) as DBDictionary;

//                // Check if the menu already exists
//                if (menuGroupDict.Contains(menuName))
//                {
//                    menuId = menuGroupDict.GetAt(menuName);
//                }
//                else
//                {
//                    // Create the new menu
//                    MenuGroup menuGroup = new MenuGroup();
//                    menuGroup.Name = menuName;

//                    // Add the menu to the menu group dictionary
//                    menuGroupDict.UpgradeOpen();
//                    menuId = menuGroupDict.SetAt(menuName, menuGroup);
//                    tr.AddNewlyCreatedDBObject(menuGroup, true);

//                    // Save the changes to the transaction
//                    tr.Commit();
//                }
//            }

//            return menuId;
//        }

//        private static void AddMenuItem(ObjectId menuId, string itemName, string commandName)
//        {
//            // Get the current document and database
//            Document doc = Application.DocumentManager.MdiActiveDocument;
//            Database db = doc.Database;

//            using (Transaction tr = db.TransactionManager.StartTransaction())
//            {
//                // Get the menu group
//                MenuGroup menuGroup = tr.GetObject(menuId, OpenMode.ForWrite) as MenuGroup;

//                // Create the new menu item
//                MenuItem menuItem = new MenuItem();
//                menuItem.CommandString = commandName;
//                menuItem.GlobalName = itemName;

//                // Add the menu item to the menu group
//                menuGroup.Add(menuItem);
//                tr.AddNewlyCreatedDBObject(menuItem, true);

//                // Save the changes to the transaction
//                tr.Commit();
//            }
//        }

//        private static void AddMenuToApplicationMenu(string menuName, ObjectId menuId)
//        {
//            // Get the current document and database
//            Document doc = Application.DocumentManager.MdiActiveDocument;
//            Database db = doc.Database;

//            // Get the main menu group dictionary
//            DBDictionary mainMenuGroupDict = db.MenuGroups;

//            // Get the "AutoCAD" menu group
//            MenuGroup mainMenuGroup = mainMenuGroupDict.GetAt("ACAD");

//            // Get the "Tools" menu
