using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace WinXound_Net
{
    public partial class wxCSoundOpcodesRepository : UserControl
    {

        private Hashtable mOpcodes = new Hashtable();
        private bool DragIsValidItem = false;
        private string DragString = "";

        public wxCSoundOpcodesRepository()
        {
            InitializeComponent();
        }

        //public void SetOpcodes(Hashtable opcodes)
        //{
        //    mOpcodes = opcodes;
        //}

        private void wxCSoundOpcodesRepository_Load(object sender, EventArgs e)
        {
            wxIntelliTipRepository.SetGreyColors();
        }



        public Hashtable FillTreeViewAndReturnOpcodes()
        {
            string mString = null;

            Int32 index = -1;
            string itemText = "";
            string oldItemText = "";

            //Check if file exists; if not use the default one included inside resources.
            if (!File.Exists(Application.StartupPath + "\\Utility\\opcodes.txt"))
            {
                using (StreamReader sr = wxGlobal.GetResource("Resources.opcodes.txt"))
                {
                    string text = sr.ReadToEnd();
                    File.WriteAllText(Application.StartupPath + "\\Utility\\opcodes.txt", text);
                }
            }


            mOpcodes = new Hashtable();

            //OLD: using (StreamReader reader = wxGlobal.GetResource("Resources.opcodes.txt"))
            using (StreamReader reader = new StreamReader(Application.StartupPath + "\\Utility\\opcodes.txt"))
            {
                while (!(reader.Peek() == -1))
                {
                    mString = reader.ReadLine();
                    if (string.IsNullOrEmpty(mString)) continue;

                    string[] split = mString.Split(";".ToCharArray());
                    if (!(split.Length == 4)) continue;

                    for (byte i = 0; i < 4; i++)
                    {
                        if (split[i].Length > 0)
                        {
                            split[i] = split[i].Trim();
                            split[i] = split[i].Remove(0, 1);
                            split[i] = split[i].Remove(split[i].Length - 1, 1);
                        }
                    }

                    //Fill mOpcodes database
                    if (!mOpcodes.Contains(split[0]))
                    {
                        mOpcodes.Add(split[0],
                                     split[1] + "|" +
                                     split[2] + "|" +
                                     split[3]);
                    }

                    //Fill treeViewOpcodes
                    itemText = split[1].Split(":".ToCharArray())[0];

                    if (itemText.ToLower() == "utilities") continue;


                    if (oldItemText != itemText)
                    {
                        TreeNode t = treeViewOpcodes.Nodes.Add(itemText);
                        t.Name = itemText;
                        index = t.Index;
                        oldItemText = itemText;
                    }

                    if (split[1].Split(":".ToCharArray()).Length > 1)
                    {
                        string itemTextInside = split[1].Split(":".ToCharArray())[1];
                        Int32 i = 0;

                        if (treeViewOpcodes.Nodes[index].Nodes[itemTextInside] == null)
                        {
                            TreeNode inside =
                                treeViewOpcodes.Nodes[index].Nodes.Add(itemTextInside);
                            inside.Name = itemTextInside;
                            i = inside.Index;
                        }
                        else
                            i = treeViewOpcodes.Nodes[index].Nodes[itemTextInside].Index;

                        treeViewOpcodes.Nodes[index].Nodes[i].Nodes.Add(split[0]);

                    }
                    else
                    {
                        treeViewOpcodes.Nodes[index].Nodes.Add(split[0]);
                    }
                }

                //No more needed. This is called by "using" statement on StreamReader.Dispose
                //reader.Close();
            }

            return mOpcodes;

        }



        private void treeViewOpcodes_KeyDown(object sender, KeyEventArgs e)
        {
            //Trick to suppress TreeView BEEP on escape press
            if (e.KeyCode == Keys.Escape)
            {
                e.SuppressKeyPress = true;
            }
        }



        private void treeViewOpcodes_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(DataFormats.Text).ToString() != DragString)
            {
                e.Effect = DragDropEffects.None;
            }
            else if (DragIsValidItem)
                e.Effect = DragDropEffects.Copy;

        }

        private void treeViewOpcodes_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.Button == MouseButtons.Left && DragIsValidItem)
                {
                    treeViewOpcodes.DoDragDrop(DragString, DragDropEffects.Copy);
                }
            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "Form CodeRepository - TextBoxUdo_MouseMove");
            }
        }

        private void treeViewOpcodes_DragOver(object sender, DragEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("DragEntered: " + dragEntered);
            if (DragIsValidItem)
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void treeViewOpcodes_ItemDrag(object sender, ItemDragEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("ITEM DRAG");

            TreeView tempTree = (sender as TreeView);
            Point targetPoint = tempTree.PointToClient(MousePosition);
            TreeNode node = tempTree.GetNodeAt(targetPoint);
            string temp = node.Text;

            if (node.Nodes.Count == 0)
            {
                if (mOpcodes.Contains(temp))
                {
                    string[] split = mOpcodes[temp].ToString().Split("|".ToCharArray());
                    //synopsis = split[2];
                    //name = split[0];
                    if (ModifierKeys == Keys.Control)
                    {
                        if (split[2].ToLower().Contains("not available"))
                        {
                            DragString = temp;
                        }
                        else
                            DragString = split[2];
                    }
                    else
                        DragString = temp;
                }
                else
                    DragString = temp;

                DragIsValidItem = true;
            }
            else
            {
                DragString = "";
                DragIsValidItem = false;
            }

            //System.Diagnostics.Debug.WriteLine("DragIsValidItem: " + DragIsValidItem);

        }

        private void treeViewOpcodes_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                TreeNode selectedNode = treeViewOpcodes.GetNodeAt(e.X, e.Y);
                treeViewOpcodes.SelectedNode = selectedNode;

                string text = selectedNode.Text;

                if (selectedNode.Nodes.Count == 0 && mOpcodes.Contains(text))
                {
                    string[] split = mOpcodes[text].ToString().Split("|".ToCharArray());
                    //tEditor.SetIntelliTip("[" + text + "] - " + split[1], split[2]);
                    wxIntelliTipRepository.ShowTip(
                        "[" + text + "] - " + split[1],
                        split[2]);
                }
            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "wxCSoundOpcodesRepository - treeViewOpcodes_MouseDown");
            }
        }

        private void treeViewOpcodes_AfterSelect(object sender, TreeViewEventArgs e)
        {
            try
            {
                TreeNode selectedNode = e.Node;
                string text = selectedNode.Text;

                if (selectedNode.Nodes.Count == 0 && mOpcodes.Contains(text))
                {
                    string[] split = mOpcodes[text].ToString().Split("|".ToCharArray());
                    //tEditor.SetIntelliTip("[" + text + "] - " + split[1], split[2]);
                    wxIntelliTipRepository.ShowTip(
                        "[" + text + "] - " + split[1],
                        split[2]);
                }
            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "wxCSoundOpcodesRepository - treeViewOpcodes_MouseDown");
            }
        }











    }
}
