﻿
using AnaderiaDemo.Helpers;
using AnaderiaDemo.Models;
using Ganaderia.Helpers;
using System.Text;

namespace AnaderiaDemo
{
    public partial class MainPage : ContentPage
    {
        List<Note> notes;
        int notesCounter;
        public MainPage()
        {
            InitializeComponent();

            FetchNotesFromDatabase();
        }

        private async void FetchNotesFromDatabase()
        {
            notes = await Database.GetItems<Note>();
            if(notes.Count() > 0)
                LoadNote();
            else
                InitializeNewNote();
        }

        private void LoadNote()
        {
            while (notesCounter >= notes.Count())
                notesCounter--;

            Nombre.Text = notes[notesCounter].Name;
            CR.IsChecked = notes[notesCounter].IsCr;
            CO.IsChecked = notes[notesCounter].IsCo;
            Fecha.Date = notes[notesCounter].CreatedAt;

            LoadNoteLines();

            PrintNote.IsVisible = true;
        }

        private async void LoadNoteLines()
        {
            NoteLines.Children.Clear();

            var allNoteLines = await Database.GetItems<NoteLine>();
            if (allNoteLines.Count() == 0)
                return;
            var noteLines = allNoteLines.Where(noteLine => noteLine.NoteId == notes[notesCounter].Id);
            if (noteLines.Count() == 0)
                return;            

            foreach (var noteLine in noteLines)
            {
                var noteLineControl = new UserControls.NoteLine();
                noteLineControl.Piece = noteLine.Piece;
                noteLineControl.Price = noteLine.Price;
                noteLineControl.Quantity = noteLine.Quantity;
                noteLineControl.Description = noteLine.Description;
                noteLineControl.Amount = noteLine.Amount;

                noteLineControl.RemoveLine += NoteLine_RemoveLine;
                noteLineControl.AmountChanged += NoteLine_AmountChanged;

                NoteLines.Children.Add(noteLineControl);
            }

            NoteLine_AmountChanged(null, null);
        }

        private void InitializeNewNote()
        {
            Fecha.Date = DateTime.Now;
            Nombre.Text = string.Empty;

            foreach(UserControls.NoteLine noteLine in NoteLines.Children)
            {
                noteLine.RemoveLine -= NoteLine_RemoveLine;
                noteLine.AmountChanged -= NoteLine_AmountChanged;
            }

            NoteLines.Children.Clear();

            for (int i = 0; i < 9; i++) 
            {
                var noteLine = new UserControls.NoteLine();
                noteLine.RemoveLine += NoteLine_RemoveLine;
                noteLine.AmountChanged += NoteLine_AmountChanged;
                NoteLines.Children.Add(noteLine);
            }
            notesCounter = notes.Count();
            PrintNote.IsVisible = false;
        }

        private void NoteLine_RemoveLine(object sender, EventArgs e)
        {
            var noteLineControl = (UserControls.NoteLine)sender;
            NoteLines.Children.Remove(noteLineControl);
            noteLineControl.RemoveLine -= NoteLine_RemoveLine;
            noteLineControl.AmountChanged -= NoteLine_AmountChanged;
            noteLineControl.Dispose();
            NoteLine_AmountChanged(null, null);
        }

        private async void Save_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (notes is not null &&
                    notes.Count > 0 &&
                    notesCounter < notes.Count)
                {
                    await UpdateNote();
                }
                else
                {
                    await CreateNote();
                }

