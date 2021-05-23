using NStack;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Windows.Forms;
using Terminal.Gui;
using Application = Terminal.Gui.Application;
using Clipboard = System.Windows.Forms.Clipboard;
using MessageBox = Terminal.Gui.MessageBox;
using PrintDialog = System.Windows.Forms.PrintDialog;
using View = Terminal.Gui.View;
using Window = Terminal.Gui.Window;

/*
 * VEdit, a TextEditor with TUI interface for Windows Command Prompt
 * Visual Laser 10 New - Ema3nto
 */

namespace Editor
{
    class Program
    {
        private static Tool objtool = new Tool();
        private static Tool.file filer = new Tool.file();
        private static Tool.edit editer = new Tool.edit();
        private static Tool.search searcher = new Tool.search();

        private static string backup = null; //a backup of a text to get is is saved
        private static string currentFile = null; //the path of current file
        private static ustring search = null;
        private static ustring replace = null;




        [STAThread] //to set || use the windows clipboard in another class
        static void Main(string[] args)
        {



            if (args != null && args.Length != 0)
            {
                if (!String.IsNullOrEmpty(args[0]))
                {
                    currentFile = args[0]; //get 1 param to open a file from cmd
                }
            }

            makewind();
        }


        static void makewind()
        {
            Application.Init();
            var win = new Window()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(0),
                Height = Dim.Fill(0),
                Title = "New File",
            };

            Application.Top.Add(win);


            string wholeText; //used to pass by ref "viewer.text" to the functions
            TextView viewer = new TextView()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };
            MenuBar barra = new MenuBar();
            StatusItem infoSt;
            StatusItem lr = new StatusItem(Key.Null, null, null);
            var status = new Terminal.Gui.StatusBar( //the status bar with some hint
                new StatusItem[]
                {
                    infoSt = new StatusItem(Key.Null, "Welcome in VEdit", ()=>
                    {
                        //do nothing, information about error or typing
                    }),
                    lr = new StatusItem(Key.Null, $"Line: {viewer.CurrentRow}, Column: {viewer.CurrentColumn}", () =>
                    {
                        //information about current line and current row
                    }),
                });

            viewer.KeyPress += delegate (View.KeyEventEventArgs args) //shortcut for the text
            {
                //current row and column
                int row = viewer.CurrentRow;
                int co = viewer.CurrentColumn;
                if (args.KeyEvent.Key == Key.CursorUp)
                    row = (row - 1 < 0) ? 0 : row - 1;
                else if (args.KeyEvent.Key == Key.CursorDown)
                    row = (row + 1 >= viewer.Lines) ? row : row + 1;
                else if (args.KeyEvent.Key == Key.CursorRight)
                    co += 1;
                else if (args.KeyEvent.Key == Key.CursorLeft)
                    co = (co - 1 < 0) ? 0 : co - 1;

                status.Items[1].Title = ($"Line: {row}, Column: {co}");


                //shortcut
                if (args.KeyEvent.IsCtrl)
                {
                    barra.OpenMenu(); //to be sure to set the focus to barra
                }
                else
                {
                    if (infoSt.Title == "Typing    ")
                        infoSt.Title = "Typing.   ";
                    else if (infoSt.Title == "Typing.   ")
                        infoSt.Title = "Typing..  ";
                    else if (infoSt.Title == "Typing..  ")
                        infoSt.Title = "Typing... ";
                    else
                        infoSt.Title = "Typing    ";
                }
            };

            viewer.MouseClick += delegate (View.MouseEventArgs args)
            {
                viewer.CursorPosition = new Terminal.Gui.Point(args.MouseEvent.X, args.MouseEvent.Y);
                status.Items[1].Title = ($"Line: {viewer.CurrentRow}, Column: {viewer.CurrentColumn}");
            };


            //CONSOLE COMPONENTS

