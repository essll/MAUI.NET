using AnaderiaDemo.Helpers;
using AnaderiaDemo.Models;

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
                InitializeNoteLines();
        }

        private void LoadNote()
        {
            Nombre.Text = notes[notesCounter].Name;
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
                NoteLines.Children.Add(noteLineControl);
            }
        }

        private void InitializeNoteLines()
        {
            for(int i = 0; i < 5; i++) 
            {
                var noteLine = new UserControls.NoteLine();
                noteLine.RemoveLine += NoteLine_RemoveLine;
                NoteLines.Children.Add(noteLine);
            }
        }

        private void NoteLine_RemoveLine(object sender, EventArgs e)
        {
            NoteLines.Children.Remove((IView)sender);
        }

        private async void Save_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (notes is not null &&  notes.Count > 0)
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
                CreatedAt = DateTime.Now
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
            NoteLines.Children.Add(noteLine);
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
            if (notesCounter < 0)
                return;
            
            var deletedNotes = await Database.DeleteItem(notes[notesCounter]);
            
            FetchNotesFromDatabase();
        }
    }
}