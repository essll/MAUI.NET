
using AnaderiaDemo.Helpers;
using AnaderiaDemo.Models;
using Ganaderia.Helpers;

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

            LoadNoteLines();    
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
            }

            NoteLines.Children.Clear();

            for (int i = 0; i < 9; i++) 
            {
                var noteLine = new UserControls.NoteLine();
                noteLine.RemoveLine += NoteLine_RemoveLine;
                NoteLines.Children.Add(noteLine);
            }
            notesCounter = notes.Count();
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
            TotalAmount.Text = string.Format("{0:#.00}", totalAmount);
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
            var html = "<html>\r\n\r\n<head>\r\n</head>\r\n\r\n<body onload=\"window.print();\">\r\n    Testing Ganaderia\r\n\r\n</body>\r\n\r\n</html>";
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