            barra = new MenuBar( //the menu top bar
                new MenuBarItem[]
                {
                        new MenuBarItem("_File", new Terminal.Gui.MenuItem[]
                        {
                            new Terminal.Gui.MenuItem("_New","         ", () =>
                            {
                                wholeText = viewer.Text.ToString();

                                if (!filer.newDoc(ref currentFile, ref wholeText,
                                    objtool.ischanged(wholeText, ref backup)))
                                {
                                    infoSt.Title = "Impossible to make the file";
                                }

                                viewer.Text = wholeText;
                                backup = wholeText;

                                if(String.IsNullOrEmpty(currentFile))
                                    win.Title = "New File";

                            },shortcut: Key.CtrlMask | Key.N),

                            new Terminal.Gui.MenuItem("_Open", "        ", () =>
                            {
                                //open file function
                                wholeText = viewer.Text.ToString();
                                if(!filer.open(ref currentFile, ref wholeText, true))
                                {
                                    infoSt.Title = "Impossible to open the file";
                                }

                                viewer.Text = wholeText;
                                backup = viewer.Text.ToString();

                                if(!String.IsNullOrEmpty(currentFile))
                                    win.Title = Path.GetFileName(currentFile);

                            },shortcut: Key.CtrlMask | Key.O),

                            new Terminal.Gui.MenuItem("Reload", "        ", () =>
                            {
                                //open file function, if a file is already open don't ask to choose the file
                                wholeText = viewer.Text.ToString();
                                if(!filer.open(ref currentFile, ref wholeText, String.IsNullOrEmpty(currentFile)?true:false))
                                {
                                    infoSt.Title = "Impossible reload the file";
                                }

                                viewer.Text = wholeText;
                                backup = viewer.Text.ToString();

                                if(!String.IsNullOrEmpty(currentFile))
                                    win.Title = Path.GetFileName(currentFile);

                            },shortcut: Key.F5),

                            new Terminal.Gui.MenuItem("_Save", "        ", () =>
                            {
                                //save as function with same name

                                if (!filer.saveAs(ref currentFile, viewer.Text.ToString(), false))
                                {
                                    infoSt.Title = "Impossible to save the file";
                                }

                                if(!String.IsNullOrEmpty(currentFile))
                                    win.Title = Path.GetFileName(currentFile);

                            },shortcut: Key.CtrlMask | Key.S),

                            new Terminal.Gui.MenuItem("Save As", "     ", () =>
                            {
                                //save as function

                                if (!filer.saveAs(ref currentFile, viewer.Text.ToString(), true))
                                {
                                    infoSt.Title = "Impossible to save the file";
                                }

                                if(!String.IsNullOrEmpty(currentFile))
                                    win.Title = Path.GetFileName(currentFile);

                            }, shortcut: Key.Null),

                            new Terminal.Gui.MenuItem("_Print", "     ", () =>
                            {
                                //print file function
                                if(!filer.print(viewer.Text.ToString()))
                                {
                                    infoSt.Title = "Impossible to print the file";
                                }

                            }, shortcut: Key.CtrlMask | Key.P),

                            new Terminal.Gui.MenuItem("Exit", "     ", () =>
                            {
                                //exit function with ask is saved
                                wholeText = viewer.Text.ToString();
                                filer.exit(ref currentFile, ref wholeText, objtool.ischanged(wholeText, ref backup));
                                viewer.Text = wholeText;

                                if(!String.IsNullOrEmpty(currentFile))
                                    win.Title = Path.GetFileName(currentFile);

                            }, shortcut: Key.CtrlMask | Key.Q)
                        }),
                        new MenuBarItem("_Edit", new Terminal.Gui.MenuItem[]
                        {
                            new Terminal.Gui.MenuItem("Cut","        ", () =>
                            {
                                if (!editer.cut(ref viewer))
                                {
                                    infoSt.Title = "Error while cutting";
                                }
                                barra.CloseMenu();
                            },shortcut: Key.CtrlMask | Key.X),
                            new Terminal.Gui.MenuItem("Copy", "        ", () =>
                            {
                                if(!editer.copy(viewer))
                                {
                                    infoSt.Title = "Error while copying";
                                }
                                barra.CloseMenu();
                            },shortcut: Key.CtrlMask | Key.C),
                            new Terminal.Gui.MenuItem("Paste", "        ", () =>
                            {
                                if(!editer.paste(ref viewer))
                                {
                                    infoSt.Title = "Error while pasting";
                                }
                                barra.CloseMenu();
                            },shortcut: Key.CtrlMask | Key.V),
                            new Terminal.Gui.MenuItem("Delete", "     Del", () =>
                            {
                                if(!editer.canc(ref viewer))
                                {
                                    infoSt.Title = "Error while deleting";
                                }
                                barra.CloseMenu();
                            },shortcut: Key.Delete),
                            new Terminal.Gui.MenuItem("Select All", "        ", () =>
                            {
                                viewer.SelectAll();
                                barra.CloseMenu();
                            },shortcut: Key.CtrlMask | Key.A)

                        }),
                        new MenuBarItem("_Search", new Terminal.Gui.MenuItem[]
                        {
                            new Terminal.Gui.MenuItem("Find","        ", () => {

                                searcher.find(ref viewer,ref search, ref replace, true, false);
                                barra.CloseMenu();
                            },shortcut: Key.CtrlMask | Key.F),
                            new Terminal.Gui.MenuItem("Next find", "        ", () =>
                            {
                                searcher.find(ref viewer,ref search, ref replace, false, false);
                                barra.CloseMenu();
                            },shortcut: Key.F3),
                            new Terminal.Gui.MenuItem("Replace", "         ", () =>
                            {
                                searcher.find(ref viewer,ref search, ref replace, true, true);
                                barra.CloseMenu();
                            },shortcut: Key.CtrlMask | Key.R),
                            new Terminal.Gui.MenuItem("Next replace", "         ", () =>
                            {
                                searcher.find(ref viewer,ref search, ref replace, false, true);
                                barra.CloseMenu();
                            },shortcut: Key.F4),
                            new Terminal.Gui.MenuItem("Replace all", "         ", () =>
                            {
                                searcher.find(ref viewer,ref search, ref replace, true, true);
                                viewer.ReplaceAllText(search, textToReplace: replace);
                                barra.CloseMenu();
                            },shortcut: Key.CtrlMask | Key.W),
                            new Terminal.Gui.MenuItem("────────────────────────────", "", () => {}),
                            new Terminal.Gui.MenuItem("Goto line", "       ", () =>
                            {
                                searcher.gotoL(ref viewer);
                                barra.CloseMenu();
                            },shortcut: Key.CtrlMask | Key.G),
                            new Terminal.Gui.MenuItem("Goto start", "       ", () =>
                            {
                                viewer.MoveHome();
                                barra.CloseMenu();
                            },shortcut: Key.CtrlMask | Key.J),
                            new Terminal.Gui.MenuItem("Goto end", "       ", () =>
                            {
                                viewer.MoveEnd();
                                barra.CloseMenu();
                            },shortcut: Key.CtrlMask | Key.K),
                        }),
                        new MenuBarItem("_VEdit", new Terminal.Gui.MenuItem[]
                        {
                            /*new Terminal.Gui.MenuItem("Setting", "         ", () =>
                            {
                                
                            }),*/
                            new Terminal.Gui.MenuItem("Help", "         ", () =>
                            {
                                //open the same ".md" that's on github
                                try
                                {
                                    Process.Start("https://github.com/VisualLaser10New/VEdit");
                                }
                                catch
                                {
                                    infoSt.Title = "Impossible to open the help, check your connection";
                                }
                            }, shortcut: Key.F1),

                            new Terminal.Gui.MenuItem("About","         ", () =>
                            {
                                //open about dialog
                                objtool.about();
                            }),
                            new Terminal.Gui.MenuItem("VL10New", "         ", () =>
                            {
                                //open visual lasser 10 new website
                                try
                                {
                                    Process.Start("https://sites.google.com/view/visuallaser10/");
                                }
                                catch
                                {
                                    infoSt.Title = "Impossible to open the website";
                                }
                            }),

                        })
                });


