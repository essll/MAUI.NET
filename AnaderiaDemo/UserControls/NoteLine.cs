namespace AnaderiaDemo.UserControls
{
    public class NoteLine : ContentView, IDisposable
    {
        Entry piece = new Entry();
        Entry quantity = new Entry();
        Entry description = new Entry();
        Entry price = new Entry();
        Entry amount = new Entry();
        Button removeLine = new Button { Text = "X" };

        public string Piece
        {
            get => piece.Text; 
            set => piece.Text = value;
        }
        public int Quantity
        {
            get => int.Parse(quantity.Text);
            set => quantity.Text = value.ToString();
        }
        public string Description
        {
            get => description.Text;
            set => description.Text = value;
        }
        public decimal Price
        {
            get => decimal.Parse(price.Text);
            set => price.Text = value.ToString();
        }
        public decimal Amount
        {
            get => decimal.Parse(amount.Text);
            set => amount.Text = string.Format("{0:#.00}", value);
        }

        public event EventHandler RemoveLine;


        public NoteLine()
        {
            Initialize();
        }

        void Initialize()
        {
            var container = new HorizontalStackLayout
            {
                Spacing = 35
            };            

            quantity.Unfocused += Entry_Unfocused;
            price.Unfocused += Entry_Unfocused;
            amount.IsEnabled = false;
            removeLine.Clicked += RemoveLine_Clicked;

            container.Children.Add(piece);
            container.Children.Add(quantity);
            container.Children.Add(description);
            container.Children.Add(price);
            container.Children.Add(amount);
            container.Children.Add(removeLine);

            Content = container;
        }

        private void RemoveLine_Clicked(object sender, EventArgs e)
        {
            RemoveLine?.Invoke(this, e);
        }

        private void Entry_Unfocused(object sender, FocusEventArgs e)
        {
            try
            {
                int qty = int.Parse(quantity.Text);
                decimal pricePerQuantity = decimal.Parse(price.Text);
                amount.Text = string.Format("{0:#.00}", qty * pricePerQuantity);
            }
            catch{ }            
        }

        public void Dispose()
        {
            quantity.Unfocused -= Entry_Unfocused;
            price.Unfocused -= Entry_Unfocused;
            removeLine.Clicked -= RemoveLine_Clicked;
        }        
    }
}
