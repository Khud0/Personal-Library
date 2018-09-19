using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Text;
using System.Linq;

/*
 * Currently known bugs:
 * 1. The collections won't get deleted. You need to restart the program to re-load them. (It just wasn't added)
*/

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1() // Initialize Form1
        {
            InitializeComponent();
        }

        string path = System.IO.Path.GetDirectoryName(Application.ExecutablePath);
        string pathDoc = System.IO.Path.GetDirectoryName(Application.ExecutablePath) + "\\Settings.xml";
        //string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Personal Library";
        //string pathDoc = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Personal Library" + "\\Settings.xml";
        List<Book> books = new List<Book>(); // Create a list of all the books
        List<string> collections = new List<string>(); // Create a list of all the book authors
        AutoCompleteStringCollection mySource = new AutoCompleteStringCollection(); // A collection for autocompleting book collections' names

        private void Form1_Load(object sender, EventArgs e) // Make a directory and an XML file, Load the Books
        {           
            if ( !Directory.Exists(path) ) // Make a folder
                Directory.CreateDirectory(path);

            if ( !File.Exists(pathDoc) ) // Make a file and its core element
            {
                XmlTextWriter xW = new XmlTextWriter(pathDoc, Encoding.UTF8);
                xW.WriteStartElement("Books");
                xW.WriteEndElement();
                xW.Close();
            }

            LoadBooks();
            SortBooks("Author");

            // Make columns for a listViewer1
            ColumnHeader headerAuthor = new ColumnHeader();
            ColumnHeader headerYearOfPublishing = new ColumnHeader();
            ColumnHeader headerPublisher = new ColumnHeader();
            ColumnHeader headerName = new ColumnHeader();

            headerAuthor.Name = "headerAuthor";
            headerAuthor.Text = "Author";
            headerAuthor.Width = 95;

            headerYearOfPublishing.Name = "headerYearOfPublishing";
            headerYearOfPublishing.Text = "Year";
            headerYearOfPublishing.Width = 60;

            headerPublisher.Name = "headerPublisher";
            headerPublisher.Text = "Publisher";
            headerPublisher.Width = 95;

            headerName.Name = "headerName";
            headerName.Text = "Name";
            headerName.Width = 95;
            
            listView1.Columns.Add(headerAuthor);
            listView1.Columns.Add(headerYearOfPublishing);
            listView1.Columns.Add(headerPublisher);
            listView1.Columns.Add(headerName);
            
            // Needed to see other columns and scrolls(?)
            listView1.Scrollable = true;
            listView1.View = View.Details;
            
            listView1.HideSelection = false; // Needed to always show the selected book, even if the listViewer1 is not in focus
        }

        private void button2_Click(object sender, EventArgs e) // Add a Book
        {
            AddBook();
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e) // What happens if you select a new item
        {
            try
            {                                                                                              
                tbAuthor.Text = books[listView1.SelectedItems[0].Index].Author; // [0] is probably the instance of an item you've selected (for multi-selections)
                tbPublisher.Text = books[listView1.SelectedItems[0].Index].Publisher;
                tbYear.Text = books[listView1.SelectedItems[0].Index].Genre; 
                tbYearOfPublishing.Text = books[listView1.SelectedItems[0].Index].YearOfPublishing; 
                tbTomes.Text = books[listView1.SelectedItems[0].Index].Tomes; 
                tbName.Text = books[listView1.SelectedItems[0].Index].Name; // Show the stored name from a list "books"
                tbCollection.Text = books[listView1.SelectedItems[0].Index].Collection; 
                tbComments.Text = books[listView1.SelectedItems[0].Index].Comments; 
            }
            catch {}
        }

        private void button3_Click(object sender, EventArgs e) // Press a "Remove Book" button
        {
            Remove();
        }

        private void removeToolStripMenuItem_Click_1(object sender, EventArgs e) // RMB - "Remove" on an item in the list
        {
            Remove();
        }          

        private void button1_Click(object sender, EventArgs e) // Press a "Save Changes" button
        {
            try
            {  
                Book currentBook = books[listView1.SelectedItems[0].Index];

                currentBook.Author = tbAuthor.Text;
                currentBook.Publisher = tbPublisher.Text;
                currentBook.Genre = tbYear.Text;
                currentBook.YearOfPublishing = tbYearOfPublishing.Text;
                currentBook.Tomes = tbTomes.Text;
                currentBook.Name = tbName.Text;
                currentBook.Collection = tbCollection.Text;
                currentBook.Comments = tbComments.Text;

                int index = listView1.SelectedItems[0].Index; // Find the currently selected item index
                listView1.SelectedItems[0].Remove(); // Remove this Item
                ListViewItem I = new ListViewItem(new[] {currentBook.Author, currentBook.YearOfPublishing, currentBook.Publisher, currentBook.Name}); // Add the Item back with new names
                listView1.Items.Insert(index, I);
                
                ClearText();
            }
            catch {}
   
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e) // Fill the XML document once you close the Form
        {
            SaveBooks();
        }
    

        private void btnSearch_Click(object sender, EventArgs e) // Search Button
        {
            string searchText = tbSearch.Text;
            if (!(searchText == String.Empty)) // If the string is not empty, select an item that corresponds
            {
                int newIndex = FindBook(searchText);
                if (newIndex != -1) // Select the item related, if it exists
                {
                    listView1.Focus();
                    listView1.Items[newIndex].Selected = true;
                    //listView1.FocusedItem = listView1.Items[newIndex];
                }
            }
        }

        #region Menu Strip
        private void exitToolStripMenuItem_Click(object sender, EventArgs e) // "//File//Exit"
        {
            System.Windows.Forms.Application.Exit();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e) // "//File//Save"
        {
            SaveBooks();
        }

        private void twitterToolStripMenuItem_Click(object sender, EventArgs e) // "//Author//Twitter"
        {
            System.Diagnostics.Process.Start("http://www.twitter.com/Khud0Steam");
        }

        private void gitHubToolStripMenuItem_Click(object sender, EventArgs e) // "//Author//GitHub"
        {
            System.Diagnostics.Process.Start("https://github.com/Khud0");
        }

        private void steamToolStripMenuItem_Click(object sender, EventArgs e) // "//Author//Steam"
        {
            System.Diagnostics.Process.Start("https://store.steampowered.com/dev/Khud0/"); 
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e) // "//File//Clear"
        {
            ClearText();
        }

        #endregion

        #region "Sort" Buttons
        private void button4_Click(object sender, EventArgs e) // Sort books by their Authors
        {
            SortBooks("Author");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            SortBooks("YearOfPublishing");
        }

        private void button6_Click(object sender, EventArgs e)
        {
            SortBooks("Publisher");
        }

        private void button7_Click(object sender, EventArgs e)
        {
            SortBooks("Name");
        }
        #endregion

        #region Methods    
        private void SaveBooks() // Save all of the books into an XML File
        {
            XmlDocument xDoc = new XmlDocument();
            
            xDoc.Load(pathDoc);

            // Remove all the Child nodes from the "Books" node (root);
            XmlNode xNode = xDoc.SelectSingleNode("Books");
            xNode.RemoveAll();

            foreach(Book b in books)
            {
                XmlNode xTop = xDoc.CreateElement("Book");
                
                XmlNode xAuthor = xDoc.CreateElement("Author");
                XmlNode xPublisher = xDoc.CreateElement("Publisher");
                XmlNode xGenre = xDoc.CreateElement("Genre"); // 
                XmlNode xYearOfPublishing = xDoc.CreateElement("YearOfPublishing");
                XmlNode xTomes = xDoc.CreateElement("Tomes");
                XmlNode xName = xDoc.CreateElement("Name");
                XmlNode xCollection = xDoc.CreateElement("Collection");
                XmlNode xComments = xDoc.CreateElement("Comments");
                
                xAuthor.InnerText = b.Author;
                xPublisher.InnerText = b.Publisher;
                xGenre.InnerText = b.Genre;
                xYearOfPublishing.InnerText = b.YearOfPublishing;
                xTomes.InnerText = b.Tomes;
                xName.InnerText = b.Name;
                xCollection.InnerText = b.Collection;
                xComments.InnerText = b.Comments;

                xTop.AppendChild(xAuthor);
                xTop.AppendChild(xPublisher);
                xTop.AppendChild(xGenre);
                xTop.AppendChild(xYearOfPublishing);
                xTop.AppendChild(xTomes);
                xTop.AppendChild(xName);
                xTop.AppendChild(xCollection);
                xTop.AppendChild(xComments);

                xDoc.DocumentElement.AppendChild(xTop);
            }

            xDoc.Save(pathDoc);
        } 

        private void LoadBooks() // Load the books from an XML File
        {

            // Open the document to read from
            XmlDocument xDoc = new XmlDocument(); 
            xDoc.Load(pathDoc);
            
            // Make a list of nodes to read from, in this case: root, all of the 1st children nodes (main, repeated one);
            XmlNodeList xList = xDoc.SelectNodes("/Books/Book");

            // Load all of the Books
            foreach(XmlNode xNode in xList)
            {
                Book b = new Book();
                b.Author = xNode.SelectSingleNode("Author").InnerText;
                b.Publisher = xNode.SelectSingleNode("Publisher").InnerText;
                b.Tomes = xNode.SelectSingleNode("Tomes").InnerText;
                b.Genre = xNode.SelectSingleNode("Genre").InnerText;
                b.YearOfPublishing = xNode.SelectSingleNode("YearOfPublishing").InnerText;
                b.Name = xNode.SelectSingleNode("Name").InnerText;
                b.Collection = xNode.SelectSingleNode("Collection").InnerText;
                b.Comments = xNode.SelectSingleNode("Comments").InnerText;

                books.Add(b); // Add all of the books to the books list   
                AddCollection(b); // Add all of the collections to the collections list (can be chosen from a drop-down list)
                

                //listView1.Items.Add(b.Name);

                ListViewItem I = new ListViewItem(new[] {b.Author, b.YearOfPublishing, b.Publisher, b.Name}); // Add the book to the list on the left
                listView1.Items.Add(I);
            }
        } 

        private void AddCollection(Book b)
        {
            if (b.Collection == null) return; // Check if the collection parameter is filled
            
            if (!collections.Contains(b.Collection)) // Add all of the collections to the collections list (can be chosen from a drop-down list)
            {
                collections.Add(b.Collection);         // If the list doesn't already have it.
                mySource.Add(b.Collection);
            }
            

            tbCollection.AutoCompleteMode = AutoCompleteMode.Suggest;
            tbCollection.AutoCompleteSource = AutoCompleteSource.CustomSource;
            tbCollection.AutoCompleteCustomSource = mySource;
        }

        private void AddBook() // Add a new book
        {
            // Save everything from the textBoxes, if nothing is inserted - save as "---"
            Book b = new Book(); 
            if (tbAuthor.Text == "") b.Author = "---"; else b.Author = tbAuthor.Text;
            if (tbPublisher.Text == "") b.Publisher = "---"; else b.Publisher = tbPublisher.Text;
            b.Tomes = tbTomes.Text;
            b.Genre = tbYear.Text;
            if (tbYearOfPublishing.Text == "") b.YearOfPublishing = "---"; else b.YearOfPublishing = tbYearOfPublishing.Text;
            if (tbName.Text == "") b.Name = "---"; else b.Name = tbName.Text;
            b.Collection = tbCollection.Text;
            b.Comments = tbComments.Text;
            
            books.Add(b);
            AddCollection(b);

            // Add | Author - Year of Publishing - Publisher - Book Name
            ListViewItem I = new ListViewItem(new[] {b.Author, b.YearOfPublishing, b.Publisher, b.Name}); // Add the book to the list on the left
            listView1.Items.Add(I);
            

            // Clear the text
            ClearText();
        }

        private void Remove() // Remove a book both from the list and from the listView
        {
            try // Removes the possibility of removing while you haven't selected anything yet (would return -1);
            {
                // IMPORTANT! Remove the book from the list before the listView, if you are doing it like that. 
                // Otherwise you won't delete what you intended to.
                books.RemoveAt(listView1.SelectedItems[0].Index);
                listView1.Items.Remove(listView1.SelectedItems[0]);   
                
                ClearText();
            } 
            catch { }
        }

        private void ClearText() // Clear the text in all TextBoxes
        {
            tbAuthor.Text = "";
            tbPublisher.Text = "";
            tbTomes.Text = "";
            tbYear.Text = "";
            tbYearOfPublishing.Text = "";
            tbName.Text = "";
            tbCollection.Text = "";
            tbComments.Text = "";
        }

        private void SortBooks(string method) // Sort the books in an alphabetical order
        {
            switch(method) // Sort the books list
            {
                case "Author": books.Sort((x,y) => x.Author.CompareTo(y.Author)); break;
                case "YearOfPublishing": books.Sort((x,y) => x.YearOfPublishing.CompareTo(y.YearOfPublishing)); break;
                case "Publisher": books.Sort((x, y) => x.Publisher.CompareTo(y.Publisher)); break;
                case "Name": books.Sort((x, y) => x.Name.CompareTo(y.Name)); break;
                
                default: books.Sort((x,y) => x.Author.CompareTo(y.Author)); break;
            }

            listView1.Items.Clear(); // Clear the entire ListViewer

            foreach(Book b in books) // Re-write all the books into the ListViewer
            {          
                ListViewItem I = new ListViewItem(new[] {b.Author, b.YearOfPublishing, b.Publisher, b.Name}); // Add the book to the list on the left
                listView1.Items.Add(I);
            }            
        }

        private int FindBook(string searchText)
        {
            foreach(Book b in books)
            {
                if ( b.Name.ToLower().Contains(searchText.ToLower()) ) return books.IndexOf(b); // Search by Book Name
                else if ( b.Author.ToLower().Contains(searchText.ToLower()) ) return books.IndexOf(b); // Search by Book Author
                else if ( b.Publisher.ToLower().Contains(searchText.ToLower()) ) return books.IndexOf(b); // Search by Book Publisher
                else if ( b.YearOfPublishing.ToLower().Contains(searchText.ToLower()) ) return books.IndexOf(b); // Search by Book Publisher
            }

            return -1;
        }


        #endregion

    }

    class Book // Book class contains: Author, Publisher, Genre, YearOfPublishing, Tomes, Name, Collection, Comments
    {

        public string Author
        {
            get;
            set;
        }

        public string Publisher
        {
            get;
            set;
        }

        public string Genre
        {
            get;
            set;
        }   

        public string YearOfPublishing
        {
            get;
            set;
        }   
        
        public string Tomes
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public string Collection
        {
            get;
            set;
        }

        public string Comments
        {
            get;
            set;
        }

    } 

}