            //ADDER
            Application.Top.Add(barra, status);
            win.Add(viewer);

            //FILE OPEN FROM CMD
            if (!string.IsNullOrEmpty(currentFile))
            {
                wholeText = viewer.Text.ToString();
                if (!filer.open(ref currentFile, ref wholeText, false))
                {
                    infoSt.Title = "Impossible to open the file";
                }

                viewer.Text = wholeText;
                backup = viewer.Text.ToString();

                if (!String.IsNullOrEmpty(currentFile))
                    win.Title = Path.GetFileName(currentFile);
            }

            Application.Run();
        }


    }

    class Tool
    {
        public class file
        {
            public bool newDoc(ref string name, ref string text, bool asktosave)
            {
                try
                {
                    if (asktosave)
                    {
                        if (!askToSave(ref name, ref text))
                            return false; //if the user has pressed cancel, no new doc
                    }

                    name = null; //reset the document
                    text = "";
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            public void exit(ref string name, ref string text, bool asktosave)
            {
                try
                {
                    if (asktosave)
                    {
                        if (!askToSave(ref name, ref text))
                            return; //if the user has pressed cancel, no new doc
                    }

                    name = null; //reset the document
                    text = "";
                    Environment.Exit(0);
                }
                catch
                {
                    Environment.Exit(1);
                }
            }

            private bool askToSave(ref string name, ref string text)
            {
                var msgAsk = MessageBox.Query("VEdit", "Do you want to save the file?", "Yes", "No", "Cancel");

                switch (msgAsk)
                {
                    case 0:
                        //yes save option
                        saveAs(ref name, text, false);
                        break;
                    case 2:
                        //cancel option
                        return false;
                }

                return true;
            }

            public bool saveAs(ref string name, string text, bool showDialog)
            {
                //ret true if file is saved else return false
                //if "replaceIfExist" is false the button "save" has clicked else has clicked the "save as"
                if (showDialog || string.IsNullOrEmpty(name))
                {
                    //if the file doesn't exist and user has clicked "save"
                    //if the user has clicked "save as"
                    //so the file does NOT exist
                    var dialog = new SaveDialog("VEdit", "Save the file as...");
                    Application.Run(dialog);
                    if (dialog.Canceled)
                    {
                        return false;
                    }

                    name = dialog.FilePath.ToString();
                    //name += dialog.FileName.ToString();
                }

                //the path has been set in both cases, now:
                //if the file exist will be replaced

                if (File.Exists(name))
                {
                    try
                    {
                        File.Delete(name);
                    }
                    catch
                    {
                        return false;
                    }
                }

                //now make the new file and save it
                try
                {
                    using (var made = File.CreateText(name))
                    {
                        made.Write(text);
                    }
                }
                catch
                {
                    return false;
                }

                return true;
            }

            public bool open(ref string name, ref string text, bool showDialog)
            {
                //open the file, load it in "text", load path in "name"

                //if the user type: "vedit nameoffile.txt", the "showDialog" must be false and the "name" cannot be changed

                if (showDialog)
                {
                    try
                    {
                        var dialog = new OpenDialog("VEdit", "Open a file...")
                        {
                            AllowsMultipleSelection = false,
                            CanChooseDirectories = false,
                            CanChooseFiles = true,
                        };
                        Application.Run(dialog);
                        if (dialog.Canceled)
                            return false;

                        //set the property
                        name = dialog.FilePath.ToString();
                    }
                    catch
                    {
                        return false;
                    }
                }

                //now set the text red
                try
                {
                    text = File.ReadAllText(name);
                }
                catch
                {
                    return false;
                }

                return true;
            }

            public bool print(string text)
            {
                //print a document
                var dialog = new PrintDialog();
                var document = new PrintDocument();
                try
                {
                    document.PrintPage += delegate (object o, PrintPageEventArgs a)
                    {
                        a.Graphics.DrawString(text, new Font("Segoe UI", 12, FontStyle.Regular), Brushes.Black, 20,
                            20);
                    };

                    dialog.Document = document; //up to here set to "document" which to stamp
                    if (dialog.ShowDialog() == DialogResult.OK) //ask the user to print
                    {
                        document.Print();
                    }
                    else
                    {
                        return false;
                    }
                }
                catch
                {
                    return false;
                }

                return true;
            }
        }

        public class edit
        {
            public bool copy(TextView input)
            {
                try
                {
                    input.Copy();
                    Clipboard.SetText(Terminal.Gui.Clipboard.Contents.ToString());
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            public bool cut(ref TextView input)
            {
                try
                {
                    input.Cut();
                    Clipboard.SetText(Terminal.Gui.Clipboard.Contents.ToString());
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            public bool paste(ref TextView input)
            {
                try
                {
                    Terminal.Gui.Clipboard.Contents = Clipboard.GetText();
                    input.Paste();
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            public bool canc(ref TextView input)
            {
                try
                {
                    ustring tmp = Terminal.Gui.Clipboard.Contents;
                    input.Cut();
                    Terminal.Gui.Clipboard.Contents = tmp;
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public class search
        {
            public bool find(ref TextView input, ref ustring to_search, ref ustring to_replace, bool showDialog, bool replace)
            {

                if (ustring.IsNullOrEmpty(input.Text))
                    return false;

                var forwardSearch = true;

                if (showDialog)
                {
                    bool cancelA = false;
                    var frwCheck = new Terminal.Gui.CheckBox(5, 9, "Forward only", true);

                    //TEXT SEARCH
                    var box = new FrameView(new Rect(5, 1, 60, 3), "Text to seek");
                    var seekText = new Terminal.Gui.TextField("")
                    {
                        Width = 55,
                        X = 2,
                        Y = 0,
                    };

                    //BUTTON
                    var Seek = new Terminal.Gui.Button(replace ? "Replace" : "Seek", true);
                    var Cancel = new Terminal.Gui.Button("Cancel", false);
                    Seek.Clicked += Application.RequestStop; //seek  click

                    Cancel.Clicked += () =>                  //exit click
                    {
                        cancelA = true;
                        Application.RequestStop();
                    };

                    //TEXT REPLACE
                    var boxR = new FrameView(new Rect(5, 5, 60, 3), "Text to supersede");
                    var repText = new Terminal.Gui.TextField("")
                    {
                        Width = 55,
                        X = 2,
                        Y = 0,
                    };

                    //THE ENTIRE DIALOG
                    Dialog dialog = new Dialog("VEdit - Seek a text", 70, 13);
                    dialog.AddButton(Seek);
                    dialog.AddButton(Cancel);
                    box.Add(seekText);
                    boxR.Add(repText);

                    if (replace)
                        dialog.Add(boxR);
                    dialog.Add(box, frwCheck);

                    Application.Run(dialog); //show the dialog



                    if (cancelA)//if cancel exit
                        return false;

                    to_search = seekText.Text;
                    to_replace = repText.Text;
                    forwardSearch = frwCheck.Checked;
                }
                if (ustring.IsNullOrEmpty(to_search) || (replace && ustring.IsNullOrEmpty(to_replace)))
                    return false;

                if (!replace)
                    to_replace = "";

                if (!forwardSearch)
                    input.CursorPosition = new Terminal.Gui.Point(0, 0);

                return input.FindNextText(to_search, out forwardSearch, textToReplace: to_replace, replace: replace);
            }

            public bool gotoL(ref TextView input)
            {
                bool cancelA = false;
                int line;

                var box = new FrameView(new Rect(2, 1, 35, 3), "Go To line - Max " + input.Lines.ToString());
                var goText = new Terminal.Gui.TextField("")
                {
                    Width = 30,
                    X = 2,
                    Y = 0,
                };

                var go = new Terminal.Gui.Button("Go", true);
                var Cancel = new Terminal.Gui.Button("Cancel", false);
                go.Clicked += Application.RequestStop; //seek  click

                Cancel.Clicked += () =>                  //exit click
                {
                    cancelA = true;
                    Application.RequestStop(); //hide the dialog
                };

                //THE ENTIRE DIALOG
                Dialog dialog = new Dialog("VEdit - Go To line", 40, 8);
                box.Add(goText);

                dialog.AddButton(go);
                dialog.AddButton(Cancel);
                dialog.Add(box);

                goText.SetFocus();

                Application.Run(dialog); //show the dialog

                if (cancelA || ustring.IsNullOrEmpty(goText.Text))//if cancel || empty -> exit
                    return false;

                if (!Int32.TryParse(goText.Text.ToString(), out line))//if NaN -> exit
                    return false;

                try
                {
                    input.CursorPosition = new Terminal.Gui.Point(0, line);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public int lctol(ustring input, int line, int column)
        {
            int output = 0;
            using (StringReader st = new StringReader(input.ToString()))
            {
                for (int i = 0; i < line; i++)
                {
                    output += st.ReadLine().Length;
                }
            }
            output += column;
            return output;
        }

        public bool ischanged(string a, ref string b)
        {
            bool c = a != b;
            b = a;

            return c;
        }

        public void about()
        {
            var msg = Terminal.Gui.MessageBox.Query("VEdit",
                "Product by\nVisual Laser 10 New - Ema3nto",
                0,
                "Ok",
                "Visit Website"
                );
            if (msg == 1)
            {
                //"visit" clicked
                try
                {
                    Process.Start("https://sites.google.com/view/visuallaser10/");
                }
                catch
                {
                    return;
                }
            }
        }
    }
}