                Save.Text = "Save";
                PrintNote.IsVisible = true;
                
            }
            catch (Exception ex)
            {
                Save.Text = "Error: " + ex.Message;
            }
        }

        private async Task CreateNote()
        {
            var note = new Note()
            {
                Name = Nombre.Text,
                CreatedAt = DateTime.Now,
                IsCo = CO.IsChecked,
                IsCr = CR.IsChecked,
                TotalAmount = decimal.Parse(TotalAmount.Text)
            };
            var createdNotes = await Database.InsertItem(note);
            if (createdNotes != 1)
                return;

            foreach (var noteLineView in NoteLines.Children)
            {
                if (noteLineView is UserControls.NoteLine noteLineControl)
                {
                    var newNoteLine = new NoteLine
                    {
                        Piece = noteLineControl.Piece,
                        Quantity = noteLineControl.Quantity,
                        Price = noteLineControl.Price,
                        Amount = noteLineControl.Amount,
                        Description = noteLineControl.Description,
                        NoteId = note.Id
                    };
                    var createdNoteLine = await Database.InsertItem(newNoteLine);
                }
            }

            FetchNotesFromDatabase();
        }

        private async Task UpdateNote()
        {
            notes[notesCounter].Name = Nombre.Text;
            notes[notesCounter].UpdateAt = DateTime.Now;
            notes[notesCounter].IsCo = CO.IsChecked;
            notes[notesCounter].IsCr = CR.IsChecked;
            notes[notesCounter].TotalAmount = decimal.Parse(TotalAmount.Text);

            var updatedNotes = await Database.UpdateItem(notes[notesCounter]);
            if (updatedNotes != 1)
                return;

            var allNoteLines = await Database.GetItems<NoteLine>();
            if (allNoteLines.Count() == 0)
                return;
            var noteLines = allNoteLines.Where(noteLine => noteLine.NoteId == notes[notesCounter].Id);

            foreach (var noteLine in noteLines)
            {
                var deletedNoteLines = await Database.DeleteItem(noteLine);
                if (deletedNoteLines != 1)
                {
                    Save.Text = "Error: Can't delete Line ID: " + noteLine.Id;
                    return;
                }
            }

            foreach (var noteLineView in NoteLines.Children)
            {
                if (noteLineView is UserControls.NoteLine noteLineControl)
                {
                    var newNoteLine = new NoteLine
                    {
                        Piece = noteLineControl.Piece,
                        Quantity = noteLineControl.Quantity,
                        Price = noteLineControl.Price,
                        Amount = noteLineControl.Amount,
                        Description = noteLineControl.Description,
                        NoteId = notes[notesCounter].Id
                    };
                    var createdNoteLines = await Database.InsertItem(newNoteLine);
                    if (createdNoteLines != 1)
                    {
                        Save.Text = "Error: Can't create line : " + newNoteLine.Piece;
                        return;
                    }
                }
            }
        }

        private void AddNoteLine_Clicked(object sender, EventArgs e)
        {
            var noteLine = new UserControls.NoteLine();
            noteLine.RemoveLine += NoteLine_RemoveLine;
            noteLine.AmountChanged += NoteLine_AmountChanged;
            NoteLines.Children.Add(noteLine);
        }

        private void NoteLine_AmountChanged(object sender, EventArgs e)
        {
            decimal totalAmount = default(decimal);
            foreach (var noteLineView in NoteLines.Children)
            {
                if (noteLineView is UserControls.NoteLine noteLineControl)
                {
                    totalAmount += noteLineControl.Amount;
                }
            }
            TotalAmount.Text = string.Format("{0:#0.00}", totalAmount);
        }

        private void Previous_Clicked(object sender, EventArgs e)
        {
            if (notesCounter == 0)
                return;
            notesCounter--;
            FetchNotesFromDatabase();
        }

        private void Next_Clicked(object sender, EventArgs e)
        {
            if(notesCounter == notes.Count() - 1)
                return;
            notesCounter++;
            FetchNotesFromDatabase();
        }

        private async void DeleteNote_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (notesCounter < 0)
                    return;

                var allNoteLines = await Database.GetItems<NoteLine>();
                if (allNoteLines.Count() == 0)
                    return;
                if (notesCounter >= notes.Count())
                {
                    notesCounter--;
                    FetchNotesFromDatabase();
                    return;
                }
                var noteLines = allNoteLines.Where(noteLine => noteLine.NoteId == notes[notesCounter].Id);

                var canDeleteNote = true;

                foreach (var noteLine in noteLines)
                {
                    var deletedNoteLines = await Database.DeleteItem(noteLine);
                    if (deletedNoteLines != 1)
                    {
                        canDeleteNote = false;
                        return;
                    }
                }
                if (canDeleteNote)
                {
                    await Database.DeleteItem(notes[notesCounter]);
                }

                FetchNotesFromDatabase();
            }
            catch (Exception)
            {
            }
            
        }

        private void NewNote_Clicked(object sender, EventArgs e)
        {            
            InitializeNewNote();
        }

        private void PrintNote_Clicked(object sender, EventArgs e)
        {
            if(notesCounter >= notes.Count) return;

            var html = "<html>\r\n\r\n<head>\r\n    <title>Ganaderia</title>\r\n</head>\r\n\r\n<body onload=\"window.print();\">\r\n<div>\r\n        Folio : {@Folio}\r\n    </div>\r\n        <div>\r\n        Fecha : {@Fecha}\r\n    </div>\r\n    <div>\r\n        Nombre : {@Nombre}\r\n    </div>\r\n    <div>\r\n        <table>\r\n            <tr>\r\n                <th>\r\n                    Cantidad KG\r\n                </th>\r\n                <th>\r\n                    Descripcion\r\n                </th>\r\n                <th>\r\n                    Precio\r\n                </th>\r\n                <th>\r\n                    Importe\r\n                </th>\r\n            </tr>\r\n            {@CustomRows}\r\n        </table>\r\n    </div>\r\n\r\n</body>\r\n\r\n</html>";

            var customRowsHtml = new StringBuilder();
            foreach (var noteLineView in NoteLines.Children)
            {
                if (noteLineView is UserControls.NoteLine noteLineControl)
                {
                    customRowsHtml.Append("<tr>");

                    customRowsHtml.Append("<td>");
                    customRowsHtml.Append(noteLineControl.Quantity.ToString());
                    customRowsHtml.Append("</td>");

                    customRowsHtml.Append("<td>");
                    customRowsHtml.Append(noteLineControl.Description);
                    customRowsHtml.Append("</td>");

                    customRowsHtml.Append("<td>");
                    customRowsHtml.Append(noteLineControl.Price);
                    customRowsHtml.Append("</td>");

                    customRowsHtml.Append("<td>");
                    customRowsHtml.Append(noteLineControl.Amount);
                    customRowsHtml.Append("</td>");

                    customRowsHtml.Append("</tr>");
                }
            }

            customRowsHtml.Append("<tr>");

            customRowsHtml.Append("<td>");
            if (notes[notesCounter].IsCr)
                customRowsHtml.Append("CR");
            customRowsHtml.Append("</td>");

            customRowsHtml.Append("<td>");
            if (notes[notesCounter].IsCo)
                customRowsHtml.Append("CO");
            customRowsHtml.Append("</td>");

            customRowsHtml.Append("<td>");
            customRowsHtml.Append("Total");
            customRowsHtml.Append("</td>");

            customRowsHtml.Append("<td>");
            customRowsHtml.Append(TotalAmount.Text);
            customRowsHtml.Append("</td>");

            customRowsHtml.Append("</tr>");


            html = html.Replace("{@Folio}", notes[notesCounter].Id.ToString());
            html = html.Replace("{@Fecha}", notes[notesCounter].CreatedAt.ToString());

            html = html.Replace("{@Nombre}", notes[notesCounter].Name);

            html = html.Replace("{@CustomRows}", customRowsHtml.ToString());

           
#if ANDROID
            AndroidHelper.PrintHtml(html);
#elif WINDOWS

            if (ViewReceipt.IsVisible)
            {
                ViewReceipt.IsVisible = false;
                ViewReceipt.Source = new HtmlWebViewSource
                {
                    Html = ""
                };
                PrintNote.Text = "Print Note";
            }
            else
            {
                ViewReceipt.Source = new HtmlWebViewSource
                {
                    Html = html
                };
                ViewReceipt.IsVisible = true;
                PrintNote.Text = "Hide Printing";
            }            

            //ViewReceipt.Reload();

            //WindowsHelper.PrintHtml(html);
#endif

        }
    }
}